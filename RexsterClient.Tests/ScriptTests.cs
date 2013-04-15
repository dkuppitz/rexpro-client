namespace Rexster.Tests
{
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class ScriptTests
    {
        private RexsterClient client;

        [TestInitialize]
        public void Initialize()
        {
            client = new RexsterClient("192.168.2.105");
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

        [TestMethod, ExpectedException(typeof(RexsterClientException))]
        public void QueryError()
        {
            client.Query("g.A()");
        }
    }
}