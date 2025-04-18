using System.Buffers;
using System.Globalization;
using System.Text;

namespace Swashbuckle.AspNetCore.Cli;

internal class VerifyingWriter(Stream stream) : TextWriter
{
    private readonly StreamReader _reader = new(stream);

    public override Encoding Encoding => _reader.CurrentEncoding;

    public override IFormatProvider FormatProvider => CultureInfo.InvariantCulture;

    public override void Write(char value)
    {
        var readChar = _reader.Read();

        if (readChar != value)
        {
            throw new VerificationException();
        }
    }

    public override void Write(char[] buffer, int index, int count)
    {
        var array = ArrayPool<char>.Shared.Rent(count);

        try
        {
            var bytesRead = _reader.Read(array, 0, count);

            if (bytesRead != count || !buffer.AsSpan(index, count).SequenceEqual(array.AsSpan(0, count)))
            {
                throw new VerificationException();
            }
        }
        finally
        {
            ArrayPool<char>.Shared.Return(array);
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (MoreBytesToRead())
        {
            throw new VerificationException();
        }

        base.Dispose(disposing);
    }

    private bool MoreBytesToRead()
    {
        return _reader.Read() != -1;
    }
}
