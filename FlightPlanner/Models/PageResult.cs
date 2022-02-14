namespace FlightPlanner.Models
{
    public class PageResult
    {
        public int Page { get; set; }
        public int TotalItems { get; set; }
        public List<Flight> Items { get; set; }

        public PageResult(List<Flight> input)
        {
            Page = 0;
            TotalItems = input.Count;
            Items = input;
        }
    }
}
