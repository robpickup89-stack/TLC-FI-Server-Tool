using System.Text;

namespace TLCFiTool.Transport;

public sealed class NdjsonFramer
{
    private readonly StringBuilder _buffer = new();

    public IEnumerable<string> Feed(ReadOnlySpan<char> chunk)
    {
        _buffer.Append(chunk);
        var lines = new List<string>();

        while (true)
        {
            var index = _buffer.ToString().IndexOf('\n');
            if (index < 0)
            {
                break;
            }

            var line = _buffer.ToString(0, index).TrimEnd('\r');
            _buffer.Remove(0, index + 1);
            if (!string.IsNullOrWhiteSpace(line))
            {
                lines.Add(line);
            }
        }

        return lines;
    }
}
