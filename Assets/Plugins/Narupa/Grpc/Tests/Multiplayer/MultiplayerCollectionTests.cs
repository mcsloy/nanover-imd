using System;
using System.Runtime.Serialization;
using Narupa.Grpc.Multiplayer;
using NSubstitute;
using NUnit.Framework;

namespace Narupa.Grpc.Tests.Multiplayer
{
    [DataContract]
    [Serializable]
    internal class Item
    {
        protected bool Equals(Item other)
        {
            return Value == other.Value;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Item) obj);
        }

        public override int GetHashCode()
        {
            return (Value != null ? Value.GetHashCode() : 0);
        }

        [DataMember(Name = "value")]
        public string Value;

        public Item(string value)
        {
            Value = value;
        }
    }

    internal class MultiplayerCollectionTests
    {
        private SharedState sharedState;
        private MultiplayerCollection<Item> collection;

        [SetUp]
        public void Setup()
        {
            sharedState = new SharedState();

            collection = sharedState.GetSharedCollection<Item>("item.");
        }

        [Test]
        public void IsInitialCollectionEmpty()
        {
            Assert.AreEqual(0, collection.Count);
        }

        private void AddItemToRemoteAndSendChanges(string key, Item item)
        {
            sharedState.SetRemoteValueAndSendChanges(key,
                                                     Serialization.Serialization
                                                                  .ToDataStructure(item));
        }

        private void RemoveItemFromRemoteAndSendChanges(string key)
        {
            sharedState.RemoveRemoteValueAndSendChanges(key);
        }


        [Test]
        public void AddItemRemote_IsItemInCollection()
        {
            var item = new Item("value");
            AddItemToRemoteAndSendChanges("item.abc", item);

            Assert.AreEqual(1, collection.Count);
            Assert.AreEqual(item, collection["item.abc"]);
        }

        [Test]
        public void AddItemRemote_IsItemCreatedInvoked()
        {
            var itemCreated = Substitute.For<Action<string>>();
            collection.ItemCreated += itemCreated;

            var item = new Item("value");
            AddItemToRemoteAndSendChanges("item.abc", item);

            itemCreated.Received(1).Invoke("item.abc");
        }

        [Test]
        public void AddItemLocal_RemoteDisagrees()
        {
            var item = new Item("value");
            collection.Add("item.abc", item);

            Assert.IsTrue(collection.ContainsKey("item.abc"));
            Assert.IsFalse(sharedState.HasRemoteSharedStateValue("item.abc"));
        }

        [Test]
        public void AddItemLocal_SendChanges_RemoteAgrees()
        {
            var item = new Item("value");
            collection.Add("item.abc", item);

            sharedState.ReplyToChanges();

            Assert.IsTrue(collection.ContainsKey("item.abc"));
            Assert.IsTrue(sharedState.HasRemoteSharedStateValue("item.abc"));
        }

        [Test]
        public void AddItemLocal_IsItemInCollection()
        {
            var item = new Item("value");
            collection.Add("item.abc", item);

            Assert.AreEqual(1, collection.Count);
            Assert.AreEqual(item, collection["item.abc"]);
        }

        [Test]
        public void AddItemLocal_IsItemCreatedInvoked()
        {
            var itemCreated = Substitute.For<Action<string>>();
            collection.ItemCreated += itemCreated;

            var item = new Item("value");
            collection.Add("item.abc", item);

            itemCreated.Received(1).Invoke("item.abc");
        }

        [Test]
        public void AddItemLocal_PushChanges_IsItemCreatedInvokedOnce()
        {
            var itemCreated = Substitute.For<Action<string>>();
            collection.ItemCreated += itemCreated;

            var itemRemoved = Substitute.For<Action<string>>();
            collection.ItemUpdated += itemRemoved;

            var item = new Item("value");
            collection.Add("item.abc", item);

            sharedState.ReplyToChanges();

            itemCreated.Received(1).Invoke("item.abc");
            itemRemoved.Received(1).Invoke("item.abc");
        }

        [Test]
        public void RemoveItemRemote_IsItemNotInCollection()
        {
            var item = new Item("value");
            AddItemToRemoteAndSendChanges("item.abc", item);

            RemoveItemFromRemoteAndSendChanges("item.abc");

            Assert.AreEqual(0, collection.Count);
        }

        [Test]
        public void RemoveItemRemote_IsItemRemovedInvoked()
        {
            var itemRemoved = Substitute.For<Action<string>>();
            collection.ItemRemoved += itemRemoved;

            var item = new Item("value");
            AddItemToRemoteAndSendChanges("item.abc", item);

            RemoveItemFromRemoteAndSendChanges("item.abc");

            itemRemoved.Received(1).Invoke("item.abc");
        }

        [Test]
        public void RemoveItemLocal_RemoteDisagrees()
        {
            var item = new Item("value");
            AddItemToRemoteAndSendChanges("item.abc", item);

            collection.Remove("item.abc");

            Assert.IsFalse(collection.ContainsKey("item.abc"));
            Assert.IsTrue(sharedState.HasRemoteSharedStateValue("item.abc"));
        }

        [Test]
        public void RemoveItemLocal_SendChanges_RemoteAgrees()
        {
            var item = new Item("value");
            AddItemToRemoteAndSendChanges("item.abc", item);

            collection.Remove("item.abc");

            sharedState.ReplyToChanges();

            Assert.IsFalse(collection.ContainsKey("item.abc"));
            Assert.IsFalse(sharedState.HasRemoteSharedStateValue("item.abc"));
        }

        [Test]
        public void RemoveItemLocal_IsItemInCollection()
        {
            var item = new Item("value");
            AddItemToRemoteAndSendChanges("item.abc", item);

            collection.Remove("item.abc");

            Assert.AreEqual(0, collection.Count);
        }

        [Test]
        public void RemoveItemLocal_IsItemRemovedInvoked()
        {
            var item = new Item("value");
            AddItemToRemoteAndSendChanges("item.abc", item);

            var itemRemoved = Substitute.For<Action<string>>();
            collection.ItemRemoved += itemRemoved;

            collection.Remove("item.abc");

            itemRemoved.Received(1).Invoke("item.abc");
        }

        [Test]
        public void RemoveItemLocal_PushChanges_IsItemRemovedInvokedOnce()
        {
            var item = new Item("value");
            AddItemToRemoteAndSendChanges("item.abc", item);

            var itemRemoved = Substitute.For<Action<string>>();
            collection.ItemRemoved += itemRemoved;

            collection.Remove("item.abc");

            sharedState.ReplyToChanges();

            itemRemoved.Received(1).Invoke("item.abc");
        }
    }
}