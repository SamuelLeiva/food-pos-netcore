using API.Dtos.Order;
using API.Dtos.Products;
using API.Dtos.Stripe;
using API.Helpers;
using API.Services.Interfaces;
using AutoMapper;
using Core.Entities;
using Core.Enums;
using Core.Interfaces;
using Core.Services;
using System.Globalization;

namespace API.Services;

public class OrderService : IOrderService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IStripeService _stripeService;

    public OrderService(IUnitOfWork unitOfWork, IMapper mapper, IStripeService stripeService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _stripeService = stripeService;
    }

    public async Task<ServiceResult<OrderDto>> CreateOrderAsync(OrderAddDto orderDto, int userId)
    {
        try
        {
            var productIds = orderDto.OrderItems.Select(oi => oi.ProductId).ToList();
            var products = _unitOfWork.Products.Find(p => productIds.Contains(p.Id)).ToList();

            if (products.Count != productIds.Count)
                // 404 Not Found: Uno o más recursos esenciales (productos) no existen.
                return ServiceResult<OrderDto>.Failure("One or more products were not found or are invalid.", 404);

            decimal calculatedTotal = 0;
            var orderItems = new List<OrderItem>();

            foreach (var itemDto in orderDto.OrderItems)
            {
                var product = products.First(p => p.Id == itemDto.ProductId);
                decimal itemPrice = Convert.ToDecimal(product.Price, CultureInfo.InvariantCulture);
                calculatedTotal += itemPrice * itemDto.Quantity;

                orderItems.Add(new OrderItem
                {
                    ProductId = itemDto.ProductId,
                    Quantity = itemDto.Quantity,
                    Price = itemPrice
                });
            }

            // --- CREACIÓN INICIAL DE LA ORDEN ---
            var order = _mapper.Map<Order>(orderDto);
            order.OrderItems = orderItems;
            order.UserId = userId;
            order.TotalAmount = calculatedTotal;
            order.Status = OrderStatus.Pending;
            _unitOfWork.Orders.Add(order);
            await _unitOfWork.SaveAsync();

            // --- CREACIÓN DEL PAYMENT INTENT EN STRIPE ---
            string stripeCustomerId = "cus_T7HI3RTj8Oi5C6"; // Placeholder

            var paymentIntentDto = new PaymentIntentDto
            {
                Amount = (long)Math.Round(calculatedTotal * 100),
                Currency = "usd",
                CustomerId = stripeCustomerId,
                CustomerEmail = orderDto.ReceiptEmail,
                CustomerName = "User Name Placeholder",
                Description = $"Order {order.Id} for user {userId}",
                OrderId = order.Id
            };

            var stripeResult = await _stripeService.CreatePaymentIntentAsync(paymentIntentDto);

            if (!stripeResult.IsSuccess)
            {
                // 503 Service Unavailable / 500 Internal Server Error: Fallo de integración con servicio externo.
                return ServiceResult<OrderDto>.Failure($"Stripe error: {stripeResult.ErrorMessage}", 503);
            }

            // --- ACTUALIZACIÓN FINAL Y RESPUESTA ---

            order.PaymentIntentId = stripeResult.Data;
            order.StripeCustomerId = stripeCustomerId;

            _unitOfWork.Orders.Update(order);
            await _unitOfWork.SaveAsync();

            var createdOrderDto = _mapper.Map<OrderDto>(order);

            return ServiceResult<OrderDto>.Success(createdOrderDto);
        }
        catch (Exception ex)
        {
            // 500 Internal Server Error: Error inesperado del servidor.
            return ServiceResult<OrderDto>.Failure($"An unexpected error occurred while creating the order: {ex.Message}", 500);
        }
    }

    public async Task<ServiceResult<OrderDto>> GetOrderByIdAsync(int id)
    {
        try
        {
            var order = await _unitOfWork.Orders.GetByIdAsync(id);

            if (order == null)
            {
                // 404 Not Found: La orden no existe.
                return ServiceResult<OrderDto>.Failure("The order requested does not exist.", 404);
            }

            var orderDto = _mapper.Map<OrderDto>(order);
            return ServiceResult<OrderDto>.Success(orderDto);
        }
        catch (Exception ex)
        {
            // 500 Internal Server Error
            return ServiceResult<OrderDto>.Failure($"An unexpected error occurred while retrieving the order: {ex.Message}", 500);
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
            // 500 Internal Server Error
            return ServiceResult<Pager<OrderDto>>.Failure($"An unexpected error ocurred while retrieving orders: {ex.Message}", 500);
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
            // 500 Internal Server Error
            return ServiceResult<Pager<OrderDto>>.Failure($"An unexpected error occurred while retrieving the orders: {ex.Message}", 500);
        }
    }

    public async Task<ServiceResult<OrderDto>> UpdateOrderAsync(int id, OrderUpdateDto orderDto)
    {
        try
        {
            var orderDb = await _unitOfWork.Orders.GetByIdAsync(id);
            if (orderDb == null)
            {
                // 404 Not Found: La orden no existe.
                return ServiceResult<OrderDto>.Failure("The order requested does not exist.", 404);
            }

            // ... (Lógica de actualización de Status)

            if (Enum.TryParse(orderDto.Status, out OrderStatus newStatus))
            {
                orderDb.Status = newStatus;
            }
            else
            {
                // 400 Bad Request: El valor enviado es inválido.
                return ServiceResult<OrderDto>.Failure("Invalid status value.", 400);
            }

            orderDb.UpdatedAt = DateTime.Now;
            _unitOfWork.Orders.Update(orderDb);
            await _unitOfWork.SaveAsync();

            var updatedOrderDto = _mapper.Map<OrderDto>(orderDb);
            return ServiceResult<OrderDto>.Success(updatedOrderDto);
        }
        catch (Exception ex)
        {
            // 500 Internal Server Error
            return ServiceResult<OrderDto>.Failure($"An unexpected error occurred while updating the order: {ex.Message}", 500);
        }
    }
}