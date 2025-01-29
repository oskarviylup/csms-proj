using Microsoft.AspNetCore.Mvc;
using Task3.Models;
using Task3.Services;

namespace IntegratorIntoAspNet.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrdersController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    /// <summary>
    /// Creating new order.
    /// </summary>
    /// <param name="createdBy">Owner's name</param>
    /// <returns>Created order's ID</returns>
    [HttpPost]
    public async Task<ActionResult> CreateOrderAsync([FromQuery] string createdBy)
    {
        if (string.IsNullOrEmpty(createdBy))
            return BadRequest("Owner's name is required");
        long orderId = await _orderService.CreateOrder(createdBy);
        return Ok(orderId);
    }

    /// <summary>
    /// Adding new item to order.
    /// </summary>
    /// <param name="orderId">Order's ID</param>
    /// <param name="productId">Product's ID</param>
    /// <param name="quantity">Product quantity</param>
    /// <returns>Added Item ID</returns>
    [HttpPost("{orderId:long}/items")]
    public async Task<ActionResult> AddOrderItem(
        [FromRoute] long orderId,
        [FromQuery] long productId,
        [FromQuery] int quantity)
    {
        if (quantity <= 0)
            return BadRequest("Quantity must be greater than 0");

        long orderItemId = await _orderService.AddOrderItem(orderId, productId, quantity);
        return Ok(orderItemId);
    }

    /// <summary>
    /// Deleting item from order.
    /// </summary>
    /// <param name="orderItemId">Order Item ID</param>
    [HttpDelete("items/{orderItemId:long}")]
    public async Task<IActionResult> DeleteOrderItem([FromRoute] long orderItemId)
    {
        await _orderService.DeleteOrderItem(orderItemId);
        return NoContent();
    }

    /// <summary>
    /// Changing order's state.
    /// </summary>
    /// <param name="orderId">Order's ID</param>
    /// <param name="newState">New State</param>
    [HttpPut("{orderId:long}/state")]
    public async Task<ActionResult> UpdateOrderState([FromRoute] long orderId, [FromQuery] OrderState newState)
    {
        switch (newState)
        {
            case OrderState.Created:
                return BadRequest("You cannot change status to <created>");

            case OrderState.Processing:
                if (!await _orderService.CheckOrderState(orderId, OrderState.Created))
                    return BadRequest("You cannot change status to <processing>");
                await _orderService.OrderToWorkState(orderId);
                return NoContent();

            case OrderState.Completed:
                if (!await _orderService.CheckOrderState(orderId, OrderState.Processing))
                    return BadRequest("You cannot change status to <completed>");
                await _orderService.OrderToCompleteState(orderId);
                return NoContent();

            case OrderState.Cancelled:
                await _orderService.OrderToCancelledState(orderId);
                return NoContent();

            default:
                throw new ArgumentOutOfRangeException(nameof(newState), newState, null);
        }
    }

    /// <summary>
    /// Getting order's history.
    /// </summary>
    /// <param name="orderId">Order's ID</param>
    /// <param name="pageNumber">Page number</param>
    /// <param name="pageSize">Page size</param>
    /// <returns>List of order's history</returns>
    [HttpGet("{orderId:long}/history")]
    public async Task<ActionResult<List<OrderHistoryItem>>> SearchByHistoryItem(
        [FromRoute] long orderId,
        [FromQuery] int pageNumber,
        [FromQuery] int pageSize)
    {
        List<OrderHistoryItem> historyItems = await _orderService.SearchByHistoryItem(pageNumber, pageSize, orderId);
        return Ok(historyItems);
    }
}