using System.Collections;
using System.ComponentModel.DataAnnotations;

namespace backend.utils
{
    public class DistinctItemsAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(
            object? value,
            ValidationContext validationContext
        )
        {
            if (value is IEnumerable enumerable)
            {
                var list = enumerable.Cast<object>().ToList();
                if (list.Count != list.Distinct().Count())
                {
                    return new ValidationResult("List items must be distinct.");
                }
            }

            return ValidationResult.Success;
        }
    }
}
