namespace BetFlowSystems.Models.DbModels
{
    public class Account
    {
        public int AccountID { get; set; }
        public AccState AccountStatus { get; set; }
        public string Title { get; set; }
        public decimal Balance { get; set; }
        public string? Description { get; set; }
        public DateTime CreatedDate { get; set; }

        public int UserID { get; set; }
        public User User { get; set; }

        public ICollection<Bet> Bets { get; set; } = new List<Bet>();
        public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    }
}
