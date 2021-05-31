using System;
using System.Collections.Generic;
using System.Linq;
using static Glyph.Utilities.InitializeArray;
using static Glyph.Utilities.ArraySwap;

namespace Glyph.Combat
{
    public class CardSystem
    {
        Card[] DeckToPop;
        public List<Card> Deck { get; private set; }

        public int? currentCardID
        {
            get
            {
                if (Deck.Count > 0)
                {
                    return Deck[0].ID;
                }
                return null;
            }
        }

        public CardSystem()
        {
            DeckToPop = PopulateArray<Card>(10);
            Deck = DeckToPop.ToList();
        }

        public void PopulateDeck()
        {
            for (int i = 0; i < DeckToPop.Length; i++)
            {
                DeckToPop[i].ID = i + 1; 
            }
            Deck = DeckToPop.ToList();
        }

        public void GetNextCardInDeck()
        {
            Deck.Add(Deck[0]);
            TakeCardFromDeck(); 
        }

        public void TakeCardFromDeck()
        {
            Deck.RemoveAt(0);
        }

        public void PlaceCardOnDeck(Card card)
        {
            Deck.Insert(0, card);
        }

        public void SortCards()
        {
            Deck.Sort();
        }

        public void SwapCards(int indexOne, int indexTwo)
        {
            Swap(Deck, indexOne, indexTwo); 
        }

        public void Shuffle()
        {
            for (int i = 0; i < Deck.Count; i++)
            {
                Random random = new Random();
                int randIndex = random.Next(0, Deck.Count);
                Swap(Deck, i, randIndex);
            }
        }

    }
}
