using System;
using System.Net;
using System.Net.Sockets;
using System.IO;
namespace TTY_POKER
{
    public class Server
    {
        public IPAddress myIp { get; private set; }
        public int port { get; private set; } 
        public bool serverStatus { get; set; }
        private TcpListener tcpListener { get; set; } 
        public Socket socketForClient {get; set;}
        public NetworkStream networkStream { get; set; } 
        public StreamReader streamReader { get; set; }
        public StreamWriter streamWriter { get; set; }
        public Server(int port) {
            myIp = IPAddress.Parse(FetchIP());
            this.port = port;
        }

        public static string FetchIP() {
            
            string localIP = "";
            foreach (IPAddress ip in Dns.GetHostAddresses(Dns.GetHostName()))
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    localIP = ip.ToString();
                    break;
                }
            }
            return localIP;
        }


        public void startListening()
        {
            try {
                tcpListener = new TcpListener(myIp, port);
                tcpListener.Start();
            } catch {
                Console.WriteLine("Could not start");
            }
        }

        public (string, IPAddress) ReadMessage() 
        {
            AcceptClient();

            string messageFromClient = "";
            IPAddress clientIp = new(0);

            try {
                ClientData();
                if (socketForClient.Connected) {
                    messageFromClient = streamReader.ReadLine();
                    clientIp = ((IPEndPoint)socketForClient.RemoteEndPoint).Address;
                    
                }
            } catch {
                Console.WriteLine("No client connected");
                throw;
            }
            Disconnect();
            return (messageFromClient, clientIp);
        }


        public void WriteMessage(IPAddress host, string message) 
        {
            TcpClient client = new TcpClient();
            client.Connect(host, port);
            
            try
            {
                networkStream = client.GetStream();
                
                streamWriter = new StreamWriter(networkStream);
                streamWriter.WriteLine(message);
                streamWriter.Flush();

                networkStream.Close();
                streamWriter.Close();
            }
            catch (Exception)
            {
                
                Console.WriteLine("Failed to join the lobby! :(");
                throw;
            }
        }

        public void AcceptClient() {
            try {
                socketForClient = tcpListener.AcceptSocket();
            } catch {
                Console.WriteLine("Could not accept client");
            }
        }

        public void ClientData() {
            // Connects server and client
            networkStream = new NetworkStream(socketForClient);
            // Ability to read from client
            streamReader = new StreamReader(networkStream);
            // Ability to write to client
            streamWriter = new StreamWriter(networkStream);
        }

        public void Disconnect() {
            networkStream.Close();
            streamReader.Close();
            streamWriter.Close();
            socketForClient.Close();
        }
    }
}