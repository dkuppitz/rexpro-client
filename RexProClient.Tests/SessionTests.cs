namespace Rexster.Tests
{
    using System.Collections.Generic;
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
            using (var session = client.StartSession())
            {
                Assert.IsNotNull(session);
            }
        }

        [TestMethod]
        public void UseSessionWithoutGraph()
        {
            using (var session = client.StartSession())
            {
                int expected = client.Query<int>("number = 1 + 2", session: session, isolate: false);
                int actual = client.Query<int>("number", session: session, isolate: false);

                Assert.AreEqual(expected, actual);
            }
        }

        [TestMethod]
        public void UseSessionWithGraph()
        {
            using (var session = client.StartSession())
            {
                var bindings = new Dictionary<string, object> { { "name", "foo" } };
                var request = new ScriptRequest("v = g.addVertex(['name':name])", bindings);
                var expected = client.ExecuteScript<Vertex<TestVertex>>(request, session, false).Result;
                var actual = client.Query<Vertex<TestVertex>>("v", session: session, isolate: false).Result;

                Assert.AreEqual(expected.Id, actual.Id);
                Assert.AreEqual(expected.Data.Name, actual.Data.Name);
            }
        }
    }
}