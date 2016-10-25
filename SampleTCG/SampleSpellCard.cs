using System;
using MyTCGLib;

namespace SampleTCG
{
   public class SampleSpellCard : SampleBaseCard
   {
      public int SpellPower
      {
         get { return GetValue((int)SampleCardValueType.Damage); }
         set 
         { 
            SetValue((int)SampleCardValueType.Damage, value);
            if (value < 0)
            {
               this.Name = "Damage" + Math.Abs(value);
            }
            else if (value > 0)
            { 
               this.Name = "Heal" + Math.Abs(value);
            }
         }
      }

      public SampleSpellCard()
         : base(SampleCardType.Spell)
      {
      }

      public override BaseCard CreateCopy()
      {
         SampleSpellCard result = new SampleSpellCard();
         this.CopyValuesTo(result);
         return result;
      }

      public override string ToString()
      {
         return string.Format("[" + this.Name + ": SpellPower={0}, Cost={1}]", SpellPower, Cost);
      }
   }
}
