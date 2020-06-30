using Narupa.Grpc.Multiplayer;
using NUnit.Framework;

namespace Narupa.Grpc.Tests.Multiplayer
{
    public class LocalMultiplayerResourceTests
    {
        private MultiplayerSession session;

        [SetUp]
        public void Setup()
        {
            session = new MultiplayerSession();
            session.UpdateRemoteValue("abc", "value");
        }

        [Test]
        public void MissingResource_NoValue()
        {
            var resource = session.GetSharedResource<string>("xyz");
            Assert.IsFalse(resource.HasValue);
        }
        
        [Test]
        public void PresentResource_HasValue()
        {
            var resource = session.GetSharedResource<string>("abc");
            Assert.IsTrue(resource.HasValue);
        }
        
        [Test]
        public void PresentResource_Value()
        {
            var resource = session.GetSharedResource<string>("abc");
            Assert.AreEqual(session.GetSharedState("abc"), resource.Value);
        }
        
        [Test]
        public void AddedResource_HasValue()
        {
            var resource = session.GetSharedResource<string>("xyz");
            session.UpdateRemoteValue("xyz", "value2");
            Assert.IsTrue(resource.HasValue);
        }
        
        [Test]
        public void AddedResource_Value()
        {
            var resource = session.GetSharedResource<string>("xyz");
            session.UpdateRemoteValue("xyz", "value2");
            Assert.AreEqual(session.GetSharedState("xyz"), resource.Value);
        }
        
        [Test]
        public void RemovedResource_HasValue()
        {
            var resource = session.GetSharedResource<string>("abc");
            session.RemoveRemoteValue("abc");
            Assert.IsFalse(resource.HasValue);
        }
        
        [Test]
        public void OverrideValue_Value()
        {
            var resource = session.GetSharedResource<string>("abc");
            resource.SetLocalValue("value2");
            Assert.AreEqual("value2", resource.Value);
        }
    }
}
