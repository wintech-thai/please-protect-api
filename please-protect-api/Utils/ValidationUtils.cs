using System.Text.RegularExpressions;

namespace Its.Otep.Api.Utils
{
    public static class ValidationUtils
    {
        public static ValidationResult ValidatePassword(string password)
        {
            var result = new ValidationResult() { Status = "OK", Description = "" };

            var regex = new Regex(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^a-zA-Z0-9]).{7,15}$");
            var ok = regex.IsMatch(password);

            if (!ok)
            {
                result.Status = "ERROR_VALIDATION_PASSWORD";
                result.Description = @"
1) Lenght of password must be between 7-15
2) Atleast 1 lower letter
3) Atleast 1 capital letter    
4) Atleast 1 special letter in this set {#, !, @, #}
";
            }

            return result;
        }

        public static ValidationResult ValidateKeyAndIV(string? keyIV)
        {
            var result = new ValidationResult() { Status = "OK", Description = "" };

            if (string.IsNullOrEmpty(keyIV))
            {
                result.Status = "ERROR_KEY_EMPTY";
                result.Description = "Key or input vector need to be 16 characters long";
                return result;
            }

            if (keyIV.Length < 16)
            {
                result.Status = "ERROR_KEY_TOO_SHORT";
                result.Description = "Key or input vector need to be 16 characters long";
                return result;
            }

            return result;
        }

        public static ValidationResult ValidateUserName(string userName)
        {
            var result = new ValidationResult() { Status = "OK", Description = "" };

            var regex = new Regex(@"^[a-zA-Z0-9._]{4,20}$");
            var ok = regex.IsMatch(userName);

            if (!ok)
            {
                result.Status = "ERROR_VALIDATION_USERNAME";
                result.Description = "User name must be in this regex format --> [a-zA-Z0-9._]{4,20}";
            }

            return result;
        }

        public static ValidationResult ValidateEmail(string email)
        {
            var result = new ValidationResult() { Status = "OK", Description = "" };

            var regex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
            var ok = regex.IsMatch(email);

            if (!ok)
            {
                result.Status = "ERROR_VALIDATION_EMAIL";
                result.Description = "Incorrect email format";
            }

            return result;
        }

        public static ValidationResult ValidatePhone(string phone)
        {
            var result = new ValidationResult() { Status = "OK", Description = "" };

            // E.164 format: +<country><number>  (สูงสุด 15 digits)
            var regex = new Regex(@"^\+[1-9][0-9]{7,14}$");
            var ok = regex.IsMatch(phone ?? "");

            if (!ok)
            {
                result.Status = "ERROR_VALIDATION_PHONE";
                result.Description = "Incorrect phone format (must be E.164, e.g. +66812345678)";
            }

            return result;
        }

        public static ValidationResult ValidationEffectiveDateInterval(DateTime? startDate, DateTime? endDate)
        {
            var result = new ValidationResult() { Status = "OK", Description = "" };

            if ((startDate != null) && (endDate != null))
            {
                if (startDate > endDate)
                {
                    result.Status = "ERROR_DATE_RANGE_INVALID";
                    result.Description = "End date must be greater than or equal start date!!!";
                }
            }

            return result;
        }
    }
}
