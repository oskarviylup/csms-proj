using System.Collections.Concurrent;
using Task2.Interfaces;
using Task2.Records;

namespace Task2.Handler;

public class LibraryOperationHandler(ILibraryOperationService operationService) : ILibraryOperationHandler,
    IRequestClient
{
    private readonly ILibraryOperationService _operationService = operationService;
    private readonly ConcurrentDictionary<Guid, TaskCompletionSource<ResponseModel>?> _pendingOperations = new();

    public void HandleOperationResult(Guid requestId, byte[] data)
    {
        if (!_pendingOperations.TryRemove(requestId, out TaskCompletionSource<ResponseModel>? tcs)) return;
        var response = new ResponseModel(data);
        tcs?.TrySetResult(response);
    }

    public void HandleOperationError(Guid requestId, Exception exception)
    {
        if (!_pendingOperations.TryRemove(requestId, out TaskCompletionSource<ResponseModel>? tcs)) return;
        tcs?.TrySetException(exception);
    }

    public async Task<ResponseModel> SendAsync(RequestModel request, CancellationToken cancellationToken)
    {
        var requestId = Guid.NewGuid();
        var tcs = new TaskCompletionSource<ResponseModel>();

        _pendingOperations.TryAdd(requestId, tcs);

        await using (cancellationToken.Register(() => CancelOperation(requestId)))
        {
            try
            {
                _operationService.BeginOperation(requestId, request, cancellationToken);
                return await tcs.Task.ConfigureAwait(false);
            }
            finally
            {
                _pendingOperations.TryRemove(requestId, out _);
            }
        }
    }

    private void CancelOperation(Guid requestId)
    {
        if (_pendingOperations.TryGetValue(requestId, out TaskCompletionSource<ResponseModel>? tcs))
        {
            tcs?.TrySetCanceled();
        }
    }
}