using System;
using System.Threading.Channels;
namespace core;
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
		var message = await _reader.ReadAsync();
		Console.WriteLine(message);
	}

	public bool Complete() => _writer.TryComplete();
}
