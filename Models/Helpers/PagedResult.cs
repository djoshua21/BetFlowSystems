namespace BetFlowSystems.Models.Helpers
{
    public class PagedResult<T>
    {
        public List<T> Items { get; set; }
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalItems { get; set; }
        public int TotalPages =>
            TotalItems == 0 ? 1 :
            (int)Math.Ceiling((double)TotalItems / PageSize);

    }
}
