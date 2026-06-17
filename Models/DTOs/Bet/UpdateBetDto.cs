namespace BetFlowSystems.Models.DTOs.Bet
{
    public class UpdateBetDto
    {
        public int BetID { get; set; }
        public BetStatus Result { get; set; }
    }
}
