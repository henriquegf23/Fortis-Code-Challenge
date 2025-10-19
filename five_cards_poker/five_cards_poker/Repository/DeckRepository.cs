using Microsoft.Extensions.Configuration;
using five_cards_poker.Models;
using five_cards_poker.Models.Interfaces;
using Microsoft.Extensions.Options;
using five_cards_poker.Repository.Interfaces;

namespace five_cards_poker.Repository
{
    public class DeckRepository: IDeckRepository
    {
        private readonly IConfiguration _configuration;

        public DeckRepository(IConfiguration configuration) 
        {
            _configuration = configuration;
        }

        public List<Card> BuildDeck() 
        {
            var cards = GetCards();
            var suits = GetSuits();

            return suits.SelectMany(suit => cards.Select(card => new Card
            {
                Value = card.Value,
                Name = card.Name,
                AlternativeValue = card.AlternativeValue,
                Suit = suit
            })).ToList();

        }

        public List<Card> GetCards()
        {
            return _configuration.GetSection("DeckConfig:Cards").Get<List<Card>>();
        }

        public List<string> GetSuits()
        {
            return _configuration.GetSection("DeckConfig:Suits").Get<List<string>>();
        }

    }
}
