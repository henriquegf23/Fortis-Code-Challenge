namespace five_cards_poker.Models
{
    public class Score
    {
        public int Rank { get; set; }
        public string Description { get; set; }
        public List<int> tiebreaker { get; set; } 
    }
}
