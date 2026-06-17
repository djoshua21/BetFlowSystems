namespace BetFlowSystems.Models
{
    public class Result
    {
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }

        public object? Data { get; set; }
    }
}
