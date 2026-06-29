namespace BetFlowSystems.Models.DbModels
{
    public class Transaction
    {
        public int TransactionID { get; set; }

        public int? AccountID { get; set; }
        public Account? Account { get; set; }

        public int? BetID { get; set; }
        public Bet? Bet { get; set; }

        public decimal Amount { get; set; }
        public DebitOrCredit TransactionType { get; set; }
        public DateTime TransactionDate { get; set; }
    }
}
