namespace ProductManagement.Api.Exceptions;
public sealed class ProductNotFoundException(int id)
: Exception($"Product {id} was not found.");
