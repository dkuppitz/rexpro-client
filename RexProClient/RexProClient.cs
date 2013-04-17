﻿namespace Rexster
{
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Net.Sockets;

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

        private readonly string host;
        private readonly int port;

        public RexProClient()
            : this("localhost", DefaultPort)
        {
        }

        public RexProClient(string host) : this(host, DefaultPort)
        {
        }

        public RexProClient(string host, int port)
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

        public ScriptResponse Query(string script, Dictionary<string, object> bindings = null, RexProSession session = null, bool isolate = true)
        {
            return this.Query<object>(script, bindings, session, isolate);
        }

        public ScriptResponse<T> Query<T>(string script, Dictionary<string, object> bindings = null, RexProSession session = null, bool isolate = true)
        {
            var request = new ScriptRequest(script, bindings);
            return this.ExecuteScript<T>(request, session, isolate);
        }

        public ScriptResponse ExecuteScript(ScriptRequest script, RexProSession session = null, bool isolate = true)
        {
            return this.ExecuteScript<object>(script, session, isolate);
        }

        public ScriptResponse<T> ExecuteScript<T>(ScriptRequest script, RexProSession session = null, bool isolate = true)
        {
            script.Meta.Isolate = isolate;
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
                        throw new RexProClientSerializationException("Unexpected message header.");
                    }

                    // skip message length bytes
                    // don't use unpacker as it throws an exception for some lengths (don't know why)
                    stream.Skip(4);

                    if (responseMessageType == MessageType.Error)
                    {
                        var msg = ErrorMessageSerializer.Unpack(stream);
                        throw new RexProClientException(msg);
                    }

                    var expectedResponseMessageType = ExpectedResponseMessageType[requestMessageType];
                    if (responseMessageType != expectedResponseMessageType)
                    {
                        var msg = string.Format(CultureInfo.InvariantCulture,
                                                "Unexpected message type '{0}', expected '{1}'.", requestMessageType,
                                                expectedResponseMessageType);
                        throw new RexProClientSerializationException(msg);
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

        public RexProSession OpenSession(/*RexProConfiguration config*/)
        {
            var request = new SessionRequest();
            var response = this.SendRequest<SessionRequest, SessionResponse>(request, MessageType.SessionRequest);
            return new RexProSession(this, response.Session);
        }

        public void KillSession(RexProSession session)
        {
            var request = new SessionRequest(session, true);
            this.SendRequest<SessionRequest, SessionResponse>(request, MessageType.SessionRequest);
        }
    }
}