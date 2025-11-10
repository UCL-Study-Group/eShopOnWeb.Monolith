using System.Collections.Generic;

namespace Microsoft.eShopWeb.ApplicationCore.Dtos;

public class CatalogItemDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string? PictureUri { get; set; }
    public int CatalogTypeId { get; set; }
    public int CatalogBrandId { get; set; }
}

public class CatalogItemResponseDto
{
    public CatalogItemDto? CatalogItem { get; set; }
}

public class CatalogItemListResponseDto
{
    public IEnumerable<CatalogItemDto> CatalogItems { get; set; } = [];
}
