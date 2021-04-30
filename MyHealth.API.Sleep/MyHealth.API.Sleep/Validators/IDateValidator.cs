using System;
using System.Collections.Generic;
using System.Text;

namespace MyHealth.API.Sleep.Validators
{
    public interface IDateValidator
    {
        /// <summary>
        /// Check the provided date is in the valid format
        /// </summary>
        /// <param name="sleepDate"></param>
        /// <returns></returns>
        bool IsSleepDateValid(string sleepDate);
    }
}
