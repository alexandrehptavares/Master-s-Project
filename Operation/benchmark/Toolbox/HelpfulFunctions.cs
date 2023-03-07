using System;
using System.Linq;

namespace benchmark.Toolbox
{
    public class HelpfulFunctions
    {
        /// <summary>
        /// Verify if is a valid 16 bit integer
        /// </summary>
        /// <param name="validThis"></param>
        /// <returns></returns>
        public static bool ValidateInt16(string validThis)
        {
            //var isNumeric = validThis.All(char.IsDigit);

            try
            {
                //Int16 Number;
                Int16.TryParse(validThis, out Int16 Number);

                if ((Number < Int16.MaxValue || Number > Int16.MinValue) && Number != 0)
                {
                    return true;
                }
                else
                    return false;
            }
            catch (Exception)
            {
                return false;
                //throw;
            }
        }
        
        /// <summary>
        /// Verify if is a valid 32 bit integer
        /// </summary>
        /// <param name="validThis"></param>
        /// <returns></returns>
        public static bool ValidateInt32(string validThis)
        {
            //var isNumeric = validThis.All(char.IsDigit);

            try
            {
                //Int16 Number;
                Int32.TryParse(validThis, out Int32 Number);

                if ((Number < Int32.MaxValue || Number > Int32.MinValue) && Number != 0)
                {
                    return true;
                }
                else
                    return false;
            }
            catch (Exception)
            {
                return false;
                //throw;
            }
        }
    }
}
