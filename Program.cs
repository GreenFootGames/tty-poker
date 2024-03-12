using System.Net;
using System.Net.Sockets;
using System.Text;

namespace TTY_POKER
{
    class Program {
        public enum Suit
        {
            Hearts,
            Diamonds,
            Clubs,
            Spades
        }

        public enum Rank
        {
            One = 1,
            Two = 2,
            Three = 3,
            Four = 4,
            Five = 5,
            Six = 6,
            Seven = 7,
            Eight = 8,
            Nine = 9,
            Ten = 10,
            Jack = 11,
            Queen = 12,
            King = 13,
            Ace
        }
        public class Card(Suit suit, Rank number)
        {
            public Suit suit = suit;
            public Rank number = number;
        }

        public class Player(string username, int money)
        {
            public string username = username;
            public int money = money;
        }

        public static void Menu() {
            string username = "bababooey";
            bool quit = false;
            while(!quit) {
                Console.WriteLine("Welcome to tty-poker! " + username);
                Console.WriteLine("1 - User management");
                Console.WriteLine("2 - Create lobby");
                Console.WriteLine("3 - Join lobby");
                Console.WriteLine("4 - Leaderboards");
                Console.WriteLine("5 - Quit");
                Console.Write(": ");
                int input = int.Parse(Console.ReadLine());
                Console.Clear();
                switch (input)
                {
                    case 1:
                    UserManagement();
                    break;
                    case 2:
                    CreateLobby(username);
                    break;
                    case 3:
                    JoinLobby(username);
                    break;
                    case 4:
                    Leaderboards();
                    break;
                    case 5:
                    quit = true;
                    break;
                }
            }
            
        }

        private static void Leaderboards()
        {
            throw new NotImplementedException();
        }

        private static void JoinLobby(string username)
        {
            Console.Write("Enter host IP: ");
            IPAddress hostIp = IPAddress.Parse(Console.ReadLine());
            Console.WriteLine("Enter port: ");
            int port = int.Parse(Console.ReadLine());
            Server client = new Server(port);
            string message = username + "-" + Server.FetchIP();
            client.WriteMessage(hostIp, message);
            
        }

        private static void UserManagement()
        {
            throw new NotImplementedException();
        }

        private static void CreateLobby(string username)
        {
            List<(string, IPAddress)> players = new List<(string, IPAddress)>();
            bool ready = false;
            Console.Write("Enter port: ");
            int port = int.Parse(Console.ReadLine());

            Server server = new Server(port);
            players.Add((username, server.myIp));
            Console.WriteLine("Your IP: " + server.myIp);

            server.startListening();
            System.Console.WriteLine("Server started!");

            Thread.Sleep(1000);
            string messageFromClient;

            while (!ready) {
                Console.WriteLine("Waiting for connection...");
                messageFromClient = server.ReadMessage();
                string[] splitMsg = messageFromClient.Split('-');
                Console.WriteLine(messageFromClient + " connected!");
                players.Add((splitMsg[0], IPAddress.Parse(splitMsg[1])));
                Console.WriteLine(players.Count + " players joined!");
                    
                

                Console.Write("Ready to start? [y/N] ");
                ready = Console.ReadLine() == "y";     
            }

            StartGame(players);
                
        }

        private static void StartGame(List<(string, IPAddress)> players)
        {
            throw new NotImplementedException();
        }

        public static void Main() {
            Card card = new Card(Suit.Clubs, Rank.Five);
            
            Menu();   
        }
    }
}