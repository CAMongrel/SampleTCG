using System;
using MyTCGLib;

namespace SampleTCG
{
   public class SampleMinionCard : SampleBaseCard
   {
      public int Attack
      {
         get { return GetValue((int)SampleCardValueType.Damage); }
         set 
         { 
            SetValue((int)SampleCardValueType.Damage, value); 

            this.Name = "Minion" + value + MaxHealth;
         }
      }

      public int MaxHealth
      {
         get { return GetValue((int)SampleCardValueType.MaxHealth); }
         set 
         { 
            SetValue((int)SampleCardValueType.MaxHealth, value);

            this.Name = "Minion" + Attack + value;
            CurrentHealth = value;
         }
      }

      public int CurrentHealth
      {
         get { return GetValue((int)SampleCardValueType.CurrentHealth); }
         set { SetValue((int)SampleCardValueType.CurrentHealth, value); }
      }

      public int CurrentMovesPerRound
      {
         get { return GetValue((int)SampleCardValueType.CurrentMovesPerRound); }
         set { SetValue((int)SampleCardValueType.CurrentMovesPerRound, value); }
      }

      public int MaxMovesPerRound
      {
         get { return GetValue((int)SampleCardValueType.MaxMovesPerRound); }
         set { SetValue((int)SampleCardValueType.MaxMovesPerRound, value); }
      }

      public bool IsSleeping
      {
         get { return GetValue((int)SampleCardValueType.SleepStatus) > 0; }
         set { SetValue((int)SampleCardValueType.SleepStatus, value ? 1 : 0); }
      }

      public SampleMinionCard()
         : base(SampleCardType.Minion)
      {
         //
      }

      public override void ApplyPlayEffect()
      {
         base.ApplyPlayEffect();

         CurrentMovesPerRound = 0;
         IsSleeping = true;
      }

      public override bool ApplyPreActionEffect()
      {
         if (CurrentMovesPerRound <= 0)
         {
            return false;
         }

         return base.ApplyPreActionEffect();
      }

      public override void ApplyPostActionEffect()
      {
         base.ApplyPostActionEffect();

         CurrentMovesPerRound -= 1;
         if (CurrentMovesPerRound < 0)
         {
            CurrentMovesPerRound = 0;
         }
      }

      public override void ApplyStartOfRoundEffect(BasePlayer activePlayer)
      {
         base.ApplyStartOfRoundEffect(activePlayer);

         IsSleeping = false;
         
         if (activePlayer == Owner)
         {
            CurrentMovesPerRound = MaxMovesPerRound;
         }
      }

      public override BaseCard CreateCopy()
      {
         SampleMinionCard result = new SampleMinionCard();
         this.CopyValuesTo(result);
         return result;
      }

      public override string ToString()
      {
         return string.Format("[" + this.Name + ": Attack={0}, Health={2}/{1}, Moves={4}/{5}, Cost={3}]", Attack, MaxHealth, CurrentHealth, Cost, CurrentMovesPerRound, MaxMovesPerRound);
      }
   }
}
