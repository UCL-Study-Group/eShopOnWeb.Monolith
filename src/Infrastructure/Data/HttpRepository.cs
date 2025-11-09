using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Ardalis.Specification;
using Mapster;
using Microsoft.eShopWeb.ApplicationCore.Dtos;
using Microsoft.eShopWeb.ApplicationCore.Entities;
using Microsoft.eShopWeb.ApplicationCore.Interfaces;

namespace Microsoft.eShopWeb.Infrastructure.Data;

public class HttpRepository<T> : IRepository<T>, IReadRepository<T> where T : class, IAggregateRoot
{
    private readonly HttpClient _client;

    public HttpRepository(HttpClient client)
    {
        _client = client;
    }

    public async Task<T?> GetByIdAsync<TId>(TId id, CancellationToken cancellationToken = new()) where TId : notnull
    {
        var route = GetApiRoute();
        
        var response = await _client.GetAsync($"{route}/{id}", cancellationToken);
        
        if (!response.IsSuccessStatusCode)
            return null;

        return await DeserializeSingleResponse(response.Content);
    }

    public async Task<T?> GetBySpecAsync(ISpecification<T> specification, CancellationToken cancellationToken = new())
    {
        var items = await GetListFromApi(cancellationToken);
        
        if (items is null)
            return null;
        
        var evaluated = specification.Evaluate(items);

        return evaluated.FirstOrDefault();
    }

    public async Task<TResult?> GetBySpecAsync<TResult>(ISpecification<T, TResult> specification,
        CancellationToken cancellationToken = new())
    {
        var items = await GetListFromApi(cancellationToken);

        if (items is null)
            return default;
    
        var evaluated = specification.Evaluate(items);

        return evaluated.FirstOrDefault();
    }

    public async Task<T?> FirstOrDefaultAsync(ISpecification<T> specification, CancellationToken cancellationToken = new())
    {
        var items = await ListAsync(specification, cancellationToken);
        
        return items.FirstOrDefault();
    }

    public async Task<TResult?> FirstOrDefaultAsync<TResult>(ISpecification<T, TResult> specification,
        CancellationToken cancellationToken = new())
    {
        var items = await ListAsync(specification, cancellationToken);
    
        return items.FirstOrDefault();
    }

    public async Task<T?> SingleOrDefaultAsync(ISingleResultSpecification<T> specification, 
        CancellationToken cancellationToken = new())
    {
        var items = await GetListFromApi(cancellationToken);

        if (items is null)
            return null;

        var evaluated = specification.Evaluate(items);

        return evaluated.SingleOrDefault();
    }

    public async Task<TResult?> SingleOrDefaultAsync<TResult>(ISingleResultSpecification<T, TResult> specification,
        CancellationToken cancellationToken = new())
    {
        var items = await GetListFromApi(cancellationToken);

        if (items is null)
            return default;

        var evaluated = specification.Evaluate(items);

        return evaluated.SingleOrDefault();
    }

    public async Task<List<T>> ListAsync(CancellationToken cancellationToken = new())
    {
        var items = await GetListFromApi(cancellationToken);

        if (items is null)
            throw new InvalidOperationException("Failed to retrieve items from API");
    
        return items.ToList();
    }
    
    public async Task<List<T>> ListAsync(ISpecification<T> specification, CancellationToken cancellationToken = new())
    {
        var items = await GetListFromApi(cancellationToken);

        if (items is null)
            throw new InvalidOperationException("Failed to retrieve items from API");
    
        var evaluated = specification.Evaluate(items);
    
        return evaluated.ToList();
    }

    public async Task<List<TResult>> ListAsync<TResult>(ISpecification<T, TResult> specification, CancellationToken cancellationToken = new())
    {
        var items = await GetListFromApi(cancellationToken);

        if (items is null)
            throw new InvalidOperationException("Failed to retrieve items from API");

        var evaluated = specification.Evaluate(items);

        return evaluated.ToList();
    }

    public async Task<int> CountAsync(ISpecification<T> specification, CancellationToken cancellationToken = new())
    {
        var items = await ListAsync(specification, cancellationToken);
        
        return items.Count;
    }

    public async Task<int> CountAsync(CancellationToken cancellationToken = new())
    {
        var items = await ListAsync(cancellationToken);
        
        return items.Count;
    }

    public async Task<bool> AnyAsync(ISpecification<T> specification, CancellationToken cancellationToken = new())
    {
        var items = await ListAsync(specification, cancellationToken);

        return items.Count != 0;
    }

    public async Task<bool> AnyAsync(CancellationToken cancellationToken = new())
    {
        var items = await ListAsync(cancellationToken);
        
        return items.Count != 0;
    }

    public async IAsyncEnumerable<T> AsAsyncEnumerable(ISpecification<T> specification)
    {
        var items = await ListAsync(specification);
        
        foreach (var item in items)
        {
            yield return item;
        }
    }

    public async Task<T> AddAsync(T entity, CancellationToken cancellationToken = new())
    {
        var route = GetApiRoute();
    
        var response = await _client.PostAsJsonAsync(route, entity, cancellationToken);
    
        response.EnsureSuccessStatusCode();
    
        var created = await DeserializeSingleResponse(response.Content);
    
        return created ?? entity;
    }

    public async Task<IEnumerable<T>> AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = new())
    {
        var results = new List<T>();
    
        foreach (var entity in entities)
        {
            var result = await AddAsync(entity, cancellationToken);
            results.Add(result);
        }
    
        return results;
    }

    public async Task UpdateAsync(T entity, CancellationToken cancellationToken = new())
    {
        var route = GetApiRoute();
    
        var response = await _client.PutAsJsonAsync(route, entity, cancellationToken);
    
        response.EnsureSuccessStatusCode();
    }

    public async Task UpdateRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = new())
    {
        foreach (var entity in entities)
        {
            await UpdateAsync(entity, cancellationToken);
        }
    }

    public async Task DeleteAsync(T entity, CancellationToken cancellationToken = new())
    {
        var route = GetApiRoute();
    
        var id = GetEntityId(entity);
    
        var response = await _client.DeleteAsync($"{route}/{id}", cancellationToken);
    
        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = new())
    {
        foreach (var entity in entities)
        {
            await DeleteAsync(entity, cancellationToken);
        }
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = new())
    {
        return Task.FromResult(0);
    }
    
    private async Task<IEnumerable<T>?> GetListFromApi(CancellationToken cancellationToken)
    {
        var route = GetApiRoute();
        var response = await _client.GetAsync(route, cancellationToken);

        if (!response.IsSuccessStatusCode)
            return null;

        return await DeserializeListResponse(response.Content);
    }

    private static async Task<IEnumerable<T>?> DeserializeListResponse(HttpContent content)
    {
        var asString = await content.ReadAsStringAsync();
    
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        if (typeof(T) == typeof(CatalogItem))
        {
            var wrapper = JsonSerializer.Deserialize<CatalogItemListResponseDto>(asString, options);
            return wrapper?.CatalogItems?.Adapt<List<CatalogItem>>() as IEnumerable<T>;
        }
        else if (typeof(T) == typeof(CatalogBrand))
        {
            var wrapper = JsonSerializer.Deserialize<CatalogBrandListResponseDto>(asString, options);
            return wrapper?.CatalogBrands?.Adapt<List<CatalogBrand>>() as IEnumerable<T>;
        }
        else if (typeof(T) == typeof(CatalogType))
        {
            var wrapper = JsonSerializer.Deserialize<CatalogTypeListResponseDto>(asString, options);
            return wrapper?.CatalogTypes?.Adapt<List<CatalogType>>() as IEnumerable<T>;
        }

        throw new NotSupportedException($"Entity type {typeof(T).Name} not supported for list deserialization");
    }

    private static async Task<T?> DeserializeSingleResponse(HttpContent content)
    {
        var asString = await content.ReadAsStringAsync();
    
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        try
        {
            if (typeof(T) == typeof(CatalogItem))
            {
                var wrapper = JsonSerializer.Deserialize<CatalogItemResponseDto>(asString, options);
                return wrapper?.CatalogItem?.Adapt<CatalogItem>() as T;
            }
            else if (typeof(T) == typeof(CatalogBrand))
            {
                var wrapper = JsonSerializer.Deserialize<CatalogBrandResponseDto>(asString, options);
                return wrapper?.CatalogBrand?.Adapt<CatalogBrand>() as T;
            }
            else if (typeof(T) == typeof(CatalogType))
            {
                var wrapper = JsonSerializer.Deserialize<CatalogTypeResponseDto>(asString, options);
                return wrapper?.CatalogType?.Adapt<CatalogType>() as T;
            }

            throw new NotSupportedException($"Entity type {typeof(T).Name} not supported for single entity deserialization");
        }
        catch (Exception)
        {
            return null;
        }
    }

    private static object GetEntityId(T entity)
    {
        var idProperty = typeof(T).GetProperty("Id") 
                         ?? throw new InvalidOperationException($"Entity {typeof(T).Name} does not have an Id property");
    
        return idProperty.GetValue(entity) 
               ?? throw new InvalidOperationException($"Entity {typeof(T).Name} has null Id");
    }

    private static string GetApiRoute()
    {
        return typeof(T).Name switch
        {
            nameof(CatalogItem) => "catalog-items",
            nameof(CatalogBrand) => "catalog-brands",
            nameof(CatalogType) => "catalog-types",
            _ => throw new NotSupportedException($"No route registered for {typeof(T).Name}")
        };
    }
}
