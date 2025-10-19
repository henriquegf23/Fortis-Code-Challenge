using five_cards_poker.Models;
using five_cards_poker.Models.DTOs;

namespace five_cards_poker.Services.Interfaces
{
    public interface IPokerActionsService
    {
        public Deck ShuffleDeck(Deck deck, int? shuffleSeed);

        public List<List<Card>> GiveCardsToPlayers(int numOfPlayers, int? shuffleSeed);
        public ValidHand ValidateHand(List<Card> hand);
        public ValidHand ValidateMultipleHands(List<List<Card>> hands);
        public PlayerDTO EvaluateHandAndTranslateScore(List<Card> hand);
        public List<PlayerDTO> EvaluateMultiHandsAndTranslateScore(List<List<Card>> hands);

        public HandDetails EvaluateHand(List<Card> playersHand);

        public Score TranslateScore(HandDetails handDetails);
        public Table ComparePlayersHands(List<List<Card>> playersHands);

        public Table GetTableResult(List<PlayerDTO> tablePlayers);

        public List<int> GetCardsOutOfEqualsGroupDesc(HandDetails hd, int maxEqualCardsRank);

        public PlayerDTO ConvertPlayerToDTO(Player p);
    }
}
