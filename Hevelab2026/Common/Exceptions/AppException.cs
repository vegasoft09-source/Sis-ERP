namespace Hevelab2026.Common.Exceptions;

public class AppException : Exception
{
    public int StatusCode { get; }

    public AppException(string message, int statusCode = 400) : base(message)
    {
        StatusCode = statusCode;
    }
}

public class NotFoundException : AppException
{
    public NotFoundException(string message) : base(message, 404) { }
}

public class UnauthorizedException : AppException
{
    public UnauthorizedException(string message = "No autorizado") : base(message, 401) { }
}

public class ForbiddenException : AppException
{
    public ForbiddenException(string message = "Acceso denegado") : base(message, 403) { }
}

public class ConflictException : AppException
{
    public ConflictException(string message) : base(message, 409) { }
}

public class ValidationException : AppException
{
    public IDictionary<string, string[]> Errors { get; }

    public ValidationException(IDictionary<string, string[]> errors)
        : base("Error de validación", 422)
    {
        Errors = errors;
    }
}
