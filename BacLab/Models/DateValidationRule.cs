using System;
using System.Globalization;
using System.Windows.Controls;

namespace BacLab.Models
{
    class DateValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            DateTime time;
            if (!DateTime.TryParse((value ?? "").ToString(),
                CultureInfo.CurrentCulture,
                DateTimeStyles.AssumeLocal | DateTimeStyles.AllowWhiteSpaces,
                out time)) return new ValidationResult(false, "Заповніть поле");

            return time.Date > DateTime.Now.Date
                ? new ValidationResult(false, "Дата не може бути більше за сьогоднішню")
                : ValidationResult.ValidResult;
        }

    }
}
