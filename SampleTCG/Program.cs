using System;
using MyTCGLib;

namespace SampleTCG
{
   enum Command
   {
      None,
      Concede,
      Play,
      PrintHand,
      Attack,
      End
   }

   class MainClass : ILogWriter
   {
      private static void PrintHelp()
      {
         Console.WriteLine("Available commands:");
         Console.WriteLine("  play\t\tSyntax: play card_index playfield_position [player_index]");
         Console.WriteLine("\t\tPlays the specified card to the specified position. If player_index is not given, the current player is assumed. Can be used for minions and spells.");

         Console.WriteLine("  attack\t\tSyntax: attack card_index playfield_position");
         Console.WriteLine("\t\tUse the minion in the specified position to attack a minion on the other row. playfield_position can be -1 for the other hero.");

         Console.WriteLine("  concede\tSyntax: concede");
         Console.WriteLine("\t\tConcedes this match, therefore losing it.");

         Console.WriteLine("  end\tSyntax: end");
         Console.WriteLine("\t\tEnds this round, starting the next for the next player.");

         Console.WriteLine("  hand\tSyntax: hand");
         Console.WriteLine("\t\tPrints the hand for this player.");
      }

      private static Command ParseCommand(string cmd, SampleMatch match)
      {
         string[] split = cmd.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
         if (split.Length == 0)
         {
            return Command.None;
         }

         string command = split[0];
         string[] parms = new string[split.Length - 1];
         Array.Copy(split, 1, parms, 0, parms.Length);

         switch (command)
         {
            case "play":
               {
                  if (parms.Length < 2)
                  {
                     Console.WriteLine("'play' command must have at least two parameters");
                     return Command.None;
                  }

                  int param1 = 0;
                  int param2 = 0;
                  int param3 = -1;
                  try
                  {
                     param1 = int.Parse(parms[0]);
                     param2 = int.Parse(parms[1]);
                  }
                  catch
                  {
                     Console.WriteLine("'play' parameters must be valid integers");
                     return Command.None;
                  }

                  if (parms.Length > 2)
                  {
                     if (int.TryParse(parms[2], out param3) == false)
                     {
                        param3 = -1;
                     }
                  }

                  int row = param3;
                  if (row == -1)
                  {
                     row = match.GetRowForPlayer(match.ActiveSamplePlayer);
                  }

                  bool res = match.ActiveSamplePlayer.PlayCardFromHandUsingIndex(param1, param2, row);
                  if (res == false)
                  { 
                     Console.WriteLine("Failed to play card '" + param1 + "' from hand to position '" + param2 + "'");
                     return Command.None;
                  }
               }
               return Command.Play;
            
            case "attack":
               {
                  if (parms.Length != 2)
                  {
                     Console.WriteLine("'attack' command must have two parameters");
                     return Command.None;
                  }

                  int param1 = 0;
                  int param2 = 0;
                  try
                  {
                     param1 = int.Parse(parms[0]);
                     param2 = int.Parse(parms[1]);
                  }
                  catch
                  {
                     Console.WriteLine("'attack' parameters must be valid integers");
                     return Command.None;
                  }

                  bool res = match.AttackWithCardUsingIndexes(match.GetRowForPlayer(match.ActiveSamplePlayer), param1,
                                                              match.GetRowForPlayer(match.InactiveSamplePlayer), param2);
                  if (res == false)
                  {
                     Console.WriteLine("Failed to attack victim '" + param2 + "' with card '" + param2 + "'");
                     return Command.None;
                  }
               }
               return Command.Attack;

            case "help":
               PrintHelp();
               return Command.None;

            case "hand":
               // end turn
               return Command.PrintHand;

            case "end":
               // end turn
               return Command.End;

            case "concede":
               match.ActivePlayer.Concede();
               return Command.Concede;
         }

         Console.WriteLine("Unknown command: " + command);
         return Command.None;
      }

      private static bool isValidPlayerSetup(string line)
      {
         return line == "1" || line == "2" || line == "3";
      }

      public static void Main(string[] args)
      {
         MainClass cls = new MainClass();
         Logger.LogWriter = cls;

         Console.WriteLine("Hello SampleTCG!");
         Console.WriteLine();
         Console.WriteLine("Choose player setup:");
         Console.WriteLine("1 - Human vs. Human");
         Console.WriteLine("2 - Human vs. CPU");
         Console.WriteLine("3 - CPU vs. CPU");
         Console.WriteLine();
         string mode = Console.ReadLine();
         while (isValidPlayerSetup(mode) == false)
         {
            Console.WriteLine("Please enter either 1, 2 or 3.");
            mode = Console.ReadLine();
         }

         SamplePlayer player1 = null;
         SamplePlayer player2 = null;

         switch (mode)
         {
            case "1":
               player1 = new SampleHumanPlayer();
               player1.Name = "Human1";
               player2 = new SampleHumanPlayer();
               player2.Name = "Human2";
               break;

            case "2":
               player1 = new SampleCPUPlayer();
               player1.Name = "CPU";
               player2 = new SampleHumanPlayer();
               player2.Name = "Human2";
               break;

            case "3":
               player1 = new SampleCPUPlayer();
               player1.Name = "CPU1";
               player2 = new SampleCPUPlayer();
               player2.Name = "CPU2";
               break;
         }

         Random rnd = new Random();

         SampleGame game = new SampleGame();
         game.InitCollection();

         SampleMatch match = new SampleMatch(rnd);
         match.Player1 = player1;
         match.Player2 = player2;

         player1.CreateRandomDeck(game.CardCollection, 20, match.Random);
         player2.CreateRandomDeck(game.CardCollection, 20, match.Random);

         match.PreStart();
         match.Start();

         while (match.IsRunning)
         {
            SampleMatchPrinter.PrintSampleMatch(match);

            if (match.ActivePlayer.IsLocallyControlled)
            {
               string line = Console.ReadLine();
               Command cmd = ParseCommand(line, match);
               while (cmd == Command.None)
               {
                  line = Console.ReadLine();
                  cmd = ParseCommand(line, match);
               }

               if (cmd == Command.PrintHand)
               {
                  match.ActivePlayer.LogHand();
               }

               if (cmd == Command.End)
               {
                  match.StartNextRound();
               }
            }
            else
            {
               SampleCPUPlayer cpu = match.ActiveSamplePlayer as SampleCPUPlayer;
               if (cpu != null)
               {
                  cpu.PerformActions();
               }

               match.StartNextRound();
            }
         }

         Console.WriteLine("Press return to exit");
         Console.ReadLine();
      }

      public void Log(string text)
      {
         Console.WriteLine(text);
      }
   }
}
