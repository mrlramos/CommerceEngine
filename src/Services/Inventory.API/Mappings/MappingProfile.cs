using AutoMapper;
using Inventory.API.DTOs;
using Inventory.API.Models;

namespace Inventory.API.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // InventoryItem -> InventoryItemDto
            CreateMap<InventoryItem, InventoryItemDto>()
                .ForMember(dest => dest.AvailableQuantity, opt => opt.MapFrom(src => src.AvailableQuantity))
                .ForMember(dest => dest.IsLowStock, opt => opt.MapFrom(src => src.IsLowStock))
                .ForMember(dest => dest.IsOutOfStock, opt => opt.MapFrom(src => src.IsOutOfStock));

            // CreateInventoryItemDto -> InventoryItem
            CreateMap<CreateInventoryItemDto, InventoryItem>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.ReservedQuantity, opt => opt.MapFrom(src => 0))
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.LastUpdated, opt => opt.Ignore());

            // UpdateInventoryItemDto -> InventoryItem
            CreateMap<UpdateInventoryItemDto, InventoryItem>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.ProductId, opt => opt.Ignore())
                .ForMember(dest => dest.SKU, opt => opt.Ignore())
                .ForMember(dest => dest.ReservedQuantity, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.LastUpdated, opt => opt.Ignore());
        }
    }
} 