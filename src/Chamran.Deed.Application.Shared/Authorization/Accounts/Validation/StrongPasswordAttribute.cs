using Abp.Authorization.Users;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.RegularExpressions;

namespace Chamran.Deed.Authorization.Accounts.Validation
{
    public class StrongPasswordAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null)
            {
                return new ValidationResult("گذرواژه نمی‌تواند خالی باشد.");
            }

            string password = value.ToString();
            string pattern = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]+$";

            if (password.Length < 8 || password.Length > AbpUserBase.MaxPlainPasswordLength)
            {
                return new ValidationResult($"گذرواژه باید بین 8 تا {AbpUserBase.MaxPlainPasswordLength} کاراکتر باشد.");
            }

            if (!Regex.IsMatch(password, pattern))
            {
                return new ValidationResult("گذرواژه باید شامل حروف کوچک، بزرگ، عدد و یک کاراکتر خاص باشد.");
            }

            return ValidationResult.Success;
        }
    }
}
