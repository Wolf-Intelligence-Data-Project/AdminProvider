namespace AdminProvider.OrdersManagement.Models.Requests
{
    public class SearchRequest
    {
        public string Query { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;

        public string? StartDate { get; set; }
        public string? EndDate { get; set; }

        public List<SortCriteria> SortCriteria { get; set; } = new List<SortCriteria>(); // Allow multiple sorting criteria
    }

}

