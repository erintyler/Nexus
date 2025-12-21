namespace Nexus.Domain.Common;

public enum ErrorType
{
    None,
    Validation,
    NotFound,
    Conflict,
    BusinessRule,
    Forbidden
}