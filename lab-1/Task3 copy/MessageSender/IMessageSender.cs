using Task3.Records;

namespace Task3.MessageSender;

public interface IMessageSender
{
    ValueTask SendAsync(Message message, CancellationToken cancellationToken);
}