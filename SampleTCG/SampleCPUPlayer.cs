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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyTCGLib;

namespace SampleTCG
{
   enum CPUPlayStyle
   {
      Offensive,
      Defensive,
   }

   class SampleCPUPlayer : SamplePlayer
   {
      public CPUPlayStyle CurrentPlayStyle { get; set; }
      public CPUPlayStyle BasePlayStyle { get; set; }

      public SampleCPUPlayer()
         : base(30)
      {
         IsLocallyControlled = false;
         CurrentPlayStyle = BasePlayStyle = CPUPlayStyle.Offensive;
      }

      /// <summary>
      /// Main AI control function. Blocks until the AI has done something or determined that there is nothing useful to do.
      /// </summary>
      public void PerformActions()
      {
         if (CurrentHealth <= (MaxHealth / 2))
         {
            CurrentPlayStyle = CPUPlayStyle.Defensive;
         }
         else
         { 
            CurrentPlayStyle = CPUPlayStyle.Offensive;
         }

         Logger.Log("[AI] I'm playing " + CurrentPlayStyle + " right now.");

         // Do something until we can't play anymore due to having no mana (or no cards etc.)
         while (CheckIfAnyActionIsPossible() == true)
         {
            bool didDoSomething = false;

            Logger.Log("[AI] Can do at least one action.");

            // First check if we can deal a killing blow to the enemy hero.
            didDoSomething = PerformKillingBlowIfPossible();
            if (didDoSomething)
            {
               continue;
            }

            // Next check if the enemy can deal (as far as we know) a killing blow to us and we have to try
            // and prevent that
            didDoSomething = PreventKillingBlowIfPossible();
            if (didDoSomething)
            {
               continue;
            }

            // If defensive, try healing first
            if (CurrentPlayStyle == CPUPlayStyle.Defensive)
            {
               // Try healing
               didDoSomething = TryHeal();
               if (didDoSomething)
               {
                  continue;
               }
            }

            // Try attacking something
            didDoSomething = TryAttackEnemy();
            if (didDoSomething)
            {
               continue;
            }

            // Try to place a minion
            didDoSomething = TryPlayMinion();
            if (didDoSomething)
            {
               continue;
            }

            // If offensive, try healing last
            if (CurrentPlayStyle == CPUPlayStyle.Offensive)
            {
               // Try healing
               didDoSomething = TryHeal();
               if (didDoSomething)
               {
                  continue;
               }
            }

            // If we reach the end of this loop without doing something, although we *could* have done something, then
            // abort the loop. The situation on the playfield is seemingly not handleable by the AI.
            // One example would be: We have only healing spells on the hand and are able to play them, but no entity
            // needs healing.
            if (didDoSomething == false)
            { 
               Logger.Log("[AI] Had some options to act, but couldn't find something useful to do. Ending round.");
               break;
            }
         }

         Logger.Log("[AI] No more actions. Ending round.");
      }

      private bool PreventKillingBlowIfPossible()
      {
         Logger.Log("[AI] Checking PreventKillingBlowIfPossible");

         SampleMinionCard[] enemyMinions = GetEnemyMinions();
         int sumOfAttack = 0;
         for (int i = 0; i < enemyMinions.Length; i++)
         {
            sumOfAttack += enemyMinions[i].Attack;
         }

         if (sumOfAttack >= CurrentHealth)
         {
            Logger.Log("[AI] Enemy can perform killing blow on me. Trying to prevent that. (NYI)");
         }
         else
         { 
            Logger.Log("[AI] Enemy has only " + sumOfAttack + " attack power on field, which is not enough to kill me.");
         }

         return false;
      }

      private bool PerformKillingBlowIfPossible()
      {
         Logger.Log("[AI] Checking PerformKillingBlowIfPossible");

         bool canKill = false;
         SampleBaseCard[] cards = GetAttackingSolution(ActiveSampleMatch.InactiveSamplePlayer.CurrentHealth, out canKill);
         if (canKill && cards.Length > 0)
         {
            Logger.Log("[AI] Can do killing blow!");
            AttackEnemyWithCards(cards, null);
            return true;
         }

         return false;
      }

      private SampleMinionCard[] GetOwnDamagedMinions()
      {
         List<SampleMinionCard> result = new List<SampleMinionCard>();
         SampleBaseCard[] minons = ActiveSampleMatch.GetCardsForPlayer(this);

         for (int i = 0; i < minons.Length; i++)
         {
            if (minons[i].CardType == SampleCardType.Minion)
            {
               SampleMinionCard minion = (SampleMinionCard)minons[i];
               if (minion.CurrentHealth < minion.MaxHealth)
               {
                  result.Add(minion);
               }
            }
         }

         return result.ToArray();
      }

      private SampleSpellCard[] GetHealingSpells()
      {
         List<SampleSpellCard> result = new List<SampleSpellCard>();
         for (int i = 0; i < Hand.Cards.Count; i++)
         {
            SampleBaseCard card = (SampleBaseCard)Hand.Cards[i];
            if (card.CardType == SampleCardType.Spell)
            {
               SampleSpellCard spell = (SampleSpellCard)card;
               if (spell.SpellPower > 0)
               {
                  result.Add(spell);
               }
            }
         }
         return result.ToArray();
      }

      private bool TryHealMinions(SampleSpellCard spell)
      {
         SampleMinionCard[] damagedMinions = GetOwnDamagedMinions();
         for (int i = 0; i < damagedMinions.Length; i++)
         {
            int row = 0;
            int index = 0;
            if (ActiveSampleMatch.GetPlayfieldPositionForCard(damagedMinions[i], out row, out index) == true)
            {
               Logger.Log("[AI] Casting heal " + spell + " on " + damagedMinions[i]);
               PlayCardFromHand(spell, index, row);
               return true;
            }
         }
         return false;
      }

      private bool TryHealHero(SampleSpellCard spell)
      {
         if (CurrentHealth < MaxHealth)
         {
            Logger.Log("[AI] Casting heal " + spell + " on hero.");
            PlayCardFromHand(spell, -1, ActiveSampleMatch.GetRowForPlayer(this));
            return true;
         }
         return false;
      }

      private bool TryHeal()
      {
         Logger.Log("[AI] Checking TryHeal");

         // Check if we have healing spells
         SampleSpellCard[] healingSpells = GetHealingSpells();
         if (healingSpells.Length == 0)
         {
            // No healing spells
            return false;
         }

         // Healing order is different, based on BasePlayStyle
         if (BasePlayStyle == CPUPlayStyle.Defensive)
         {
            // Heal hero, if necessary
            if (TryHealHero(healingSpells[0]) == true)
            {
               return true;
            }

            // Heal minions last
            if (TryHealMinions(healingSpells[0]) == true)
            {
               return true;
            }
         }
         else if (BasePlayStyle == CPUPlayStyle.Offensive)
         { 
            // Heal minions first
            if (TryHealMinions(healingSpells[0]) == true)
            {
               return true;
            }

            // Heal hero, if necessary
            if (TryHealHero(healingSpells[0]) == true)
            {
               return true;
            }
         }

         return false;
      }

      private bool TryPlayMinion()
      {
         Logger.Log("[AI] Checking TryPlayMinion");

         List<SampleMinionCard> minions = new List<SampleMinionCard>();
         for (int i = 0; i < Hand.Cards.Count; i++)
         {
            SampleBaseCard card = (SampleBaseCard)Hand.Cards[i];
            if (card.CardType == SampleCardType.Minion)
            {
               minions.Add(card as SampleMinionCard);
            }
         }

         if (minions.Count == 0)
         {
            Logger.Log("[AI] Got zero minions on hand. Not possible to play one.");
            return false;
         }

         for (int i = 0; i < minions.Count; i++)
         {
            if (minions[i].Cost <= CurrentMana)
            {
               if (PlayCardFromHand(minions[i], 0, ActiveSampleMatch.GetRowForPlayer(this)) == true)
               {
                  Logger.Log("[AI] Played minion '" + minions[i] + "' to playfield.");
                  return true;
               }
            }
         }

         Logger.Log("[AI] Got some minions on hand, but can't play any.");

         return false;
      }

      /// <summary>
      /// Gets maximum amount of spell damage, that can be dealt in this round, with the current hand and remaining mana.
      /// </summary>
      /// <returns>The cards necessary to deal the damage</returns>
      /// <param name="totalDamage">Total damage. Will be negative values</param>
      private SampleSpellCard[] GetSpellDamage(out int totalDamage)
      {
         totalDamage = 0;
         int currentMana = CurrentMana;

         List<SampleSpellCard> cards = new List<SampleSpellCard>();

         for (int i = 0; i < Hand.Cards.Count; i++)
         {
            SampleBaseCard card = (SampleBaseCard)Hand.Cards[i];
            if (card.CardType == SampleCardType.Spell)
            {
               SampleSpellCard spell = (SampleSpellCard)card;
               if (spell.SpellPower < 0 && spell.Cost <= currentMana)
               {
                  // It's a damage spell
                  totalDamage += -spell.SpellPower;
                  cards.Add(spell);
                  currentMana -= spell.Cost;
               }
            }
         }

         return cards.ToArray();
      }

      private SampleBaseCard[] GetAttackingSolution(int targetHealth, out bool canKill)
      {
         canKill = false;

         List<SampleBaseCard> cards = new List<SampleBaseCard>();

         SampleMinionCard[] ownMinions = GetUsableMinions();
         int sumOfOwnAttack = 0;

         SampleSpellCard[] spells = GetSpellDamage(out sumOfOwnAttack);
         // GetSpellDamage returns negative values (since the spell power of spells for damage spells is negative).
         sumOfOwnAttack = -sumOfOwnAttack;
         if (spells.Length > 0)
         {
            cards.AddRange(spells);
         }

         if (sumOfOwnAttack >= targetHealth)
         {
            canKill = true;
         }
         else
         {
            // TODO: We could order the ownMinions list by "disposable" minions first.

            for (int i = 0; i < ownMinions.Length; i++)
            {
               sumOfOwnAttack += ownMinions[i].Attack;
               cards.Add(ownMinions[i]);

               if (sumOfOwnAttack >= targetHealth)
               {
                  canKill = true;
                  break;
               }
            }
         }

         return cards.ToArray();
      }

      private void AttackEnemyWithCards(SampleBaseCard[] cards, SampleMinionCard victimMinion)
      {
         int victimRow = 0;
         int victimIndex = 0;
         if (victimMinion != null)
         {
            if (ActiveSampleMatch.GetPlayfieldPositionForCard(victimMinion, out victimRow, out victimIndex) == false)
            {
               return;
            }
         }
         else
         {
            victimRow = ActiveSampleMatch.GetRowForPlayer(ActiveSampleMatch.InactiveSamplePlayer);
            victimIndex = -1;
         }

         for (int i = 0; i < cards.Length; i++)
         {
            if (cards[i].CardType == SampleCardType.Minion)
            {
               SampleMinionCard minion = (SampleMinionCard)cards[i];
               int row = 0;
               int index = 0;
               if (ActiveSampleMatch.GetPlayfieldPositionForCard(minion, out row, out index) == true)
               {
                  Logger.Log("[AI] Attacking '" + (victimMinion != null ? victimMinion.ToString() : "enemy hero") + "' with minion '" + minion + "' at position " + index + ".");
                  ActiveSampleMatch.AttackWithCardUsingIndexes(row, index, victimRow, victimIndex);

                  return;
               }
            }
            else if (cards[i].CardType == SampleCardType.Spell)
            {
               SampleSpellCard spell = (SampleSpellCard)cards[i];
               Logger.Log("[AI] Casting damage spell '" + spell + "' on victim '" + (victimMinion != null ? victimMinion.ToString() : "enemy hero") + "'.");
               PlayCardFromHand(spell, victimIndex, victimRow);

               return;
            }
         }
      }

      private bool TryAttackEnemy()
      {
         Logger.Log("[AI] Checking TryAttackEnemy");

         SampleMinionCard[] ownMinions = GetUsableMinions();
         SampleMinionCard[] enemyMinions = GetEnemyMinions();

         int enemyRow = ActiveSampleMatch.GetRowForPlayer(ActiveSampleMatch.InactiveSamplePlayer);

         // Check if we should attack the enemy hero, which we will do if 
         // there are no enemy minions on the playfield
         if (ownMinions.Length > 0 && enemyMinions.Length == 0)
         {
            for (int i = 0; i < ownMinions.Length; i++)
            {
               int row = 0;
               int index = 0;
               if (ActiveSampleMatch.GetPlayfieldPositionForCard(ownMinions[0], out row, out index) == true)
               {
                  Logger.Log("[AI] Attacking enemy hero with minion '" + ownMinions[0] + "' at position " + index + ".");
                  ActiveSampleMatch.AttackWithCardUsingIndexes(row, index, enemyRow, -1);
               }
            }
            return true;
         }

         // Focus on killing enemy minions
         for (int i = 0; i < enemyMinions.Length; i++)
         {
            bool canKill = false;
            SampleBaseCard[] cards = GetAttackingSolution(enemyMinions[i].CurrentHealth, out canKill);
            if (canKill && cards.Length > 0)
            {
               AttackEnemyWithCards(cards, enemyMinions[i]);
               return true;
            }
         }

         // If we couldn't kill a minion yet, try to damage one as much as possible
         if (enemyMinions.Length > 0)
         {
            bool canKill = false;

            SampleBaseCard[] cards = GetAttackingSolution(enemyMinions[0].CurrentHealth, out canKill);
            if (cards.Length > 0)
            { 
               AttackEnemyWithCards(cards, enemyMinions[0]);
               return true;
            }
         }

         return false;
      }

      /// <summary>
      /// Returns a list of minions of the enemy player. This function is only usable if the CPU is the active player
      /// </summary>
      private SampleMinionCard[] GetEnemyMinions()
      {
         SampleBaseCard[] cardsOnPlayfield = ActiveSampleMatch.GetCardsForPlayer(ActiveSampleMatch.InactiveSamplePlayer);
         List<SampleMinionCard> result = new List<SampleMinionCard>(cardsOnPlayfield.Length);

         for (int i = 0; i < cardsOnPlayfield.Length; i++)
         {
            result.Add((SampleMinionCard)cardsOnPlayfield[i]);
         }

         return result.ToArray();
      }

      /// <summary>
      /// Returns a list of minions who can still perform some action in this round.
      /// </summary>
      private SampleMinionCard[] GetUsableMinions()
      {
         List<SampleMinionCard> result = new List<SampleMinionCard>();

         SampleBaseCard[] cardsOnPlayfield = ActiveSampleMatch.GetCardsForPlayer(this);
         for (int i = 0; i < cardsOnPlayfield.Length; i++)
         {
            SampleMinionCard card = (SampleMinionCard)cardsOnPlayfield[i];
            if (card.CurrentMovesPerRound > 0)
            {
               result.Add(card);
            }
         }

         return result.ToArray();
      }

      private bool CheckIfAnyActionIsPossible()
      {
         if (ActiveSampleMatch.IsRunning == false)
         {
            // Match is over, so no action is possible
            return false;
         }

         bool canPlayAtLeastOneCard = false;
         for (int i = 0; i < Hand.Cards.Count; i++)
         {
            SampleBaseCard card = (SampleBaseCard)Hand.Cards[i];
            if (CurrentMana >= card.Cost)
            {
               canPlayAtLeastOneCard = true;
               break;
            }
         }

         SampleMinionCard[] minions = GetUsableMinions();
         bool canDoAtLeastOneMove = minions.Length > 0;

         return canDoAtLeastOneMove || canPlayAtLeastOneCard;
      }
   }
}
