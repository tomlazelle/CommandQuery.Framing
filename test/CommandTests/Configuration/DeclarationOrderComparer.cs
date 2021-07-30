using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace CommandTests.Configuration
{
    public class DeclarationOrderComparer : IComparer<MemberInfo>
    {
        private IDictionary<int, string> _sorted = new Dictionary<int, string>();

        public DeclarationOrderComparer()
        {
            _sorted.Add(0, "Given");
            _sorted.Add(1, "AndGiven");
            _sorted.Add(2, "When");
            _sorted.Add(3, "AndWhen");
            _sorted.Add(4, "Then");
            _sorted.Add(5, "AndThen");

        }
        /// <inheritdoc/>
        public int Compare(MemberInfo first, MemberInfo second)
        {
            var a = _sorted.FirstOrDefault(x => first.Name.StartsWith(x.Value)).Key;
            var b = _sorted.FirstOrDefault(x => second.Name.StartsWith(x.Value)).Key;


            if (a < b)
            {
                return -1;
            }

            if (a > b)
            {
                return 1;
            }

            return 0;
        }
    }
}