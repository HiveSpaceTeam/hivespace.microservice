using HiveSpace.Core.Exceptions.Models;

namespace HiveSpace.Core.Exceptions;

/// <summary>
/// Exception thrown when a configuration value is missing or invalid.
/// Uses error codes only - no hardcoded messages for dual-language support.
/// </summary>
public class ConfigurationException : HiveSpaceException
{
    private const int HttpStatusCode = 500;

    public ConfigurationException(IEnumerable<Error> errorList, bool? enableData = false) 
        : base(errorList, HttpStatusCode, enableData)
    {
    }

    public ConfigurationException(IEnumerable<Error> errorList, Exception innerException, bool? enableData = false) 
        : base(errorList, innerException, HttpStatusCode, enableData)
    {
    }
}
