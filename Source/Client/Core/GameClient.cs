using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace CelestialLeague.Client.Core
{
    public class GameClient : IDisposable
    {
        private TcpClient? _tcpClient;
        private NetworkStream? _networkStream;
        private readonly CancellationTokenSource _cancellationTokenSource = new();
        private Task? _receiveTask;
        private bool _isConnected;
        private bool _isDisposed;

        // private NetworkClient? _networkClient;
        // private ConnectionManger? _connectionManager;
    }
}