using Dapper;
using Microsoft.Data.Sqlite;
using System.Security.Claims;
using System.Security.Principal;
using WebApplication1.Models;
using WebApplication1.ViewModel;

namespace WebApplication1.Service;
//資料庫操作邏輯服務
public class Database
{
    private readonly string[] _productColumnNameArray = { nameof(Product.ID), nameof(Product.Name), nameof(Product.Describe), nameof(Product.Price), nameof(Product.WhoAdd) };
    private readonly string[] _userColumnNameArray = { nameof(User.Account), nameof(User.Password), nameof(User.Role) };
    private readonly string[] _shoppingRecordColumnNameArray = { nameof(ShoppingRecord.UserName), nameof(ShoppingRecord.ProductName), nameof(ShoppingRecord.ProductCount), nameof(ShoppingRecord.ProductPrice), nameof(ShoppingRecord.ShoppingTime), nameof(ShoppingRecord.ProductID) };
    private readonly string _connectString;
    private readonly IHttpContextAccessor _iHttpContextAccessor;
    private readonly ILogger<Database> _logger;

	public Database(string connectString, IHttpContextAccessor httpContextAccessor, ILogger<Database> logger)
    {
        _connectString = connectString;
        _iHttpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    internal List<T> GetAll<T>(string tableName)
    {
        //讀取所有欄位值
        using (var connection = new SqliteConnection(_connectString))
        {
            switch (tableName)
            {
                case nameof(Product):
                    List<T> products = connection.Query<T>("SELECT * FROM Product").ToList();
                    return products;
                default:
                    return new List<T>();
            }
        }
    }

    internal List<T> SearchKeyWord<T>(string tableName, string columnName, string searchWord)
    {
        //搜選指定欄位值是否包含關鍵字
        using (var connection = new SqliteConnection(_connectString))
        {
            string sql;
            switch (tableName)
            {
                case nameof(Product):
                    if (!_productColumnNameArray.Contains(columnName)) throw new ArgumentException("無效的欄位名稱");
                    sql = $"SELECT * FROM Product WHERE {columnName} LIKE @word";
                    List<T> products = connection.Query<T>(sql, new { word = $"%{searchWord}%" }).ToList();
                    return products;
                default:
                    return new List<T>();
            }
        }
    }

    internal List<T> SearchColumnSpecificValue<T>(string tableName, string columnName, string value)
    {
        //搜選指定欄位值是否等於給定值
        using (var connection = new SqliteConnection(_connectString))
        {
            List<T> results;
            switch (tableName)
            {
                case nameof(Product):
                    if (!_productColumnNameArray.Contains(columnName)) throw new ArgumentException("無效的欄位名稱");
                    results = connection.Query<T>($"SELECT * FROM Product WHERE {columnName} = @word", new { word = value }).ToList();
                    return results;
                case nameof(User):
                    if (!_userColumnNameArray.Contains(columnName)) throw new ArgumentException("無效的欄位名稱");
                    results = connection.Query<T>($"SELECT * FROM User WHERE {columnName} = @word", new { word = value }).ToList();
                    return results;
                case nameof(ShoppingRecord):
                    if (!_shoppingRecordColumnNameArray.Contains(columnName)) throw new ArgumentException("無效的欄位名稱");
                    results = connection.Query<T>($"SELECT * FROM ShoppingRecord WHERE {columnName} = @word", new { word = value }).ToList();
                    return results;
                default:
                    throw new ArgumentException("無效的資料表名稱");
            }
        }
    }

    internal bool AddDataForProduct(AddProductViewModel dataModel, string productID)
    {
        //Product表專用新增資料方法
        using (var connection = new SqliteConnection(_connectString))
        {

            string sql;
            if (dataModel.Describe is null) sql = "INSERT INTO Product (ID,Name,Price,WhoAdd) VALUES (@ID,@Name,@Price,@WhoAdd)";
            else sql = "INSERT INTO Product (ID,Name,Describe,Price,WhoAdd) VALUES (@ID,@Name,@Describe,@Price,@WhoAdd)";
            string whoAddValue = "Error";
            if (_iHttpContextAccessor.HttpContext.User.Identity.IsAuthenticated) whoAddValue = _iHttpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.Name);
            int rowEffected = connection.Execute(sql, new { ID = productID, Name = dataModel.Name, Describe = dataModel.Describe, Price = dataModel.Price, WhoAdd = whoAddValue });
            if(rowEffected > 0) return true;
            else
            {
                _logger.LogWarning("{warningTime}:AddDataForProduct方法，新增商品失敗，商品ID:{productID}", DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"), productID);
                return false;
            }
		}
    }

    internal bool AddDataForUser(string account, string password)
    {
        //User表專用新增資料方法
        using (var connection = new SqliteConnection(_connectString))
        {
            string tableNmae = nameof(User);
            string columnName_1 = nameof(User.Account);
            string columnName_2 = nameof(User.Password);
            string sql = $"INSERT INTO {tableNmae} ({columnName_1},{columnName_2}) VALUES (@Account,@Password)";
			int rowEffected = connection.Execute(sql, new { Account = account, Password = password });
            if(rowEffected > 0) return true;
            else
            {
				_logger.LogWarning("{warningTime}:AddDataForUser方法，新增帳號密碼失敗，帳號:{account}，密碼{password}", DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"), account,password);
                return false;
            }
		}
    }

    internal bool AddDataForShoppingRecord(ShoppingRecord dataModel)
    {
        //ShoppingRecord表專用新增資料方法
        using (var connection = new SqliteConnection(_connectString))
        {
            string tableNmae = nameof(ShoppingRecord);
            string sqlpart1 = $"INSERT INTO {tableNmae} (";
            string sqlpart2 = " VALUES (";
            foreach (string item in _shoppingRecordColumnNameArray)
            {
                if(item == _shoppingRecordColumnNameArray.Last())
                {
                    sqlpart1 += $"{item})"; ;
                    sqlpart2 += $"@{item})"; ;
                }
                else
                {
                    sqlpart1 += $"{item},";
                    sqlpart2 += $"@{item},";
                }
            }
			int rowEffected = connection.Execute(sqlpart1+ sqlpart2, new { UserName = dataModel.UserName , ProductName = dataModel.ProductName, ProductPrice = dataModel.ProductPrice, ProductCount= dataModel.ProductCount , ShoppingTime = dataModel.ShoppingTime , ProductID = dataModel.ProductID});
            if(rowEffected > 0) return true;
			else
			{
				_logger.LogWarning("{warningTime}:AddDataForShoppingRecord方法，新增購物紀錄失敗，商品:{@product}", DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"),dataModel);
				return false;
			}
		}
    }

    internal bool UpdateProduct(EditProductViewModel dataModel)
    {
        //Product表專用修改資料方法
        using (var connection = new SqliteConnection(_connectString))
        {
            string tableName = nameof(Product);
            string sql = $"UPDATE {tableName} SET {nameof(Product.Name)} = @name, {nameof(Product.Describe)} = @describe, {nameof(Product.Price)} = @price WHERE {nameof(Product.ID)} = @id";
            int rowEffected = connection.Execute(sql, new { name = dataModel.Name, describe = dataModel.Describe, price = dataModel.Price, id = dataModel.ID});
			if (rowEffected > 0) return true;
			else
			{
				_logger.LogWarning("{warningTime}:UpdateProduct方法，更新商品資料失敗，商品:{@product}", DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"), dataModel);
				return false;
			}
		}
    }

    internal bool Delete(string idDeleted)
    {
        using (var connection = new SqliteConnection(_connectString))
        {
            string tableName = nameof(Product);
            string sql = $"DELETE FROM {tableName} WHERE ID = @id";
			int rowEffected = connection.Execute(sql, new { id = idDeleted });
			if (rowEffected > 0) return true;
            else
            {
				_logger.LogWarning("{warningTime}:Delete方法，商品移除失敗，商品ID:{productID}", DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"), idDeleted);
                return false;

			}
        }
    }
}

