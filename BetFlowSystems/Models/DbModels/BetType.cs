namespace BetFlowSystems.Models.DbModels
{
    public class BetType
    {
        public int BetTypeID { get; set; }
        public string Sport { get; set; }
        public string EventName { get; set; }
        public string? Description { get; set; }
        public ICollection<Bet> Bets { get; set; } = new List<Bet>();
    }
}
