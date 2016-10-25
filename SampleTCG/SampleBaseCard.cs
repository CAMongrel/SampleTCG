using System;
using MyTCGLib;

namespace SampleTCG
{
   public enum SampleCardType
   {
      Minion,
      Spell
   }

   public enum SampleCardValueType
   {
      Damage,     // Used for minion attack and spell damage (negative for damage spells, positive for heal spells)
      MaxHealth,
      CurrentHealth,
      Cost,
      MaxMovesPerRound,
      CurrentMovesPerRound,
      SleepStatus,
   }

   public class SampleBaseCard : BaseCard
   {
      public SampleCardType CardType { get; private set; }

      public int Cost
      {
         get { return GetValue((int)SampleCardValueType.Cost); }
         set { SetValue((int)SampleCardValueType.Cost, value); }
      }

      public SampleBaseCard(SampleCardType setCardType)
      {
         CardType = setCardType;
      }
   }
}
