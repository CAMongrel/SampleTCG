using System;
using MyTCGLib;

namespace SampleTCG
{
   public class SamplePlayer : BasePlayer
   {
      public int MaxHealth { get; private set; }
      public int CurrentHealth { get; private set; }

      public int MaxMana { get; private set; }
      public int CurrentMana { get; private set; }

      public SampleMatch ActiveSampleMatch
      {
         get { return ActiveMatch as SampleMatch; }
         set { ActiveMatch = value; }
      }

      public SamplePlayer(int setMaxHealth)
      {
         Hand = new BaseHand();
         Deck = new BaseDeck();

         MaxHealth = CurrentHealth = setMaxHealth;
         MaxMana = CurrentMana = 0;
      }

      /// <summary>
      /// Invoke a current health change (heal or damage)
      /// </summary>
      /// <param name="healthMod">Health mod. Negative for damage, positive for heal.</param>
      public void ReceiveHealth(int healthMod)
      {
         CurrentHealth += healthMod;
         if (CurrentHealth > MaxHealth)
         {
            CurrentHealth = MaxHealth;
         }
      }

      public override void StartTurn()
      {
         base.StartTurn();

         Logger.Log("Start turn of " + this.Name);

         if (MaxMana < 10)
         {
            MaxMana++;
         }

         CurrentMana = MaxMana;
      }

      private bool CanPlayCard(SampleBaseCard card, int arenaX, int arenaY)
      {
         if (CurrentMana < card.Cost)
         {
            return false;
         }

         if (ActiveSampleMatch != null)
         {
            if (ActiveSampleMatch.CanPlayCard(card, this, arenaX, arenaY) == false)
            {
               return false;
            }
         }

         return true;
      }

      private bool PlayCard(SampleBaseCard card, int arenaX, int arenaY)
      {
         if (ActiveSampleMatch != null)
         {
            if (ActiveSampleMatch.PlayCard(card, this, arenaX, arenaY) == false)
            {
               return false;
            }
         }

         return true;
      }

      public bool PlayCardFromHandUsingIndex(int index, int arenaX, int arenaY)
      {
         return PlayCardFromHand(Hand.PeekCard(index) as SampleBaseCard, arenaX, arenaY);
      }

      public bool PlayCardFromHand(SampleBaseCard card, int arenaX, int arenaY)
      {
         if (card == null)
         {
            return false;
         }

         if (CanPlayCard(card, arenaX, arenaY) == false)
         {
            return false;
         }

         if (Hand.RemoveCard(card) == false)
         {
            return false;
         }

         if (PlayCard(card, arenaX, arenaY) == false)
         {
            return false;
         }

         this.CurrentMana -= card.Cost;

         return true;
      }
   }
}
