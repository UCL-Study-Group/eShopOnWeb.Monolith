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
            .MapWith(dto => new CatalogItem(
                dto.CatalogTypeId,
                dto.CatalogBrandId,
                dto.Description,
                dto.Name,
                dto.Price,
                dto.PictureUri ?? string.Empty
            ));

        TypeAdapterConfig<CatalogBrandDto, CatalogBrand>
            .NewConfig()
            .MapWith(dto => new CatalogBrand(dto.Name));

        TypeAdapterConfig<CatalogTypeDto, CatalogType>
            .NewConfig()
            .MapWith(dto => new CatalogType(dto.Name));
    }
}
