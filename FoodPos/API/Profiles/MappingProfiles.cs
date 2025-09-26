using API.Dtos.Categories;
using API.Dtos.Order;
using API.Dtos.Products;
using API.Dtos.Roles;
using AutoMapper;
using Core.Entities;
using Core.Enums;

namespace API.Profiles;

public class MappingProfiles : Profile
{
    public MappingProfiles()
    {
        CreateMap<Product, ProductDto>()
            .ReverseMap();

        CreateMap<Category, CategoryDto>()
            .ReverseMap();

        CreateMap<Product, ProductListDto>()
            .ForMember(dest => dest.Category, src => src.MapFrom(src => src.Category.Name))
            .ReverseMap()
            .ForMember(src => src.Category, dest => dest.Ignore());
        CreateMap<Product, ProductAddUpdateDto>()
            .ReverseMap()
            .ForMember(src => src.Category, dest => dest.Ignore());

        CreateMap<Role, RoleDto>()
            .ReverseMap();

        CreateMap<Category, CategoryDto>()
            .ReverseMap();
        CreateMap<Category, CategoryAddUpdateDto>()
            .ReverseMap();

        CreateMap<OrderItem, OrderItemDto>()
            .ForMember(dest => dest.ProductName, src => src.MapFrom(src => src.Product.Name));
        CreateMap<OrderItemAddUpdateDto, OrderItem>()
            .ForMember(dest => dest.Price, opt => opt.Ignore())
            .ForMember(dest => dest.Order, opt => opt.Ignore());

        CreateMap<Order, OrderDto>()
            .ForMember(dest => dest.OrderItems, opt => opt.MapFrom(src => src.OrderItems));
        CreateMap<OrderAddDto, Order>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => Enum.Parse(typeof(OrderStatus), src.Status)))
            .ForMember(dest => dest.TotalAmount, opt => opt.Ignore())
            .ForMember(dest => dest.PaymentIntentId, opt => opt.Ignore())
            .ForMember(dest => dest.StripeCustomerId, opt => opt.Ignore())
            .ForMember(dest => dest.User, opt => opt.Ignore());
        CreateMap<OrderUpdateDto, Order>()
            .ForMember(
                dest => dest.Status,
                opt => opt.MapFrom(src => Enum.Parse(typeof(OrderStatus), src.Status))
            )
            .ForMember(dest => dest.OrderItems, opt => opt.Ignore())
            .ForMember(dest => dest.TotalAmount, opt => opt.Ignore())
            .ForMember(dest => dest.PaymentIntentId, opt => opt.Ignore())
            .ForMember(dest => dest.StripeCustomerId, opt => opt.Ignore())
            .ForMember(dest => dest.ReceiptEmail, opt => opt.Ignore())
            .ForMember(dest => dest.User, opt => opt.Ignore());
    }
}
