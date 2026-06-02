using System.ComponentModel.DataAnnotations;
using WebApplication1.CustomValidationAttribute;

namespace WebApplication1.ViewModel
{
    //新增商品頁面使用Model
    public class AddProductViewModel
    {
        [Required(ErrorMessage = "請輸入商品名稱")]
        [NoRepeat]
        [Display(Name = "商品名稱:")]
        public string Name { get; set; }

        [Display(Name = "商品描述:")]
        public string? Describe { get; set; }

        [Required(ErrorMessage = "請輸入商品價格")]
        [RegularExpression(@"^[1-9]\d*$", ErrorMessage = "無效")]
        [Display(Name="商品價格:")]
        public int Price { get; set; }

        [Display(Name = "商品圖片:")]
        public IFormFile? Picture { get; set; }
    }
}
