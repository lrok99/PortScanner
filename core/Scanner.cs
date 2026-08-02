using System;
using System.Threading.Tasks;
using System.Threading.Channels;
using System.Net.Sockets;
using System.Threading;
namespace core
{
    public sealed class Scanner
    {
        private const int MaxPort = 65535;
        private const int MinPort = 1;
        private const int DefaultTimeout = 500; // in milliseconds
        private const int DefaultConcurrency = 300; // Default number of concurrent tasks
        private readonly string _host;
        private readonly int _startPort;
        private readonly int _endPort;
        private readonly int _timeout;

        public Scanner(string host, int startPort, int endPort, int timeout = DefaultTimeout)
        {
            _host = host;
            _startPort = startPort;
            _endPort = endPort;
            _timeout = timeout;
        }


        public async Task StartScanningAsync()
        {
            var _channel = Channel.CreateBounded<int>(new BoundedChannelOptions(DefaultConcurrency << 1));

            var readTask = new Task[DefaultConcurrency];
            for (int i = 0; i < DefaultConcurrency; i++)
            {
                readTask[i] = ReadFromChannelAsync(_channel.Reader);
            }
            var writeTask = WriteToChannelAsync(_channel.Writer);
            await Task.WhenAll(writeTask, Task.WhenAll(readTask));
        }

        private async Task WriteToChannelAsync(ChannelWriter<int> writer)
        {
            try
            {
                foreach (var port in Enumerable.Range(_startPort, _endPort - _startPort + 1))
                {
                    await writer.WriteAsync(port);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error writing to channel: {ex.Message}");
            }
            finally
            {
                writer.Complete();
            }
        }
        private async Task ReadFromChannelAsync(ChannelReader<int> reader)
        {
            try
            {
                await foreach (var port in reader.ReadAllAsync())
                {
                    await CheckPortAsync(port);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading from channel: {ex.Message}");
            }
        }
        private async Task CheckPortAsync(int port)
        {
            using var client = new TcpClient();
            using var cts = new CancellationTokenSource(_timeout);
            try
            {
                    
                await client.ConnectAsync(_host, port, cts.Token);
                Console.WriteLine($"Port {port} is open.");
            }
            catch(OperationCanceledException)
            {
                // Timeout occurred
            }
            catch(SocketException ex) when (ex.SocketErrorCode == SocketError.ConnectionRefused)
            {
                // Port is closed or unreachable
            }
            catch (SocketException ex) when (ex.SocketErrorCode == SocketError.TimedOut)
            {
                // Connection attempt timed out
            }
            catch (SocketException ex) when (ex.SocketErrorCode == SocketError.AccessDenied)
            {
                Console.WriteLine($"Access denied to port {port}: {_host}");
            }
            catch (SocketException ex)
            {
                // Handle other socket exceptions if needed
                Console.WriteLine($"Socket error on port {port}: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error checking port {port}: {ex.Message}");
            }
            
        }
    }

}

