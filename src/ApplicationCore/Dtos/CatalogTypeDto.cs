using System.Collections.Generic;

namespace Microsoft.eShopWeb.ApplicationCore.Dtos;

public class CatalogTypeDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class CatalogTypeResponseDto
{
    public CatalogTypeDto? CatalogType { get; set; }
}

public class CatalogTypeListResponseDto
{
    public IEnumerable<CatalogTypeDto> CatalogTypes { get; set; } = [];
}
