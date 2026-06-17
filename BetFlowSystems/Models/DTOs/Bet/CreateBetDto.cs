namespace BetFlowSystems.Models.DTOs.Bet
{
    public class CreateBetDto
    {

        public int AccountID { get; set; }
        public int BetTypeID { get; set; }

        public decimal BetAmount { get; set; }
        public decimal PossibleWinAmount { get; set; }

        public DateTime BetDate { get; set; }


    }
}
