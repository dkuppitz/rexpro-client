namespace Rexster
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Net.Sockets;

    using MsgPack;
    using MsgPack.Serialization;
    
    using Rexster.Messages;

    public class RexsterClient
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

        private static readonly Dictionary<string, object> EmptyBindings = new Dictionary<string, object>();

        private readonly string host;
        private readonly int port;

        public RexsterClient()
            : this("localhost", DefaultPort)
        {
        }

        public RexsterClient(string host) : this(host, DefaultPort)
        {
        }

        public RexsterClient(string host, int port)
        {
            this.host = host;
            this.port = port;
        }

        public string Host
        {
            get { return this.host; }
        }

        public int Port
        {
            get { return this.port; }
        }

        public ScriptResponse Query(string script)
        {
            return this.Query<object>(script, EmptyBindings);
        }

        public ScriptResponse<T> Query<T>(string script)
        {
            return this.Query<T>(script, EmptyBindings);
        }

// ReSharper disable MethodOverloadWithOptionalParameter

        public ScriptResponse<T> Query<T>(string script, params Tuple<string, object>[] bindings)
        {
            return Query<T>(script, bindings.ToDictionary(_ => _.Item1, _ => _.Item2));
        }

        public ScriptResponse<T> Query<T>(string script, params KeyValuePair<string, object>[] bindings)
        {
            return Query<T>(script, bindings.ToDictionary(_ => _.Key, _ => _.Value));
        }

// ReSharper restore MethodOverloadWithOptionalParameter

        public ScriptResponse<T> Query<T>(string script, Dictionary<string, object> bindings)
        {
            var request = new ScriptRequest(script, bindings);
            return this.ExecuteScript<T>(request);
        }

        public ScriptResponse ExecuteScript(ScriptRequest script)
        {
            return this.ExecuteScript<object>(script);
        }

        public ScriptResponse<T> ExecuteScript<T>(ScriptRequest script)
        {
            return this.SendRequest<ScriptRequest, ScriptResponse<T>>(script, MessageType.ScriptRequest);
        }

        private TResponse SendRequest<TRequest, TResponse>(TRequest message, byte requestMessageType)
            where TRequest : RexProMessage
            where TResponse : RexProMessage
        {
            var serializer = MessagePackSerializer.Create<TResponse>();

            using (var tcpClient = new TcpClient(this.host, this.port))
            using (var stream = tcpClient.GetStream())
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
                        throw new RexsterClientSerializationException("Unexpected message header.");
                    }

                    // skip message length bytes
                    // don't use unpacker as it throws an exception for some lengths (don't know why)
                    stream.Skip(4);

                    if (responseMessageType == MessageType.Error)
                    {
                        var msg = ErrorMessageSerializer.Unpack(stream);
                        throw new RexsterClientException(msg);
                    }

                    if (responseMessageType != ExpectedResponseMessageType[requestMessageType])
                    {
                        var msg = string.Format(CultureInfo.InvariantCulture,
                                                "Unexpected message type '{0}', expected '{1}'.", requestMessageType,
                                                MessageType.MsgPackScriptResponse);
                        throw new RexsterClientSerializationException(msg);
                    }

                    return serializer.Unpack(stream);
                }
            }
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

        public SessionResponse OpenSession()
        {
            var request = new SessionRequest();
            return this.SendRequest<SessionRequest, SessionResponse>(request, MessageType.SessionRequest);
        }

        public SessionResponse KillSession(Guid session)
        {
            return this.KillSession(session.ToByteArray());
        }

        public SessionResponse KillSession(byte[] session)
        {
            var request = new SessionRequest(session, true);
            return this.SendRequest<SessionRequest, SessionResponse>(request, MessageType.SessionRequest);
        }
    }
}