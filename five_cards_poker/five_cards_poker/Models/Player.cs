using five_cards_poker.Models.Interfaces;

namespace five_cards_poker.Models
{
    public class Player : IPlayer
    {
        public int Id { get; set; }
        public List<Card>? Hand { get; set; }

        public HandDetails HandDetails { get; set; }
        public Score Score { get; set; }

    }
}
