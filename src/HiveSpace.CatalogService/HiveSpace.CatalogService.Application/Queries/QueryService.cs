//using System.Data;

//namespace HiveSpace.CatalogService.Application.Queries;
//public class QueryService(NichoShopDbContext dbContext) : IQueryService
//{
//    private readonly NichoShopDbContext _dbContext = dbContext;

//    public async Task<List<CategoryViewModel>> GetCategoryViewModelsAsync()
//    {
//        return await _dbContext.Category
//            .Where(x => x.ParentId == null && x.FileImageId != null)
//            .Select(c => new CategoryViewModel
//            {
//                Id = c.Id,
//                Name = c.Name,
//                DisplayName = c.DisplayName,
//                FileImageId = c.FileImageId
//            })
//            .ToListAsync();
//    }

//    public async Task<List<LocationViewModel>> GetLocationViewModelsAsync(int type, string? parentCode)
//    {
//        // Get provinces
//        if (type == (int)LocationType.Province)
//        {
//            return await _dbContext.Province
//                .Select(c => new LocationViewModel
//                {
//                    Code = c.Code,
//                    Name = c.Name,
//                    FullName = c.FullName,
//                    NameEn = c.NameEn,
//                    FullNameEn = c.FullNameEn,
//                    CodeName = c.CodeName,
//                })
//                .ToListAsync();
//        }
//        // Get districts
//        else if (type == (int)LocationType.District)
//        {
//            return await _dbContext.District
//                .Where(x => x.ProvinceCode == parentCode)
//                .Select(c => new LocationViewModel
//                {
//                    Code = c.Code,
//                    Name = c.Name,
//                    FullName = c.FullName,
//                    NameEn = c.NameEn,
//                    FullNameEn = c.FullNameEn,
//                    CodeName = c.CodeName,
//                })
//                .ToListAsync();
//        }
//        // Get wards
//        else if (type == (int)LocationType.Ward)
//        {
//            return await _dbContext.Ward
//                .Where(x => x.DistrictCode == parentCode)
//                .Select(c => new LocationViewModel
//                {
//                    Code = c.Code,
//                    Name = c.Name,
//                    FullName = c.FullName,
//                    NameEn = c.NameEn,
//                    FullNameEn = c.FullNameEn,
//                    CodeName = c.CodeName,
//                })
//                .ToListAsync();
//        }
//        return [];
//    }

//    public async Task<List<CartItemViewModel>> GetCartItemViewModelsAsync(Guid userId)
//    {
//        var res = new List<CartItemViewModel>();
//        var paramUserId = new NpgsqlParameter("@userId", userId);

//        string query = $@"
//            SELECT
//              ci.""Id"",
//              p.""Name"" AS ProductName,
//              STRING_AGG(attr ->> 'value', ', ') AS ProductVariantName,
//              s.""Amount"" AS Price,
//              ci.""Quantity"",
//              s.""Currency"",
//              ci.""SkuId"",
//              ci.""IsSelected""
//            FROM shopping_carts sc
//                   LEFT JOIN cart_items ci
//                     ON sc.""Id"" = ci.""ShoppingCartId""
//                   LEFT JOIN skus s
//                     ON ci.""SkuId"" = s.""Id""
//                   LEFT JOIN products p
//                     ON p.""Id"" = s.""ProductId"",
//                 LATERAL jsonb_array_elements(s.""SkuVariants"") AS attr
//            WHERE sc.""CustomerId"" = '{userId.ToString()}'
//            GROUP BY ci.""Id"",
//                     p.""Name"",
//                     s.""Amount"",
//                     ci.""Quantity"",
//                     s.""Currency"",
//                     ci.""SkuId"";";

//        var cartItems = await _dbContext.Database.ExecuteSqlRawAsync(query);

//        using (var command = _dbContext.Database.GetDbConnection().CreateCommand())
//        {
//            command.CommandText = query;
//            command.CommandType = CommandType.Text;

//            await _dbContext.Database.OpenConnectionAsync();

//            using (var reader = await command.ExecuteReaderAsync())
//            {
//                while (await reader.ReadAsync())
//                {
//                    var cartItem = new CartItemViewModel
//                    {
//                        Id = reader.GetGuid(0),
//                        ProductName = reader.GetString(1),
//                        ProductVariantName = reader.GetString(2),
//                        Amount = reader.GetDecimal(3),
//                        Quantity = reader.GetInt32(4),
//                        Currency = reader.GetString(5),
//                        SkuId = reader.GetInt32(6),
//                        IsSelected = reader.GetBoolean(7),
//                    };
//                    res.Add(cartItem);
//                }
//            }
//            await _dbContext.Database.CloseConnectionAsync();
//        }
//        return res;
//    }

//    public async Task<List<ProductSearchViewModel>> GetProductSearchViewModelAsync(ProductSearchRequestDto param)
//    {
//        List<ProductSearchViewModel> res = new List<ProductSearchViewModel>();

//        // Handle sort
//        string querySort = string.Empty;
//        if (param.Sort == "ASC")
//        {
//            querySort = @"ORDER BY skus.""Amount"" ";
//        }
//        else if (param.Sort == "DESC")
//        {
//            querySort = @"ORDER BY skus.""Amount"" DESC ";
//        }

//        // Handle offset
//        int offSet = param.PageSize * (param.PageIndex - 1);

//        string query = $@"
//            WITH first_sku_per_product
//            AS
//            (SELECT
//                skus.*,
//                ROW_NUMBER() OVER (PARTITION BY skus.""ProductId""
//                ORDER BY skus.""Id"") AS rn
//              FROM skus)
//            SELECT
//              p.""Id"" AS ProductId,
//              p.""Name"" AS ProductName,
//              skus.""Amount"",
//              skus.""Currency"",
//              'https://down-vn.img.susercontent.com/file/vn-11134207-7ras8-m3kcw18uw8ip34_tn.webp' AS ProductImage
//            FROM products p
//              LEFT JOIN first_sku_per_product AS skus
//                ON p.""Id"" = skus.""ProductId""
//                AND skus.rn = 1
//            WHERE p.""Name"" LIKE '%{param.Keyword}%' " +
//            querySort.ToString()
//            + $@"LIMIT {param.PageSize} OFFSET {offSet};";
//        var items = await _dbContext.Database.ExecuteSqlRawAsync(query);

//        using (var command = _dbContext.Database.GetDbConnection().CreateCommand())
//        {
//            command.CommandText = query;
//            command.CommandType = CommandType.Text;

//            await _dbContext.Database.OpenConnectionAsync();

//            using (var reader = await command.ExecuteReaderAsync())
//            {
//                while (await reader.ReadAsync())
//                {
//                    var product = new ProductSearchViewModel
//                    {
//                        ProductId = reader.GetInt32(0),
//                        ProductName = reader.GetString(1),
//                        Amount = reader.GetDecimal(2),
//                        Currency = reader.GetString(3),
//                        ProductImage = reader.GetString(4),
//                    };
//                    res.Add(product);
//                }
//            }
//            await _dbContext.Database.CloseConnectionAsync();
//        }
//        return res;
//    }

//    public async Task<List<ProductHomeViewModel>> GetProductHomeViewModelAsync(ProductHomeRequestDto param)
//    {
//        List<ProductHomeViewModel> res = new List<ProductHomeViewModel>();

//        // Handle offset
//        int offSet = param.PageSize * (param.PageIndex - 1);

//        string query = $@"
//            WITH first_sku_per_product
//            AS
//            (SELECT
//                skus.*,
//                ROW_NUMBER() OVER (PARTITION BY skus.""ProductId""
//                ORDER BY skus.""Id"") AS rn
//              FROM skus)
//            SELECT
//              p.""Id"" AS ProductId,
//              p.""Name"" AS ProductName,
//              skus.""Amount"",
//              skus.""Currency"",
//              'https://down-vn.img.susercontent.com/file/vn-11134207-7ras8-m3kcw18uw8ip34_tn.webp' AS ProductImage
//            FROM products p
//              LEFT JOIN first_sku_per_product AS skus
//                ON p.""Id"" = skus.""ProductId""
//                AND skus.rn = 1
//            LIMIT {param.PageSize} 
//            OFFSET {offSet};";
//        var items = await _dbContext.Database.ExecuteSqlRawAsync(query);

//        using (var command = _dbContext.Database.GetDbConnection().CreateCommand())
//        {
//            command.CommandText = query;
//            command.CommandType = CommandType.Text;

//            await _dbContext.Database.OpenConnectionAsync();

//            using (var reader = await command.ExecuteReaderAsync())
//            {
//                while (await reader.ReadAsync())
//                {
//                    var product = new ProductHomeViewModel
//                    {
//                        ProductId = reader.GetInt32(0),
//                        ProductName = reader.GetString(1),
//                        Amount = reader.GetDecimal(2),
//                        Currency = reader.GetString(3),
//                        ProductImage = reader.GetString(4),
//                    };
//                    res.Add(product);
//                }
//            }
//            await _dbContext.Database.CloseConnectionAsync();
//        }
//        return res;
//    }

//    public async Task<List<ProductsByCategoryDto>> GetProductSearchViewModelAsync(int categoryId)
//    {
//        List<ProductsByCategoryDto> res = new List<ProductsByCategoryDto>();

//        string query = $@"
//            WITH first_sku_per_product
//            AS
//            (SELECT
//                skus.*,
//                ROW_NUMBER() OVER (PARTITION BY skus.""ProductId""
//                ORDER BY skus.""Id"") AS rn
//              FROM skus)
//            SELECT
//              p.""Id"" AS ProductId,
//              p.""Name"" AS ProductName,
//              skus.""Amount"",
//              skus.""Currency"",
//              'https://down-vn.img.susercontent.com/file/vn-11134207-7ras8-m3kcw18uw8ip34_tn.webp' AS ProductImage
//            FROM products p
//              LEFT JOIN first_sku_per_product AS skus
//                ON p.""Id"" = skus.""ProductId""
//                AND skus.rn = 1
//              LEFT JOIN product_categories pc
//                ON p.""Id"" = pc.""ProductId""
//              LEFT JOIN categories c
//                ON pc.""CategoryId"" = c.""Id""
//            WHERE c.""Id"" = " + categoryId.ToString() + ";";
//        var items = await _dbContext.Database.ExecuteSqlRawAsync(query);

//        using (var command = _dbContext.Database.GetDbConnection().CreateCommand())
//        {
//            command.CommandText = query;
//            command.CommandType = CommandType.Text;

//            await _dbContext.Database.OpenConnectionAsync();

//            using (var reader = await command.ExecuteReaderAsync())
//            {
//                while (await reader.ReadAsync())
//                {
//                    var product = new ProductsByCategoryDto
//                    {
//                        ProductId = reader.GetInt32(0),
//                        ProductName = reader.GetString(1),
//                        Amount = reader.GetDecimal(2),
//                        Currency = reader.GetString(3),
//                        ProductImage = reader.GetString(4),
//                    };
//                    res.Add(product);
//                }
//            }
//            await _dbContext.Database.CloseConnectionAsync();
//        }
//        return res;
//    }

//    public async Task<List<ProductCategoryViewModel>> GetCategoryTree(int productId)
//    {
//        var res = new List<ProductCategoryViewModel>();
//        var paramUserId = new NpgsqlParameter("@productId", productId);

//        string query = $@"
//           WITH RECURSIVE t(""Id"",""Name"", ""DisplayName"", ""ParentId"") AS (
//                    Select * from categories WHERE ""Id""= '{productId}'
//                  UNION ALL
//                   Select pc.* from t JOIN categories pc on pc.""Id"" = t.""ParentId""
//                )
//                SELECT * FROM t";

//        using (var command = _dbContext.Database.GetDbConnection().CreateCommand())
//        {
//            command.CommandText = query;
//            command.CommandType = CommandType.Text;

//            await _dbContext.Database.OpenConnectionAsync();

//            using (var reader = await command.ExecuteReaderAsync())
//            {
//                while (await reader.ReadAsync())
//                {
//                    var categoryViewModel = new ProductCategoryViewModel
//                    {
//                        CategoryId = reader.GetInt32(0),
//                        CategoryName = reader.GetString(2),
//                        ParentId = reader.IsDBNull(3) ? null : reader.GetInt32(3)
//                    };
//                    res.Add(categoryViewModel);
//                }
//            }
//            await _dbContext.Database.CloseConnectionAsync();
//        }
//        return res;
//    }
//}
