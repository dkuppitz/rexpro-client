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

        private readonly string host;
        private readonly int port;
        private GraphSettings settings;
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
        {
            this.host = host;
            this.port = port;
            this.Settings = settings;
        }

        public RexProClient(Func<TcpClient> tcpClientProvider) 
            : this(tcpClientProvider, GraphSettings.Default)
        {
        }

        public RexProClient(Func<TcpClient> tcpClientProvider, GraphSettings settings)
        {
            this.tcpClientProvider = tcpClientProvider;
        }

        public string Host
        {
            get { return this.host; }
        }

        public int Port
        {
            get { return this.port; }
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

        private TcpClient NewTcpClient() {
            System.Console.WriteLine("NEW CLIENT: "+tcpClientProvider+" / "+host+" / "+port);
            return (tcpClientProvider != null ? 
                tcpClientProvider() : new TcpClient(this.host, this.port));
        }

        private TResponse SendRequest<TRequest, TResponse>(TRequest message, byte requestMessageType)
            where TRequest : RexProMessage
            where TResponse : RexProMessage
        {
            TResponse result;

            Stream stream;

            if (message.Session != null)
            {
                var guid = new Guid(message.Session);
                stream = SessionStreams.GetOrAdd(guid, _ => NewTcpClient().GetStream());
            }
            else
            {
                stream = NewTcpClient().GetStream();
            }

            using (var packer = Packer.Create(stream, false))
            {
                packer.Pack(ProtocolVersion).Pack(requestMessageType);

                PackMessage(stream, message);

                using (var unpacker = Unpacker.Create(stream, false))
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
                    stream.Skip(4);

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

            if (message.Session == null)
            {
                stream.Close();
                stream.Dispose();
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

            SessionStreams.GetOrAdd(sessionGuid, _ => NewTcpClient().GetStream());
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