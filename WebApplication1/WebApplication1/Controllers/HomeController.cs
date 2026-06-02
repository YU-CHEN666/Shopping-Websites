using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Claims;
using WebApplication1.Models;
using WebApplication1.Service;
//負責首頁、收尋特定商品、修改商品清單、刪除商品清單頁面
namespace WebApplication1.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        private readonly Database _database;
        
        public HomeController(ILogger<HomeController> logger, Database db)
        {
            _logger = logger;
            _database = db;
        }

        public IActionResult HomePage()
        {
            var result = _database.GetAll<Product>(nameof(Product));
            ViewData["WhichPageCome"] = "Home";
            return View(result);
        }

        public IActionResult SearchKeyword(string keyword)
        {
            var result = _database.SearchKeyWord<Product>(nameof(Product), nameof(Product.Name), keyword);
            ViewData["WhichPageCome"] = "Home";
            return View(result);
        }

        [HttpPost]
        [Authorize]
        public IActionResult EditProductPage()
        {
            string userName = User.FindFirstValue(ClaimTypes.Name);
            var result = _database.SearchColumnSpecificValue<Product>(nameof(Product), nameof(Product.WhoAdd),userName);
            ViewData["WhichPageCome"] = "Edit";
            return View(result);
        }

        [HttpPost]
        [Authorize(Policy = "OnlyRoot")]
        public IActionResult DeleteProductPage()
        {
            var results = _database.GetAll<Product>(nameof(Product));
            ViewData["WhichPageCome"] = "Delete";
            return View(results);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
