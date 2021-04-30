﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace MyHealth.API.Sleep.Validators
{
    public class DateValidator : IDateValidator
    {
        public bool IsSleepDateValid(string sleepDate)
        {
            bool isDateValid = false;
            string pattern = "d/MM/yyyy";
            DateTime parsedSleepDate;

            if (DateTime.TryParseExact(sleepDate, pattern, null, DateTimeStyles.None, out parsedSleepDate))
            {
                isDateValid = true;
            }

            return isDateValid;
        }
    }
}