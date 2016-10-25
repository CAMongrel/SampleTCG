using System;
using System.Collections.Generic;
using MyTCGLib;

namespace SampleTCG
{
   public class SampleGame : BaseGame
   {
      public List<SampleBaseCard> CardCollection { get; private set; }

      public SampleGame()
      {
         CardCollection = new List<SampleBaseCard>();
      }

      public void InitCollection()
      {
         // Create minions with more attack than health
         for (int i = 0; i < 10; i++)
         {
            SampleMinionCard card = new SampleMinionCard();
            card.Attack = (i + 1);
            card.MaxHealth = (i + 2);
            card.Cost = (i + 1);
            card.MaxMovesPerRound = 1;
            CardCollection.Add(card);
         }

         // Create minions with more health than attack
         for (int i = 0; i < 10; i++)
         {
            SampleMinionCard card = new SampleMinionCard();
            card.Attack = (i + 2);
            card.MaxHealth = (i + 1);
            card.Cost = (i + 1);
            card.MaxMovesPerRound = 1;
            CardCollection.Add(card);
         }

         // Create damage spells
         for (int i = 0; i < 10; i++)
         {
            SampleSpellCard card = new SampleSpellCard();
            card.SpellPower = -(i + 1);
            card.Cost = (i + 1);
            CardCollection.Add(card);
         }

         // Create heal spells
         for (int i = 0; i < 10; i++)
         {
            SampleSpellCard card = new SampleSpellCard();
            card.SpellPower = (i + 1);
            card.Cost = (i + 1);
            CardCollection.Add(card);
         }

         Console.WriteLine("Card collection:");
         foreach (var card in CardCollection)
         {
            Console.WriteLine(card.ToString());
         }
      }
   }
}
