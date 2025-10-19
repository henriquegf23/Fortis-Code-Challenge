using five_cards_poker.Models.Interfaces;

namespace five_cards_poker.Models
{
    public class Card : ICard
    {
        public int Value { get; set; }
        public string Name { get; set; }
        public int? AlternativeValue { get; set; }
        public string? Suit { get; set; }
    }
}
