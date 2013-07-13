namespace Rexster.Messages
{
    /// <summary>
    /// Defines the different message types accepted and sent by RexPro Servers.
    /// </summary>
    internal static class MessageType
    {
        /// <summary>
        /// Used for responses from the RexPro Server indicating a problem has occurred.
        /// </summary>
        public const byte Error = 0;

        /// <summary>
        /// Used for requests to open or close a session with the RexPro Server.
        /// </summary>
        public const byte SessionRequest = 1;

        /// <summary>
        /// Used for responses from the RexPro Server to a session request containing session information.
        /// </summary>
        public const byte SessionResponse = 2;

        /// <summary>
        /// Used for requests to process a Gremlin script on the RexPro Server.
        /// </summary>
        public const byte ScriptRequest = 3;

        /// <summary>
        /// Used for responses to a script request.
        /// </summary>
        public const byte ScriptResponse = 5;
    }
}
