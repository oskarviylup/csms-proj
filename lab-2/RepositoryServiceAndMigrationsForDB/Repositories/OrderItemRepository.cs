using Npgsql;
using System.Collections.ObjectModel;
using Task3.Models;

namespace Task3.Repositories;

public class OrderItemRepository : IOrderItemRepository
{
    public async Task<long> CreateOrderItem(long orderId, long productId, int quantity, NpgsqlDataSource dataSource)
    {
        const string query = @"
            INSERT INTO order_items (order_id, product_id, order_item_quantity, order_item_deleted) 
            VALUES (@OrderId, @ProductId, @Quantity, @Deleted) 
            RETURNING order_item_id";

        await using NpgsqlConnection connection = await dataSource.OpenConnectionAsync();
        await using NpgsqlTransaction transaction = await connection.BeginTransactionAsync();

        await using var command = new NpgsqlCommand(query, connection, transaction);
        command.Parameters.AddWithValue("@OrderId", orderId);
        command.Parameters.AddWithValue("@ProductId", productId);
        command.Parameters.AddWithValue("@Quantity", quantity);
        command.Parameters.AddWithValue("@Deleted", false);

        long orderItemId = (long)(await command.ExecuteScalarAsync() ?? throw new InvalidOperationException());
        await transaction.CommitAsync();
        return orderItemId;
    }

    public async Task<bool> SoftDeleteOrderItem(long orderItemId, NpgsqlDataSource dataSource)
    {
        const string query = "UPDATE order_items SET order_item_deleted = @Deleted WHERE order_item_id = @OrderItemId";

        await using NpgsqlConnection connection = await dataSource.OpenConnectionAsync();
        await using NpgsqlTransaction transaction = await connection.BeginTransactionAsync();

        await using var command = new NpgsqlCommand(query, connection, transaction);
        command.Parameters.AddWithValue("@OrderItemId", orderItemId);
        command.Parameters.AddWithValue("@Deleted", true);

        int rowsAffected = (int)(await command.ExecuteScalarAsync() ?? throw new InvalidOperationException());
        await transaction.CommitAsync();
        return rowsAffected > 0;
    }

    public async Task<List<OrderItem>> SearchOrderItems(
        int pageNumber,
        int pageSize,
        NpgsqlDataSource dataSource,
        Collection<long>? orderIds = null,
        Collection<long>? productIds = null,
        bool? isDeleted = null)
    {
        const string query = """
                             
                                         SELECT order_item_id, order_id, product_id, order_item_quantity, order_item_deleted
                                         FROM order_items
                                         WHERE (cardinality(@OrderIds) = 0 OR order_id = ANY(@OrderIds))
                                           AND (cardinality(@ProductIds) = 0 OR product_id = ANY(@ProductIds))
                                           AND (@IsDeleted IS NULL OR order_item_deleted = @IsDeleted)
                                         ORDER BY order_item_id
                                         LIMIT @PageSize
                             """;

        var orderItems = new List<OrderItem>();

        await using NpgsqlConnection connection = await dataSource.OpenConnectionAsync();
        await using NpgsqlTransaction transaction = await connection.BeginTransactionAsync();

        await using var command = new NpgsqlCommand(query, connection, transaction);
        command.Parameters.AddWithValue("@OrderIds", orderIds ?? new Collection<long>());
        command.Parameters.AddWithValue("@ProductIds", productIds ?? new Collection<long>());
        command.Parameters.AddWithValue("@IsDeleted", isDeleted ?? throw new ArgumentNullException(nameof(isDeleted)));
        command.Parameters.AddWithValue("@Offset", (pageNumber - 1) * pageSize);
        command.Parameters.AddWithValue("@PageSize", pageSize);

        NpgsqlDataReader reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            orderItems.Add(new OrderItem
            {
                Id = reader.GetInt64(0),
                OrderId = reader.GetInt64(1),
                ProductId = reader.GetInt64(2),
                Quantity = reader.GetInt32(3),
                IsDeleted = reader.GetBoolean(4),
            });
        }

        await reader.CloseAsync();
        await transaction.CommitAsync();
        return orderItems;
    }
}