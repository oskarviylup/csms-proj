using Task3.Models;

namespace Task3.Services;

public interface IOrderService
{
    Task<long> CreateOrder(string createdBy);

    Task<long> AddOrderItem(long orderId, long productId, int quantity);

    Task DeleteOrderItem(long orderItemId);

    Task<bool> CheckOrderState(long orderId, OrderState orderState);

    Task OrderToWorkState(long orderId, OrderState newState = OrderState.Processing);

    Task OrderToCompleteState(long orderId, OrderState newState = OrderState.Completed);

    Task OrderToCancelledState(long orderId, OrderState newState = OrderState.Cancelled);

    Task<List<OrderHistoryItem>> SearchByHistoryItem(int pageNumber, int pageSize, long orderId);
}