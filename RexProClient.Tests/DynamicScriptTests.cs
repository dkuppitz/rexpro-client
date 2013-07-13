namespace Rexster.Tests
{
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Rexster.Tests.Properties;

    [TestClass]
    public class DynamicScriptTests
    {
        private RexProClient client;

        [TestInitialize]
        public void Initialize()
        {
            client = TestClientFactory.CreateClient();
            client.Query("g.V.remove();g.commit()");
        }

        [TestCleanup]
        public void Cleanup()
        {
            client.Query("g.V.remove();g.commit()");
        }

        private static string InitScript(string script)
        {
            return
                "g.addVertex(0, ['name':'V1']);" +
                "g.addVertex(1, ['name':'V2']);" +
                "g.addVertex(2, ['name':'V3']);" +
                script;
        }

        [TestMethod]
        public void QueryScalarValue()
        {
            var script = InitScript("g.V.count()");
            var count = client.Query(script);

            Assert.AreEqual(3, count);
        }

        [TestMethod]
        public void QuerySingleVertex()
        {
            var script = InitScript("g.V.next()");
            var vertex = client.Query(script);

            Assert.IsNotNull(vertex);
            Assert.IsNotNull(vertex._id);
            Assert.IsNotNull(vertex._properties);
            Assert.IsNotNull(vertex._properties.name);
        }

        [TestMethod]
        public void QuerySingleMap()
        {
            var script = InitScript("g.V.next().map()");
            var item = client.Query(script);

            Assert.IsNotNull(item);
            Assert.IsNotNull(item.name);
        }

        [TestMethod]
        public void QueryMultipleVertices()
        {
            var script = InitScript("g.V");
            var vertices = client.Query<dynamic[]>(script);

            Assert.IsNotNull(vertices);
            Assert.AreEqual(3, vertices.Length);
            Assert.IsTrue(vertices.Any(vertex => vertex._properties.name == "V1"));
            Assert.IsTrue(vertices.Any(vertex => vertex._properties.name == "V2"));
            Assert.IsTrue(vertices.Any(vertex => vertex._properties.name == "V3"));
        }

        [TestMethod]
        public void QueryMultipleMaps()
        {
            var script = InitScript("g.V.map()");
            var items = client.Query<dynamic[]>(script);

            Assert.IsNotNull(items);
            Assert.AreEqual(3, items.Length);
            Assert.IsTrue(items.Any(item => item.name == "V1"));
            Assert.IsTrue(items.Any(item => item.name == "V2"));
            Assert.IsTrue(items.Any(item => item.name == "V3"));
        }

        [TestMethod]
        public void QueryEdge()
        {
            var script = InitScript("e=g.addEdge(null,g.v(0),g.v(1),'knows')");
            var edge = client.Query(script);

            Assert.IsNotNull(edge);
            Assert.AreEqual(0, (int)edge._outV);
            Assert.AreEqual(1, (int)edge._inV);
            Assert.AreEqual("knows", (string)edge._label);
        }

        [TestMethod]
        public void DynamicLinq()
        {
            var vertices = client.Query<dynamic[]>(InitScript("g.V"));
            var idQuery =
                from vertex in vertices
                select vertex.Id;

            var list = string.Join(",", idQuery);

            Assert.IsFalse(string.IsNullOrEmpty(list));
        }

        [TestMethod]
        public void DynamicLinqEmpty()
        {
            var vertices = client.Query<dynamic[]>("g.V");
            var idQuery =
                from vertex in vertices
                select vertex.Id;

            var list = string.Join(",", idQuery);

            Assert.IsTrue(string.IsNullOrEmpty(list));
        }

        [TestMethod]
        public void QueryPath()
        {
            const string script = "g.addEdge(null,g.v(0),g.v(1),'knows');" +
                                  "g.addEdge(null,g.v(0),g.v(2),'knows');" +
                                  "g.addEdge(null,g.v(1),g.v(2),'knows');" +
                                  "g.v(0).out().loop(1){true}{true}.path()";

            var paths = client.Query<IEnumerable<dynamic>>(InitScript(script));
            var pathLengths =
                (from path in paths
                 select new
                 {
                     Path = path,
                     path.Count
                 }).ToArray();

            Assert.IsNotNull(paths);
            Assert.AreEqual(3, pathLengths.Length);
            Assert.AreEqual(2, pathLengths.Min(p => p.Count));
            Assert.AreEqual(3, pathLengths.Max(p => p.Count));
            Assert.AreEqual(2, pathLengths.Count(p => p.Count == 2));
            Assert.AreEqual(1, pathLengths.Count(p => p.Count == 3));
        }

        [TestMethod]
        public void UseVertexAsBinding()
        {
            using (var session = client.StartSession())
            {
                var v1 = client.Query("g.addVertex()", session: session);
                var v2 = client.Query("g.addVertex()", session: session);
                var bindings = new Dictionary<string, object>
                {
                    { "v1", v1 },
                    { "v2", v2 },
                    { "label", "knows" }
                };
                client.Query("g.addEdge(g.v(v1), g.v(v2), label)", bindings, session);
                bindings.Remove("v2");
                var v3 = client.Query("g.v(v1).out(label).next()", bindings, session);
                client.Query("g.rollback()");

                Assert.AreEqual(v2._id, v3._id);
            }
        }

        [TestMethod]
        public void LongText()
        {
            var escapedText = Resources.LongText
                                       .Replace("\r", "\\r")
                                       .Replace("\n", "\\n");
            var script = string.Format("g.addVertex(['text':'''{0}''']).map()", escapedText);
            var vertex = client.Query(script);

            Assert.IsNotNull(vertex);
            Assert.AreEqual(Resources.LongText, (string)vertex.text);
        }

        [TestMethod]
        public void LongTextParams()
        {
            const string script = "g.addVertex(['text':text]).map()";

            var parameters = new Dictionary<string, object>
            {
                {"text", Resources.LongText}
            };

            var vertex = client.Query(script, parameters);

            Assert.IsNotNull(vertex);
            Assert.AreEqual(Resources.LongText, (string)vertex.text);
        }
    }
}