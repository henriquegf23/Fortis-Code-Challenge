using five_cards_poker.Models.Interfaces;
using five_cards_poker.Repository;
using five_cards_poker.Repository.Interfaces;
using System.Runtime.CompilerServices;

namespace five_cards_poker.Models
{
    public class Deck : IDeck
    {
        private readonly IDeckRepository _deckRepository;

        //Array 52 cards
        public List<Card> cards { get; set; }

        public Deck(IDeckRepository deckRepository) 
        {
            _deckRepository = deckRepository;

            cards = _deckRepository.BuildDeck();
        }
    }
}
