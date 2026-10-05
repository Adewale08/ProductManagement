namespace ProductManagement.Api.Exceptions;

public sealed class CustomerNotFoundException(int id)
: Exception($"Customer {id} was not found.");
