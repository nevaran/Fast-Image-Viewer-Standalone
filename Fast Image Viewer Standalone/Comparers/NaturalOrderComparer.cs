using FIVStandard.Views;
using System;
using System.Collections;
using System.Text.RegularExpressions;

namespace FIVStandard.Comparers
{
    public sealed class NaturalOrderComparer : IComparer
    {
        private bool _ignoreCase = true;

        public NaturalOrderComparer(bool ignoreCase)
        {
            _ignoreCase = ignoreCase;
        }

        public int Compare(object x1, object y1)
        {
            ThumbnailItemData x = x1 as ThumbnailItemData;
            ThumbnailItemData y = y1 as ThumbnailItemData;

            // check for null values first: a null reference is considered to be less than any reference that is not null 
            if (x.ThumbnailName == null && y.ThumbnailName == null)
            {
                return 0;
            }
            if (x.ThumbnailName == null)
            {
                return -1;
            }
            if (y.ThumbnailName == null)
            {
                return 1;
            }

            StringComparison comparisonMode = _ignoreCase ? StringComparison.CurrentCultureIgnoreCase : StringComparison.CurrentCulture;

            string[] splitX = Regex.Split(x.ThumbnailName.Replace(" ", ""), "([0-9]+)");
            string[] splitY = Regex.Split(y.ThumbnailName.Replace(" ", ""), "([0-9]+)");

            int comparer = 0;

            for (int i = 0; comparer == 0 && i < splitX.Length; i++)
            {
                if (splitY.Length <= i)
                {
                    comparer = 1; // x > y 
                }

                int numericX = -1;
                int numericY = -1;
                if (int.TryParse(splitX[i], out numericX))
                {
                    if (int.TryParse(splitY[i], out numericY))
                    {
                        comparer = numericX - numericY;
                    }
                    else
                    {
                        comparer = 1; // x > y 
                    }
                }
                else
                {
                    comparer = string.Compare(splitX[i], splitY[i], comparisonMode);
                }
            }

            return comparer;
        }
    }
}
