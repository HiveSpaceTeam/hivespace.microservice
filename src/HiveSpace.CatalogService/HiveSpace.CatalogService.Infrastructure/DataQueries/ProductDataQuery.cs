using HiveSpace.CatalogService.Application.DataQueries;
using HiveSpace.CatalogService.Application.Models.ViewModels;
using HiveSpace.CatalogService.Domain.Aggregates.External;
using HiveSpace.CatalogService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HiveSpace.CatalogService.Infrastructure.DataQueries
{
    public class ProductDataQuery(CatalogDbContext dbContext) : IProductDataQuery
    {
        public async Task<ProductDetailViewModel?> GetProductDetailViewModelAsync(int productId, CancellationToken cancellationToken = default)
        {
            var query = dbContext.Products
                .AsNoTracking()
                .Where(p => p.Id == productId)
                .Include(p => p.Categories)
                .Include(p => p.Images)
                .Include(p => p.Attributes)
                .Include(p => p.Variants)
                    .ThenInclude(v => v.Options)
                .Include(p => p.Skus)
                    .ThenInclude(s => s.Images)
                .Include(p => p.Skus)
                    .ThenInclude(s => s.SkuVariants);

            var product = await query.FirstOrDefaultAsync(cancellationToken);

            if (product == null)
                return null;

            var attributeValueLookup = await dbContext.AttributeValues
                .ToDictionaryAsync(av => av.Id, av => av.Name, cancellationToken);

            // Include ParentId so we can group attributes by their parent group
            var attributeLookup = await dbContext.Attributes
                .Select(ad => new { ad.Id, ad.Name, ad.ParentId })
                .ToDictionaryAsync(ad => ad.Id, cancellationToken);

            var currentSeller = await dbContext.StoreRef
                .Where(s => s.OwnerId == product.SellerId)
                .FirstOrDefaultAsync(cancellationToken);

            return new ProductDetailViewModel
            {
                Id = product.Id,
                SellerId = product.SellerId,
                Name = product.Name,
                Description = product.Description,
                Categories = product.Categories.ToList(),
                Images = product.Images.ToList(),
                Attributes = product.Attributes.Select(a =>
                {
                    attributeLookup.TryGetValue(a.AttributeId, out var attrMeta);
                    var groupId = attrMeta?.ParentId;
                    var groupName = groupId.HasValue && attributeLookup.TryGetValue(groupId.Value, out var group)
                        ? group.Name : null;
                    return new ProductAttributeViewModel
                    {
                        AttributeId = a.AttributeId,
                        AttributeName = attrMeta?.Name ?? "",
                        GroupId = groupId,
                        GroupName = groupName,
                        SelectedValueIds = a.SelectedValueIds.ToList(),
                        FreeTextValue = a.FreeTextValue,
                        NameValue = a.SelectedValueIds
                            .Select(id => attributeValueLookup.TryGetValue(id, out var v) ? v : "")
                            .ToList()
                    };
                }).ToList(),
                Skus = product.Skus.ToList(),
                Variants = product.Variants.ToList(),
                CurrentSeller = currentSeller == null ? null : new CurrentSeller
                {
                    Id = currentSeller.Id,
                    StoreName = currentSeller.StoreName,
                    LogoUrl = currentSeller.LogoUrl
                }
            };
        }
    }
}