using five_cards_poker.Models;
using five_cards_poker.Models.DTOs;

namespace five_cards_poker.Services.Interfaces
{
    public interface IPokerService
    {
        public Table PlayPoker(int numOfPlayers, int? shuffleSeed);

        public Deck ShuffleDeck(Deck deck, int? shuffleSeed);

        public List<Card> GiveCardsToPlayers(Deck deck, int maxCardOnHand);

        public HandDetails EvaluateHand(List<Card> playersHand);

        public Score TranslateScore(HandDetails handDetails);

        public Table GetTableResult(List<PlayerDTO> tablePlayers);

        public List<int> GetCardsOutOfEqualsGroupDesc(HandDetails hd, int maxEqualCardsRank);

        public PlayerDTO ConvertPlayerToDTO(Player p);
    }
}
