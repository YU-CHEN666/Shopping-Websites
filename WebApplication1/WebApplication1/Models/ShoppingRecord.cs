namespace WebApplication1.Models
{
	//ShoppingRecord資料表
	public class ShoppingRecord
    {
        public string UserName { get; set; }
        public string ProductName { get; set; }
        public int ProductPrice { get; set; }
        public int ProductCount { get; set; }
        public string ShoppingTime { get; set; }

		public string ProductID { get; set; }
	}
}
