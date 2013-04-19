namespace Rexster.Tests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Rexster.Messages;
    using Rexster.Tests.Properties;

    [TestClass]
    public class ResultCastTests
    {
        private RexProClient client;

        [TestInitialize]
        public void Initialize()
        {
            client = new RexProClient(Settings.Default.RexProHost, Settings.Default.RexProPort);
        }

        [TestMethod]
        public void ValueTypeCast()
        {
            var script = new ScriptRequest("1+2");
            int result = client.ExecuteScript<int>(script);
            Assert.AreEqual(3, result);
        }

        [TestMethod]
        public void VertexCast()
        {
            var script = new ScriptRequest("g = new TinkerGraph(); g.addVertex(['name':'foo'])");
            Vertex<TestVertex> result = client.ExecuteScript<Vertex<TestVertex>>(script);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Data);
            Assert.AreEqual("0", result.Id);
            Assert.AreEqual("foo", result.Data.Name);
        }

        [TestMethod]
        public void ObjectCast()
        {
            var script = new ScriptRequest("g = new TinkerGraph(); g.addVertex(['name':'foo']).map()");
            TestVertex result = client.ExecuteScript<TestVertex>(script);

            Assert.IsNotNull(result);
            Assert.AreEqual("foo", result.Name);
        }
    }
}