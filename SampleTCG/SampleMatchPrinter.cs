using System;
using System.Collections.Generic;
using MyTCGLib;

namespace SampleTCG
{
   public static class SampleMatchPrinter
   {
      internal class SampleMatchPrinterBuffer : ILogWriter
      {
         internal List<string> buffer = new List<string>();

         public void Log(string text)
         {
            buffer.Add(text);
         }
      }

      private static int GetFirstCardPositionX(SampleMatch match, int row)
      {
         int numberOfCardsInRow = match.GetNumberOfCards(row);

         int widthOfCard = 6;
         int fullWidth = widthOfCard * match.PlayfieldWidth;
         int xPos = (fullWidth / 2) - (widthOfCard / 2) * numberOfCardsInRow;
         return xPos;
      }

      private static string GetFullLine(SampleMatch match, bool isBorderline)
      {
         string line = isBorderline ? "+" : "|";
         for (int i = 0; i < (match.CardWidth * match.PlayfieldWidth); i++)
         {
            line += isBorderline ? "=" : " ";
         }
         line += isBorderline ? "+" : "|";
         return line;
      }

      private static string ApplyCardsToLine(SampleMatch match, bool isBorderLine, int row, string orgline, int cardrow)
      {
         int numCards = match.GetNumberOfCards(row);
         if (numCards == 0)
         {
            return orgline;
         }

         int startX = GetFirstCardPositionX(match, row);

         string line = "+";
         string emptyLine = "|";
         for (int j = 0; j < numCards; j++)
         {
            line += "=====+";

            SampleMinionCard card = match.Playfield[j, row] as SampleMinionCard;

            if (card != null)
            {
               switch (cardrow)
               {
                  case 1:
                     if (card.IsSleeping)
                     {
                        emptyLine += " Zzz |";
                     }
                     else
                     {
                        emptyLine += "   " + card.CurrentMovesPerRound + " |";
                     }
                     break;
                  case 2:
                     emptyLine += "A" + card.Attack.ToString("D3") + " |";
                     break;
                  case 3:
                     emptyLine += "L" + card.CurrentHealth.ToString("D3") + " |";
                     break;
                  default:
                     emptyLine += "     |";
                     break;
               }
            }
            else
            {
               emptyLine += "     |";
            }
         }

         string lineToUse = isBorderLine ? line : emptyLine;

         return orgline.Remove(startX, lineToUse.Length).Insert(startX, lineToUse);
      }

      private static string ReplaceWithName(string line, string name, int maxLength)
      {
         string prefix = "Name: ";

         int lenToUse = prefix.Length + name.Length;
         if (lenToUse > maxLength)
         {
            lenToUse = maxLength;
         }

         string remainder = line.Substring(lenToUse);
         return prefix + name.Substring(0, lenToUse - prefix.Length) + remainder;
      }

      private static void LogHero(SampleMatch match, SamplePlayer player, bool isUpper)
      { 
         int heroStartIndex = (int)((float)match.PlayfieldWidth / 2.0f - 0.5f);
         string emptyLine = new String(' ', heroStartIndex * match.CardWidth);
         if (isUpper == true)
         {
            Logger.Log(emptyLine + "+=====+");
         }

         string manaSubstring = player.CurrentMana.ToString("D2") + "/" + player.MaxMana.ToString("D2");
         string manaString = "        Mana: " + manaSubstring;

         Logger.Log(emptyLine + "|     |");
         Logger.Log(ReplaceWithName(emptyLine, player.Name, emptyLine.Length - 1) + "| " + player.CurrentHealth.ToString("D3") + " |" + manaString);
         Logger.Log(emptyLine + "|     |");

         if (isUpper == false)
         {
            Logger.Log(emptyLine + "+=====+");
         }
      }

      public static void PrintSampleMatch(SampleMatch match)
      {
         ILogWriter prevWriter = Logger.LogWriter;

         SampleMatchPrinterBuffer buffer = new SampleMatchPrinterBuffer();

         try
         {
            Logger.LogWriter = buffer;

            Logger.Log("========================================================================================================");
            Logger.Log("Round: " + match.CurrentRound);
            Logger.Log("Active Player: " + match.ActivePlayer.Name);
            Logger.Log("--------------------------------------------------------------------------------------------------------");

            // Draw upper hero
            LogHero(match, match.Player1, true);

            string borderLine = GetFullLine(match, true);
            string middleLine = GetFullLine(match, false);

            Logger.Log(ApplyCardsToLine(match, true, 0, borderLine, 0));
            for (int i = 0; i < match.PlayfieldHeight; i++)
            {
               for (int y = 0; y < match.CardHeight - 1; y++)
               {
                  Logger.Log(ApplyCardsToLine(match, false, i, middleLine, y + 1));
               }
               Logger.Log(ApplyCardsToLine(match, true, i, borderLine, 0));
            }

            // Draw upper hero
            LogHero(match, match.Player2, false);

            // Add hand to buffer
            int maxLineLength = borderLine.Length + match.CardWidth;
            List<BaseCard> handCards = match.ActivePlayer.Hand.Cards;

            int linePos = 4;
            int gapLength = maxLineLength - buffer.buffer[linePos].Length;
            string gap = new String(' ', gapLength);

            buffer.buffer[linePos] += gap + "Hand of " + match.ActivePlayer.Name;

            for (int i = 0; i < handCards.Count; i++)
            {
               linePos++;

               if (linePos >= buffer.buffer.Count)
               {
                  gapLength = maxLineLength;

                  gap = new String(' ', gapLength);
                  buffer.buffer.Add(gap + i + ": " + handCards[i]);
               }
               else
               {
                  gapLength = maxLineLength - buffer.buffer[linePos].Length;
                  if (gapLength < 0)
                  {
                     gapLength = 0;
                  }

                  gap = new String(' ', gapLength);
                  buffer.buffer[linePos] += gap + i + ": " + handCards[i];
               }
            }
         }
         finally
         {
            Logger.LogWriter = prevWriter;
         }

         // Write buffer to real output
         if (Logger.LogWriter != buffer)
         { 
            for (int i = 0; i < buffer.buffer.Count; i++)
            {
               Logger.Log(buffer.buffer[i]);
            }
         }
      }
   }
}
