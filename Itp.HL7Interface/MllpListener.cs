using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;

namespace Itp.HL7Interface;

public delegate Task MllpConnectionHandler(IMllpConnection connection);

public class MllpListener : IDisposable
{
    private readonly TcpListener listener;
    private readonly ILogger Logger;
    private readonly MllpConnectionHandler Handler;
    private readonly X509Certificate2? Certificate;
    private readonly Task ListenerPromise;
    private bool isDisposed;

    public MllpListener(IPEndPoint endpoint, MllpConnectionHandler handler, X509Certificate2? certificate, ILogger log)
    {
        if (certificate is not null && !certificate.HasPrivateKey)
        {
            throw new ArgumentException("Certificate must have a private key", nameof(certificate));
        }

        this.Logger = log ?? throw new ArgumentNullException(nameof(log));
        this.Handler = handler ?? throw new ArgumentNullException(nameof(handler));
        this.Certificate = certificate;

        listener = new TcpListener(endpoint);
        listener.Start();

        ListenerPromise = Task.Run(ListenAsync);
    }

    public void Dispose()
    {
        if (isDisposed) return;
        isDisposed = true;

        GC.SuppressFinalize(this);
        listener.Stop();
        ListenerPromise.GetAwaiter().GetResult();
    }

    private async Task ListenAsync()
    {
        try
        {
            while (true)
            {
                var client = await listener.AcceptTcpClientAsync().ConfigureAwait(false);
                ThreadPool.QueueUserWorkItem(_ => RunConnection(client), null);
            }
        }
        catch (SocketException ex) when (ex.SocketErrorCode == SocketError.OperationAborted)
        {
            // no-op, cancelled
        }
        catch (SocketException ex) when (ex.SocketErrorCode == SocketError.Interrupted)
        {
            // no-op, cancelled
        }
        catch (OperationCanceledException)
        {
            // nop
        }
        catch (ObjectDisposedException)
        {
            // nop
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Exception occurred while listening for connection");
        }
    }

    private async Task<Stream> AuthenticateAsync(Stream tcp)
    {
        if (Certificate is null) return tcp;

        var ssl = new SslStream(tcp);
        await ssl.AuthenticateAsServerAsync(Certificate, false,
            SslProtocols.Tls12 | SslProtocols.Tls13, false);
        return ssl;
    }

    private async void RunConnection(TcpClient client)
    {
        try
        {
            using (client)
            {
                using var stream = await AuthenticateAsync(client.GetStream()).ConfigureAwait(false);
                using var connection = new MllpConnection(client, stream);
                await Handler(connection).ConfigureAwait(false);
            }
        }
        catch (OperationCanceledException)
        {
            // nop
        }
        catch (ObjectDisposedException)
        {
            // nop
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Exception occurred while handling a connection from {Endpoint}", client.Client.RemoteEndPoint?.ToString());
        }
    }
}