namespace five_cards_poker.Models.DTOs
{
    public class PlayerDTO 
    {
        public int? Id { get; set; }
        public List<CardDTO> Hand { get; set; }
        public Score Score { get; set; }
    }
}
