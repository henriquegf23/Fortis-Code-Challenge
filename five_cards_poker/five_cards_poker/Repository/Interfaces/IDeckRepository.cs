using five_cards_poker.Models;

namespace five_cards_poker.Repository.Interfaces
{
    public interface IDeckRepository
    {
        public List<Card> BuildDeck();

        public List<Card> GetCards();
        public List<string> GetSuits();
    }
}
