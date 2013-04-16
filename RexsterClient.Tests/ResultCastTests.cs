namespace Rexster.Tests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class ResultCastTests
    {
        private RexsterClient client;

        [TestInitialize]
        public void Initialize()
        {
            client = new RexsterClient("192.168.2.105");
        }

        [TestMethod]
        public void ValueTypeCast()
        {
            var result = (int)client.Query<int>("1+2");
            Assert.AreEqual(3, result);
        }

        [TestMethod]
        public void VertexCast()
        {
            var result = (Vertex<TestVertex>)client.Query<Vertex<TestVertex>>("g = new TinkerGraph(); g.addVertex(['name':'foo'])");

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Data);
            Assert.AreEqual("0", result.Id);
            Assert.AreEqual("foo", result.Data.Name);
        }

        [TestMethod]
        public void ObjectCast()
        {
            var result = (TestVertex)client.Query<TestVertex>("g = new TinkerGraph(); g.addVertex(['name':'foo']).map()");

            Assert.IsNotNull(result);
            Assert.AreEqual("foo", result.Name);
        }
    }
}