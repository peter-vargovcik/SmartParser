using SmartParser.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartParser.Algorithms
{
    class StringCombinations
    {
        private string[] _strings;
        private int precision;

        public StringCombinations(string[] strings, int precision)
        {
            this._strings = strings;
            this.precision = precision;
        }
        

        public List<string[]> GetCombinations()
        {
            //HashSet<string> output = new HashSet<string>();
            var valueToBeEvaluated = "abcdefgh";
            var allCombinations = permutation(valueToBeEvaluated).ToArray();

            List<string[]> allCombinations2 = permutation(_strings);

            HashSet<string[]> output = new HashSet<string[]>();

            for (int i = precision; i < _strings.Length; i++)
            {
                output = _iterate(allCombinations2);
                allCombinations2 = output.ToList().CloneList();
            }

            /*
            foreach(char a in valueToBeEvaluated)
            {
                var thisList = allCombinations.CloneArray();
                foreach (var item in thisList)
                {
                    var selected = item.Replace(a.ToString(),"");
                    output.Add(selected);
                }
            }

            //var newOne = output.ToArray<string>().CloneArray();
            var newOne = output.ToArray<string>().CloneArray();
            output = new HashSet<string>();

            foreach (char a in valueToBeEvaluated)
            {                
                foreach (var item in newOne)
                {
                    var selected = item.Replace(a.ToString(), "");
                    output.Add(selected);
                }
            }
            */
            return output.Where(x=>x.Length <= (_strings.Length-precision)).ToList<string[]>();
        }

        private HashSet<string[]> _iterate(List<string[]> arrayList)
        {
            HashSet<string[]> output = new HashSet<string[]>();

            foreach (string stringValue in _strings)
            {
                var thisList = arrayList.CloneList();
                foreach (var item in thisList)
                {
                    var a = item.TakeWhile(x=> x == stringValue).ToArray();
                    //var selected = item.Replace(stringValue, "");
                    output.Add(a);
                }
            }
            return output;
        }

        public static List<string> permutation(string s)
        {
            // The result
            List<string> res = new List<string>();
            // If input string's length is 1, return {s}
            if (s.Length == 1)
            {
                res.Add(s);
            }
            else if (s.Length > 1)
            {
                int lastIndex = s.Length - 1;
                // Find out the last character
                string last = s.Substring(lastIndex);
                // Rest of the string
                string rest = s.Substring(0, lastIndex);
                // Perform permutation on the rest string and
                // merge with the last character
                res = merge(permutation(rest), last);
            }
            return res;
        }

        public static List<string> merge(List<string> list, string c)
        {
            List<string> res = new List<string>();
            // Loop through all the string in the list
            foreach (string s in list)
            {
                // For each string, insert the last character to all possible postions
                // and add them to the new list
                for (int i = 0; i <= s.Length; ++i)
                {
                    string ps = new StringBuilder(s).Insert(i, c).ToString();
                    res.Add(ps);
                }
            }
            return res;
        }

        public static List<string[]> permutation(string[] s)
        {
            // The result
            var res = new List<string[]>();
            // If input string's length is 1, return {s}
            if (s.Length == 1)
            {
                res.Add(s);
            }
            else if (s.Length > 1)
            {
                int lastIndex = s.Length - 1;
                // Find out the last character
                var last = s[lastIndex].Clone<string>();
                // Rest of the string
                var rest = s.Take(lastIndex).ToArray().CloneArray();
                // Perform permutation on the rest string and
                // merge with the last character
                res = merge(permutation(rest), last);
            }
            return res;
        }

        public static List<string[]> merge(List<string[]> list, string toBeAddedString)
        {
            var res = new List<string[]>();
            // Loop through all the string in the list
            foreach (string[] listItem in list)
            {
                // For each string, insert the last character to all possible postions
                // and add them to the new list
                for (int i = 0; i <= listItem.Length; ++i)
                {
                    var clone = listItem.CloneArray();
                    clone.ToList().Insert(i, toBeAddedString);
                    res.Add(clone);
                }
            }
            return res;
        }
    }
}
