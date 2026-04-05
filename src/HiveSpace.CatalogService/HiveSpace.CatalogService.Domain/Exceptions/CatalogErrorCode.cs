using HiveSpace.Domain.Shared.Errors;

namespace HiveSpace.CatalogService.Domain.Exceptions;

public class CatalogErrorCode(int id, string name, string code) : DomainErrorCode(id, name, code)
{
    // Product
    public static readonly CatalogErrorCode ProductNotFound      = new(3001, "ProductNotFound",      "CAT3001");
    public static readonly CatalogErrorCode InvalidProductStatus = new(3002, "InvalidProductStatus", "CAT3002");

    // SKU
    public static readonly CatalogErrorCode SkuNotFound       = new(3010, "SkuNotFound",       "CAT3010");
    public static readonly CatalogErrorCode InvalidSku        = new(3011, "InvalidSku",        "CAT3011");
    public static readonly CatalogErrorCode InsufficientStock = new(3012, "InsufficientStock", "CAT3012");
    public static readonly CatalogErrorCode InvalidQuantity   = new(3013, "InvalidQuantity",   "CAT3013");

    // Category
    public static readonly CatalogErrorCode CategoryNotFound         = new(3020, "CategoryNotFound",         "CAT3020");
    public static readonly CatalogErrorCode CategoryAlreadyExists    = new(3021, "CategoryAlreadyExists",    "CAT3021");
    public static readonly CatalogErrorCode InvalidCategoryHierarchy = new(3022, "InvalidCategoryHierarchy", "CAT3022");

    // Attribute
    public static readonly CatalogErrorCode AttributeNotFound      = new(3030, "AttributeNotFound",      "CAT3030");
    public static readonly CatalogErrorCode AttributeValueNotFound = new(3031, "AttributeValueNotFound", "CAT3031");
    public static readonly CatalogErrorCode InvalidAttributeType   = new(3032, "InvalidAttributeType",   "CAT3032");
    public static readonly CatalogErrorCode InvalidAttribute       = new(3033, "InvalidAttribute",       "CAT3033");

    // Value objects
    public static readonly CatalogErrorCode InvalidMoney = new(3040, "InvalidMoney", "CAT3040");
}
