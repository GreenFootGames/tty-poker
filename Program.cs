using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Channels;

namespace TTY_POKER_CLIENT
{
    class Program
    {
        static IPAddress serverIp = IPAddress.Parse("127.0.0.1");
        const int port = 12345;
        static string Username = "<user>";
        static long Money = 0;

        class Card(int rank, int suit)
        {
            public int Rank { get; set; } = rank;
            public int Suit { get; set; } = suit;

            public override string ToString()
            {
                return Suit switch
                {
                    1 => "|" + Rank + "\u2665" + "|",
                    2 => "|" + Rank + "\u2666" + "|",
                    3 => "|" + Rank + "\u2663" + "|",
                    4 => "|" + Rank + "\u2660" + "|",
                    _ => Rank + " of " + Suit,
                };
            }

            
        }

        class Hand(List<Program.Card> hand)
        {
            private readonly int[] rankOrder = [2,3,4,5,6,7,8,9,10,11,12,13,14,2,3,4,5];
            public List<Card> hand { get; set; } = hand;

            public int HandChecker(int playerCardCount) {
            /* COMBO POINT CHART
            *  3000  Royal Flush
            *  2500  (FlushCheck() + 1500)  Straight Flush
            *  2000  (ComboCheck(4) + 1700)  Four of a Kind
            *  1500  (ComboCheck(2) + ComboCheck(3) + 1200)  Full House // FullHouseCheck() also checks PairCheck() and DrillCheck()
            *  1000  Flush
            *  500   Straight
            *  200   Three of a Kind + 15 for any additional drill + the drill ranks
            *  100   One Pair + 15 for any additional pair + the pair ranks
            *  2-14  High Card
            */
            // COMBO + HIGHEST CARD IN COMBO, 100 DIFF BETWEEN COMBOS TO PREVENT POINT OVERLAP 
                return 
                RoyalFlushCheck() + 
                StraightCheck() + 
                PokerCheck() + 
                FullHouseCheck() + 
                FlushCheck() + 
                HighCardCheck(playerCardCount);
            }


            private int RoyalFlushCheck() {
                List<Card> potentialCards = [];
                for (int i = 10; i < 15; i++)
                {
                    if (hand.Any(x => x.Rank == i)) {
                        potentialCards.Add(hand.First(x => x.Rank == i)); 
                    }
                }
                if (potentialCards.Count == 5 && potentialCards.All(x => x.Suit == potentialCards[0].Suit)) {
                    return 3000 + potentialCards.Max(x => x.Rank);

                }
                return 0;
            }
            

            private int PokerCheck() {
                int pokerPoints = ComboCheck(4);
                if (pokerPoints > 0) {
                    return pokerPoints + 1700;
                }
                return 0;
            }


            private int FullHouseCheck() {
                int pairAndDrillPoints = ComboCheck(2) + ComboCheck(3);
                if(pairAndDrillPoints > 300) {
                    return 1250 + pairAndDrillPoints;
                } else {
                    return pairAndDrillPoints;
                }
                
            }


            // Checks Both Straights and Straight Flushes
            private int StraightCheck() {
                List<Card> potentialCards = [];
                bool found = false;
                for (int i = 0; i < rankOrder.Length && !found; i++)
                {
                    if (hand.Any(x => x.Rank == rankOrder[i])) {
                        potentialCards.Add(hand.First(x => x.Rank == rankOrder[i]));
                    } else {
                        if (potentialCards.Count >= 5) {
                            found = true;
                        } else {
                            potentialCards.Clear();
                        }
                    }
                }
                if (potentialCards.Count >= 5) {
                    int strFlushCheckResult = StraightFlushLoopCheck(potentialCards);
                    //System.Console.WriteLine(strFlushCheckResult);
                    if (strFlushCheckResult > 0) {
                        return 1500 + strFlushCheckResult;
                    }
                    return 500 + potentialCards.Max(x => x.Rank);
                }
                return 0;
            }


            // Flush Confirmation for Straight Flush
            private int StraightFlushLoopCheck(List<Card> potentialCards) {
                int curSuit = potentialCards[0].Suit;
                int suitCombo = 1;
                for (int i = 1; i < potentialCards.Count; i++)
                {
                    if (curSuit == potentialCards[i].Suit) {
                        suitCombo++;
                    } else {
                        if (suitCombo >= 5) {
                            return potentialCards[i-1].Rank;
                        }
                        suitCombo = 1;
                        curSuit = potentialCards[i].Suit;
                    }
                }
                if (suitCombo >= 5) {
                    return potentialCards[^1].Rank;
                }
                return -1;
            }


            private int FlushCheck() {
                List<Card> potentialCards = [];
                for (int i = 1; i < 5; i++) {
                    potentialCards = hand.Where(x => x.Suit == i).ToList();
                    if(potentialCards.Count >= 5) {
                        return 1000 + potentialCards.Max(x => x.Rank);
                    }
                }
                return 0;
            }
            

            // Pair, Drill and Poker Check possible in the same function using the 'combo' parameter
            private int ComboCheck(int combo) {
                List<Card> potentialCards = [];
                List<int> comboRanks = [];
                int i = 0;
                //int pairCount = 0; -- replaced by pairRanks
                while (i < hand.Count)
                {
                    
                    if (!comboRanks.Contains(hand[i].Rank) && hand.Count(x => x.Rank == hand[i].Rank) >= combo) {
                        potentialCards = hand.Where(x => x.Rank == hand[i].Rank).ToList();
                        
                        if (potentialCards.Select(x => x.Suit).Distinct().Count() == combo) {
                            //potentialCards.ForEach(System.Console.WriteLine);
                            comboRanks.Add(potentialCards[0].Rank);
                        }
                        
                    }
                    i++;        
                }
                
                switch (comboRanks.Count)
                {
                    case 1:
                        return (combo - 1) * 100 + comboRanks.Max();
                    case int n when n >= 2:
                        int points = (combo - 1) * 100;
                        foreach (int rank in comboRanks)
                        {
                            points += rank;
                        }
                        return points + (comboRanks.Count - 1) * 15;
                    default:
                        return 0;
                    
                }
            }


            private int HighCardCheck(int range) {
                return hand.GetRange(0, range).Max(x => x.Rank);
            }


            public override string ToString()
            {
                string handToStr = "";
                foreach (Card card in hand)
                {
                    handToStr += card.ToString() + " ";
                }
                return handToStr.TrimEnd();
            }

        }
        static async Task Main(string[] args)
        {
            
            await MainMenu();
            //PlayMatch(2, 3);
            /* HAND CHECKER DEBUGGING
            Card card1 = new Card(1, 2);
            Card card2 = new Card(2, 3);
            Card card3 = new Card(5, 4);
            Card card4 = new Card(4, 1);
            Card card5 = new Card(3, 2);
            Card card6 = new Card(6, 4);
            Card card7 = new Card(7, 3);
            Card card8 = new Card(11, 4);
            Hand hand = new Hand(new List<Card>() {card1, card2, card3, card4, card5, card6, card7, card8});
            System.Console.WriteLine(hand);
            System.Console.WriteLine(hand.HandChecker());
            */
        }


        static async Task MainMenu() {
            bool playing = true;
            while (playing) {
                if (Username == "<user>") {
                    Console.Clear();
                    Console.WriteLine("Welcome to tty-poker!");
                    Console.WriteLine("1 - Login");
                    Console.WriteLine("2 - Registration");
                    //Console.WriteLine("3 - Continue as Guest");
                    Console.WriteLine("3 - Quit");
                    int input = 1;
                    Console.Write(":" + input);
                    Console.SetCursorPosition(Console.CursorLeft - input.ToString().Length, Console.CursorTop);
                    ConsoleKeyInfo keyInfo;

                    // Continuously read each key press
                    while ((keyInfo = Console.ReadKey(true)).Key != ConsoleKey.Enter)
                    {   
                        if (keyInfo.Key == ConsoleKey.UpArrow && input < 3) {
                            input++;
                        }
                        if (keyInfo.Key == ConsoleKey.DownArrow && input > 1) {
                            input--;
                        }
                        // Display the key pressed

                        Console.Write(input);
                        Console.SetCursorPosition(Console.CursorLeft - input.ToString().Length, Console.CursorTop);
                    }
                    Console.Clear();
                    switch (input)
                    {
                        case 1:
                            Login();
                            break;
                        case 2:
                            Registration();
                            break;
                        case 3:
                            //GenerateGuest();
                            Console.Clear();
                            playing = false;
                            break;
                        /*
                        case 4:

                            break;
                        */
                    }
                } else {
                    Console.Clear();
                    Console.WriteLine($"Welcome to tty-poker, {Username}!\tMoney: {Money}$");
                    Console.WriteLine("1 - Join a match");
                    Console.WriteLine("2 - Host a match");
                    Console.WriteLine("3 - Logout");
                    Console.WriteLine("4 - Quit");

                    int input = 1;
                    Console.Write(":" + input);
                    Console.SetCursorPosition(Console.CursorLeft - input.ToString().Length, Console.CursorTop);
                    ConsoleKeyInfo keyInfo;

                    // Continuously read each key press
                    while ((keyInfo = Console.ReadKey(true)).Key != ConsoleKey.Enter)
                    {   
                        if (keyInfo.Key == ConsoleKey.UpArrow && input < 4) {
                            input++;
                        }
                        if (keyInfo.Key == ConsoleKey.DownArrow && input > 1) {
                            input--;
                        }
                        // Display the key pressed

                        Console.Write(input);
                        Console.SetCursorPosition(Console.CursorLeft - input.ToString().Length, Console.CursorTop);
                    }
                    Console.Clear();
                    switch (input)
                    {
                        case 1:
                            Join();
                            break;
                        case 2:
                            //throw new Exception();
                            await Host();
                            break;
                        case 3:
                            Logout();
                            break;
                        
                        case 4:
                            Console.Clear();
                            playing = false;
                            break;
                        
                    }
                }
                
            
            }
        }

        private static void Logout()
        {
            Username = "<user>";
            Money = 0;
        }

        private static void Join()
        {
            TcpClient client = new TcpClient();
            Console.Write("Enter lobby code: ");
            string code = Console.ReadLine()!;
            client.Connect(serverIp, port);
            NetworkStream stream = client.GetStream();
            byte[] message = Encoding.UTF8.GetBytes(code);
            stream.Write(message);


        }

        private static async Task Host()
        {
            TcpClient client = new TcpClient();
            Console.WriteLine("Creating lobby...");
            client.Connect(serverIp, port);
            NetworkStream stream = client.GetStream();
            byte[] message = Encoding.UTF8.GetBytes($"3-{Username}");
            stream.Write(message, 0, message.Length);
            byte[] response = new byte[1024];
            stream.Read(response);
            TrimTrailingNullBytes(response);
            string code = Encoding.UTF8.GetString(response);
            Console.Clear();
            System.Console.WriteLine("Lobby code: " + code);
            List<(string, long)> players = new List<(string, long)>() { (Username, Money) };
            int playerCount = players.Count;
            bool first = true;

            using (CancellationTokenSource cts = new CancellationTokenSource())
            {
                Task keyListenerTask = ListenForEscapeKeyAsync(cts);
                Task serverListenerTask = ListenForPlayerUpdates(cts, stream, players);


                // Main loop that does some work
                while (!cts.Token.IsCancellationRequested)
                {
                    //System.Console.Write("1\t");
                    if (playerCount < players.Count || first) {
                        first = false;
                        playerCount = players.Count;
                        Console.SetCursorPosition(0, 1);
                        Console.Write(new string(' ', Console.WindowWidth));
                        Console.SetCursorPosition(0, 1);
                        for (int i = 0; i < players.Count; i++)
                        {
                            System.Console.Write(i + 1 + "-" + players[i].Item1);
                        }
                        Console.Write("\nReady? (Press Enter): ");
                    }
                     // Simulate work with a delay
                    await Task.Delay(100);
                }

                // Wait for the key listener to complete
                await keyListenerTask;
                await serverListenerTask; 
            }
            Console.WriteLine("\nStarting Game.......");
            await Task.Delay(300);
            // TODO: Implement Match
            //PlayMatch(players.Count, 3);

            
        }

        private static async Task ListenForPlayerUpdates(CancellationTokenSource cts, NetworkStream stream, List<(string, long)> players)
        {
            await Task.Run(async () => {
                while(!cts.IsCancellationRequested) {
                    byte[] msgBytes = new byte[1024];
                    stream.Read(msgBytes);
                    msgBytes = TrimTrailingNullBytes(msgBytes);
                    foreach (var item in msgBytes)
                    {
                        System.Console.Write(item + "\t");
                    }
                    if (msgBytes.Length > 0) {
                        string[] data = Encoding.UTF8.GetString(msgBytes).Split('-');
                        players.Add((data[0], long.Parse(data[1])));
                        System.Console.WriteLine("mesg: ");
                    }
                }
            
            
            });
        }

        private static async Task ListenForEscapeKeyAsync(CancellationTokenSource cts)
        {
            await Task.Run(() =>
            {
                while (!cts.Token.IsCancellationRequested)
                {
                    if (Console.KeyAvailable && Console.ReadKey(true).Key == ConsoleKey.Enter)
                    {
                        //Console.WriteLine("Escape key pressed! Stopping the loop...");
                        cts.Cancel();
                    }
                }
            });
        }

        private static void Login()
        {
            byte[] response = new byte[1024];
            string username = "VALUE";
            string password = "VALUE";
            bool invalidCombo;
            do
            {
                TcpClient client = new TcpClient();
                Console.WriteLine("--- LOGIN ---");
                bool validInputs = false;
                while (!validInputs) { 
                    Console.SetCursorPosition(0, 1);
                    Console.Write(new string(' ', Console.WindowWidth));
                    Console.SetCursorPosition(0, 1);
                    Console.Write("Username: ");
                    username = Console.ReadLine()!;
                    if (username.Length > 0) {
                        validInputs = true;
                    }
                }
                validInputs = false;
                while (!validInputs) { 
                    Console.SetCursorPosition(0, 2);
                    Console.Write(new string(' ', Console.WindowWidth));
                    Console.SetCursorPosition(0, 2);
                    Console.Write("Password: ");
                    password = Console.ReadLine()!;
                    if (password.Length > 0) {
                        validInputs = true;
                    }
                }

                Console.WriteLine("Verifying login...");
                client.Connect(serverIp, port);
                NetworkStream stream = client.GetStream();
                byte[] message = Encoding.UTF8.GetBytes($"1-{username}-{password}");
                stream.Write(message, 0, message.Length);
                //byte[] response = new byte[1];
                stream.Read(response);

                if (response[0] != 0) {
                    invalidCombo = false;
                } else {
                    invalidCombo = true;
                    Console.Clear();
                }
                client.Close();
            } while (invalidCombo);
            Username = username;
            Money = long.Parse(Encoding.UTF8.GetString(response));
        }


        private static void Registration()
        {
            string username = "VALUE";
            string password = "VALUE";
            bool takenUsername;
            do
            {
                TcpClient client = new TcpClient();
                Console.WriteLine("--- REGISTER ---");
                bool validInputs = false;
                
                while (!validInputs) { 
                    Console.SetCursorPosition(0, 1);
                    Console.Write(new string(' ', Console.WindowWidth));
                    Console.SetCursorPosition(0, 1);
                    Console.Write("Username: ");
                    username = Console.ReadLine()!;
                    if (username.Length > 0) {
                        validInputs = true;
                    }
                }
                validInputs = false;
                while (!validInputs) { 
                    Console.SetCursorPosition(0, 2);
                    Console.Write(new string(' ', Console.WindowWidth));
                    Console.SetCursorPosition(0, 2);
                    Console.Write("Password: ");
                    password = Console.ReadLine()!;
                    if (password.Length > 0) {
                        validInputs = true;
                    }
                }

                Console.WriteLine("Verifying username...");
                client.Connect(serverIp, port);
                NetworkStream stream = client.GetStream();
                byte[] message = Encoding.UTF8.GetBytes("2-" + username + "-" + password);
                stream.Write(message, 0, message.Length);
                byte[] response = new byte[1];
                stream.Read(response);
                // 49 = "1" in UTF-8
                if (response[0] == 1) {
                    takenUsername = false;
                } else {
                    takenUsername = true;
                    Console.Clear();
                }
                client.Close();
            } while (takenUsername);
            Username = username;
            Money = 40;
        }

        /* Not sure if it's a good idea
        private static void GenerateGuest()
        {
            TcpClient client = new TcpClient();
            Console.WriteLine("Generating guest...");
            client.Connect(serverIp, port);
            NetworkStream stream = client.GetStream();
            byte[] message = Encoding.UTF8.GetBytes("3");
            stream.Write(message, 0, message.Length);
            byte[] response = new byte[1024];
            stream.Read(response);
            Money = 40;
        }
        */
        
        static int PlayMatch(int playerCount, int startingCardCount) {
            // Player Money
            int[] playerMoney = new int[playerCount];
            for (int i = 0; i < playerMoney.Length; i++)
            {
                playerMoney[i] = 40;
            }
            int[] foldedPlayers = Enumerable.Repeat(-1, playerCount).ToArray();

            //while ()

            // Winner Pots
            int pot = 0;
            int roundPot;

            // Generate Player Hands
            List<Hand> allHands = [];
            Console.Clear();
            Console.SetCursorPosition(0, 5);
            for (int i = 0; i < playerCount; i++)
            {
                allHands.Add(GenerateHand(startingCardCount));
                Console.Write(i + 1 + ". ~" + allHands[i] + "~\t");
            }
            
            // Generate The Dealer's Cards
            Hand communityHand = GenerateHand(3);

            // DEBUG: HAND GENERATION
            /*
            foreach (List<Card> hand in allHands)
            {
                hand.ForEach(Console.WriteLine);
                System.Console.WriteLine();
            }
            */

            // TODO: INITIAL TURN WHEN BIDDING GETS INTRODUCED / THROW IN
            Console.SetCursorPosition(0, 1);
            Console.WriteLine("\t\tBIDDING:");
            (roundPot, playerMoney, foldedPlayers) = Bid(playerMoney, foldedPlayers);
            System.Console.WriteLine(foldedPlayers.Sum());
            pot += roundPot;
            
            // FIRST ROUND: Show 3 Cards from the Dealer's Hand and Then Start Bidding
            Console.SetCursorPosition(0, 0);
            string dealerLine = "";
            for (int i = 0; i < 3; i++)
            {
                dealerLine += communityHand.hand[i] + "\t";
            }
            Console.Write(dealerLine);
            // TODO: BIDDING TIME / THROW IN
            (roundPot, playerMoney, foldedPlayers) = Bid(playerMoney, foldedPlayers);
            System.Console.WriteLine(foldedPlayers.Sum());
            pot += roundPot;
            Console.SetCursorPosition(dealerLine.Length + dealerLine.Count(x => x == '\t') * 3, 0);

            // SECOND ROUND: Show 1 More Card
            Combine(communityHand, GenerateHand(1));
            dealerLine += communityHand.hand[3] + "\t";
            Console.Write(communityHand.hand[3] + "\t");
            
            // TODO: BIDDING TIME / THROW IN
            (roundPot, playerMoney, foldedPlayers) = Bid(playerMoney, foldedPlayers);
            System.Console.WriteLine(foldedPlayers.Sum());
            pot += roundPot;
            Console.SetCursorPosition(dealerLine.Length + dealerLine.Count(x => x == '\t') * 3, 0);

            // LAST ROUND: Show the Last Card
            Combine(communityHand, GenerateHand(1));
            Console.Write(communityHand.hand[4] + "\t");

            Console.SetCursorPosition(0, 6);
            Console.WriteLine("RESULTS:");
            // Player Points List
            List<int> playerPoints = new(allHands.Count);
            // Calculate Results
            for (int i = 0; i < allHands.Count; i++)
            {
                int points = Combine(allHands[i], communityHand).HandChecker(startingCardCount);
                if (foldedPlayers[i] == -1) {
                    playerPoints.Add(points);
                } else {
                    playerPoints.Add(0);
                }
                Console.Write(i + 1 + ". " + points + "\t");
            }
            int winnerIndex = playerPoints.IndexOf(playerPoints.Max());
            // System.Console.WriteLine(winnerIndex);
            ToNatural(foldedPlayers);
            pot += foldedPlayers.Sum();
            Console.WriteLine();
            Console.WriteLine("MONEY: ");
            for (int i = 0; i < playerMoney.Length; i++)
            {
                playerMoney[i] -= foldedPlayers[i];
                
                if (i == winnerIndex) {
                    Console.Write(i + 1 + ". " + playerMoney[i] + " + " + pot + "\t");
                    playerMoney[i] += pot;
                } else {
                    Console.Write(i + 1 + ". " + playerMoney[i] + "\t"); 
                }
                
            }
            return 1;
        }

        static Hand GenerateHand(int cardCount) {
            Hand hand = new(new List<Card>(cardCount));
            for (int i = 0; i < cardCount; i++)
            {
                Random random = new();
                hand.hand.Add(new Card(random.Next(2, 15), random.Next(1, 5)));
            }
            return hand;
        }

        static (int, int[], int[]) Bid(int[] playerMoney, int[] foldedPlayers) {
            Console.SetCursorPosition(0, 15);
            foreach (int item in foldedPlayers)
            {
                System.Console.Write(item + "\t");
            }
            int[] playerBids = Enumerable.Repeat(-1, playerMoney.Length).ToArray();
            for (int j = 0; j < foldedPlayers.Length; j++) {
                if (foldedPlayers[j] != -1) {
                    playerBids[j] = 0;
                } 
            }
            Console.SetCursorPosition(0, 2);
            Console.Write(new string(' ', Console.WindowWidth));
            Console.SetCursorPosition(0, 2);
            int i = 0;
            int input = 1;
            string throwInIndicator = "X";

            if (playerBids.Where(x => x != 0).Count() > 1) {
                do 
                {
                    //System.Console.WriteLine(foldedPlayers[i]);
                    if (foldedPlayers[i] == -1) {
                        int minBid = input;

                        Console.Write(i + 1 + ". " + input);
                        Console.SetCursorPosition(Console.CursorLeft - input.ToString().Length, Console.CursorTop);

                        ConsoleKeyInfo keyInfo;

                        // Continuously read each key press
                        while ((keyInfo = Console.ReadKey(true)).Key != ConsoleKey.Enter)
                        {   
                            if (keyInfo.Key == ConsoleKey.UpArrow && input < playerMoney.Min()) {
                                input++;
                            }
                            if (keyInfo.Key == ConsoleKey.DownArrow && input > minBid - 1) {
                                for (int j = 0; j < input.ToString().Length; j++)
                                {
                                    Console.Write(" ");
                                }
                                Console.SetCursorPosition(Console.CursorLeft - input.ToString().Length, Console.CursorTop);
                                input--;

                            }
                            // Display the key pressed
                            if (input == minBid - 1) {
                                Console.Write(throwInIndicator);
                                Console.SetCursorPosition(Console.CursorLeft - throwInIndicator.ToString().Length, Console.CursorTop);
                            } else {
                                Console.Write(input);
                                Console.SetCursorPosition(Console.CursorLeft - input.ToString().Length, Console.CursorTop);
                            }

                        }
                        // THROWING IN OR BIDDING
                        if (input == minBid - 1) {
                            //System.Console.WriteLine("AAAAAAAAAAAAA");
                            foldedPlayers[i] = playerBids[i] < 0 ? 0 : playerBids[i];
                            //foldedPlayers[i] = playerBids[i];
                            //Console.SetCursorPosition(0, 10);
                            //System.Console.WriteLine(foldedPlayers[i]);
                            playerBids[i] = 0;
                            input = minBid;
                        } else {
                            playerBids[i] = input;
                        }
                        Console.Write("\t");
                    } else {
                        playerBids[i] = 0;
                    }


                    i++;
                    if (i == playerMoney.Length) {
                        i = 0;
                    }
                } while (playerBids.Where(x => x != 0).Distinct().Count() != 1);
            }
            

            ToNatural(playerBids);
            for (int j = 0; j < playerMoney.Length; j++)
            {
                
                playerMoney[j] -= playerBids[j];
                
            }
            return (playerBids.Sum(), playerMoney, foldedPlayers);
        }

        


        static Hand Combine(Hand A, Hand B) {
            Hand result = A;
            foreach (Card item in B.hand)
            {
                result.hand.Add(item);
            }
            return result;
        }

        static void ToNatural(int[] l) {
            for (int i = 0; i < l.Length; i++)
            {
                if (l[i] < 0) {
                    l[i] = 0;
                }
            }
        }


        static byte[] TrimTrailingNullBytes(byte[] byteArray)
        {
            // Find the index of the last non-null byte
            int lastNonNullIndex = byteArray.Length - 1;
            while (lastNonNullIndex >= 0 && byteArray[lastNonNullIndex] == 0)
            {
                lastNonNullIndex--;
            }

            // Create a new byte array with the trimmed length
            byte[] trimmedArray = new byte[lastNonNullIndex + 1];
            Array.Copy(byteArray, 0, trimmedArray, 0, trimmedArray.Length);

            return trimmedArray;
        }
    }
}
