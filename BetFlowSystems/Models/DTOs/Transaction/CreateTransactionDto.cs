namespace BetFlowSystems.Models.DTOs.Transaction
{
    public class CreateTransactionDto
    {

        public int? AccountID { get; set; }
        public int BetID { get; set; }
        public decimal Amount { get; set; }
        public DebitOrCredit TransactionType { get; set; }
    }
}
