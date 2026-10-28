namespace ProductManagement.Api.Results;

public enum AuthFailureReason
{
    None,
    Validation,
    Conflict,
    InvalidCredentials
}

public class AuthServiceResult
{
    public bool Succeeded { get; init; }

    public string Message { get; init; } = string.Empty;

    public IEnumerable<string>? Errors { get; init; }

    public AuthFailureReason FailureReason { get; init; }
}