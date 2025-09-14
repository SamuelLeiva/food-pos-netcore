using API.Dtos.Categories;
using API.Dtos.Products;
using API.Dtos.Roles;
using AutoMapper;
using Core.Entities;

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

        CreateMap<Role, CreateRoleDto>()
            .ReverseMap();

    }
}
