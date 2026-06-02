using System.ComponentModel.DataAnnotations;
using WebApplication1.CustomValidationAttribute;

namespace WebApplication1.ViewModel
{
	//刪除商品頁面使用Model
	public class DeleteProductViewModel
    {
        public string ID { get; set; }

        [Display(Name = "商品名稱:")]
        public string Name { get; set; }

        [Display(Name = "商品描述:")]
        public string Describe { get; set; }

        [Display(Name = "商品價格:")]
        public int Price { get; set; }
    }
}
