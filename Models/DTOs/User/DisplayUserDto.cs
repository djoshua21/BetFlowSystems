namespace BetFlowSystems.Models.DTOs.User
{
    public class DisplayUserDto
    {
        public int UserID { get; set; }
        public string IdNumber { get; set; }
        public bool IsActive { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public string Address { get; set; }
        public DateTime CreatedDate { get; set; }


    }
}
