using API.Dtos.Order;
using API.Dtos.Products;
using API.Helpers;
using Core.Services;

namespace API.Services.Interfaces;

public interface IOrderService
{
    Task<ServiceResult<OrderDto>> CreateOrderAsync(OrderAddDto orderDto, int userId);
    Task<ServiceResult<OrderDto>> UpdateOrderAsync(int id, OrderUpdateDto orderDto);
    Task<ServiceResult<OrderDto>> GetOrderByIdAsync(int id);
    Task<ServiceResult<Pager<OrderDto>>> GetOrdersAsync(Params orderParams);
    Task<ServiceResult<Pager<OrderDto>>> GetOrdersByUserAsync(int userId, Params orderParams);
}
