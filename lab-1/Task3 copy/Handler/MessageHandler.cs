using Task3.Records;

namespace Task3.Handler;

public class MessageHandler : IMessageHandler
{
    public async ValueTask HandleAsync(IEnumerable<Message> messages, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        IEnumerable<string> batchedMessages = messages.Select(x => $"[{x.Title}] {x.Text}");
        string result = string.Join(Environment.NewLine, batchedMessages);

        await Task.Run(
            () =>
            {
                Console.WriteLine(result);
            },
            cancellationToken).ConfigureAwait(false);
    }
}