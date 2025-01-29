using Npgsql;
using System.Collections.ObjectModel;
using Task3.Models;

namespace Task3.Repositories;

public interface IOrderRepository
{
    Task<long> CreateOrder(string createdBy, NpgsqlDataSource dataSource);

    Task<bool> UpdateOrderStatus(long orderId, OrderState newState, NpgsqlDataSource dataSource);

    Task<List<Order>> SearchOrders(
        int pageNumber,
        int pageSize,
        NpgsqlDataSource dataSource,
        Collection<long>? orderIds = null,
        OrderState? state = null,
        string? createdBy = null);
}