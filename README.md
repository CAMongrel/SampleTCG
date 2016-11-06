# SampleTCG

## Overview

This project is a sample trading card game implemented using version 0.1 of the base TCG library MyTCGLib (https://github.com/CAMongrel/MyTCGLib). 

The game is **very** loosely based on Hearthstone by Blizzard. The base layout of the board is a grid of seven-by-two cells, with two heroes on each side. The resource is mana which increases every turn by one until it reaches its maximum of ten. Every card costs mana, with the possible card types being minions and spells. Minions can attack other minions or the enemy hero and have an attack and a health value. A minion dies when its health reaches zero (or below). 
Each player has a deck of 30 cards and each turn a new card is drawn from the deck onto the hand. The full collection of procedurally generated cards is larger than that (although not by much), so not every deck will be the same. 

The game is won when the other heroes' health is reduced to zero.

## Compiling, Running and supported Platforms

Supported platforms are Windows, Linux and macOS. The project is a text-only console project, which means it should (and does) compile out of the box on any of the supported platforms.

Simply open the SampleTCG.sln file in either Visual Studio (2015), MonoDevelop or Xamarin Studio. Compile & run & have fun.

## Available cards

The cards are procedurally generated and follow very simple rules:

### Minion cards

Minions cost anywhere from 1 to 10 mana and will have either "attack==mana & health==mana+1" or "attack==mana+1 & health==mana", which means that some of the minions are more offensively oriented than the others.

### Spell cards

Spells come in two types: Healing spells and damage spells. Healing spells heal for "mana==health", whereas damage spells cause damage for "mana==damage".

### The full collection

The full collection contains 40 cards: 10 offensive minions, 10 defensive minions, 10 healing spells, 10 damage spells.

At the beginning of each match, the deck of 30 cards is randomly filled from the collection.

## Strategic depth

In short: There is none, since you can't create your own deck.

## Tactical depth

The tactical depth is very limited, although there are *some* tactical moves you can do, however those are mostly limited to using your spells and minions correctly.

## AI

SampleTCG has a very simple AI. It's as good as you'd expect from two hours of working on it. 

## Screenshot

Surprisingly enough, SampleTCG is a console (no, not *that* kind of console) game, so the graphics aren't pretty. I did, however, spend some time on making the text display of the board somewhat nice, which you can see on the following screenshot:

![Awesome graphics](/Screenshots/main.JPG?raw=true "Awesome graphics")

