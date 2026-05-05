using System.ComponentModel.DataAnnotations;

namespace WelfareLink.ProgramManagement.API.Utilities
{
    public class MinimumAgeAttribute : ValidationAttribute
    {
        private readonly int _minimumAge;

        public MinimumAgeAttribute(int minimumAge)
        {
            _minimumAge = minimumAge;
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            // Check if they actually provided a date
            if (value is DateTime dob)
            {
                // Calculate if their birthday + minimum age is greater than today
                if (dob.AddYears(_minimumAge) > DateTime.Today)
                {
                    return new ValidationResult(ErrorMessage ?? $"You must be at least {_minimumAge} years old.");
                }

                // If they are old enough, the validation passes!
                return ValidationResult.Success;
            }

            return new ValidationResult("Invalid date format.");
        }
    }
}