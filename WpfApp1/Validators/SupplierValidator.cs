using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using WpfApp1.Models.Validation;

namespace WpfApp1.Validators
{
    /// <summary>
    /// Валидатор данных клиента.
    /// Использует встроенный механизм валидации .NET (DataAnnotations).
    /// </summary>
    public class SupplierValidator
    {
        public List<ValidationResult> Validate(SupplierValidationModel model)
        {
            var context = new ValidationContext(model);
            var results = new List<ValidationResult>();
            Validator.TryValidateObject(model, context, results, true);
            return results;
        }
    }
}