namespace Rexster.Tests
{
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Rexster.Messages;
    using Rexster.Tests.Properties;

    [TestClass]
    public class ScriptTests
    {
        private RexsterClient client;

        [TestInitialize]
        public void Initialize()
        {
            client = new RexsterClient(Settings.Default.RexsterHost, Settings.Default.RexsterPort);
        }

        private static string InitScript(string script)
        {
            return
                "g = new TinkerGraph();" +
                "g.addVertex(['name':'V1']);" +
                "g.addVertex(['name':'V2']);" +
                "g.addVertex(['name':'V3']);" +
                script;
        }

        [TestMethod]
        public void QueryScalarValue()
        {
            var script = InitScript("g.V.count()");
            var count = client.Query<int>(script).Result;

            Assert.AreEqual(3, count);
        }

        [TestMethod]
        public void QuerySingleVertex()
        {
            var script = InitScript("g.V.next()");
            var vertex = client.Query<Vertex<TestVertex>>(script).Result;

            Assert.IsNotNull(vertex);
        }

        [TestMethod]
        public void QuerySingleMap()
        {
            var script = InitScript("g.V.next().map()");
            var item = client.Query<TestVertex>(script).Result;

            Assert.IsNotNull(item);
        }

        [TestMethod]
        public void QueryMultipleVertices()
        {
            var script = InitScript("g.V");
            var vertices = client.Query<Vertex<TestVertex>[]>(script).Result;

            Assert.IsNotNull(vertices);
            Assert.AreEqual(3, vertices.Length);
        }

        [TestMethod]
        public void QueryMultipleMaps()
        {
            var script = InitScript("g.V.map()");
            var items = client.Query<TestVertex[]>(script).Result;

            Assert.IsNotNull(items);
            Assert.AreEqual(3, items.Length);
            Assert.IsTrue(items.Any(item => item.Name == "V1"));
            Assert.IsTrue(items.Any(item => item.Name == "V2"));
            Assert.IsTrue(items.Any(item => item.Name == "V3"));
        }

        [TestMethod]
        public void QueryEdge()
        {
            var script = InitScript("g.addEdge(null,g.v(0),g.v(1),'knows')");
            var edge = client.Query<Edge>(script).Result;

            Assert.IsNotNull(edge);
            Assert.AreEqual("0", edge.OutVertex);
            Assert.AreEqual("1", edge.InVertex);
            Assert.AreEqual("knows", edge.Label);
        }

        [TestMethod, ExpectedException(typeof(RexsterClientException))]
        public void QueryError()
        {
            client.Query("g.A()");
        }

        [TestMethod]
        public void QueryNoReturn()
        {
            var script = InitScript("null");
            var res = client.Query(script);

            Assert.IsNotNull(res);
        }

        [TestMethod]
        public void ExecuteScriptRequest()
        {
            var script = new ScriptRequest(InitScript("g.V.count()"));
            var count = client.ExecuteScript<long>(script).Result;

            Assert.AreEqual(3, count);
        }
    }
}