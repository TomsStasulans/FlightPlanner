using FlightPlanner.Data;
using FlightPlanner.Models;

namespace FlightPlanner.Storage
{
    public static class FlightStorage
    {
        private static readonly object _lock = new();

        public static Flight ConvertToFlight(AddFlightRequest request)
        {
            lock (_lock)
            {
                var currentFlight = new Flight
                {
                    From = request.From,
                    To = request.To,
                    ArrivalTime = request.ArrivalTime,
                    DepartureTime = request.DepartureTime,
                    Carrier = request.Carrier,
                };
                return currentFlight;
            }
        }

        public static List<Airport> SearchAirports(string search, IFlightPlannerDbContext context)
        {
            lock (_lock)
            {
                search = search.ToLower().Trim();
                var fromAirports = context.Flights.Where(f =>
                        f.From.AirportName.ToLower().Trim().Contains(search) ||
                        f.From.City.ToLower().Trim().Contains(search) ||
                        f.From.Country.ToLower().Trim().Contains(search))
                    .Select(f => f.From).ToList();

                var toAirports = context.Flights.Where(f =>
                        f.To.AirportName.ToLower().Trim().Contains(search) ||
                        f.To.City.ToLower().Trim().Contains(search) ||
                        f.To.Country.ToLower().Trim().Contains(search))
                    .Select(f => f.To).ToList();

                return toAirports.Concat(fromAirports).ToList();
            }
        }

        public static bool IsValid(AddFlightRequest request)
        {
            lock (_lock)
            {
                if (request == null)
                    return false;

                if (string.IsNullOrEmpty(request.To.AirportName) ||
                    string.IsNullOrEmpty(request.To.City) ||
                    string.IsNullOrEmpty(request.To.Country) ||
                    string.IsNullOrEmpty(request.From.AirportName) ||
                    string.IsNullOrEmpty(request.From.City) ||
                    string.IsNullOrEmpty(request.From.Country) ||
                    string.IsNullOrEmpty(request.ArrivalTime) ||
                    string.IsNullOrEmpty(request.DepartureTime) ||
                    string.IsNullOrEmpty(request.Carrier))
                    return false;

                if (request.From.AirportName.ToLower().Trim() == request.To.AirportName.ToLower().Trim())
                    return false;

                var departureTime = DateTime.Parse(request.DepartureTime);
                var arrivalTime = DateTime.Parse(request.ArrivalTime);
                if (departureTime >= arrivalTime)
                    return false;

                return true;
            }
        }

        public static bool SearchIsValid(SearchFlightRequest request)
        {
            lock (_lock)
            {
                return string.IsNullOrEmpty(request.DepartureDate) ||
                       string.IsNullOrEmpty(request.From) ||
                       string.IsNullOrEmpty(request.To) ||
                       request.From.ToLower().Trim() == request.To.ToLower().Trim();
            }
        }

        public static PageResult SearchFlight(SearchFlightRequest request, IFlightPlannerDbContext context)
        {
            lock (_lock)
            {
                var search = context.Flights.Where(f =>
                    f.From.AirportName.ToLower().Trim() == request.From.ToLower().Trim() &&
                    f.To.AirportName.ToLower().Trim() == request.To.ToLower().Trim() &&
                    f.DepartureTime.Substring(0, 10) == request.DepartureDate).ToList();

                return new PageResult(search);
            }
        }
    }
}
