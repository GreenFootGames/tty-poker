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
            FetchIP();
            this.port = port;
        }

        public void FetchIP() {
            using (WebClient client = new WebClient())
        {
            try
            {
                string ipString = client.DownloadString("https://api.ipify.org");
                myIp = IPAddress.Parse(ipString);
                // Display your public IP address
                //Console.WriteLine("Your public IP address is: " + ipString);
            }
            catch (WebException ex)
            {
                Console.WriteLine("Failed to retrieve public IP address. Exception: " + ex.Message);
            }
        }
        }


        public void startListening()
        {
            try {
                tcpListener = new TcpListener(myIp, port);
                tcpListener.Start();
            } catch {
                System.Console.WriteLine("Could not start");
            }
        }

        public void AcceptClient() {
            try {
                socketForClient = tcpListener.AcceptSocket();
            } catch {
                System.Console.WriteLine("Could not accept client");
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