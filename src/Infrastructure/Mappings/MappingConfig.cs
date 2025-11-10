namespace Microsoft.eShopWeb.Infrastructure.Mappings;

public static class MappingConfig
{
    public static void RegisterMappings()
    {
        CatalogMappings.Configure();
    }
}
