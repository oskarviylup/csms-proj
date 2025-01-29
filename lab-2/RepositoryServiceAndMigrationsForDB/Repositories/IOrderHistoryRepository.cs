using Npgsql;
using Task3.Models;
using Task3.Models.Payloads;

namespace Task3.Repositories;

public interface IOrderHistoryRepository
{
    Task<long> AddHistoryItem(long orderId, OrderHistoryItemKind kind, PayloadBase payload, NpgsqlDataSource dataSource);

    Task<List<OrderHistoryItem>> SearchHistoryItems(
        int pageNumber,
        int pageSize,
        NpgsqlDataSource dataSource,
        long? orderId = null,
        OrderHistoryItemKind? kind = null);
}