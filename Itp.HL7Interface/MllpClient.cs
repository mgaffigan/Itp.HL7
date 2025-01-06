using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Itp.HL7Interface;

public static class MllpClient
{
    public static async Task<IMllpConnection> ConnectAsync(string serverName, int port, CancellationToken ct)
    {
        var client = new TcpClient();
        try
        {
            await client.ConnectAsync(serverName, port, ct);
            return new MllpConnection(client, client.GetStream());
        }
        catch
        {
            client.Dispose();
            throw;
        }
    }

    public static async Task<IMllpConnection> ConnectTlsAsync(string serverName, int port, CancellationToken ct)
    {
        var client = new TcpClient();
        try
        {
            await client.ConnectAsync(serverName, port, ct);
            var ssl = new SslStream(client.GetStream());
            await ssl.AuthenticateAsClientAsync(serverName, new X509CertificateCollection(),
                SslProtocols.Tls12 | SslProtocols.Tls13,
                false);
            return new MllpConnection(client, ssl);
        }
        catch
        {
            client.Dispose();
            throw;
        }
    }

    public static async Task<ReadOnlyMemory<byte>> TransceiveAsync(string serverName, int port, ReadOnlyMemory<byte> message, CancellationToken ct)
    {
        using var connection = await ConnectAsync(serverName, port, ct);
        return await connection.TransceiveAsync(message, ct);
    }

    public static async Task<string> TransceiveAsync(string serverName, int port, string message, CancellationToken ct)
    {
        using var connection = await ConnectAsync(serverName, port, ct);
        return await connection.TransceiveAsync(message, ct);
    }

    public static async Task SendAsync(string serverName, int port, ReadOnlyMemory<byte> message, CancellationToken ct)
    {
        using var connection = await ConnectAsync(serverName, port, ct);
        await connection.SendAsync(message, ct);
    }

    public static async Task SendAsync(string serverName, int port, string message, CancellationToken ct)
    {
        await SendAsync(serverName, port, Encoding.UTF8.GetBytes(message), ct);
    }
}