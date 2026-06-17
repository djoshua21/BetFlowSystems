using BetFlowSystems.Models.DbModels;

namespace BetFlowSystems.Models.DTOs.User
{
    public class UpdateUserDto
    {
        public int UserID { get; set; }
        public bool IsActive { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public string Address { get; set; }


    }
}
