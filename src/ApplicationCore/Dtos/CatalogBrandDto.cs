using System.Collections.Generic;

namespace Microsoft.eShopWeb.ApplicationCore.Dtos;

public class CatalogBrandDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class CatalogBrandResponseDto
{
    public CatalogBrandDto? CatalogBrand { get; set; }
}

public class CatalogBrandListResponseDto
{
    public IEnumerable<CatalogBrandDto> CatalogBrands { get; set; } = [];
}
