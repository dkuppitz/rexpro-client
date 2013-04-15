namespace Rexster
{
    public static class MessageType
    {
        public const byte Error = 0;
        public const byte SessionRequest = 1;
        public const byte SessionResponse = 2;
        public const byte ScriptRequest = 3;
        public const byte ConsoleScriptResponse = 4;
        public const byte MsgPackScriptResponse = 5;
        public const byte GraphsonScriptResponse = 6;
    }
}
