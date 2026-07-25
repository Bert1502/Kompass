namespace Kompass.Application.B56Import;

public sealed class B56SnapshotFormatException : Exception
{
    public B56SnapshotFormatException(
        Guid importId,
        string message,
        Exception? innerException = null)
        : base(message, innerException)
    {
        ImportId = importId;
    }

    public Guid ImportId { get; }
}
