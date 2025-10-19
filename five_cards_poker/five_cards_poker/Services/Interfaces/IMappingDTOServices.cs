using five_cards_poker.Models;
using five_cards_poker.Models.DTOs;

namespace five_cards_poker.Services.Interfaces
{
    public interface IMappingDTOServices
    {

        public PlayerDTO ConvertPlayerToDTO(Player p);

        public CardDTO ConvertCardToDTO(Card c);
    }
}
