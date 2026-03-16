using HiveSpace.CatalogService.Application.DataQueries;
using HiveSpace.CatalogService.Application.Models.ViewModels;
using HiveSpace.CatalogService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HiveSpace.CatalogService.Infrastructure.DataQueries
{
    public class ProductDataQuery(CatalogDbContext dbContext) : IProductDataQuery
    {
        public async Task<ProductDetailViewModel?> GetProductDetailViewModelAsync(Guid productId, CancellationToken cancellationToken = default)
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

            var attributeLookup = await dbContext.Attributes
                .ToDictionaryAsync(ad => ad.Id, ad => ad.Name, cancellationToken);

            return new ProductDetailViewModel
            {
                Id = product.Id,
                SellerId = product.SellerId,
                Name = product.Name,
                Description = product.Description,
                Categories = product.Categories.ToList(),
                Images = product.Images.ToList(),
                Attributes = product.Attributes.Select(a => new ProductAttributeViewModel
                {
                    AttributeId = a.AttributeId,
                    AttributeName = attributeLookup.ContainsKey(a.AttributeId) ? attributeLookup[a.AttributeId] : "",
                    SelectedValueIds = a.SelectedValueIds.ToList(),
                    FreeTextValue = a.FreeTextValue,
                    NameValue = a.SelectedValueIds.Select(id => attributeValueLookup.ContainsKey(id) ? attributeValueLookup[id] : "").ToList()
                }).ToList(),
                Skus = product.Skus.ToList(),
                Variants = product.Variants.ToList()
            };
        }
    }
}