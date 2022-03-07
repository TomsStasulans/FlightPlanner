using FlightPlanner.Data;
using FlightPlanner.Models;
using FlightPlanner.Storage;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FlightPlanner.Controllers
{
    [Route("admin-api")]
    [ApiController]
    [Authorize]
    public class AdminApiController : ControllerBase
    {
        private static readonly object _lock = new();
        private readonly IFlightPlannerDbContext _context;

        public AdminApiController(IFlightPlannerDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Route("flights/{id}")]
        public IActionResult GetFlights(int id)
        {
            lock (_lock)
            {
                var flight = _context.Flights
                    .Include(f => f.From)
                    .Include(f => f.To)
                    .SingleOrDefault(f => f.Id == id);
                if (flight == null)
                {
                    return NotFound();
                }

                return Ok(flight);
            }
        }

        [HttpDelete]
        [Route("Flights/{id}")]
        public IActionResult DeleteFlights(int id)
        {
            lock (_lock)
            {
                var flight = _context.Flights
                    .Include(f => f.From)
                    .Include(f => f.To)
                    .SingleOrDefault(f => f.Id == id);
                if (flight != null)
                {
                    _context.Flights.Remove(flight);
                    _context.SaveChanges();
                }

                return Ok();
            }
        }

        [HttpPut]
        [Route("flights")]
        [Authorize]
        public IActionResult PutFlights(AddFlightRequest request)
        {
            lock (_lock)
            {
                if (!FlightStorage.IsValid(request))
                {
                    return BadRequest();
                }

                if (Exists(request))
                {
                    return Conflict();
                }

                var flight = FlightStorage.ConvertToFlight(request);
                _context.Flights.Add(flight);
                _context.SaveChanges();
                return Created("", flight);
            }
        }

        private bool Exists(AddFlightRequest request)
        {
            return _context.Flights
                .Include(f => f.To)
                .Include(f => f.From)
                .Any(flight =>
                flight.Carrier.ToLower().Trim() == request.Carrier.ToLower().Trim() &&
                flight.DepartureTime == request.DepartureTime &&
                flight.ArrivalTime == request.ArrivalTime &&
                flight.To.AirportName.ToLower().Trim() == request.To.AirportName.ToLower().Trim() &&
                flight.From.AirportName.ToLower().Trim() == request.From.AirportName.ToLower().Trim());
        }
    }
}
