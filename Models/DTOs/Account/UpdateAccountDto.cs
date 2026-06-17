using Microsoft.Identity.Client;

namespace BetFlowSystems.Models.DTOs.Account
{
    public class UpdateAccountDto
    {
        public int AccountId { get; set; }
        public AccState AccountStatus { get; set; }
        public string Title { get; set; }
        public string? Description { get; set; }

    }
}
