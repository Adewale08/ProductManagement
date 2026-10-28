namespace ProductManagement.Api.Exceptions;

public sealed class OrderConflictException(string message)
    : Exception(message);
