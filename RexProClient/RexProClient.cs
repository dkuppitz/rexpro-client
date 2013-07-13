namespace Rexster
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Net.Sockets;
    using System.Text;
    using System.Threading;
    using Newtonsoft.Json;
    using Rexster.Messages;

    public class RexProClient
    {
        private const byte ProtocolVersion = 1;
        private const byte SerializerType = 1;

        private const int DefaultPort = 8184;

        private static readonly byte[] InitialBuffer = new byte[]
        {
            ProtocolVersion,
            SerializerType,
            0, 0, 0, 0 // Reserved Bytes
        };

        private static readonly IDictionary<byte, byte> ExpectedResponseMessageType = new Dictionary<byte, byte>
        {
            { MessageType.SessionRequest, MessageType.SessionResponse },
            { MessageType.ScriptRequest, MessageType.ScriptResponse }
        };

        private static readonly ConcurrentDictionary<Guid, NetworkStream> SessionStreams =
            new ConcurrentDictionary<Guid, NetworkStream>();

        private readonly string host;
        private readonly int port;
        private readonly Func<TcpClient> tcpClientProvider;
        private GraphSettings settings;

        public RexProClient()
            : this("localhost", DefaultPort, GraphSettings.Default)
        {
        }

        public RexProClient(string host)
            : this(host, DefaultPort, GraphSettings.Default)
        {
        }

        public RexProClient(string host, int port)
            : this(host, port, GraphSettings.Default)
        {
        }

        public RexProClient(string host, int port, GraphSettings settings)
            : this(CreateDefaultTcpProvider(host, port), settings)
        {
            this.host = host;
            this.port = port;
        }

        public RexProClient(Func<TcpClient> tcpClientProvider)
            : this(tcpClientProvider, GraphSettings.Default)
        {
        }

        public RexProClient(Func<TcpClient> tcpClientProvider, GraphSettings settings)
        {
            if (tcpClientProvider == null)
                throw new ArgumentNullException("tcpClientProvider");

            this.tcpClientProvider = tcpClientProvider;
            this.Settings = settings;
        }

        public string Host
        {
            get
            {
                if (this.host != null)
                    return this.host;

                throw new RexProClientException("No static host provided.");
            }
        }

        public int Port
        {
            get
            {
                if (this.port > 0)
                    return this.port;

                throw new RexProClientException("No static port provided.");
            }
        }

        public GraphSettings Settings
        {
            get { return this.settings; }
            set { this.settings = value ?? GraphSettings.Default; }
        }

        private static Func<TcpClient> CreateDefaultTcpProvider(string host, int port)
        {
            return () => new TcpClient(host, port);
        }

        public dynamic Query(string script, Dictionary<string, object> bindings = null, RexProSession session = null,
                             bool transaction = true)
        {
            var request = new ScriptRequest(script, bindings);
            return this.ExecuteScript<object>(request, session, transaction).Result;
        }

        public T Query<T>(string script, Dictionary<string, object> bindings = null, RexProSession session = null,
                          bool transaction = true)
        {
            var request = new ScriptRequest(script, bindings);
            return this.ExecuteScript<T>(request, session, transaction).Result;
        }

        public ScriptResponse ExecuteScript(ScriptRequest script, RexProSession session = null, bool transaction = true)
        {
            return this.ExecuteScript<object>(script, session, transaction);
        }

        public ScriptResponse<T> ExecuteScript<T>(ScriptRequest script, RexProSession session = null,
                                                  bool transaction = true)
        {
            script.Meta.InSession = session != null;
            script.Meta.Isolate = session == null;
            script.Meta.Transaction = transaction;
            script.Session = session;

            if (session == null)
            {
                if (script.Meta.GraphName == null)
                    script.Meta.GraphName = this.settings.GraphName;
                if (script.Meta.GraphObjectName == null)
                    script.Meta.GraphObjectName = this.settings.GraphObjectName;
            }

            return this.SendRequest<ScriptRequest, ScriptResponse<T>>(script);
        }

        private TResponse SendRequest<TRequest, TResponse>(TRequest message)
            where TRequest : RexProMessage
            where TResponse : RexProMessage
        {
            byte requestMessageType;
            int messageLength;
            var messageBytes = BuildRequestMessageBuffer(message, out requestMessageType,
                                                         out messageLength);

            var netStream = message.Session != Guid.Empty
                                ? SessionStreams.GetOrAdd(message.Session, _ => this.tcpClientProvider().GetStream())
                                : this.tcpClientProvider().GetStream();

            try
            {
                netStream.Write(messageBytes, 0, messageLength);
                return ParseResponse<TResponse>(netStream, requestMessageType);
            }
            finally
            {
                if (message.Session == Guid.Empty)
                {
                    netStream.Close();
                    netStream.Dispose();
                }
            }
        }

        private static byte[] BuildRequestMessageBuffer<TRequest>(TRequest message,
                                                                  out byte requestMessageType,
                                                                  out int messageLength)
            where TRequest : RexProMessage
        {
            int offset;
            var json = JsonConvert.SerializeObject(message.ToSerializableArray());
            var jsonBytes = Encoding.UTF8.GetBytes(json);
            var jsonLength = jsonBytes.Length;
            var messageBytes = new byte[(offset = InitialBuffer.Length) + jsonBytes.Length + 5];

            Array.Copy(InitialBuffer, messageBytes, offset);

            if (message is SessionRequest)
            {
                messageBytes[offset++] = requestMessageType = 1;
            }
            else if (message is ScriptRequest)
            {
                messageBytes[offset++] = requestMessageType = 3;
            }
            else
            {
                throw new RexProClientException(string.Format("Unsupported message type: {0}",
                                                              message.GetType().Name));
            }

            messageBytes[offset++] = (byte) ((jsonLength >> 24) & 255);
            messageBytes[offset++] = (byte) ((jsonLength >> 16) & 255);
            messageBytes[offset++] = (byte) ((jsonLength >> 8) & 255);
            messageBytes[offset++] = (byte) (jsonLength & 255);

            Array.Copy(jsonBytes, 0, messageBytes, offset, jsonLength);

            messageLength = offset + jsonLength;

            return messageBytes;
        }

        private static TResponse ParseResponse<TResponse>(NetworkStream netStream, byte requestMessageType)
            where TResponse : RexProMessage
        {
            TResponse result;

            const int headerLength = 11;
            var headerBytes = new byte[headerLength];
            var bytesRead = 0;

            while (bytesRead != headerLength)
            {
                var bytes = netStream.Read(headerBytes, bytesRead, headerLength - bytesRead);
                bytesRead += bytes;
            }

            var expectedResponseMessageType = ExpectedResponseMessageType[requestMessageType];

            if ((headerBytes[0] != ProtocolVersion) ||
                (headerBytes[1] != 1) ||
                (headerBytes[6] | expectedResponseMessageType) != expectedResponseMessageType)
            {
                throw new RexProClientSerializationException("Unexpected message header.");
            }

            var messageLength = (headerBytes[7] << 24) |
                                (headerBytes[8] << 16) |
                                (headerBytes[9] << 8) |
                                (headerBytes[10]);

            const int bufferSize = 4096;
            var buffer = new byte[bufferSize];
            bytesRead = 0;

            using (var stream = new MemoryStream())
            {
                while (bytesRead < messageLength)
                {
                    var bytes = netStream.Read(buffer, 0, bufferSize);
                    if (bytes > 0)
                    {
                        stream.Write(buffer, 0, bytes);
                        bytesRead += bytes;
                    }
                }

                var json = Encoding.UTF8.GetString(stream.ToArray());

                if (headerBytes[6] == MessageType.Error)
                {
                    var error = new ErrorResponse();
                    error.LoadJson(json);
                    throw new RexProClientException(error);
                }

                if (headerBytes[6] != expectedResponseMessageType)
                {
                    var msg = string.Format(CultureInfo.InvariantCulture,
                                            "Unexpected message type '{0}', expected '{1}'.",
                                            headerBytes[6], expectedResponseMessageType);
                    throw new RexProClientSerializationException(msg);
                }

                switch (headerBytes[6])
                {
                    case MessageType.SessionResponse:
                        result = Activator.CreateInstance<TResponse>();
                        result.LoadJson(json);
                        break;

                        //case MessageType.ScriptResponse:
                    default:
                        result = Activator.CreateInstance<TResponse>();
                        result.LoadJson(json);
                        break;
                }
            }

            return result;
        }

        public RexProSession StartSession()
        {
            var request = new SessionRequest(this.settings);
            var response = this.SendRequest<SessionRequest, SessionResponse>(request);
            var session = new RexProSession(this, response.Session);
            var sessionGuid = response.Session;

            SessionStreams.GetOrAdd(sessionGuid, _ => this.tcpClientProvider().GetStream());
            session.Kill += (sender, args) =>
            {
                while (SessionStreams.ContainsKey(sessionGuid))
                {
                    NetworkStream stream;
                    if (SessionStreams.TryRemove(sessionGuid, out stream))
                    {
                        stream.Close();
                        stream.Dispose();
                    }
                    else Thread.SpinWait(10);
                }
            };

            return session;
        }

        public void KillSession(RexProSession session)
        {
            var request = new SessionRequest(this.settings, session, true);
            this.SendRequest<SessionRequest, SessionResponse>(request);
        }
    }
}