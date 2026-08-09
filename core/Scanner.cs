using System;
using System.Threading.Tasks;
using System.Linq;
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
        private readonly Message _message;
        private long _finishedCount = 0;

        public Scanner(string host, int startPort, int endPort, int timeout = DefaultTimeout)
        {
            if (string.IsNullOrWhiteSpace(host)) throw new ArgumentException("host cannot be null or empty", nameof(host));
            if (startPort < MinPort || startPort > MaxPort) throw new ArgumentOutOfRangeException(nameof(startPort));
            if (endPort < MinPort || endPort > MaxPort) throw new ArgumentOutOfRangeException(nameof(endPort));
            if (startPort > endPort) throw new ArgumentException("startPort must be less than or equal to endPort");

            _host = host;
            _startPort = startPort;
            _endPort = endPort;
            _timeout = timeout;
            _message = new Message();
        }


        public async Task StartScanningAsync()
        {
            var options = new BoundedChannelOptions(DefaultConcurrency << 1)
            {
                SingleReader = false,
                SingleWriter = true,
                FullMode = BoundedChannelFullMode.Wait
            };
            var _channel = Channel.CreateBounded<int>(options);
            Console.CursorVisible = false;
            var consumingTask = _message.StartConsumingAsync();

            var readTasks = Enumerable.Range(0, DefaultConcurrency).Select(_ => ReadFromChannelAsync(_channel.Reader)).ToArray();
            var writeTask = WriteToChannelAsync(_channel.Writer);
            var showProcessTask = ShowProcessAsync(_channel.Reader);
            try
            {
                await Task.WhenAll(writeTask, Task.WhenAll(readTasks));
            }
            finally {
                _message.Complete();
            }
            //await consumingTask;

            await Task.WhenAll(consumingTask,showProcessTask);
            Console.CursorVisible= true;
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
                //Console.WriteLine($"Error writing to channel: {ex.Message}");
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
                    Interlocked.Increment(ref _finishedCount);
                }
            }
            catch (Exception ex)
            {
                //Console.WriteLine($"Error reading from channel: {ex.Message}");
            }
        }
        private async Task CheckPortAsync(int port)
        {
            using var client = new TcpClient();
            using var cts = new CancellationTokenSource(_timeout);
            try
            {
                    
                await client.ConnectAsync(_host, port, cts.Token);
                //await _message.SendMessageAsync($"Port {port} is open on {_host}");
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
                //Console.WriteLine($"Access denied to port {port}: {_host}");
            }
            catch (SocketException ex)
            {
                // Handle other socket exceptions if needed
                //Console.WriteLine($"Socket error on port {port}: {ex.Message}");
            }
            catch (Exception ex)
            {
                //Console.WriteLine($"Error checking port {port}: {ex.Message}");
            }
            
        }


        private async Task ShowProcessAsync(ChannelReader<int> reader)
        {
            int totalPorts = _endPort - _startPort + 1;
            const int barLength = 30;

            // Method to update the progress bar
            void UpdateProgress()
            {
                long current = Interlocked.Read(ref _finishedCount);
                double progress = (double)current / totalPorts * 100.0;
                int filled = (int)(progress / 100 * barLength);
                filled = Math.Clamp(filled, 0, barLength);
                string bar = new string('=', filled) + new string(' ', barLength - filled);
                string line = $"Progress: {progress:F2}% [{bar}]";
                // clear the current line and write the new progress
                Console.Write($"\r{line.PadRight(Console.WindowWidth - 1)}");
            }

            // Loop to update progress periodically
            while (!reader.Completion.IsCompleted || Interlocked.Read(ref _finishedCount) < totalPorts)
            {
                UpdateProgress();
                await Task.Delay(100);
            }

            // One final update to ensure 100% is shown
            UpdateProgress();
            Console.WriteLine(); 

        }
    }

}

