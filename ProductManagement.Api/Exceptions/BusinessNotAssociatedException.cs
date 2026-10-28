namespace ProductManagement.Api.Exceptions;

public sealed class BusinessNotAssociatedException()
    : Exception("No business is associated with the current user.");
