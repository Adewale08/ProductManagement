namespace ProductManagement.Api.Exceptions;

public sealed class OrderNotFoundException(int id)
    : Exception($"Order {id} was not found.");
