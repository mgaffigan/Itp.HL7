using Microsoft.Extensions.Logging;
using System.Buffers;
using System.IO.Pipelines;
using System.Net;
using System.Net.Sockets;

namespace Itp.HL7Interface;

internal sealed class MllpConnection : IMllpConnection
{
    private readonly TcpClient Client;
    private readonly Stream Stream;
    private readonly PipeReader Reader;
    private bool isDisposed;

    public MllpConnection(TcpClient client, Stream stream)
    {
        this.Client = client ?? throw new ArgumentNullException(nameof(client));
        this.Stream = stream ?? throw new ArgumentNullException(nameof(stream));

        this.Reader = PipeReader.Create(Stream);
    }

    public void Dispose()
    {
        if (isDisposed) return;
        isDisposed = true;

        GC.SuppressFinalize(this);
        Client.Dispose();
    }

    public IPEndPoint LocalEndPoint => (IPEndPoint)(Client.Client.LocalEndPoint ?? throw new InvalidOperationException("Local endpoint is not available."));

    public IPEndPoint RemoteEndPoint => (IPEndPoint)(Client.Client.RemoteEndPoint ?? throw new InvalidOperationException("Remote endpoint is not available."));

    public async Task<ReadOnlyMemory<byte>?> ReceiveAsync(CancellationToken ct)
    {
        ObjectDisposedException.ThrowIf(isDisposed, this);

        // Separated by \x1c, optional \r trailer, optional \v header
        // [\v] {message} \x1c [\r]
        //
        // use Reader to find MllpConstants.MessageSeparator
        while (true)
        {
            ct.ThrowIfCancellationRequested();

            ReadResult result;
            try
            {
                result = await Reader.ReadAsync(ct);
            }
            catch (SocketException ex)
                when (ex.SocketErrorCode == SocketError.OperationAborted
                    || ex.SocketErrorCode == SocketError.Interrupted
                    || ex.SocketErrorCode == SocketError.ConnectionAborted)
            {
                return null;
            }
            catch (OperationCanceledException)
            {
                throw;
            }

            if (result.IsCanceled) throw new OperationCanceledException();
            var buffer = result.Buffer;
            if (buffer.Length == 0 && result.IsCompleted) return null;

            // trim any \r or \v bytes
            if (TrimStart(ref buffer))
            {
                continue;
            }

            var iSeparator = buffer.PositionOf(MllpConstants.MessageSeparator);
            if (iSeparator == null)
            {
                if (buffer.Length > MllpConstants.MaxMessageSize)
                {
                    throw new InvalidOperationException($"Message too long: {buffer.Length} bytes");
                }

                // Need more data
                Reader.AdvanceTo(buffer.Start, buffer.End);
                continue;
            }

            // message is inclusive of the ETX byte
            var endOfMessage = buffer.GetPosition(1, iSeparator.Value);

            // copy the message to a new buffer
            // trim the SOM and EOMEB bytes
            var message = buffer.Slice(0, endOfMessage).ToArray();

            // advance the reader
            Reader.AdvanceTo(endOfMessage);

            return message;
        }
    }

    private bool TrimStart(ref ReadOnlySequence<byte> buffer)
    {
        if (buffer.Length == 0) return false;

        var first = buffer.First.Span;
        var i = 0;
        for (; i < first.Length && (first[i] == MllpConstants.StartOfMessageByte || first[i] == MllpConstants.EndOfMessageExtraByte); i++)
        {
            // nop
        }
        if (i > 0)
        {
            Reader.AdvanceTo(buffer.GetPosition(i, buffer.Start));
            return true;
        }
        return false;
    }

    public async Task SendAsync(ReadOnlyMemory<byte> message, CancellationToken ct)
    {
        ObjectDisposedException.ThrowIf(isDisposed, this);
        if (!MllpConstants.IsValidMessage(message.Span))
        {
            throw new ArgumentException("Messages containing \\x1C cannot be sent in MLLP", nameof(message));
        }

        var temp = new byte[message.Length + 3];
        temp[0] = MllpConstants.StartOfMessageByte;
        message.Span.CopyTo(temp.AsSpan(1, message.Length));
        temp[temp.Length - 2] = MllpConstants.MessageSeparator;
        temp[temp.Length - 1] = MllpConstants.EndOfMessageExtraByte;

        await Stream.WriteAsync(temp, ct);
    }
}