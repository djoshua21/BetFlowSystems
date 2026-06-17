namespace BetFlowSystems.Models.DTOs.Admin
{
    public class UpdateAdminDto
    {

        public string AdminId { get; set; } 

        public string Email { get; set; }

        public string Role { get; set; }

        public bool IsLocked { get; set; }

        public string? NewPassword { get; set; }

        public string? ConfirmPassword { get; set; }


    }
}
