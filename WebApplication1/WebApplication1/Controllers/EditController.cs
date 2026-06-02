using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using WebApplication1.Models;
using WebApplication1.Service;
using WebApplication1.ViewModel;
//負責接收新增商品、修改商品、刪除商品表單
namespace WebApplication1.Controllers
{
    public class EditController : Controller
    {
        private readonly Database _database;
        private readonly FileProcess _fileProcess;
        public EditController(Database database,FileProcess fileProcess)
        {
            _database = database;
            _fileProcess = fileProcess;
        }

        [HttpPost]
		[Authorize]
		public IActionResult AddProductPage()
        {
            return View("AddProduct");
        }

        [HttpPost]
		[Authorize]
		public IActionResult AddProduct(AddProductViewModel dataModel)
        {
            if (ModelState.IsValid)
            {
                string? productID;
                productID = Guid.NewGuid().ToString();
				if (!_fileProcess.SaveFile(dataModel.Picture, productID))
				{
					//檔案儲存到伺服器時發生錯誤
					ViewData["result"] = false;
					ViewData["message"] = "很抱歉你上傳的檔案有問題，請重新回到新增商品頁面再次嘗試";
					return View("ProcessResult");
				}
				if (!_database.AddDataForProduct(dataModel, productID))
                {
                    //資料存入資料庫時發生錯誤
                    ViewData["result"] = false;
                    ViewData["message"] = "很抱歉你輸入的資料有問題，請重新回到新增商品頁面再次嘗試";
                    return View("ProcessResult");
                }
				ViewData["result"] = true;
                ViewData["message"] = "成功新增";
                return View("ProcessResult");
            }
            return View(dataModel);
        }

        [HttpPost]
		[Authorize]
		public IActionResult EditProduct([FromForm] string? idSelected, EditProductViewModel dataModel)
        {
			//按鈕的路由
			if (idSelected is not null)
            {
                var result = _database.SearchColumnSpecificValue<EditProductViewModel>(nameof(Product), nameof(Product.ID), idSelected);
                return View(result[0]);
            }
            if (ModelState.IsValid)
            {
                if (!_database.UpdateProduct(dataModel))
                {
                    //資料庫更新資料時發生錯誤
                    ViewData["result"] = false;
                    ViewData["message"] = "很抱歉你輸入的資料有問題，請重新回到修改商品頁面再次嘗試";
                    return View("ProcessResult");
                }
                if (dataModel.Picture is not null)
                {
                    if (!_fileProcess.SaveFile(dataModel.Picture, dataModel.ID))
                    {
                        //檔案儲存到伺服器時發生錯誤
                        ViewData["result"] = false;
                        ViewData["message"] = "很抱歉你上傳的檔案有問題，請重新回到修改商品頁面再次嘗試";
                        return View("ProcessResult");
                    }
                }
                ViewData["result"] = true;
                ViewData["message"] = "成功修改";
                return View("ProcessResult");
            }
            return View(dataModel);
        }
       
        [HttpPost]
		[Authorize(Policy = "OnlyRoot")]
		public IActionResult DeleteProduct([FromForm] string idSelected, [FromForm] string idDeleted)
        {
            if(idSelected is not null)
            {
                var results = _database.SearchColumnSpecificValue<DeleteProductViewModel>(nameof(Product), nameof(DeleteProductViewModel.ID), idSelected);
                return View(results[0]);
            }
            if(idDeleted is not null)
            {
                if(!_database.Delete(idDeleted))
                {
                    ViewData["result"] = false;
                    ViewData["message"] = "商品刪除失敗，請重新回到刪除商品頁面再次嘗試";
                    return View("ProcessResult");
                }
                if(!_fileProcess.DeleteFile(idDeleted))
                {
                    ViewData["result"] = false;
                    ViewData["message"] = "商品刪除失敗，請重新回到刪除商品頁面再次嘗試";
                    return View("ProcessResult");
                }
            }
            ViewData["result"] = true;
            ViewData["message"] = "成功刪除";
            return View("ProcessResult");
        }
	}
}
