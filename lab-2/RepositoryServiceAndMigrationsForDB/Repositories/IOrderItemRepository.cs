using Npgsql;
using System.Collections.ObjectModel;
using Task3.Models;

namespace Task3.Repositories;

public interface IOrderItemRepository
{
    Task<long> CreateOrderItem(long orderId, long productId, int quantity, NpgsqlDataSource dataSource);

    Task<bool> SoftDeleteOrderItem(long orderItemId, NpgsqlDataSource dataSource);

    Task<List<OrderItem>> SearchOrderItems(
        int pageNumber,
        int pageSize,
        NpgsqlDataSource dataSource,
        Collection<long>? orderIds = null,
        Collection<long>? productIds = null,
        bool? isDeleted = null);
}