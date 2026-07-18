namespace NovaPOS.Core.Exceptions;

public class LicenseRestrictionException : Exception
{
    public LicenseRestrictionException(string message) : base(message)
    {
    }

    public LicenseRestrictionException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
