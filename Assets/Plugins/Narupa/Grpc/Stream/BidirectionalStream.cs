using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Grpc.Core;
using Narupa.Core.Async;
using Channel = System.Threading.Channels.Channel;

namespace Narupa.Grpc.Stream
{
    /// <summary>
    /// Delegate for a gRPC server streaming call
    /// </summary>
    public delegate AsyncDuplexStreamingCall<TRequest, TReply> BidirectionalStreamingCall<TRequest, TReply>(
        Metadata headers = null,
        DateTime? deadline = null,
        CancellationToken cancellationToken = default);
    
     /// <summary>
    /// Wraps the incoming response stream of a gRPC call and raises an event
    /// when new content is received.
    /// </summary>
    public sealed class BidirectionalStream<TOutgoing, TIncoming> : Cancellable, IAsyncClosable
    {
        /// <summary>
        /// Callback for when a new item is received from the stream.
        /// </summary>
        public event Action<TIncoming> MessageReceived;

        private AsyncDuplexStreamingCall<TOutgoing, TIncoming> streamingCall;
        
        private Task sendingTask;

        private Task recievingTask;

        private readonly Channel<TOutgoing> messageQueue;

        private BidirectionalStream(params CancellationToken[] externalTokens) : base(externalTokens)
        {
            messageQueue = Channel.CreateUnbounded<TOutgoing>(new UnboundedChannelOptions
            {
                SingleWriter = true,
                SingleReader = true
            });
        }

        /// <summary>
        /// Call a gRPC method with the provided <paramref name="request" />,
        /// and return a stream which has not been started yet.
        /// </summary>
        public static BidirectionalStream<TOutgoing, TIncoming> CreateStreamFromServerCall(
            BidirectionalStreamingCall<TOutgoing, TIncoming> grpcCall,
            params CancellationToken[] externalTokens)
        {
            var stream = new BidirectionalStream<TOutgoing, TIncoming>(externalTokens);

            stream.streamingCall = grpcCall(Metadata.Empty,
                                            null,
                                            stream.GetCancellationToken());

            return stream;
        }

        /// <summary>
        /// Start consuming the stream and raising events. Returns the
        /// iteration task.
        /// </summary>
        public Task StartStreams()
        {
            if (sendingTask != null || recievingTask != null)
                throw new InvalidOperationException("Streaming has already started.");

            if (IsCancelled)
                throw new InvalidOperationException("Stream has already been closed.");

            sendingTask = IncomingStream<TIncoming>.IncomingStreamTask(
                streamingCall.ResponseStream,
                OnMessageReceived,
                GetCancellationToken());
            
            recievingTask = OutgoingStream<TOutgoing, TIncoming>.StartSendingLoop(streamingCall.RequestStream,
                                                              messageQueue.Reader,
                                                              GetCancellationToken());

            return Task.WhenAll(sendingTask, recievingTask);
        }

        private void OnMessageReceived(TIncoming message)
        {
            MessageReceived?.Invoke(message);
        }
        
        /// <summary>
        /// Send a message asynchronously over the stream.
        /// </summary>
        public async Task QueueMessageAsync(TOutgoing message)
        {
            if (sendingTask == null)
                throw new InvalidOperationException("Stream has not started yet.");
            if (message == null)
                throw new ArgumentNullException(nameof(message), "Message cannot be null.");

            await messageQueue.Writer.WriteAsync(message);
        }

        /// <inheritdoc cref="IAsyncClosable.CloseAsync" />
        public async Task CloseAsync()
        {
            try
            {
                messageQueue.Writer.TryComplete();

                if (sendingTask != null)
                    await sendingTask;

                Cancel();
            }
            catch (TaskCanceledException)
            {
            }

            streamingCall.Dispose();
        }
    }
}