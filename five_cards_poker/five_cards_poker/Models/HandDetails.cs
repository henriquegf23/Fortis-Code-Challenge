namespace five_cards_poker.Models
{
    public class HandDetails
    {

        public Dictionary<string, int> ValidateSuits { get; set; }

        public int IsThereAces { get; set; }
        public Dictionary<int, int> ValidateEquals { get; set; }
        public bool IsInOrder { get; set; }

        public int SumOrder { get; set; }
        public int HighestCard { get; set; }
    }
}
