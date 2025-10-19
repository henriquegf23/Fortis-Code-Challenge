using five_cards_poker.Models.DTOs;

namespace five_cards_poker.Models
{
    public class Table
    {
        public List<PlayerDTO> Players { get; set; }
        public List<PlayerDTO> Winners { get; set; }
        public string FinalResult { get; set; }

    }
}
