namespace Rexster.Tests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Rexster.Messages;
    using Rexster.Tests.Properties;

    [TestClass]
    public class SessionTests
    {
        private RexProClient client;

        [TestInitialize]
        public void Initialize()
        {
            client = new RexProClient(Settings.Default.RexProHost, Settings.Default.RexProPort);
        }

        [TestMethod]
        public void OpenCloseSession()
        {
            using (var session = client.OpenSession())
            {
                Assert.IsNotNull(session);
            }
        }

        [TestMethod]
        public void UseSession()
        {
            using (var session = client.OpenSession())
            {
                var request1 = new ScriptRequest("v = g.addVertex(['name':'foo'])");
                var expected = client.ExecuteScript<Vertex<TestVertex>>(request1, session, false).Result;
                var actual = client.Query<Vertex<TestVertex>>("v", session: session, isolate: false).Result;

                Assert.AreEqual(expected, actual);
            }
        }
    }
}