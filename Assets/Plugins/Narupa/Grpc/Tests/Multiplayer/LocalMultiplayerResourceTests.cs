using System;
using Narupa.Grpc.Multiplayer;
using NSubstitute;
using NUnit.Framework;

namespace Narupa.Grpc.Tests.Multiplayer
{
    public class LocalMultiplayerResourceTests
    {
        private SharedState sharedState;

        [SetUp]
        public void Setup()
        {
            sharedState = new SharedState();
        }

        private const string Key = "item.abc";

        public MultiplayerResource<string> GetResource()
        {
            return sharedState.GetSharedResource<string>(Key);
        }

        [Test]
        public void EmptySharedState_ResourceDoesNotHaveValue()
        {
            var resource = GetResource();
            Assert.IsFalse(resource.HasValue);
        }
        
        [Test]
        public void SharedStateHasItem_ResourceHasCorrectValue()
        {
            sharedState.SetRemoteValueAndSendChanges(Key, "value");
            var resource = GetResource();
            Assert.IsTrue(resource.HasValue);
            Assert.AreEqual("value", resource.Value);
        }

        [Test]
        public void SharedStateHasItem_ResourceMadeFirst_ResourceHasCorrectValue()
        {
            var resource = GetResource();
            sharedState.SetRemoteValueAndSendChanges(Key, "value");
            Assert.IsTrue(resource.HasValue);
            Assert.AreEqual("value", resource.Value);
        }
        
        [Test]
        public void SharedStateItemUpdated_ResourceHasCorrectValue()
        {
            sharedState.SetRemoteValueAndSendChanges(Key, "value");
            var resource = GetResource();
            sharedState.SetRemoteValueAndSendChanges(Key, "value2");
            Assert.IsTrue(resource.HasValue);
            Assert.AreEqual("value2", resource.Value);
        }
        
        [Test]
        public void SharedStateItemUpdated_ResourceMadeFirst_ResourceHasCorrectValue()
        {
            var resource = GetResource();
            sharedState.SetRemoteValueAndSendChanges(Key, "value");
            sharedState.SetRemoteValueAndSendChanges(Key, "value2");
            Assert.IsTrue(resource.HasValue);
            Assert.AreEqual("value2", resource.Value);
        }
        
        [Test]
        public void RemoteValueUpdated_ValueUpdatedInvoked()
        {
            sharedState.SetRemoteValueAndSendChanges(Key, "value");
            var resource = GetResource();

            var listener = Substitute.For<Action>();
            resource.ValueUpdated += listener;
            
            sharedState.SetRemoteValueAndSendChanges(Key, "value2");
            
            listener.Received(1).Invoke();
        }
        
        [Test]
        public void RemoteValueUpdated_ValueChangedInvoked()
        {
            sharedState.SetRemoteValueAndSendChanges(Key, "value");
            var resource = GetResource();

            var listener = Substitute.For<Action>();
            resource.ValueChanged += listener;
            
            sharedState.SetRemoteValueAndSendChanges(Key, "value2");
            
            listener.Received(1).Invoke();
        }
        
        [Test]
        public void RemoteValueUpdated_RemoteValueChangedInvoked()
        {
            sharedState.SetRemoteValueAndSendChanges(Key, "value");
            var resource = GetResource();

            var listener = Substitute.For<Action>();
            resource.RemoteValueChanged += listener;
            
            sharedState.SetRemoteValueAndSendChanges(Key, "value2");
            
            listener.Received(1).Invoke();
        }
        
        [Test]
        public void SharedStateItemRemoved_ResourceHasNoValue()
        {
            sharedState.SetRemoteValueAndSendChanges(Key, "value");
            var resource = GetResource();
            sharedState.RemoveRemoteValueAndSendChanges(Key);
            Assert.IsFalse(resource.HasValue);
        }
        
        [Test]
        public void RemoveLocalValue_RemoteDisagrees()
        {
            sharedState.SetRemoteValueAndSendChanges(Key, "value");
            var resource = GetResource();
            resource.Remove();
            Assert.IsFalse(resource.HasValue);
            Assert.IsTrue(sharedState.HasRemoteSharedStateValue(Key));
        }
        
        [Test]
        public void RemoveLocalValue_PushUpdate_RemoteAgrees()
        {
            sharedState.SetRemoteValueAndSendChanges(Key, "value");
            var resource = GetResource();
            resource.Remove();
            
            sharedState.ReplyToChanges();
            
            Assert.IsFalse(resource.HasValue);
            Assert.IsFalse(sharedState.HasRemoteSharedStateValue(Key));
        }
        
        [Test]
        public void RemoveLocalValue_Resource_ValueRemoved()
        {
            sharedState.SetRemoteValueAndSendChanges(Key, "value");
            var resource = GetResource();

            var listener = Substitute.For<Action>();
            resource.ValueRemoved += listener;
            
            resource.Remove();
            
            listener.Received(1).Invoke();
        }
        
        [Test]
        public void RemoveLocalValue_Resource_ValueChanged()
        {
            sharedState.SetRemoteValueAndSendChanges(Key, "value");
            var resource = GetResource();

            var listener = Substitute.For<Action>();
            resource.ValueChanged += listener;

            resource.Remove();
            
            listener.Received(1).Invoke();
        }
        
        [Test]
        public void RemoveLocalValue_Resource_RemoteValueChanged()
        {
            sharedState.SetRemoteValueAndSendChanges(Key, "value");
            var resource = GetResource();

            var listener = Substitute.For<Action>();
            resource.RemoteValueChanged += listener;
            
            resource.Remove();
            
            listener.Received(0).Invoke();
        }
        
        [Test]
        public void SetLocalValue_RemoteDisagrees()
        {
            sharedState.SetRemoteValueAndSendChanges(Key, "value");
            var resource = GetResource();
            resource.SetLocalValue("value2");
            Assert.AreEqual("value2", resource.Value);
            Assert.AreEqual("value", sharedState.GetRemoteSharedStateValue(Key));
        }
        
        [Test]
        public void SetLocalValue_PushUpdate_RemoteAgrees()
        {
            sharedState.SetRemoteValueAndSendChanges(Key, "value");
            var resource = GetResource();
            resource.SetLocalValue("value2");
            
            sharedState.ReplyToChanges();
            
            Assert.AreEqual("value2", resource.Value);
            Assert.AreEqual("value2", sharedState.GetRemoteSharedStateValue(Key));
        }
        
        [Test]
        public void SetLocalValue_Resource_ValueUpdated()
        {
            sharedState.SetRemoteValueAndSendChanges(Key, "value");
            var resource = GetResource();

            var listener = Substitute.For<Action>();
            resource.ValueUpdated += listener;
            
            resource.SetLocalValue("value2");
            
            listener.Received(1).Invoke();
        }
        
        [Test]
        public void SetLocalValue_Resource_ValueChanged()
        {
            sharedState.SetRemoteValueAndSendChanges(Key, "value");
            var resource = GetResource();

            var listener = Substitute.For<Action>();
            resource.ValueChanged += listener;
            
            resource.SetLocalValue("value2");
            
            listener.Received(1).Invoke();
        }
        
        [Test]
        public void SetLocalValue_Resource_RemoteValueChanged()
        {
            sharedState.SetRemoteValueAndSendChanges(Key, "value");
            var resource = GetResource();

            var listener = Substitute.For<Action>();
            resource.RemoteValueChanged += listener;
            
            resource.SetLocalValue("value2");
            
            listener.Received(0).Invoke();
        }
    }
}