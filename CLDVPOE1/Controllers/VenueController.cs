using CLDVPOE1.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Azure.Storage.Blobs;

namespace CLDVPOE1.Controllers
{
    public class VenueController : Controller
    {
        private readonly DatabaseContext _context;
        public VenueController(DatabaseContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index()
        {
            var Venue = await _context.Venue.ToListAsync();
            return View(Venue);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Venue venue)
        {
            if (ModelState.IsValid)
            {
                if (venue.ImageFile != null)
                {
                    var blobUrl = await UploadImageToBlobAsync(venue.ImageFile);

                    venue.ImageURL = blobUrl;
                }
                _context.Add(venue);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Venue created succesfully.";
                return RedirectToAction(nameof(Index));
            }
            return View(venue);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var venue = await _context.Venue
                .FirstOrDefaultAsync(m => m.VenueId == id);

            if (venue == null)
            {
                return NotFound();
            }

            return View(venue);
        }

        // STEP 1: Confirm Deletion (GET)
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var venue = await _context.Venue.FirstOrDefaultAsync(v => v.VenueId == id);
            if (venue == null) return NotFound();

            return View(venue);
        }

        // STEP 2: Perform Deletion (POST) - Logic to restrict the deletion of venues associated with active bookings.
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var venue = await _context.Venue.FindAsync(id);
            if (venue == null) return NotFound();

            var hasBookings = await _context.Booking.AnyAsync(b => b.VenueId == id);
            if (hasBookings)
            {
                TempData["ErrorMessage"] = "Cannot delete venue because it has existing bookings.";
                return RedirectToAction(nameof(Index));
            }

            _context.Venue.Remove(venue);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Venue deleted successfully.";
            return RedirectToAction(nameof(Index));
        }
        private bool VenueExist(int id)
        {
            return _context.Venue.Any(v => v.VenueId == id);
        }
        
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var Venue = await _context.Venue.FindAsync(id);
            if (Venue == null)
            {
                return NotFound();
            }
            return View(Venue);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Venue venue)
        {
            if (id != venue.VenueId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    if (venue.ImageFile != null)
                    {
                        var blobUrl = await UploadImageToBlobAsync(venue.ImageFile);

                        venue.ImageURL = blobUrl;
                    }
                    else
                    {

                    }
                    _context.Update(venue);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Venue updated succesfully.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!VenueExist(venue.VenueId))
                        return NotFound();
                    else
                        throw;
                }
                
                return RedirectToAction(nameof(Index));
            }
            return View(venue);
        }
        private async Task<string> UploadImageToBlobAsync(IFormFile ImageFile)
        {
            var connectionString = "DefaultEndpointsProtocol=https;AccountName=poeimagestorage;AccountKey=32Lw+Lpwkn7Ia2Zi2pb/R51uq1guUOUn4VXA+bIHsCDjvzzt42LdpNhTgDvXRVo0Q5fg24vvDAsS+AStQcXbKw==;EndpointSuffix=core.windows.net";
            var containerName = "images";

            var blobServiceClient = new BlobServiceClient(connectionString);
            var containerClient = blobServiceClient.GetBlobContainerClient(containerName);
            var blobClient = containerClient.GetBlobClient(Guid.NewGuid() + Path.GetExtension(ImageFile.FileName));

            var blobHttpHeaders = new Azure.Storage.Blobs.Models.BlobHttpHeaders
            {
                ContentType = ImageFile.ContentType
            };

            using (var stream = ImageFile.OpenReadStream())
            {
                await blobClient.UploadAsync(stream, new Azure.Storage.Blobs.Models.BlobUploadOptions
                {
                    HttpHeaders = blobHttpHeaders
                });
            }
            return blobClient.Uri.ToString();
        }
        private bool VenueExists(int id)
        {
            return _context.Venue.Any(e => e.VenueId == id);
        }
    }
}
