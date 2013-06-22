namespace Rexster
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Net.Sockets;
    using System.Reflection;
    using System.Threading;

    using MsgPack;
    using MsgPack.Serialization;
    
    using Rexster.Messages;

    public class RexProClient
    {
        private const byte ProtocolVersion = 0;
        private const int DefaultPort = 8184;

        private static readonly IDictionary<byte, byte> ExpectedResponseMessageType = new Dictionary<byte, byte>
        {
            { MessageType.SessionRequest, MessageType.SessionResponse },
            { MessageType.ScriptRequest, MessageType.MsgPackScriptResponse }
        };

        private static readonly MessagePackSerializer<ErrorResponse> ErrorMessageSerializer =
            MessagePackSerializer.Create<ErrorResponse>();

        private static readonly ConcurrentDictionary<Guid, NetworkStream> SessionStreams =
            new ConcurrentDictionary<Guid, NetworkStream>();

        private GraphSettings settings;
        private readonly string host;
        private readonly int port;
        private readonly Func<TcpClient> tcpClientProvider;

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

        private static Func<TcpClient> CreateDefaultTcpProvider(string host, int port)
        {
            return () => new TcpClient(host, port);
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

        public dynamic Query(string script, Dictionary<string, object> bindings = null, RexProSession session = null, bool transaction = true)
        {
            var request = new ScriptRequest(script, bindings);
            return this.ExecuteScript<object>(request, session, transaction).Result;
        }

        public T Query<T>(string script, Dictionary<string, object> bindings = null, RexProSession session = null, bool transaction = true)
        {
            var request = new ScriptRequest(script, bindings);
            return this.ExecuteScript<T>(request, session, transaction).Result;
        }

        public ScriptResponse ExecuteScript(ScriptRequest script, RexProSession session = null, bool transaction = true)
        {
            return this.ExecuteScript<object>(script, session, transaction);
        }

        public ScriptResponse<T> ExecuteScript<T>(ScriptRequest script, RexProSession session = null, bool transaction = true)
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

            return this.SendRequest<ScriptRequest, ScriptResponse<T>>(script, MessageType.ScriptRequest);
        }

        private TResponse SendRequest<TRequest, TResponse>(TRequest message, byte requestMessageType)
            where TRequest : RexProMessage
            where TResponse : RexProMessage
        {
            TResponse result;

            NetworkStream netStream;

            if (message.Session != null)
            {
                var guid = new Guid(message.Session);
                netStream = SessionStreams.GetOrAdd(guid, _ => tcpClientProvider().GetStream());
            }
            else
            {
                netStream = tcpClientProvider().GetStream();
            }

            using (var packer = Packer.Create(netStream, false))
            {
                packer.Pack(ProtocolVersion).Pack(requestMessageType);

                PackMessage(netStream, message);

                using (var unpacker = Unpacker.Create(netStream, false))
                {
                    byte protocolVersion, responseMessageType;

                    if (
                        !(unpacker.ReadByte(out protocolVersion) && protocolVersion == 0 &&
                          unpacker.ReadByte(out responseMessageType)))
                    {
                        throw new RexProClientSerializationException("Unexpected message header.");
                    }

                    // skip message length bytes
                    // don't use unpacker as it throws an exception for some lengths (don't know why)
                    //
                    // Thanks to Ozcan Degirmenci (glikoz) for pointing out that reading the stream without taking the
                    // message length header into account can/will probably fail.
                    const int bufferSize = 4096;
                    var buffer = new byte[bufferSize];
                    var bytesRead = 0;
                    var messageLength =
                        (netStream.ReadByte() << 24) |
                        (netStream.ReadByte() << 16) |
                        (netStream.ReadByte() << 8) |
                        (netStream.ReadByte());

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
                            Thread.SpinWait(10);
                        }

                        stream.Seek(0, SeekOrigin.Begin);

                        if (responseMessageType == MessageType.Error)
                        {
                            var error = ErrorMessageSerializer.Unpack(stream);
                            throw new RexProClientException(error);
                        }

                        var expectedResponseMessageType = ExpectedResponseMessageType[requestMessageType];
                        if (responseMessageType != expectedResponseMessageType)
                        {
                            var msg = string.Format(CultureInfo.InvariantCulture,
                                                    "Unexpected message type '{0}', expected '{1}'.",
                                                    requestMessageType, expectedResponseMessageType);
                            throw new RexProClientSerializationException(msg);
                        }

                        var responseType = typeof (TResponse);
                        if (responseType.IsDynamicScriptResponse())
                        {
                            PropertyInfo pi;

                            var tmp = MessagePackSerializer.Create<DynamicScriptResponse>().Unpack(stream);

                            result = Activator.CreateInstance<TResponse>();
                            result.Request = tmp.Request;
                            result.Session = tmp.Session;

                            if (null != (pi = responseType.GetProperty("Meta")))
                                pi.GetSetMethod().Invoke(result, new object[] { tmp.Meta });
                            if (null != (pi = responseType.GetProperty("Result")))
                                pi.GetSetMethod().Invoke(result, new[] { tmp.Result });
                            if (null != (pi = responseType.GetProperty("Bindings")))
                                pi.GetSetMethod().Invoke(result, new object[] { tmp.Bindings });
                        }
                        else
                        {
                            result = MessagePackSerializer.Create<TResponse>().Unpack(stream);
                        }
                    }
                }
            }

            if (message.Session == null)
            {
                netStream.Close();
                netStream.Dispose();
            }

            return result;
        }

        private static void PackMessage<T>(Stream stream, T message)
            where T : RexProMessage
        {
            byte[] messageBytes;

            using (var messageStream = new MemoryStream())
            using (var messagePacker = Packer.Create(messageStream))
            {
                messagePacker.Pack(message);
                messageBytes = messageStream.ToArray();
            }

            var length = messageBytes.Length;
            stream.WriteByte((byte) ((length >> 24) & 0xFF));
            stream.WriteByte((byte) ((length >> 16) & 0xFF));
            stream.WriteByte((byte) ((length >> 8) & 0xFF));
            stream.WriteByte((byte) (length & 0xFF));
            stream.Write(messageBytes, 0, length);
        }

        public RexProSession StartSession()
        {
            var request = new SessionRequest(this.settings);
            var response = this.SendRequest<SessionRequest, SessionResponse>(request, MessageType.SessionRequest);
            var session = new RexProSession(this, response.Session);
            var sessionGuid = new Guid(response.Session);

            SessionStreams.GetOrAdd(sessionGuid, _ => tcpClientProvider().GetStream());
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
            this.SendRequest<SessionRequest, SessionResponse>(request, MessageType.SessionRequest);
        }
    }
}
