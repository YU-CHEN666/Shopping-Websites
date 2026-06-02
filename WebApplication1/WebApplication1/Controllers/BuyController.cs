using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WebApplication1.Models;
using WebApplication1.Service;
using WebApplication1.ViewModel;
//購物車、購物紀錄、付款方式頁面
namespace WebApplication1.Controllers
{
    public class BuyController : Controller
    {
        private readonly Database _database;
        private readonly ShoppingCarManage _shoppingCarManage;
		private readonly ILogger<BuyController> _logger;
		public BuyController(Database database, ShoppingCarManage shoppingCarManage, ILogger<BuyController> logger)
        {
            _database = database;
            _shoppingCarManage = shoppingCarManage;
			_logger = logger;
        }

        [HttpPost]
        public IActionResult ShoppingCar()
        {
			TempData.Remove("payHasResult");
			List<ShoppingCarViewModel> buyList = _shoppingCarManage.GetShoppingProducts();
			return View(buyList);
		}

		[HttpPost]
		public IActionResult PaymentMethod()
		{
			TempData.Keep("payHasResult");
			if(TempData["payHasResult"] is null)
			{
				string[] paypaymentMethodArray = { "7-11取貨付款", "Line Pay", "信用卡" };
				TempData["payHasProcess"] = "Notyet";
				return View(paypaymentMethodArray);
			}
			return RedirectToAction(nameof(HomeController.HomePage), nameof(HomeController).Replace("Controller", ""));
		}

		[HttpPost]
		public IActionResult PayResult(string paymentMethod)
		{
			if(TempData["payHasProcess"] is not null)
			{
				TempData["payHasResult"] = "Yes";
				if (_shoppingCarManage.RecordToDatebase())
				{
					ViewData["result"] = true;
					ViewData["message"] = "付款成功";
					return View("ProcessResult");
				}
				else
				{
					_logger.LogWarning("{warningTime}付款方式:{paymentMethod}失敗", DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"),paymentMethod);
					ViewData["result"] = false;
					ViewData["message"] = "付款失敗請聯絡客服";
					return View("ProcessResult");
				}
			}
			return RedirectToAction(nameof(HomeController.HomePage), nameof(HomeController).Replace("Controller", ""));
		}

		[HttpPost]
		[Authorize]
		public IActionResult BuyRecord()
		{
			if (User.Identity.IsAuthenticated)
			{
				string userName = User.FindFirstValue(ClaimTypes.Name);
				List<ShoppingRecord> result = _database.SearchColumnSpecificValue<ShoppingRecord>(nameof(ShoppingRecord), nameof(ShoppingRecord.UserName), userName);
				return View(result);
			}
			return View(new List<ShoppingRecord>());
		}
	}
}
