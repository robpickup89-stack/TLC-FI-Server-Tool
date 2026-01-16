using System.Buffers;
using System.Text;

namespace TLCFI.Transport;

public sealed class NdjsonReader
{
    private readonly Decoder _decoder = Encoding.UTF8.GetDecoder();
    private readonly StringBuilder _buffer = new();

    public IEnumerable<string> ProcessBytes(ReadOnlySpan<byte> data)
    {
        var chars = ArrayPool<char>.Shared.Rent(data.Length);
        try
        {
            _decoder.Convert(data, chars, false, out var bytesUsed, out var charsUsed, out _);
            _buffer.Append(chars, 0, charsUsed);
        }
        finally
        {
            ArrayPool<char>.Shared.Return(chars);
        }

        return SplitLines();
    }

    public IEnumerable<string> Flush()
    {
        _decoder.Reset();
        return SplitLines(force: true);
    }

    private IEnumerable<string> SplitLines(bool force = false)
    {
        var output = new List<string>();
        var text = _buffer.ToString();
        var start = 0;
        for (var i = 0; i < text.Length; i++)
        {
            if (text[i] == '\n')
            {
                var length = i - start;
                if (length > 0 && text[start + length - 1] == '\r')
                {
                    length--;
                }

                if (length > 0)
                {
                    output.Add(text.Substring(start, length));
                }
                start = i + 1;
            }
        }

        if (force && start < text.Length)
        {
            var remaining = text.Substring(start).Trim();
            if (!string.IsNullOrEmpty(remaining))
            {
                output.Add(remaining);
            }
            start = text.Length;
        }

        _buffer.Clear();
        if (start < text.Length)
        {
            _buffer.Append(text.Substring(start));
        }

        return output;
    }
}
