using BetFlowSystems.Models.DbModels;

namespace BetFlowSystems.Models.DTOs.Bet
{
    public class DisplayBetDto
    {

        public int BetID { get; set; }

        public int? AccountID { get; set; }
        public int BetTypeID { get; set; }
        public decimal BetAmount { get; set; }
        public decimal PossibleWinAmount { get; set; }
        public BetStatus Result { get; set; }

        public DateTime BetDate { get; set; }
        public DateTime LastUpdatedDate { get; set; }

    }
}
