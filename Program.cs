using System.Net;

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
        public class Card {
            public Suit suit;
            public Rank number;
            public Card(Suit suit, Rank number) {
                this.suit = suit;
                this.number = number;

            }
        }

        public class Player {
            public string username;
            public int money;
            public Player(string username, int money) {
                this.username = username;
                this.money = money;
            }
        }

        public static void Menu() {
            string username = String.Empty;
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
                    CreateLobby();
                    break;
                    case 3:
                    JoinLobby();
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

        private static void JoinLobby()
        {
            System.Console.Write("Enter host IP: ");
            IPAddress hostIp = IPAddress.Parse(Console.ReadLine());
            System.Console.WriteLine("Enter port: ");
            int port = int.Parse(Console.ReadLine());
        
        }

        private static void UserManagement()
        {
            throw new NotImplementedException();
        }

        private static void CreateLobby()
        {
            List<string> players = new List<string>();
            bool ready = false;
            
            int port = 3000;

            Server server = new Server(port);
            server.FetchIP();
            System.Console.WriteLine(server.myIp);

            server.startListening();
            System.Console.WriteLine("Server started!");

            Thread.Sleep(1000);
            System.Console.WriteLine("Waiting for connection...");

            while (!ready) {
                server.AcceptClient();

                string messageFromClient = "";

                try {
                    server.ClientData();

                    if (server.socketForClient.Connected) {
                        messageFromClient = server.streamReader.ReadLine();
                        System.Console.WriteLine(messageFromClient + " connected!");
                        players.Add(messageFromClient);
                        System.Console.WriteLine(players.Count);
                    }
                } catch {
                    System.Console.WriteLine("No client connected");
                }

                System.Console.Write("Ready to start? [y/N] ");
                ready = Console.ReadLine() == "y";     
            }

            StartGame(players);
                
        }

        private static void StartGame(List<string> players)
        {
            throw new NotImplementedException();
        }

        public static void Main() {
            Card card = new Card(Suit.Clubs, Rank.Five);
            
            Menu();   
        }
    }
}