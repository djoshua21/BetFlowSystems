namespace BetFlowSystems.Models.DTOs.Admin
{
    public class CreateAdminDto
    {

        public string Email { get; set; }

        public string Password { get; set; }

        public string ConfirmPassword { get; set; }

        public string Role { get; set; } = "Clerk";

    }
}
