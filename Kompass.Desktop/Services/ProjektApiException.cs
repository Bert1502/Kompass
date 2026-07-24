namespace Kompass.Desktop.Services;

public sealed class ProjektApiException : Exception
{
    public ProjektApiException(
        string message)
        : base(message)
    {
    }

    public ProjektApiException(
        string message,
        Exception innerException)
        : base(message, innerException)
    {
    }
}
