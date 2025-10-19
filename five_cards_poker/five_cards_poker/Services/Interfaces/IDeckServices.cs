using five_cards_poker.Models;

namespace five_cards_poker.Services.Interfaces
{
    public interface IDeckServices
    {
        public Deck ShuffleCards(Deck deck, int? shuffleSeed);
    }
}
