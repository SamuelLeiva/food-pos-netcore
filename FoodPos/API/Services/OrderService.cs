using API.Dtos.Order;
using API.Dtos.Products;
using API.Helpers;
using API.Services.Interfaces;
using AutoMapper;
using Core.Entities;
using Core.Interfaces;
using Core.Services;

namespace API.Services;

public class OrderService : IOrderService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public OrderService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ServiceResult<OrderDto>> CreateOrderAsync(OrderAddUpdateDto orderDto)
    {
        try
        {
            var order = _mapper.Map<Order>(orderDto);
            _unitOfWork.Orders.Add(order);
            await _unitOfWork.SaveAsync();

            await _unitOfWork.Orders.GetByIdAsync(order.Id);

            var createdOrderDto = _mapper.Map<OrderDto>(order);
            return ServiceResult<OrderDto>.Success(createdOrderDto);
        }
        catch (Exception ex)
        {
            return ServiceResult<OrderDto>.Failure($"An unexpected error occurred while creating the order: {ex.Message}");
        }
    }

    public async Task<ServiceResult<OrderDto>> GetOrderByIsAsync(int id)
    {
        try
        {
            var order = await _unitOfWork.Orders.GetByIdAsync(id);
            if (order == null)
            {
                return ServiceResult<OrderDto>.Failure("The order requested does not exist.");
            }
            var orderDto = _mapper.Map<OrderDto>(order);
            return ServiceResult<OrderDto>.Success(orderDto);
        }
        catch (Exception ex)
        {
            return ServiceResult<OrderDto>.Failure($"An unexpected error occurred while retrieving the order: {ex.Message}");
        }
    }

    public async Task<ServiceResult<Pager<OrderDto>>> GetOrdersAsync(Params orderParams)
    {
        try
        {
            var result = await _unitOfWork.Orders.GetAllAsync(orderParams.PageIndex, orderParams.PageSize, orderParams.Search);
            var orderListDto = _mapper.Map<List<OrderDto>>(result.registers);
            var pager = new Pager<OrderDto>(orderListDto, result.totalRegisters, orderParams.PageIndex, orderParams.PageSize, orderParams.Search);
            return ServiceResult<Pager<OrderDto>>.Success(pager);
        }
        catch (Exception ex)
        {
            return ServiceResult<Pager<OrderDto>>.Failure($"An unexpected error ocurred while retrieving orders: {ex.Message}");
        }
    }

    public async Task<ServiceResult<Pager<OrderDto>>> GetOrdersByUserAsync(int userId, Params orderParams)
    {
        try
        {
            var result = await _unitOfWork.Orders.GetOrdersByUserIdAsync(userId, orderParams.PageIndex, orderParams.PageSize, orderParams.Search);
            var orderListDto = _mapper.Map<List<OrderDto>>(result.registers);
            var pager = new Pager<OrderDto>(orderListDto, result.totalRegisters, orderParams.PageIndex, orderParams.PageSize, orderParams.Search);
            return ServiceResult<Pager<OrderDto>>.Success(pager);
        }
        catch (Exception ex)
        {
            return ServiceResult<Pager<OrderDto>>.Failure($"An unexpected error occurred while retrieving the orders: {ex.Message}");
        }
    }

    public async Task<ServiceResult<OrderDto>> UpdateOrderAsync(int id, OrderAddUpdateDto orderDto)
    {
        try
        {
            var orderDb = await _unitOfWork.Orders.GetByIdAsync(id);
            if (orderDb == null)
            {
                return ServiceResult<OrderDto>.Failure("The order requested does not exist.");
            }

            _mapper.Map(orderDto, orderDb);
            orderDb.UpdatedAt = DateTime.Now;
            await _unitOfWork.SaveAsync();

            var updatedOrderDto = _mapper.Map<OrderDto>(orderDb);
            return ServiceResult<OrderDto>.Success(updatedOrderDto);
        }
        catch (Exception ex)
        {
            return ServiceResult<OrderDto>.Failure($"An unexpected error occurred while updating the product: {ex.Message}");
        }
    }
}
