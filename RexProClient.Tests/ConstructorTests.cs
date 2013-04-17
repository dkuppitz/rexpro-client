namespace Rexster.Tests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class ConstructorTests
    {
        [TestMethod]
        public void ConstructorNoParams()
        {
            var client = new RexsterClient();
            Assert.AreEqual("localhost", client.Host);
            Assert.AreEqual(8184, client.Port);
        }

        [TestMethod]
        public void ConstructorWithHost()
        {
            const string host = "127.0.0.1";
            var client = new RexsterClient(host);
            Assert.AreEqual(host, client.Host);
            Assert.AreEqual(8184, client.Port);
        }

        [TestMethod]
        public void ConstructorWithHostAndPort()
        {
            const string host = "127.0.0.1";
            const int port = 8185;
            var client = new RexsterClient(host, port);
            Assert.AreEqual(host, client.Host);
            Assert.AreEqual(port, client.Port);
        }
    }
}