namespace BetFlowSystems.Models.DTOs.Transaction
{
    public class DisplayTransactionDto
    {

        public int TransactionID { get; set; }
        public int? AccountID { get; set; }
        public int? BetID { get; set; }
        public decimal Amount { get; set; }
        public DebitOrCredit TransactionType { get; set; }
        public DateTime TransactionDate { get; set; }

    }
}
