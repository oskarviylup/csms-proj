using Itmo.Dev.Platform.Common.Extensions;
using System.Threading.Channels;
using Task3.Handler;
using Task3.MessageSender;
using Task3.Records;

namespace Task3.Processor;

public class MessageProcessor : IMessageProcessor, IMessageSender
{
    private readonly Channel<Message> _channel;
    private readonly int _batchSize;
    private readonly TimeSpan _batchTimeout;
    private readonly IMessageHandler _messageHandler;

    public MessageProcessor(int channelCapacity, IMessageHandler messageHandler, int batchSize, TimeSpan batchTimeout)
    {
        var options = new BoundedChannelOptions(channelCapacity)
        {
            Capacity = channelCapacity,
            FullMode = BoundedChannelFullMode.DropOldest,
        };
        _channel = Channel.CreateBounded<Message>(options);
        _messageHandler = messageHandler;
        _batchSize = batchSize;
        _batchTimeout = batchTimeout;
    }

    public async Task ProcessAsync(CancellationToken cancellationToken)
    {
        IAsyncEnumerable<Message> messageList = _channel.Reader.ReadAllAsync(cancellationToken);

        await foreach (IReadOnlyList<Message> chunk in AsyncEnumerableExtensions.ChunkAsync(messageList, _batchSize, _batchTimeout))
        {
            await _messageHandler.HandleAsync(chunk, cancellationToken).ConfigureAwait(false);
        }
    }

    public void Complete()
    {
        _channel.Writer.Complete();
    }

    public async ValueTask SendAsync(Message message, CancellationToken cancellationToken)
    {
        await _channel.Writer.WriteAsync(message, cancellationToken).ConfigureAwait(false);
    }
}