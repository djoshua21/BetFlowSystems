namespace BetFlowSystems.Models.DTOs.Admin
{
    public class DisplayAdminDto
    {

        public string AdminID { get; set; }

        public string Email { get; set; }

        public string Role { get; set; }

        public bool IsLocked { get; set; }
    }
}
