namespace Rexster.Tests
{
    using Rexster.Messages;
    using Rexster.Tests.Properties;

    static class TestClientFactory
    {
        private static readonly object Lock = new object();
        private static volatile RexProClient _client;

        public static RexProClient CreateClient()
        {
            if (_client == null)
            {
                lock (Lock)
                {
                    if (_client == null)
                    {
                        var settings = GraphSettings.Default;
                        settings.GraphName = Settings.Default.GraphName;
                        _client = new RexProClient(Settings.Default.RexProHost, Settings.Default.RexProPort, settings);
                    }
                }
            }

            return _client;
        }
    }
}
