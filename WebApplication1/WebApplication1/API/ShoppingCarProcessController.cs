using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Service;
namespace WebApplication1.API
{
	[Route("api/[controller]")]
	[ApiController]
	public class ShoppingCarProcessController : ControllerBase
	{
		private readonly ShoppingCarManage _shoppingCarManage;
		public ShoppingCarProcessController(ShoppingCarManage shoppingCarManage)
		{
			_shoppingCarManage = shoppingCarManage;
		}

		[HttpPost]
		[Route("Check")]
		public async Task<ActionResult<string>> CheckProductExist([FromForm] string idSelected)
		{
			if(_shoppingCarManage.CheckExist(idSelected))
			{
				return Content("Yes");
			}
			return Content("No");
		}
		[HttpPost]
		[Route("Add")]
		public ActionResult<string> AddProduct([FromForm] string idSelected, [FromForm] int buyNumber)
		{
			if(_shoppingCarManage.AddProduct(idSelected, buyNumber))
			{
				return Content("success");
			}
			return Content("false");
		}

		[HttpPost]
		[Route("Edit")]
		public ActionResult<string> EditProductCount([FromForm] string idSelected, [FromForm] int buyNumber)
		{
			if (_shoppingCarManage.EditProductCount(idSelected,buyNumber))
			{
				return Content("success");
			}
			return Content("false");
		}

		[HttpPost]
		[Route("Delete")]
		public ActionResult<string> DeleteProduct([FromForm] string idSelected)
		{
			if (_shoppingCarManage.DeleteProduct(idSelected))
			{
				return Content("success");
			}
			return Content("false");
		}

		[HttpPost]
		[Route("Count")]
		public ActionResult<string> GetProductCount()
		{
			string count = _shoppingCarManage.GetCount().ToString();
			return count;
		}
	}
}
