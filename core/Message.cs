using System;
using System.Threading.Channels;
namespace PortScanner.core;
public class Message
{
	private readonly Channel<string> _channel;
	private readonly ChannelWriter<string> _writer;
    private readonly ChannelReader<string> _reader;
    public Message()
	{
		_channel = Channel.CreateUnbounded<string>();
		_writer = _channel.Writer;
        _reader = _channel.Reader;
    }

	public async Task SendMessageAsync(string message)
	{
		await _writer.WriteAsync(message);
	}

    public async Task StartConsumingAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await foreach (var message in _reader.ReadAllAsync(cancellationToken))
            {
                Console.WriteLine($"Received message: {message}");
            }

        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Message consumption canceled.");
        }
        catch (ChannelClosedException)
        {
            Console.WriteLine("Channel closed.");
        }
    }


    public bool Complete() => _writer.TryComplete();
}
