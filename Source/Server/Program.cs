using System.Net;
using CelestialLeague.Server.Core;
using CelestialLeague.Server.Utils;

namespace CelestialLeague.Server
{
    internal class Program
    {
        private static GameServer? _gameServer;
        private static readonly CancellationTokenSource _cancellationTokenSource = new();

        static async Task Main(string[] args)
        {
            Console.CancelKeyPress += OnCancelKeyPress;
            
            try
            {
                var ipAddress = ParseIPAddress(args);
                var port = ParsePort(args);
                
                var logger = new Logger();
                
                _gameServer = new GameServer(ipAddress, port, logger);
                
                logger.Info("Starting Celestial League Server...");
                
                await _gameServer.StartAsync();
                
                logger.Info("Server is running. Press Ctrl+C to stop.");
                await WaitForCancellation(_cancellationTokenSource.Token);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fatal error: {ex.Message}");
                Environment.Exit(1);
            }
            finally
            {
                await Cleanup();
            }
        }
        
        private static IPAddress ParseIPAddress(string[] args)
        {
            for (int i = 0; i < args.Length - 1; i++)
            {
                if (args[i] == "--ip" || args[i] == "-i")
                {
                    if (IPAddress.TryParse(args[i + 1], out var ip))
                        return ip;
                }
            }
            
            return IPAddress.Loopback;
        }
        
        private static int ParsePort(string[] args)
        {
            for (int i = 0; i < args.Length - 1; i++)
            {
                if (args[i] == "--port" || args[i] == "-p")
                {
                    if (int.TryParse(args[i + 1], out var port) && port > 0 && port <= 65535)
                        return port;
                }
            }
            
            return 7777;
        }
        
        private static async Task WaitForCancellation(CancellationToken cancellationToken)
        {
            try
            {
                await Task.Delay(Timeout.Infinite, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                // expected when cancellation
            }
        }
        
        private static void OnCancelKeyPress(object? sender, ConsoleCancelEventArgs e)
        {
            e.Cancel = true; // prevent immediate termination
            Console.WriteLine("\nShutdown requested...");
            _cancellationTokenSource.Cancel();
        }
        
        private static async Task Cleanup()
        {
            if (_gameServer != null)
            {
                Console.WriteLine("Stopping server...");
                await _gameServer.StopAsync();
                _gameServer.Dispose();
                Console.WriteLine("Server stopped.");
            }
            
            _cancellationTokenSource.Dispose();
        }
    }
}
