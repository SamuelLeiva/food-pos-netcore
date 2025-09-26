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
                return ServiceResult<OrderDto>.Failure("One or more products were not found or are invalid.");

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
                    Price = itemPrice // Snapshot del precio unitario (decimal)
                });
            }

            // --- CREACIÓN INICIAL DE LA ORDEN ---
            var order = _mapper.Map<Order>(orderDto);
            order.OrderItems = orderItems;
            order.UserId = userId;
            order.TotalAmount = calculatedTotal;
            order.Status = OrderStatus.Pending;
            _unitOfWork.Orders.Add(order);
            await _unitOfWork.SaveAsync(); // Obtener el Order.Id

            // --- CREACIÓN DEL PAYMENT INTENT EN STRIPE ---

            // Nota: Debes obtener o crear el CustomerId de Stripe para este usuario.
            // Aquí se usa un placeholder, pero deberías buscarlo en tu User.
            // Pora hora se usa un usuario por defecto para las pruebas
            string stripeCustomerId = "cus_T7HI3RTj8Oi5C6";

            var paymentIntentDto = new PaymentIntentDto
            {
                Amount = (long)Math.Round(calculatedTotal * 100), // CONVERSIÓN A CENTAVOS (long)
                Currency = "usd", // Asumimos USD
                CustomerId = stripeCustomerId,
                CustomerEmail = orderDto.ReceiptEmail,
                CustomerName = "User Name Placeholder", // Debes obtener este nombre
                Description = $"Order {order.Id} for user {userId}",
                OrderId = order.Id
            };

            // Llamada al servicio de Stripe usando el DTO
            var stripeResult = await _stripeService.CreatePaymentIntentAsync(paymentIntentDto);

            if (!stripeResult.IsSuccess)
            {
                // Manejo de fallos: La orden existe, pero el pago falló en Stripe.
                return ServiceResult<OrderDto>.Failure($"Stripe error: {stripeResult.ErrorMessage}");
            }

            // --- ACTUALIZACIÓN FINAL Y RESPUESTA ---

            order.PaymentIntentId = stripeResult.Data; // El service devuelve el ClientSecret
            order.StripeCustomerId = stripeCustomerId;

            _unitOfWork.Orders.Update(order);
            await _unitOfWork.SaveAsync();

            var createdOrderDto = _mapper.Map<OrderDto>(order);
            // Si necesitas devolver el ClientSecret, debes agregarlo al OrderDto temporalmente
            // createdOrderDto.ClientSecret = stripeResult.Data; 

            return ServiceResult<OrderDto>.Success(createdOrderDto);
        }
        catch (Exception ex)
        {
            return ServiceResult<OrderDto>.Failure($"An unexpected error occurred while creating the order: {ex.Message}");
        }
    }
    public async Task<ServiceResult<OrderDto>> GetOrderByIdAsync(int id)
    {
        try
        {
            // Cargar OrderItems y Products para el DTO
            var order = await _unitOfWork.Orders.GetByIdAsync(id); // Usa la operación específica

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

    public async Task<ServiceResult<OrderDto>> UpdateOrderAsync(int id, OrderUpdateDto orderDto)
    {
        try
        {
            var orderDb = await _unitOfWork.Orders.GetByIdAsync(id);
            if (orderDb == null)
            {
                return ServiceResult<OrderDto>.Failure("The order requested does not exist.");
            }

            // SOLO MAPEAMOS EL STATUS, IGNORANDO OrderItems para prevenir manipulación.
            if (Enum.TryParse(orderDto.Status, out OrderStatus newStatus))
            {
                orderDb.Status = newStatus;
            }
            else
            {
                return ServiceResult<OrderDto>.Failure("Invalid status value.");
            }

            orderDb.UpdatedAt = DateTime.Now;
            _unitOfWork.Orders.Update(orderDb);
            await _unitOfWork.SaveAsync();

            var updatedOrderDto = _mapper.Map<OrderDto>(orderDb);
            return ServiceResult<OrderDto>.Success(updatedOrderDto);
        }
        catch (Exception ex)
        {
            return ServiceResult<OrderDto>.Failure($"An unexpected error occurred while updating the order: {ex.Message}");
        }
    }
}
