namespace Rexster.Messages
{
    /// <summary>
    /// Defines the different serialization channels supported by RexPro Servers.
    /// </summary>
    public static class Channel
    {
        /// <summary>
        /// Console serialization channel.
        /// </summary>
        /// <remarks>Do not use. Only MsgPack is supported by RexProClient.</remarks>
        public const int Console = 1;

        /// <summary>
        /// MsgPack serialization channel.
        /// </summary>
        public const int MsgPack = 2;

        /// <summary>
        /// GraphSON serialization channel.
        /// </summary>
        /// <remarks>Do not use. Only MsgPack is supported by RexProClient.</remarks>
        public const int Graphson = 3;
    }
}
