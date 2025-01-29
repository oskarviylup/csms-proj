using Npgsql;
using System.Collections.ObjectModel;
using Task3.Models;
using InvalidOperationException = System.InvalidOperationException;

namespace Task3.Repositories;

public class OrderRepository : IOrderRepository
{
    public async Task<long> CreateOrder(string createdBy, NpgsqlDataSource dataSource)
    {
        const string creatingQuery = @"
            INSERT INTO orders (order_state, order_created_at, order_created_by) 
            VALUES (@State, @CreatedAt, @CreatedBy) 
            RETURNING order_id";

        await using NpgsqlConnection connection = await dataSource.OpenConnectionAsync();
        await using NpgsqlTransaction transaction = await connection.BeginTransactionAsync();
        using var command = new NpgsqlCommand(creatingQuery, connection, transaction);
        command.Parameters.AddWithValue("@State", OrderState.Created);
        command.Parameters.AddWithValue("@CreatedAt", DateTime.UtcNow);
        command.Parameters.AddWithValue("@CreatedBy", createdBy);

        long orderId = (long)(await command.ExecuteScalarAsync() ?? throw new InvalidOperationException());
        await transaction.CommitAsync();
        return orderId;
    }

    public async Task<bool> UpdateOrderStatus(long orderId, OrderState newState, NpgsqlDataSource dataSource)
    {
        const string query = "UPDATE orders SET order_state = @NewState WHERE order_id = @OrderId";

        await using NpgsqlConnection connection = await dataSource.OpenConnectionAsync();
        await using NpgsqlTransaction transaction = await connection.BeginTransactionAsync();

        await using var command = new NpgsqlCommand(query, connection, transaction);
        command.Parameters.AddWithValue("@OrderId", orderId);
        command.Parameters.AddWithValue("@NewState", newState);

        int rowsAffected = (int)(await command.ExecuteScalarAsync() ?? throw new InvalidOperationException());
        await transaction.CommitAsync();
        return rowsAffected > 0;
    }

    public async Task<List<Order>> SearchOrders(
        int pageNumber,
        int pageSize,
        NpgsqlDataSource dataSource,
        Collection<long>? orderIds = null,
        OrderState? state = null,
        string? createdBy = null)
    {
        const string query = @"
            SELECT order_id, order_state, order_created_at, order_created_by
            FROM orders
            WHERE (cardinality(@OrderIds) = 0 OR order_id = ANY(@OrderIds))
              AND (@Status IS NULL OR order_state = @Status::order_state)
              AND (@CreatedBy IS NULL OR order_created_by = @CreatedBy)
            ORDER BY order_id
            LIMIT @PageSize";

        var orders = new List<Order>();

        await using NpgsqlConnection connection = await dataSource.OpenConnectionAsync();
        await using NpgsqlTransaction transaction = await connection.BeginTransactionAsync();

        await using var command = new NpgsqlCommand(query, connection, transaction);
        command.Parameters.AddWithValue("@OrderIds", orderIds ?? new Collection<long>());
        command.Parameters.AddWithValue("@Status", state ?? throw new ArgumentNullException(nameof(state)));
        command.Parameters.AddWithValue("@CreatedBy", createdBy ?? throw new ArgumentNullException(nameof(createdBy)));
        command.Parameters.AddWithValue("@Offset", (pageNumber - 1) * pageSize);
        command.Parameters.AddWithValue("@PageSize", pageSize);

        await using NpgsqlDataReader reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            orders.Add(new Order()
            {
                Id = reader.GetInt64(0),
                State = Enum.Parse<OrderState>(reader.GetString(1), ignoreCase: true),
                CreatedAt = reader.GetDateTime(2),
                CreatedBy = reader.GetString(3),
            });
        }

        await reader.CloseAsync();
        await transaction.CommitAsync();
        return orders;
    }
}