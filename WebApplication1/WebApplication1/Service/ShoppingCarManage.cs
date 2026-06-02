using System.Collections.Concurrent;
using System.Security.Claims;
using WebApplication1.Models;
using WebApplication1.ViewModel;
namespace WebApplication1.Service
{
	//購物車本體，新增商品、移除商品、修改商品數量操作服務
    public class ShoppingCarManage
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, int>> _loginCar = new ConcurrentDictionary<string, ConcurrentDictionary<string, int>>();
        private readonly ConcurrentDictionary<string, Dictionary<string, int>> _anonymousCar = new ConcurrentDictionary<string, Dictionary<string, int>>();
		private readonly ConcurrentDictionary<string, int> _shoppingCarCount = new ConcurrentDictionary<string,int>();
		private readonly IServiceScopeFactory _scopeFactory;
		private readonly ILogger<ShoppingCarManage> _logger;

		public ShoppingCarManage(IHttpContextAccessor httpContextAccessor,IServiceScopeFactory serviceScopeFactory, ILogger<ShoppingCarManage> logger)
        {
            _httpContextAccessor = httpContextAccessor;
            _scopeFactory = serviceScopeFactory;
			_logger = logger;
        }

        internal List<ShoppingCarViewModel> GetShoppingProducts()
        {
			//傳回購物車內的所有商品
            var httpContext = _httpContextAccessor.HttpContext;
			List<ShoppingCarViewModel> result = new List<ShoppingCarViewModel>();
			using (IServiceScope scope = _scopeFactory.CreateScope())
            {
				Database database = scope.ServiceProvider.GetRequiredService<Database>();
				if (httpContext.User.Identity.IsAuthenticated)
				{
					string userName = httpContext.User.FindFirstValue(ClaimTypes.Name);
					if (_loginCar.TryGetValue(userName, out ConcurrentDictionary<string, int> productsDict))
					{
						foreach (var item in productsDict)
						{
							Product product = database.SearchColumnSpecificValue<Product>(nameof(Product), nameof(Product.ID), item.Key)[0];
							ShoppingCarViewModel shoppingCarViewModel = new ShoppingCarViewModel()
							{
								product = product,
								productNumber = item.Value,
							};
							result.Add(shoppingCarViewModel);
						}
						return result;
					}
				}
				else
				{
					string? anonymousID = httpContext.Request.Cookies["anonymousID"];
					if (anonymousID is not null)
					{
						if (_anonymousCar.TryGetValue(anonymousID, out Dictionary<string, int> productsDict))
						{
							foreach (var item in productsDict)
							{
								Product product = database.SearchColumnSpecificValue<Product>(nameof(Product), nameof(Product.ID), item.Key)[0];
								ShoppingCarViewModel shoppingCarViewModel = new ShoppingCarViewModel()
								{
									product = product,
									productNumber = item.Value,
								};
								result.Add(shoppingCarViewModel);
							}
							return result;
						}
					}
				}
			}
			return new List<ShoppingCarViewModel>();
		}

        internal bool AddProduct(string productID,int productCount)
        {
			//新增商品到購物車內
			using (IServiceScope scope = _scopeFactory.CreateScope())
            {
				Database database = scope.ServiceProvider.GetRequiredService<Database>();
				List<Product> checkProductID = database.SearchColumnSpecificValue<Product>(nameof(Product), nameof(Product.ID), productID);
				if (checkProductID.Any())
				{
					var httpContext = _httpContextAccessor.HttpContext;
					if (httpContext.User.Identity.IsAuthenticated)
					{
						string userName = httpContext.User.FindFirstValue(ClaimTypes.Name);
						ConcurrentDictionary<string, int> productsDict = _loginCar.GetOrAdd(userName, key => new ConcurrentDictionary<string, int>());
						if(productsDict.TryAdd(productID, productCount))
						{
							_shoppingCarCount.AddOrUpdate(userName, 1, (key, oldValue) => oldValue+1); return true;
						}
					}
					else
					{
						string? anonymousID = httpContext.Request.Cookies["anonymousID"];
						Dictionary<string, int> productsDict = _anonymousCar.GetOrAdd(anonymousID, key => new Dictionary<string, int>());
						if (productsDict.TryAdd(productID, productCount))
						{
							_shoppingCarCount.AddOrUpdate(anonymousID, 1, (key, oldValue) => oldValue + 1); return true;
						}
					}
					if (httpContext.User.Identity.IsAuthenticated) _logger.LogWarning("{warningTime}:loginCar新增商品失敗，ID:{productID}", DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"),productID);
					else _logger.LogWarning("{warningTime}:anonymousCar新增商品失敗，ID:{productID}", DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"), productID);
					return false;
				}
			}
			_logger.LogInformation("{infoTime}:找不到商品，ID:{productID}無法新增商品到購物車", DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"), productID);
            return false;
        }

        internal bool EditProductCount(string productID, int productCount)
        {
            //修改商品數量
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext.User.Identity.IsAuthenticated)
            {
                string userName = httpContext.User.FindFirstValue(ClaimTypes.Name);
                if (_loginCar.TryGetValue(userName, out ConcurrentDictionary<string, int> productsDict))
                {
                    if (productsDict.TryGetValue(productID, out int _))
                    {
                        productsDict.AddOrUpdate(productID, productCount, (key, oldValue) => productCount);
                        return true;
                    }
                }
				_logger.LogWarning("{warningTime}:loginCar修改商品數量失敗，ID:{productID}", DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"), productID);
				return false;
            }
            else
            {
                string? anonymousID = httpContext.Request.Cookies["anonymousID"];
                if (_anonymousCar.TryGetValue(anonymousID, out Dictionary<string, int> productsDict))
                {
                    productsDict[productID] = productCount;
                    return true;
                }
				_logger.LogWarning("{warningTime}:anonymousCar修改商品數量失敗，ID:{productID}", DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"), productID);
				return false;
            }
        }

        internal bool DeleteProduct(string productID)
        {
			//從購物車內移除商品
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext.User.Identity.IsAuthenticated)
            {
                string userName = httpContext.User.FindFirstValue(ClaimTypes.Name);
                if(_loginCar.TryGetValue(userName,out ConcurrentDictionary<string, int> productsDict))
                {
                    if(productsDict.TryRemove(productID, out int _))
                    {
						if(_shoppingCarCount.TryGetValue(userName, out int currentCount))
						{
							_shoppingCarCount.AddOrUpdate(userName, currentCount - 1, (key, oldValue) => oldValue - 1);
							return true;
						}
                    }
                }
				_logger.LogWarning("{warningTime}:loginCar移除商品失敗，ID:{productID}", DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"), productID);
				return false;
            }
            else
            {
                string? anonymousID = httpContext.Request.Cookies["anonymousID"];
                if (_anonymousCar.TryGetValue(anonymousID, out Dictionary<string, int> productsDict))
                {
					if (productsDict.Remove(productID))
					{
						if (_shoppingCarCount.TryGetValue(anonymousID, out int currentCount))
						{
							_shoppingCarCount.AddOrUpdate(anonymousID, currentCount - 1, (key, oldValue) => oldValue - 1);
							return true;
						}
					}
				}
				_logger.LogWarning("{warningTime}:anonymousCar移除商品失敗，ID:{productID}", DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"), productID);
				return false;
            }
        }

        internal bool RecordToDatebase()
        {
            //紀錄到資料庫，僅限已登入才能紀錄
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext.User.Identity.IsAuthenticated)
            {
				using (IServiceScope scope = _scopeFactory.CreateScope())
                {
					Database database = scope.ServiceProvider.GetRequiredService<Database>();
					string userName = httpContext.User.FindFirstValue(ClaimTypes.Name);
					if (_loginCar.TryGetValue(userName, out ConcurrentDictionary<string, int> productsDict))
					{
						string currentTime = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
						foreach (var item in productsDict)
						{
							Product product = database.SearchColumnSpecificValue<Product>(nameof(Product), nameof(Product.ID), item.Key)[0];
							ShoppingRecord shoppingRecord = new()
							{
								UserName = userName,
								ProductName = product.Name,
								ProductPrice = product.Price,
								ProductCount = item.Value,
								ShoppingTime = currentTime,
								ProductID = item.Key,
							};
							if (!database.AddDataForShoppingRecord(shoppingRecord))
							{
								_logger.LogWarning("{warningTime}:購物紀錄寫入資料庫失敗，商品:{@product}", DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"), shoppingRecord);
								return false;
							}
						}
						if (!_loginCar.TryRemove(userName, out var _))
						{
							_logger.LogWarning("{warningTime}:購物紀錄已寫入資料庫，loginCar移除使用者失敗，使用者:{userName}", DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"), userName);
							return false;
						}
						if (!_shoppingCarCount.TryRemove(userName, out var _))
						{
							_logger.LogWarning("{warningTime}:購物紀錄已寫入資料庫，shoppingCarCount移除使用者失敗，使用者:{userName}", DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"), userName);
							return false;
						}
						return true;
					}
					_logger.LogWarning("{warningTime}:loginCar找不到使用者:{userName}", DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"), userName);
					return false;
				}
			}
			else
			{
				string? anonymousID = httpContext.Request.Cookies["anonymousID"];
				if (!_anonymousCar.TryRemove(anonymousID, out var _))
				{
					_logger.LogWarning("{warningTime}:anonymousCar移除匿名者失敗，匿名者:{anonymousID}", DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"),anonymousID);
					return false;
				}
				if (!_shoppingCarCount.TryRemove(anonymousID, out var _))
				{
					_logger.LogWarning("{warningTime}:shoppingCarCount移除匿名者失敗，匿名者:{anonymousID}", DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"), anonymousID);
					return false;
				} 
				return true;
			}
        }

		internal bool CheckExist(string productID)
		{
			//檢查購物車是否已存在商品
			var httpContext = _httpContextAccessor.HttpContext;
			if (httpContext.User.Identity.IsAuthenticated)
			{
				string userName = httpContext.User.FindFirstValue(ClaimTypes.Name);
				if (_loginCar.TryGetValue(userName, out ConcurrentDictionary<string, int> productsDict))
				{
					if (productsDict.TryGetValue(productID, out int _))
					{
						_logger.LogInformation("{infoTime}:loginCar商品已存在，ID:{productID}", DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"), productID);
						return true;
					} 
				}
				return false;
			}
			else
			{
				string? anonymousID = httpContext.Request.Cookies["anonymousID"];
				if (anonymousID is null)
				{
					string id = Guid.NewGuid().ToString();
					anonymousID = id;
					CookieOptions options = new CookieOptions
					{ 
						HttpOnly = true,
						Secure = true,
						SameSite = SameSiteMode.Lax
					};
					httpContext.Response.Cookies.Append("anonymousID", id, options);
				}
				if (_anonymousCar.TryGetValue(anonymousID, out Dictionary<string, int> productsDict))
				{
					if (productsDict.ContainsKey(productID))
					{
						_logger.LogInformation("{infoTime}:anonymousCar商品已存在，ID:{productID}", DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"), productID);
						return true;
					}
				}
				return false;
			}
		}

		internal int GetCount()
		{
			//取得購物車商品總數量
			var httpContext = _httpContextAccessor.HttpContext;
			if (httpContext.User.Identity.IsAuthenticated)
			{
				string userName = httpContext.User.FindFirstValue(ClaimTypes.Name);
				if (_shoppingCarCount.TryGetValue(userName, out int count)) return count;
			}
			else
			{
				string? anonymousID = httpContext.Request.Cookies["anonymousID"];
				if (anonymousID is not null)
				{
					if (_shoppingCarCount.TryGetValue(anonymousID, out int count)) return count;
				}
			}
			return 0;
		}

		internal ClearnState ClearnAnonymousCar(CancellationToken stoppingToken)
		{
			//清空匿名購物車
			if (!stoppingToken.IsCancellationRequested)
			{
				var keys = _anonymousCar.Keys;
				_anonymousCar.Clear();
				foreach(var item in keys)
				{
					if (stoppingToken.IsCancellationRequested) return ClearnState.StopTokenStop;
					if (!_shoppingCarCount.TryRemove(item, out var _)) return ClearnState.Fail;
				}
				return ClearnState.Success;
			}
			return ClearnState.StopTokenStop;
		}
	}

	internal enum ClearnState { Success,Fail,StopTokenStop}
}
