namespace ProductManagement.Api.Exceptions;

public sealed class ValidationException(string message)
: Exception(message);