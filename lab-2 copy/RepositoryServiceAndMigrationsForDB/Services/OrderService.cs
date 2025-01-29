using Npgsql;
using System.Collections.ObjectModel;
using Task3.Models;
using Task3.Models.Payloads;
using Task3.Repositories;

namespace Task3.Services;

public class OrderService(NpgsqlDataSource dataSource) : IOrderService
{
    private readonly OrderRepository _orderRepository = new();
    private readonly OrderHistoryRepository _orderHistoryRepository = new();
    private readonly OrderItemRepository _orderItemRepository = new();

    public async Task<bool> CheckOrderState(long orderId, OrderState orderState)
    {
        var preList = new Collection<long> { orderId };
        List<Order> orderList = await _orderRepository.SearchOrders(1, 1, dataSource, preList);
        return orderList[0].State == orderState;
    }

    public async Task<long> CreateOrder(string createdBy)
    {
        long orderId = await _orderRepository.CreateOrder(createdBy, dataSource);
        var historyPayload = new OrderCreatedPayload
        {
            CreatedAt = DateTime.UtcNow,
        };
        await _orderHistoryRepository.AddHistoryItem(
            orderId,
            OrderHistoryItemKind.Created,
            historyPayload,
            dataSource);

        return orderId;
    }

    public async Task<long> AddOrderItem(long orderId, long productId, int quantity)
    {
        if (!await CheckOrderState(orderId, OrderState.Created)) return 1;
        long orderItemId = await _orderItemRepository.CreateOrderItem(orderId, productId, quantity, dataSource);
        var historyPayload = new ItemAddedPayload()
        {
            ProductId = productId,
            Quantity = quantity,
        };
        await _orderHistoryRepository.AddHistoryItem(
            orderId,
            OrderHistoryItemKind.ItemAdded,
            historyPayload,
            dataSource).ConfigureAwait(false);
        return orderItemId;
    }

    public async Task DeleteOrderItem(long orderItemId)
    {
        var preList = new Collection<long> { orderItemId };
        List<OrderItem> list = await _orderItemRepository.SearchOrderItems(1, 1, dataSource, preList);
        if (await CheckOrderState(list[0].OrderId, OrderState.Created))
        {
            await _orderItemRepository.SoftDeleteOrderItem(orderItemId, dataSource);
            var historyPayload = new ItemRemovedPayload()
            {
                ProductId = list[0].ProductId,
            };
            await _orderHistoryRepository.AddHistoryItem(
                list[0].OrderId,
                OrderHistoryItemKind.ItemRemoved,
                historyPayload,
                dataSource);
        }
    }

    public async Task OrderToWorkState(long orderId, OrderState newState = OrderState.Processing)
    {
        var preList = new Collection<long> { orderId };
        List<Order> orderList = await _orderRepository.SearchOrders(1, 1, dataSource, preList);

        if (orderList[0].State == OrderState.Created)
        {
            await _orderRepository.UpdateOrderStatus(orderId, newState, dataSource);
            var historyPayload = new StateChangedPayload()
            {
                OldState = OrderState.Created,
                NewState = OrderState.Processing,
            };
            await _orderHistoryRepository.AddHistoryItem(
                orderId,
                OrderHistoryItemKind.StateChanged,
                historyPayload,
                dataSource);
            Console.WriteLine("Order state changed from {0} to {1}", orderList[0].State, newState);
        }

        Console.WriteLine("Operation cancelled. You can not change state {0} to {1}", orderList[0].State, newState);
    }

    public async Task OrderToCompleteState(long orderId, OrderState newState = OrderState.Completed)
    {
        var preList = new Collection<long> { orderId };
        List<Order> orderList = await _orderRepository.SearchOrders(1, 1, dataSource, preList);

        if (orderList[0].State == OrderState.Processing)
        {
            await _orderRepository.UpdateOrderStatus(orderId, newState, dataSource);
            var historyPayload = new StateChangedPayload()
            {
                OldState = OrderState.Processing,
                NewState = OrderState.Completed,
            };
            await _orderHistoryRepository.AddHistoryItem(
                orderId,
                OrderHistoryItemKind.ItemRemoved,
                historyPayload,
                dataSource);
            Console.WriteLine("Order state changed from {0} to {1}", orderList[0].State, newState);
        }

        Console.WriteLine("Operation cancelled. You can not change state {0} to {1}", orderList[0].State, newState);
    }

    public async Task OrderToCancelledState(long orderId, OrderState newState = OrderState.Cancelled)
    {
        var preList = new Collection<long> { orderId };
        List<Order> orderList = await _orderRepository.SearchOrders(1, 1, dataSource, preList);

        await _orderRepository.UpdateOrderStatus(orderId, newState, dataSource);
        var historyPayload = new StateChangedPayload()
        {
            OldState = orderList[0].State,
            NewState = OrderState.Cancelled,
        };
        await _orderHistoryRepository.AddHistoryItem(
            orderId,
            OrderHistoryItemKind.ItemRemoved,
            historyPayload,
            dataSource);
    }

    public async Task<List<OrderHistoryItem>> SearchByHistoryItem(int pageNumber, int pageSize, long orderId)
    {
        List<OrderHistoryItem> historyItemsList = await _orderHistoryRepository.SearchHistoryItems(pageNumber, pageSize, dataSource, orderId);
        return historyItemsList;
    }
}