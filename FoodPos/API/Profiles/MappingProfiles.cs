using API.Dtos;
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
    }
}
