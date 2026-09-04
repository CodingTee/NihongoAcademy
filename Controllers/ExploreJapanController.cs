using Microsoft.AspNetCore.Mvc;
using NihongoAcademy.Models.ExploreJapan;
using NihongoAcademy.Services;

namespace NihongoAcademy.Controllers
{
    public class ExploreJapanController : Controller
    {
        public IActionResult Index()
        {
            ViewData["Title"] = "Explore Japan";
            return View();
        }

        // ---- Category pages ---------------------------------------------------

        public IActionResult Regions()
        {
            var regions = JapanRegions.AllOrdered;
            ViewData["Title"] = "Regions of Japan";
            ViewData["SubtitleJa"] = "9つの地方";
            ViewData["Lead"] = "Tap a region to dive into its geography, capital, highlights, cuisine, and festivals.";
            return View("Regions", regions);
        }

        public IActionResult Food()
            => CategoryPage("Food", "bi-cup-hot", "食べ物",
                "Six iconic dishes you'll encounter traveling from Hokkaido to Okinawa.");

        public IActionResult Festivals()
            => CategoryPage("Festivals", "bi-music-note-beamed", "祭り",
                "Six matsuri (festivals) celebrated across Japan's seasons.");

        public IActionResult Dialects()
            => CategoryPage("Dialects", "bi-chat-quote", "方言",
                "Six regional flavors of spoken Japanese, from Kansai theatre to Okinawan roots.");

        public IActionResult Landmarks()
            => CategoryPage("Landmarks", "bi-building", "名所",
                "Six landmarks that define the visual identity of Japan.");

        public IActionResult Culture()
            => CategoryPage("Culture", "bi-heart", "文化",
                "Six living traditions — arts, crafts, and ceremonies passed down for centuries.");

        // ---- Region detail page ----------------------------------------------

        [Route("ExploreJapan/Regions/{slug}")]
        public IActionResult RegionDetail(string slug)
        {
            var region = JapanRegions.FindBySlug(slug);
            if (region is null) return NotFound();

            var (prev, next) = JapanRegions.Neighbors(slug);
            ViewData["Title"] = $"{region.Name} — Regions";
            ViewData["SubtitleJa"] = region.NameJa;
            ViewData["Lead"] = region.Tagline;
            ViewData["PrevRegion"] = prev;
            ViewData["NextRegion"] = next;
            return View("RegionDetail", region);
        }

        // ---- Shared rendering ------------------------------------------------

        private IActionResult CategoryPage(string category, string icon, string ja, string lead)
        {
            var items = ExploreJapanContent.For(category) ?? new List<ExploreItem>();
            ViewData["Title"] = category;
            ViewData["Icon"] = icon;
            ViewData["SubtitleJa"] = ja;
            ViewData["Lead"] = lead;
            return View(category, items);
        }
    }
}