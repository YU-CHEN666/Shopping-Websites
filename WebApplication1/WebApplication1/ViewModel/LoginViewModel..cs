using System.ComponentModel.DataAnnotations;
namespace WebApplication1.ViewModel
{
    //登入頁面使用Model
    public class LoginViewModel
    {
        [Required(ErrorMessage = "必填")]
        [RegularExpression(@"^[^\u4E00-\u9FFF\u3000-\u303f\uff01-\uffee]+$", ErrorMessage = "請勿輸入中文或全形符號")]
        [Display(Name = "帳號")]
        public string Account { get; set; }

        [Required(ErrorMessage = "必填")]
        [Display(Name = "密碼")]
        public string Password { get; set; }
    }
}
