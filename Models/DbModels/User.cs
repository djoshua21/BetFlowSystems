namespace BetFlowSystems.Models.DbModels
{
    public class User

    {
        public int UserID { get; set; }
        public string IdNumber { get; set; }
        public bool IsActive { get; set; } = true;
        public string Name { get; set; }
        public string Surname { get; set; }
        public string Address { get; set; }
        public DateTime CreatedDate { get; set; }

        public ICollection<Account> Accounts { get; set; } = new List<Account>();

    }
}
