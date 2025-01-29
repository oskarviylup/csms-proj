using Newtonsoft.Json.Linq;
using Npgsql;
using Task3.Models;
using Task3.Models.Payloads;

namespace Task3.Repositories;

public class OrderHistoryRepository : IOrderHistoryRepository
{
    public async Task<long> AddHistoryItem(long orderId, OrderHistoryItemKind kind, PayloadBase payload, NpgsqlDataSource dataSource)
    {
        var historyItem = new OrderHistoryItem
        {
            OrderId = orderId,
            CreatedAt = DateTime.UtcNow,
            Kind = kind,
            Payload = payload,
        };
        const string query = """
                             
                                         INSERT INTO order_history (order_id, order_history_item_created_at, order_history_item_kind, order_history_item_payload)
                                         VALUES (@OrderId, @CreatedAt, @Kind, @Payload)
                                         RETURNING order_history_item_id
                             """;

        await using NpgsqlConnection connection = await dataSource.OpenConnectionAsync();
        await using NpgsqlTransaction transaction = await connection.BeginTransactionAsync();

        await using var command = new NpgsqlCommand(query, connection, transaction);
        command.Parameters.AddWithValue("@OrderId", historyItem.OrderId);
        command.Parameters.AddWithValue("@CreatedAt", historyItem.CreatedAt);
        command.Parameters.AddWithValue("@Kind", historyItem.Kind);
        command.Parameters.AddWithValue("@Payload", historyItem.Payload);

        long historyItemId = (long)(await command.ExecuteScalarAsync() ?? throw new InvalidOperationException());
        await transaction.CommitAsync();
        return historyItemId;
    }

    public async Task<List<OrderHistoryItem>> SearchHistoryItems(
        int pageNumber,
        int pageSize,
        NpgsqlDataSource dataSource,
        long? orderId = null,
        OrderHistoryItemKind? kind = null)
    {
        const string query = """
                             
                             
                                         SELECT order_history_item_id, order_id, order_history_item_created_at, order_history_item_kind, order_history_item_payload
                                         FROM order_history
                                         WHERE (@OrderId IS NULL OR order_id = @OrderId)
                                           AND (@Kind IS NULL OR order_history_item_kind = @Kind::order_history_item_kind)
                                         ORDER BY order_history_item_id
                                         LIMIT @PageSize
                             """;

        var historyItems = new List<OrderHistoryItem>();

        await using NpgsqlConnection connection = await dataSource.OpenConnectionAsync();
        await using NpgsqlTransaction transaction = await connection.BeginTransactionAsync();

        await connection.OpenAsync();
        await using var command = new NpgsqlCommand(query, connection);
        command.Parameters.AddWithValue("@OrderId", orderId ?? throw new ArgumentNullException(nameof(orderId)));
        command.Parameters.AddWithValue("@Kind", kind ?? throw new ArgumentNullException(nameof(kind)));
        command.Parameters.AddWithValue("@Offset", (pageNumber - 1) * pageSize);
        command.Parameters.AddWithValue("@PageSize", pageSize);

        NpgsqlDataReader reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            historyItems.Add(new OrderHistoryItem
            {
                Id = reader.GetInt64(0),
                OrderId = reader.GetInt64(1),
                CreatedAt = reader.GetDateTime(2),
                Kind = Enum.Parse<OrderHistoryItemKind>(reader.GetString(3), ignoreCase: true),
                Payload = OrderHistoryItem.DeserializePayload(reader.GetString(4), Enum.Parse<OrderHistoryItemKind>(reader.GetString(3))) ?? throw new InvalidOperationException(),
            });
        }

        await reader.CloseAsync();
        await transaction.CommitAsync();
        return historyItems;
    }
}