namespace Kompass.Application.B56Import;

public interface IB56BauteilcodeParser
{
    B56Bauteilcode Parsen(string text);
}
