/*
   Copyright 2016 Henning Thoele

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/

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
