namespace WebApplication1.Models
{
    //Product資料表
    public class Product
    {
        public string ID { get; set; }
        public string Name { get; set; }
        public string Describe { get; set; }
        public int Price { get; set; }

        public string WhoAdd { get; set; }
    }
}
