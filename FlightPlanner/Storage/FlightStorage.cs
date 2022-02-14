using FlightPlanner.Models;

namespace FlightPlanner.Storage
{
    public static class FlightStorage
    {
        private static List<Flight> _flights = new();
        private static int _id;
        private static readonly object _lock = new();

        public static Flight AddFlight(AddFlightRequest request)
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
                    Id = ++_id
                };

                _flights.Add(currentFlight);
                return currentFlight;
            }
        }

        public static List<Airport> SearchAirport(string search)
        {
            lock (_lock)
            {
                search = search.ToLower().Trim();
                var fromAirports = _flights.Where(f =>
                        f.From.AirportName.ToLower().Trim().Contains(search) ||
                        f.From.City.ToLower().Trim().Contains(search) ||
                        f.From.Country.ToLower().Trim().Contains(search))
                    .Select(f => f.From).ToList();

                var toAirports = _flights.Where(f =>
                        f.To.AirportName.ToLower().Trim().Contains(search) ||
                        f.To.City.ToLower().Trim().Contains(search) ||
                        f.To.Country.ToLower().Trim().Contains(search))
                    .Select(f => f.To).ToList();

                return toAirports.Concat(fromAirports).ToList();
            }
        }

        public static PageResult SearchFlights(SearchFlightRequest request)
        {
            return new PageResult(_flights);
        }

        public static Flight GetFlight(int id)
        {
            lock (_lock)
            {
                return _flights.SingleOrDefault(f => f.Id == id);
            }
        }

        public static void DeleteFlight(int id)
        {
            lock (_lock)
            {
                var flight = GetFlight(id);
                if (flight != null)
                    _flights.Remove(flight);
            }
        }

        public static void ClearFlights()
        {
            lock (_lock)
            {
                _flights.Clear();
                _id = 0;
            }
        }

        public static bool Exists(AddFlightRequest request)
        {
            lock (_lock)
            {
                return _flights.Any(flight =>
                    flight.Carrier.ToLower().Trim() == request.Carrier.ToLower().Trim() &&
                    flight.DepartureTime == request.DepartureTime &&
                    flight.ArrivalTime == request.ArrivalTime &&
                    flight.To.AirportName.ToLower().Trim() == request.To.AirportName.ToLower().Trim() &&
                    flight.From.AirportName.ToLower().Trim() == request.From.AirportName.ToLower().Trim());
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
    }
}
