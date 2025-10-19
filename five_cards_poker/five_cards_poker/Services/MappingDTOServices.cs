using five_cards_poker.Models;
using five_cards_poker.Models.DTOs;
using five_cards_poker.Services.Interfaces;

namespace five_cards_poker.Services
{
    public class MappingDTOServices : IMappingDTOServices
    {

        private static readonly Dictionary<int, string> _cardNames = new()
        {
            {2, "2"}, {3, "3"}, {4, "4"}, {5, "5"}, {6, "6"}, {7, "7"},
            {8, "8"}, {9, "9"}, {10, "10"}, {11, "Jack"}, {12, "Queen"},
            {13, "King"}, {14, "Ace"}
        };

        public PlayerDTO ConvertPlayerToDTO(Player p)
        {
            int id = p.Id;
            Score score = p.Score;
            int tiebreaker = p.HandDetails.HighestCard;
            List<CardDTO> hand = new List<CardDTO>();

            foreach (Card c in p.Hand)
            {
                hand.Add(new CardDTO { Rank = c.Name, Suit = c.Suit });
            }


            return new PlayerDTO
            {
                Id = id,
                Score = score,
                Hand = hand
            };
        }

        public CardDTO ConvertCardToDTO(Card c)
        {
            CardDTO cardDTO = new CardDTO();

            cardDTO.Suit = c.Suit;

            if (_cardNames.TryGetValue(c.Value, out string name))
                cardDTO.Rank = name;
            else
                cardDTO.Rank = c.Name;

            return cardDTO;
        }
    }
}
