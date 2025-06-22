using CLDVPOE1.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CLDVPOE1.Controllers
{
    public class BookingController : Controller
    {
        private readonly DatabaseContext _context;
        public BookingController(DatabaseContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index(string searchString)
        {
            var Booking = _context.Booking
                 .Include(b => b.Event)
                 .Include(b => b.Venue)
                .AsQueryable();
            if (!string.IsNullOrEmpty(searchString))
            {
                Booking = Booking.Where(b =>
                b.Venue.VenueName.Contains(searchString) ||
                b.Event.EventName.Contains(searchString) );
            }
                return View(await Booking.ToListAsync());
        }

        public IActionResult Create()
        {
            ViewData["Events"] = _context.Event.ToList();
            ViewData["Venues"] = _context.Venue.ToList();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Booking booking)
        {
            var selectedEvent = await _context.Event.FirstOrDefaultAsync(e => e.EventId == booking.EventId);

            if (selectedEvent == null)
            {
                ModelState.AddModelError("", "Selected event not found.");
                ViewData["Events"] = _context.Event.ToList();
                ViewData["Venues"] = _context.Venue.ToList();
                return View(booking);
            }

            // Check manually for double booking
            var conflict = await _context.Booking
                .Include(b => b.Event)
                .AnyAsync(b => b.VenueId == booking.VenueId &&
                               b.Event.EventDate.Date == selectedEvent.EventDate.Date);

            if (conflict)
            {
                ModelState.AddModelError("", "This venue is already booked for that date.");
                ViewData["Events"] = _context.Event.ToList();
                ViewData["Venues"] = _context.Venue.ToList();
                return View(booking);
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Add(booking);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Booking created successfully.";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException ex)
                {
                    // If database constraint fails (e.g., unique key violation), show friendly message
                    ModelState.AddModelError("", "This venue is already booked for that date.");
                    ViewData["Events"] = _context.Event.ToList();
                    ViewData["Venues"] = _context.Venue.ToList();
                    return View(booking);
                }
            }

            ViewData["Events"] = _context.Event.ToList();
            ViewData["Venues"] = _context.Venue.ToList();
            return View(booking);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var booking = await _context.Booking
                .Include(b => b.Event)
                .Include(b => b.Venue)
                .FirstOrDefaultAsync(m => m.BookingId == id);

            if (booking == null)
            {
                return NotFound();
            }

            return View(booking);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            var Booking = await _context.Booking.FirstOrDefaultAsync(m => m.BookingId == id);

            {
                if (Booking == null)
                {
                    return NotFound();
                }
                return View(Booking);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var Booking = await _context.Booking.FindAsync(id);
            _context.Booking.Remove(Booking);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool BookingExist(int id)
        {
            return _context.Booking.Any(b => b.BookingId == id);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var Booking = await _context.Booking.FindAsync(id);
            if (Booking == null)
            {
                return NotFound();
            }
            return View(Booking);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, Booking booking)
        {
            if (id != booking.BookingId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(booking);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BookingExist(booking.BookingId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }

                }
                return RedirectToAction(nameof(Index));
            }
            return View(booking);
        }
    }
}
