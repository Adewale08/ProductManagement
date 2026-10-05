namespace ProductManagement.Api.Exceptions;
public sealed class DuplicateSkuException(string sku)
: Exception($"A product with SKU '{sku}' already exists.");