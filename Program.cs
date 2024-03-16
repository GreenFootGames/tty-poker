using System.Net;
using System.Net.Sockets;
using System.Text;

namespace TTY_POKER
{
    class Program {

        public static readonly IPAddress SERVER_IP = IPAddress.Parse("");
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
            Console.Write("Enter port: ");
            int port = int.Parse(Console.ReadLine());
            Server client = new Server(port);
            client.WriteMessage(hostIp, username);
            client.startListening();
            Console.WriteLine("Waiting for host to start...");
            string startMessage = client.ReadMessage().Item1;
            if (startMessage == "start")
            {
                ClientPlay(username);
            } else {
                Console.WriteLine("Host encountered an error! :(");
            }
            
        }

        private static void ClientPlay(string username)
        {
            throw new NotImplementedException();
        }

        private static string UserManagement()
        {
            string username = String.Empty;
            
            int input = int.Parse(Console.ReadLine());
            while (username == String.Empty) {
                Console.WriteLine("What do you wish to do?");
                Console.WriteLine("1 - Login");
                Console.WriteLine("2 - Register");
                Console.Write(": ");
                switch (input) {
                case 1:
                Login();
                break;

                case 2:
                Register();
                break;
                }
            }
            
            return username;
        }

        private static void Register()
        {
            bool uniqueUsername = false;
            Server client = new Server(3000);
            Console.Title = "TTY-POKER - Registration";
            string username, password;
            do
            {
                Console.Write("Username: ");
                username = Console.ReadLine();
                client.WriteMessage(SERVER_IP, "check-" + username);
                client.startListening();
                Console.WriteLine("Checking username...");
                if (client.ReadMessage().Item1 == "y") {
                    uniqueUsername = true;
                } else {
                    Console.WriteLine("The username is already taken! :(");
                }
            } while (!uniqueUsername);
            bool match = false;
            do
            {
                Console.Write("Password: ");
                password = Console.ReadLine();
                Console.Write("Confirm password: ");
                if (Console.ReadLine() == password) {
                    match = true;
                }  else {
                    Console.WriteLine("The two passwords don't match! :(");
                }
            } while (!match);
            
            client.WriteMessage(SERVER_IP, "new-" + username + "-" + password);
            client.startListening();
            
            Console.WriteLine(client.ReadMessage());

        }

        private static void Login()
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
            IPAddress clientIp;

            while (!ready) {
                Console.WriteLine("Waiting for connection...");
                (messageFromClient, clientIp) = server.ReadMessage();
                
                Console.WriteLine(messageFromClient + " connected!");
                players.Add((messageFromClient, clientIp));
                Console.WriteLine(players.Count + " players joined!");
                    
                

                Console.Write("Ready to start? [y/N] ");
                ready = Console.ReadLine() == "y";     
            }

            StartGame(players, server);
                
        }

        private static void StartGame(List<(string, IPAddress)> players, Server host)
        {
            for (int i = 1; i < players.Count; i++) {
                host.WriteMessage(players[i].Item2, "start");
            }
        }

        public static void Main() {
            Card card = new Card(Suit.Clubs, Rank.Five);
            
            Menu();   
        }
    }
}