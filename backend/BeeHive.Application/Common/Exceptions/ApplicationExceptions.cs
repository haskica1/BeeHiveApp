namespace BeeHive.Application.Common.Exceptions;

/// <summary>Thrown when a requested resource cannot be found in the data store.</summary>
public class NotFoundException : Exception
{
    public NotFoundException(string name, object key)
        : base($"Entity '{name}' with key '{key}' was not found.") { }
}

/// <summary>Thrown when input fails business-rule validation.</summary>
public class ValidationException : Exception
{
    public IDictionary<string, string[]> Errors { get; }

    public ValidationException(IDictionary<string, string[]> errors)
        : base("One or more validation failures occurred.")
    {
        Errors = errors;
    }
}

/// <summary>Thrown when an operation is not allowed in the current state.</summary>
public class BusinessRuleException : Exception
{
    public BusinessRuleException(string message) : base(message) { }
}
