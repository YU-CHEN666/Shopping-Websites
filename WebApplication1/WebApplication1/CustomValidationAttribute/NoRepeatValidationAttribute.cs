using System.ComponentModel.DataAnnotations;
using WebApplication1.Models;
using WebApplication1.Service;
using WebApplication1.ViewModel;

//不可重複自訂驗證標籤
namespace WebApplication1.CustomValidationAttribute
{
    public class NoRepeat: ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if(value is not null)
            {
                Database databaseService = (Database)validationContext.GetService(typeof(Database));
                List<Product> results = databaseService.SearchColumnSpecificValue<Product>(nameof(Product),validationContext.MemberName, value.ToString());
                var whoIsValidType = validationContext.ObjectType;
                switch(whoIsValidType.Name)
                {
                    case nameof(AddProductViewModel):
                        if(results.Any()) return new ValidationResult("商品名稱已存在"); 
                        return ValidationResult.Success;
                    case nameof(EditProductViewModel):
                        EditProductViewModel whoIsValidInstance = (EditProductViewModel)validationContext.ObjectInstance;
                        if(results.Any())
                        {
                            if (results[0].ID == whoIsValidInstance.ID) return ValidationResult.Success;
                            return new ValidationResult("商品名稱已存在");
                        }
                        return ValidationResult.Success;
                    default:
                        return new ValidationResult("發生錯誤");
                }
            }
            else return new ValidationResult("請輸入商品名稱");
        }

    }
}
