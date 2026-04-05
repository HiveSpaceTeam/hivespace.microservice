using HiveSpace.CatalogService.Application.Products;
using HiveSpace.CatalogService.Application.Products.Dtos;
using HiveSpace.CatalogService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HiveSpace.CatalogService.Infrastructure.DataQueries;

public class ProductDataQuery(CatalogDbContext dbContext) : IProductDataQuery
{
    public async Task<ProductDetailDto?> GetProductDetailAsync(int productId, CancellationToken cancellationToken = default)
    {
        var product = await dbContext.Products
            .AsNoTracking()
            .Where(p => p.Id == productId)
            .Include(p => p.Categories)
            .Include(p => p.Images)
            .Include(p => p.Attributes)
            .Include(p => p.Variants).ThenInclude(v => v.Options)
            .Include(p => p.Skus).ThenInclude(s => s.Images)
            .Include(p => p.Skus).ThenInclude(s => s.SkuVariants)
            .FirstOrDefaultAsync(cancellationToken);

        if (product is null) return null;

        var attributeValueLookup = await dbContext.AttributeValues
            .ToDictionaryAsync(av => av.Id, av => av.Name, cancellationToken);

        var attributeLookup = await dbContext.Attributes
            .Select(ad => new { ad.Id, ad.Name, ad.ParentId })
            .ToDictionaryAsync(ad => ad.Id, cancellationToken);

        var currentSeller = await dbContext.StoreRef
            .Where(s => s.OwnerId == product.SellerId)
            .FirstOrDefaultAsync(cancellationToken);

        return new ProductDetailDto
        {
            Id          = product.Id,
            SellerId    = product.SellerId,
            Name        = product.Name,
            Description = product.Description,
            Categories  = product.Categories.ToList(),
            Images      = product.Images.ToList(),
            Attributes  = product.Attributes.Select(a =>
            {
                attributeLookup.TryGetValue(a.AttributeId, out var attrMeta);
                var groupId   = attrMeta?.ParentId;
                var groupName = groupId.HasValue && attributeLookup.TryGetValue(groupId.Value, out var group) ? group.Name : null;

                return new ProductAttributeDto
                {
                    AttributeId      = a.AttributeId,
                    AttributeName    = attrMeta?.Name ?? string.Empty,
                    GroupId          = groupId,
                    GroupName        = groupName,
                    SelectedValueIds = a.SelectedValueIds.ToList(),
                    FreeTextValue    = a.FreeTextValue,
                    NameValue        = a.SelectedValueIds
                        .Select(id => attributeValueLookup.TryGetValue(id, out var v) ? v : string.Empty)
                        .ToList()
                };
            }).ToList(),
            Skus     = product.Skus.ToList(),
            Variants = product.Variants.ToList(),
            CurrentSeller = currentSeller is null ? null : new CurrentSellerDto(
                currentSeller.Id,
                currentSeller.StoreName,
                currentSeller.LogoUrl)
        };
    }
}
