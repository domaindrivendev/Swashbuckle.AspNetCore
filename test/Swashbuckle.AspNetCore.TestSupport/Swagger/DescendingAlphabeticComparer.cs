using System.Collections.Generic;

namespace Swashbuckle.AspNetCore.TestSupport
{
    public class DescendingAlphabeticComparer : IComparer<string>
    {
        public int Compare(string x, string y)
        {
            return y.CompareTo(x);
        }
    }
}