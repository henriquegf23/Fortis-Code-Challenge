using five_cards_poker.Models;
using five_cards_poker.Repository.Interfaces;
using five_cards_poker.Services.Interfaces;

namespace five_cards_poker.Services
{
    public class DeckServices: IDeckServices
    {
        public DeckServices()
        {

        }

        //Suffle method
        public Deck ShuffleCards(Deck deck, int? shuffleSeed)
        {
            Random random;
            if (shuffleSeed == null)
                random = new Random();
            else
                random = new Random(shuffleSeed.Value);
                

            int totalCards = deck.cards.Count;
            while (totalCards > 1)
            {
                totalCards--;
                int aux = random.Next(totalCards + 1);
                var auxCard = deck.cards[aux];
                deck.cards[aux] = deck.cards[totalCards];
                deck.cards[totalCards] = auxCard;
            }

            return deck;
        }
    }
}
