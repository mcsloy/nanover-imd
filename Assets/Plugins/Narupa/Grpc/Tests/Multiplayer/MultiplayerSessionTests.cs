using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Narupa.Grpc.Multiplayer;
using Narupa.Testing.Async;
using NSubstitute;
using NSubstitute.Proxies.CastleDynamicProxy;
using NUnit.Framework;
using UnityEngine;

namespace Narupa.Grpc.Tests.Multiplayer
{
    internal class MultiplayerSessionTests
    {
        private MultiplayerService service;
        private GrpcServer server;
        private MultiplayerSession session;
        private GrpcConnection connection;

        [SetUp]
        public void AsyncSetup()
        {
            AsyncUnitTests.RunAsyncSetUp(this);
        }

        [AsyncSetUp]
        public async Task Setup()
        {
            service = new MultiplayerService();
            server = new GrpcServer(service);

            session = new MultiplayerSession();

            connection = new GrpcConnection("localhost", server.Port);
            session.OpenClient(connection);

            await Task.Delay(500);
        }

        [TearDown]
        public void AsyncTearDown()
        {
            AsyncUnitTests.RunAsyncTearDown(this);
        }

        [AsyncTearDown]
        public async Task TearDown()
        {
            session?.CloseClient();
            if (connection != null)
                await connection.CloseAsync();
            if (server != null)
                await server.CloseAsync();
        }

        private static IEnumerable<AsyncUnitTests.AsyncTestInfo> GetTests()
        {
            return AsyncUnitTests.FindAsyncTestsInClass<MultiplayerSessionTests>();
        }

        [Test]
        public void TestAsync([ValueSource(nameof(GetTests))] AsyncUnitTests.AsyncTestInfo test)
        {
            AsyncUnitTests.RunAsyncTest(this, test);
        }

        [AsyncTest]
        public async Task ValueChanged_ClientGetsUpdate()
        {
            service.SetValueDirect("abc", 1.2);

            void HasReceivedKey() => CollectionAssert.Contains(session.RemoteSharedStateDictionary.Keys,
                                                               "abc");

            await AsyncAssert.PassesWithinTimeout(HasReceivedKey);
        }

        [AsyncTest]
        public async Task ValueChanged_ClientCallback()
        {
            var callback = Substitute.For<Action<string, object>>();
            session.SharedStateRemoteKeyUpdated += callback;

            void HasReceivedCallback() =>
                callback.Received(1).Invoke(Arg.Is("abc"), Arg.Any<object>());

            service.SetValueDirect("abc", 1.2);

            await AsyncAssert.PassesWithinTimeout(HasReceivedCallback);
        }

        [AsyncTest]
        public async Task ValueChanged_MultiplayerResource()
        {
            var value = session.GetSharedResource<double>("abc");
            service.SetValueDirect("abc", 1.2);

            void HasReceivedValue() => Assert.AreEqual(1.2, value.Value);

            await AsyncAssert.PassesWithinTimeout(HasReceivedValue);
        }

        [AsyncTest]
        public async Task ValueChanged_MultiplayerResourceCallback()
        {
            var callback = Substitute.For<Action>();
            var value = session.GetSharedResource<double>("abc");
            value.ValueChanged += callback;

            service.SetValueDirect("abc", 1.2);

            void HasReceivedCallback() => callback.Received(1).Invoke();

            await AsyncAssert.PassesWithinTimeout(HasReceivedCallback);
        }

        [AsyncTest]
        public async Task TryLock_Success()
        {
            server.ReplyLatency = 400;
            var value = session.GetSharedResource<double>("abc");
            value.ObtainLock();
            Assert.AreEqual(MultiplayerResourceLockState.Pending, value.LockState);

            void LockSuccessful()
            {
                Assert.AreEqual(MultiplayerResourceLockState.Locked, value.LockState);
                Assert.IsTrue(service.Locks.TryGetValue("abc", out var v)
                           && v.Equals(session.AccessToken));
            }

            await AsyncAssert.PassesWithinTimeout(LockSuccessful, timeout: 2000);
        }
        
        [Test]
        public void GetSharedResource_CalledTwice_ReturnSameObject()
        {
            var resource1 = session.GetSharedResource<string>("abc");
            var resource2 = session.GetSharedResource<string>("abc");
            Assert.AreSame(resource1, resource2);
        }
        
        /// <summary>
        /// Utility method to call a function in a separate scope, to ensure variables are
        /// disposed of correctly.
        /// </summary>
        private T EvaluateInLocalScope<T>(Func<T> func)
        {
            return func();
        }
        
        [Test]
        public void GetSharedResource_ObjectRegeneratedWhenNotReferenced()
        {
            var hash1 = EvaluateInLocalScope(() =>
            {
                var resource = session.GetSharedResource<string>("abc");
                var hash = resource.GetHashCode();

                return hash;
            });
            
            GC.Collect();
            GC.WaitForPendingFinalizers();

            var resource2 = session.GetSharedResource<string>("abc");
            var hash2 = resource2.GetHashCode();
            
            Assert.AreNotEqual(hash1, hash2);
        }
    }
}