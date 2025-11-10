using Mapster;
using Microsoft.eShopWeb.ApplicationCore.Dtos;
using Microsoft.eShopWeb.ApplicationCore.Entities;

namespace Microsoft.eShopWeb.Infrastructure.Mappings;

public class CatalogMappings
{
    public static void Configure()
    {
        TypeAdapterConfig<CatalogItemDto, CatalogItem>
            .NewConfig()
            .ConstructUsing(dto => new CatalogItem(
                dto.CatalogTypeId,
                dto.CatalogBrandId,
                dto.Description,
                dto.Name,
                dto.Price,
                dto.PictureUri ?? string.Empty
            ))
            .Map(dest => dest.Id, src => src.Id);

        TypeAdapterConfig<CatalogBrandDto, CatalogBrand>
            .NewConfig()
            .ConstructUsing(dto => new CatalogBrand(dto.Name))
            .Map(dest => dest.Id, src => src.Id);

        TypeAdapterConfig<CatalogTypeDto, CatalogType>
            .NewConfig()
            .ConstructUsing(dto => new CatalogType(dto.Name))
            .Map(dest => dest.Id, src => src.Id);
    }
}
