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
using MyTCGLib;

namespace SampleTCG
{
   public class SampleMatch : BaseMatch
   {
      public SamplePlayer Player1
      {
         get { return (SamplePlayer)playerList[0]; }
         set
         {
            playerList[0] = value;
            playerList[0].ActiveMatch = this;
         }
      }
      public SamplePlayer Player2
      {
         get { return (SamplePlayer)playerList[1]; }
         set
         {
            playerList[1] = value;
            playerList[1].ActiveMatch = this;
         }
      }

      public SamplePlayer ActiveSamplePlayer
      {
         get { return ActivePlayer as SamplePlayer; }
         private set { ActivePlayer = value; }
      }
      public SamplePlayer InactiveSamplePlayer
      {
         get 
         {
            return ActiveSamplePlayer == Player1 ? Player2 : Player1; 
         }
      }

      public int PlayfieldWidth { get; private set; }
      public int PlayfieldHeight { get; private set; }
      public int CardWidth { get; private set; }
      public int CardHeight { get; private set; }
      public SampleBaseCard[,] Playfield { get; private set; }

      public SampleMatch(Random setRandom = null)
         : base(setRandom)
      {
         playerList = new List<BasePlayer>(2);
         playerList.Add(null);
         playerList.Add(null);
         ActiveSamplePlayer = null;

         PlayfieldWidth = 7;
         PlayfieldHeight = 2;
         CardWidth = 6;
         CardHeight = 4;
         Playfield = new SampleBaseCard[PlayfieldWidth, PlayfieldHeight];
      }

      protected override void ClearPlayfield()
      {
         base.ClearPlayfield();
      }

      protected override void PreparePlayers()
      {
         base.PreparePlayers();

         Player1.Deck.Shuffle(Random);
         Player2.Deck.Shuffle(Random);

         CreateInitialHands();
      }

      private void CreateInitialHands()
      {
         int initialHandSize = 3;

         for (int i = 0; i < initialHandSize; i++)
         {
            Player1.DrawCard();
            Player2.DrawCard();
         }
      }

      public int GetNumberOfCards(int row)
      {
         int numberOfCardsInRow = 0;
         for (int i = 0; i < PlayfieldWidth; i++)
         {
            if (Playfield[i, row] == null)
            {
               // No cards on playfield at all (on this row)
               return numberOfCardsInRow;
            }
            numberOfCardsInRow++;
         }
         return numberOfCardsInRow;
      }

      public SampleBaseCard[] GetCardsForRow(int row)
      {
         List<SampleBaseCard> resList = new List<SampleBaseCard>();
         for (int i = 0; i < PlayfieldWidth; i++)
         {
            if (Playfield[i, row] != null)
            {
               resList.Add(Playfield[i, row]);
            }
         }
         return resList.ToArray();
      }

      /// <summary>
      /// Gets the playfield position for card.
      /// </summary>
      /// <returns><c>true</c>, if playfield position for card was gotten, <c>false</c> if the card is not on the playfield.</returns>
      /// <param name="card">Card.</param>
      /// <param name="row">Row.</param>
      /// <param name="index">Index.</param>
      public bool GetPlayfieldPositionForCard(SampleBaseCard card, out int row, out int index)
      {
         row = -1;
         index = -1;

         for (int y = 0; y < PlayfieldHeight; y++)
         {
            for (int x = 0; x < PlayfieldWidth; x++)
            {
               if (Playfield[x, y] == card)
               {
                  row = y;
                  index = x;
                  return true;
               }
            }  
         }

         return false;
      }

      public SampleBaseCard[] GetCardsForPlayer(SamplePlayer player)
      {
         int row = GetRowForPlayer(player);
         return GetCardsForRow(row);
      }

      public bool CanPlayCard(SampleBaseCard card, SamplePlayer player, int x, int y)
      {
         if (card.CardType == SampleCardType.Minion)
         {
            return CanPlayMinionCard(card as SampleMinionCard, player, x, y);
         }
         else if (card.CardType == SampleCardType.Spell)
         {
            return CanPlaySpellCard(card as SampleSpellCard, player, x, y);
         }

         return false;
      }

      public bool CanPlaySpellCard(SampleSpellCard card, SamplePlayer player, int x, int y)
      {
         if (x < -1 || x >= PlayfieldWidth)
         {
            return false;
         }
         if (y < 0 || y >= PlayfieldHeight)
         {
            return false;
         }

         if (x == -1)
         {
            // Target is hero
            return true;
         }

         int row = y;

         int numCardsOnPlayfield = GetNumberOfCards(row);
         if (x >= numCardsOnPlayfield)
         {
            return false;
         }

         return true;
      }

      public bool CanPlayMinionCard(SampleMinionCard card, SamplePlayer player, int x, int y)
      {
         int row = GetRowForPlayer(player);
         if (row == -1)
         {
            return false;
         }

         int numCardsOnPlayfield = GetNumberOfCards(row);
         if (numCardsOnPlayfield >= PlayfieldWidth)
         {
            return false;
         }

         if (x > numCardsOnPlayfield)
         {
            return false;
         }

         return true;
      }

      private void InsertCard(SampleBaseCard card, int position, int row)
      {
         for (int i = PlayfieldWidth - 1; i > position; i--)
         {
            Playfield[i, row] = Playfield[i - 1, row];
         }
         Playfield[position, row] = card;
      }

      private void RemoveCard(int position, int row)
      {
         for (int i = position; i < PlayfieldWidth - 1; i++)
         {
            Playfield[i, row] = Playfield[i + 1, row];
         }
         Playfield[PlayfieldWidth - 1, row] = null;
      }

      public bool PlayCard(SampleBaseCard card, SamplePlayer player, int x, int y)
      {
         if (card.CardType == SampleCardType.Minion)
         {
            return PlayMinionCard(card as SampleMinionCard, player, x, y);
         }
         else if (card.CardType == SampleCardType.Spell)
         {
            return PlaySpellCard(card as SampleSpellCard, player, x, y);
         }

         return false;
      }

      public bool PlaySpellCard(SampleSpellCard card, SamplePlayer player, int x, int y)
      {
         if (CanPlaySpellCard(card, player, x, y) == false)
         {
            return false;
         }

         if (x == -1)
         {
            if (card.ApplyPreActionEffect() == false)
            {
               Logger.Log("PlaySpellCard of card '" + card + "' was aborted by PreAction trigger");
               return false;
            }

            // Target is hero
            SamplePlayer pl = playerList[y] as SamplePlayer;
            pl.ReceiveHealth(card.SpellPower);

            if (pl.CurrentHealth < 0)
            {
               DeclareLoser(pl);
            }

            card.ApplyPostActionEffect();

            return true;
         }

         SampleMinionCard targetCard = Playfield[x, y] as SampleMinionCard;
         if (targetCard == null)
         {
            return false;
         }

         if (card.ApplyPreActionEffect() == false)
         {
            Logger.Log("PlaySpellCard of card '" + card + "' was aborted by PreAction trigger");
            return false;
         }

         if (targetCard.ApplyPreTargetEffect(card) == false)
         {
            Logger.Log("PlaySpellCard of card '" + card + "' was aborted by PreTarget trigger of target card '" + targetCard + "'");
            return false;
         }

         targetCard.CurrentHealth += card.SpellPower;

         targetCard.ApplyPostTargetEffect(card);

         if (targetCard.CurrentHealth <= 0)
         {
            targetCard.ApplyDeathEffect();

            RemoveCard(x, y);
         }
         if (targetCard.CurrentHealth > targetCard.MaxHealth)
         {
            targetCard.CurrentHealth = targetCard.MaxHealth;
         }

         card.ApplyPostActionEffect();

         return true;
      }

      public bool PlayMinionCard(SampleMinionCard card, SamplePlayer player, int x, int y)
      {
         if (CanPlayCard(card, player, x, y) == false)
         {
            return false;
         }

         int row = GetRowForPlayer(player);
         if (row == -1)
         {
            return false;
         }

         InsertCard(card, x, row);

         card.ApplyPlayEffect();

         return true;
      }

      public int GetRowForPlayer(SamplePlayer pl)
      {
         for (int i = 0; i < playerList.Count; i++)
         {
            if (playerList[i] == pl)
            {
               return i;
            }
         }
         return -1;
      }

      public bool AttackWithCardUsingIndexes(int attackerRow, int attackerIndex, int victimRow, int victimIndex)
      {
         if (attackerIndex == -1)
         {
            // Hero attack is not supported
            return false;
         }
         
         SampleMinionCard minion = Playfield[attackerIndex, attackerRow] as SampleMinionCard;
         if (minion == null)
         {
            return false;
         }

         if (victimIndex == -1)
         {
            if (minion.ApplyPreActionEffect() == false)
            {
               Logger.Log("AttackWithCardUsingIndexes of card '" + minion + "' was aborted by PreAction trigger");
               return false;
            }

            // Target is hero
            SamplePlayer pl = playerList[victimRow] as SamplePlayer;
            pl.ReceiveHealth(-minion.Attack);

            if (pl.CurrentHealth < 0)
            {
               DeclareLoser(pl);
            }

            minion.ApplyPostActionEffect();

            return true;
         }

         SampleMinionCard targetMinion = Playfield[victimIndex, victimRow] as SampleMinionCard;
         if (targetMinion == null)
         {
            return false;
         }

         if (minion.ApplyPreActionEffect() == false)
         {
            Logger.Log("AttackWithCardUsingIndexes of card '" + minion + "' was aborted by PreAction trigger");
            return false;
         }

         if (targetMinion.ApplyPreAttackedEffect(minion) == false)
         {
            Logger.Log("AttackWithCardUsingIndexes of card '" + minion + "' was aborted by PreAttack trigger of target card '" + targetMinion + "'");
            return false;
         }
         if (minion.ApplyPreAttackedEffect(targetMinion) == false)
         {
            Logger.Log("AttackWithCardUsingIndexes of card '" + minion + "' was aborted by PreAttack trigger of source card '" + minion + "'");
            return false;
         }

         targetMinion.CurrentHealth -= minion.Attack;
         minion.CurrentHealth -= targetMinion.Attack;

         targetMinion.ApplyPostAttackedEffect(minion);
         minion.ApplyPostAttackedEffect(targetMinion);

         if (minion.CurrentHealth <= 0)
         {
            minion.ApplyDeathEffect();

            RemoveCard(attackerIndex, attackerRow);
         }
         if (targetMinion.CurrentHealth <= 0)
         {
            targetMinion.ApplyDeathEffect();

            RemoveCard(victimIndex, victimRow);
         }

         minion.ApplyPostActionEffect();

         return true;
      }

      protected override void DeactivatePlayer(BasePlayer player)
      {
         base.DeactivatePlayer(player);

         if (player != null)
         {
            int row = GetRowForPlayer(player as SamplePlayer);
            if (row >= 0)
            { 
               for (int i = 0; i < PlayfieldWidth; i++)
               {
                  SampleMinionCard card = Playfield[i, row] as SampleMinionCard;
                  if (card != null)
                  {
                     card.ApplyEndOfRoundEffect(player);
                  }
               }
            }
         }
      }

      protected override void ActivatePlayer(BasePlayer player)
      {
         base.ActivatePlayer(player);

         if (player != null)
         {
            int row = GetRowForPlayer(player as SamplePlayer);
            if (row >= 0)
            {
               for (int i = 0; i < PlayfieldWidth; i++)
               {
                  SampleMinionCard card = Playfield[i, row] as SampleMinionCard;
                  if (card != null)
                  {
                     card.ApplyStartOfRoundEffect(player);
                  }
               }
            }
         }
      }
   }
}
