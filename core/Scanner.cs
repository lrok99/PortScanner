using System;
using System.Threading.Tasks;
using System.Linq;
using System.Threading.Channels;
using System.Net.Sockets;
using System.Threading;
using System.Collections.Concurrent;
namespace PortScanner.core
{
    public sealed class Scanner
    {
        private const int MaxPort = 65535;
        private const int MinPort = 1;
        private const int DefaultTimeout = 500; // in milliseconds
        private const int DefaultConcurrency = 200; // Default number of concurrent tasks
        private readonly string _host;
        private readonly int _startPort;
        private readonly int _endPort;
        private readonly int _timeout;
        private readonly int _concurrency;
        private readonly string? _outputFile;
        private readonly Message _message;
        private long _finishedCount = 0;
        private ConcurrentQueue<string> _messages = new ConcurrentQueue<string>();

        public Scanner(ScannerOption option)
        {
            if (string.IsNullOrWhiteSpace(option.Host)) throw new ArgumentException("host cannot be null or empty", nameof(option.Host));
            if (option.StartPort < MinPort || option.StartPort > MaxPort) throw new ArgumentOutOfRangeException(nameof(option.StartPort));
            if (option.EndPort < MinPort || option.EndPort > MaxPort) throw new ArgumentOutOfRangeException(nameof(option.EndPort));
            if (option.StartPort > option.EndPort) throw new ArgumentException("startPort must be less than or equal to endPort");
            

            _host = option.Host;
            _startPort = option.StartPort;
            _endPort = option.EndPort;
            _timeout = option.Timeout;
            _message = new Message();
            _concurrency = option.Concurrency;
            _outputFile = option.Output;
        }


        public async Task StartScanningAsync(CancellationToken cancellationToken = default)
        {
            var options = new BoundedChannelOptions(_concurrency << 1)
            {
                SingleReader = false,
                SingleWriter = true,
                FullMode = BoundedChannelFullMode.Wait
            };
            var _channel = Channel.CreateBounded<int>(options);
            Console.CursorVisible = false;
            var consumingTask = _message.StartConsumingAsync(cancellationToken);

            var readTasks = Enumerable.Range(0, _concurrency).Select(_ => ReadFromChannelAsync(_channel.Reader, cancellationToken)).ToArray();
            var writeTask = WriteToChannelAsync(_channel.Writer, cancellationToken);
            var showProcessTask = ShowProcessAsync(_channel.Reader, cancellationToken);
            try
            {
                await Task.WhenAll(writeTask, Task.WhenAll(readTasks));
            }
            finally
            {
                _message.Complete();
                try
                {
                    await consumingTask;
                    //output file
                }
                catch (OperationCanceledException) { }
                catch { }
                Console.CursorVisible = true;
            }
            await showProcessTask;
            while(_messages.TryDequeue(out var message))
            {
                Console.WriteLine(message);
            }
        }

        private async Task WriteToChannelAsync(ChannelWriter<int> writer, CancellationToken cancellationToken = default)
        {
            try
            {
                foreach (var port in Enumerable.Range(_startPort, _endPort - _startPort + 1))
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    await writer.WriteAsync(port, cancellationToken);
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
        private async Task ReadFromChannelAsync(ChannelReader<int> reader, CancellationToken cancellationToken = default)
        {
            try
            {
                await foreach (var port in reader.ReadAllAsync(cancellationToken))
                {
                    await CheckPortAsync(port, cancellationToken);
                    Interlocked.Increment(ref _finishedCount);
                }
            }
            catch (Exception ex)
            {
                //Console.WriteLine($"Error reading from channel: {ex.Message}");
            }
        }
        private async Task CheckPortAsync(int port, CancellationToken cancellationToken = default)
        {
            using var client = new TcpClient();
            using var timeoutCts = new CancellationTokenSource(_timeout);
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);
            try
            {
                await client.ConnectAsync(_host, port, linkedCts.Token);
                _messages.Enqueue($"Port {port} is open on {_host}");
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


        private async Task ShowProcessAsync(ChannelReader<int> reader, CancellationToken cancellationToken = default)
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
            while ((!reader.Completion.IsCompleted || Interlocked.Read(ref _finishedCount) < totalPorts) && !cancellationToken.IsCancellationRequested)
            {
                UpdateProgress();
                try { await Task.Delay(100, cancellationToken); } catch (OperationCanceledException) { break; }
            }

            // One final update to ensure 100% is shown
            UpdateProgress();
            Console.WriteLine(); 

        }
    }

}

