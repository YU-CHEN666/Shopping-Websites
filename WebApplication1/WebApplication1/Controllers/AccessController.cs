using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WebApplication1.Models;
using WebApplication1.Service;
using WebApplication1.ViewModel;
using BC = BCrypt.Net.BCrypt;
//使用者登入、登出
namespace WebApplication1.Controllers
{
    public class AccessController : Controller
    {
        private readonly Database _database;
        public AccessController(Database database)
        {
            _database = database;
        }
        
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel dataModel)
        {
            if (ModelState.IsValid)
            {
                var user = _database.SearchColumnSpecificValue<User>(nameof(WebApplication1.Models.User), nameof(WebApplication1.Models.User.Account), dataModel.Account);
                if (user.Any())
                {
                    bool isCorrect = BC.Verify(dataModel.Password, user[0].Password);
                    if (isCorrect)
                    {
                        await UserInformationCreate(user[0]);
                        return RedirectToAction(nameof(HomeController.HomePage), nameof(HomeController).Replace("Controller", ""));
                    }
                    else
                    {
                        ModelState.AddModelError(nameof(dataModel.Password), "密碼錯誤");
                        return View(dataModel);
                    }
                }
                else
                {
                    ModelState.AddModelError(nameof(dataModel.Account), "帳號錯誤");
                    return View(dataModel);
                }
            }
            return View(dataModel);
        }

        [HttpGet]
        public IActionResult Prohibit()
        {
            ViewData["message"] = "您沒有權限進入";
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(LoginViewModel dataModel)
        {
            if (ModelState.IsValid)
            {
                var user = _database.SearchColumnSpecificValue<User>(nameof(WebApplication1.Models.User), nameof(WebApplication1.Models.User.Account), dataModel.Account);
                if(user.Any())
                {
                    ModelState.AddModelError(nameof(dataModel.Account), "帳號已存在");
                    return View("Login", dataModel);
                }
                else
                {
                    string hashPassword = BC.HashPassword(dataModel.Password);
                    bool isSuccess = _database.AddDataForUser(dataModel.Account, hashPassword);
                    if (isSuccess)
                    {
                        await UserInformationCreate(new User()
                        {
                            Account = dataModel.Account,
                            Role = "User",
                        });
                        return RedirectToAction(nameof(HomeController.HomePage), nameof(HomeController).Replace("Controller", ""));
                    }
                    else
                    {
                        ViewData["result"] = false;
                        ViewData["message"] = "發生錯誤無法註冊，請聯繫客服。";
                        return View("ProcessResult");
                    }
                }
            }
            return View("Login", dataModel); 
        }

        [HttpPost]
        public async Task<IActionResult> LogOut()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction(nameof(HomeController.HomePage), nameof(HomeController).Replace("Controller", ""));
        }

		[HttpGet]
		[Authorize(Policy = "OnlyRoot")]
		public IActionResult OnlyRootTest()
        {
			return RedirectToAction(nameof(HomeController.HomePage), nameof(HomeController).Replace("Controller", ""));
		}


		private async Task UserInformationCreate(User userInformation)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name,userInformation.Account),
                new Claim(ClaimTypes.Role,userInformation.Role),
            };
            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));
        }
    }
}
