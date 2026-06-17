namespace BetFlowSystems.Models.DTOs.Account
{
    public class DisplayAccountDto
    {
        public int AccountID { get; set; }
        public AccState AccountStatus { get; set; }
        public string Title { get; set; }
        public decimal Balance { get; set; }
        public string? Description { get; set; }
        public DateTime CreatedDate { get; set; }
        public int UserID { get; set; }

    }
}
