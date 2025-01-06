using System.Net;
using System.Text;

namespace Itp.HL7Interface;

public interface IMllpConnection : IDisposable
{
    IPEndPoint LocalEndPoint { get; }
    IPEndPoint RemoteEndPoint { get; }

    Task<ReadOnlyMemory<byte>?> ReceiveAsync(CancellationToken ct);

    Task SendAsync(ReadOnlyMemory<byte> message, CancellationToken ct);
}

public static class MllpConnectionExtensions
{
    public static async Task<ReadOnlyMemory<byte>> TransceiveAsync(
        this IMllpConnection connection, ReadOnlyMemory<byte> message, CancellationToken ct)
    {
        await connection.SendAsync(message, ct);
        var response = await connection.ReceiveAsync(ct);
        return response ?? throw new IOException("Connection closed before response was received.");
    }

    public static async Task SendAsync(this IMllpConnection connection, string message, CancellationToken ct)
    {
        await connection.SendAsync(Encoding.UTF8.GetBytes(message), ct);
    }

    public static async Task<string?> ReceiveAsStringAsync(this IMllpConnection connection, CancellationToken ct)
    {
        var message = await connection.ReceiveAsync(ct);
        return message == null ? null : Encoding.UTF8.GetString(message.Value.Span).ReplaceLineEndings();
    }

    public static async Task<string> TransceiveAsync(this IMllpConnection connection, string message, CancellationToken ct)
    {
        await connection.SendAsync(message, ct);
        var response = await connection.ReceiveAsStringAsync(ct);
        return response ?? throw new IOException("Connection closed before response was received.");
    }
}
