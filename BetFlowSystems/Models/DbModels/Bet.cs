namespace BetFlowSystems.Models.DbModels
{
    public class Bet
    {
        public int BetID { get; set; }

        public int? AccountID { get; set; }
        public Account? Account { get; set; }
        public int BetTypeID { get; set; }
        public BetType BetType { get; set; }

        public decimal BetAmount { get; set; }
        public decimal PossibleWinAmount { get; set; }
        public BetStatus Result { get; set; }

        public DateTime BetDate { get; set; }
        public DateTime LastUpdatedDate { get; set; }

        public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    }
}
