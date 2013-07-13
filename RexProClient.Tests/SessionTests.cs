namespace Rexster.Tests
{
    using System.Collections.Generic;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Rexster.Messages;

    [TestClass]
    public class SessionTests
    {
        private RexProClient client;

        [TestInitialize]
        public void Initialize()
        {
            client = TestClientFactory.CreateClient();
            client.Query("g.V.remove();g.commit()");
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
                var expected = client.Query<int>("number = 1 + 2", session: session);
                var actual = client.Query<int>("number", session: session);

                Assert.AreEqual(expected, actual);
            }
        }

        [TestMethod]
        public void UseSessionWithGraph()
        {
            using (var session = client.StartSession())
            {
                var bindings = new { name = "foo" };
                var request = new ScriptRequest("v = g.addVertex(['name':name])", bindings);
                var expected = client.ExecuteScript<Vertex<TestVertex>>(request, session).Result;
                var actual = client.Query<Vertex<TestVertex>>("v", session: session);

                Assert.AreEqual(expected.Id, actual.Id);
                Assert.AreEqual(expected.Data.Name, actual.Data.Name);
            }
        }
    }
}