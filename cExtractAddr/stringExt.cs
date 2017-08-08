using System.Linq;
using System.Text.RegularExpressions;

namespace cExtractAddr
{
    public static class stringExt
    {
        public static string getLastSplitBySpace(this string str)
        {
            string[] _arr = str.Split(' ');
            string _val = _arr.Last();
            return _val;
        }

        public static string manageStrAddress(this string addr)
        {
            if (!string.IsNullOrEmpty(addr))
            {
                addr = Regex.Replace(addr, "\\s+", " "); // remove multi space to 1space 
                addr = addr.Replace("จ. ", "จ.")
                           .Replace("อ. ", "อ.")
                           .Replace("ต. ", "ต.")
                           .Replace("ซ. ", "ซ.")
                           .Replace("ถ. ", "ถ.")
                           .Replace("ม. ", "ม.")
                           .Trim();
                
                // clear inline postcode
                string _postCode = addr.Split(' ').Last();
                if (Regex.IsMatch(_postCode, "[0-9]+"))
                {
                    addr = addr.Substring(0, addr.Length - _postCode.Length).Trim();
                }
            }
            return addr;
        }

        public static bool isAddressEng(this string addr)
        {
            bool b = Regex.IsMatch(addr, "^[0-9a-zA-Z ',./@#&:;\\-\\(\\)]+$");
            return b;
        }
        public static bool isAddressNo(this string addr)
        {
            bool b = Regex.IsMatch(addr, "^[0-9,./\\-\\(\\)]+$") && Regex.IsMatch(addr, "[0-9]+");
            return b;
        }
        public static bool isAddressNoSpecialChar(this string addr)
        {
            bool b = Regex.IsMatch(addr, "[,./\\-\\(\\)]+") && Regex.IsMatch(addr, "[0-9]+");
            return b;
        }

        public static bool isEmpty(this string str)
        {
            return string.IsNullOrEmpty(str);
        }
        public static bool isNotEmpty(this string str)
        {
            return !string.IsNullOrEmpty(str);
        }

        public static string removeWordInString(this string str, string wordToRemove, bool isLastWordOnly = false)
        {
            string _tmp = str;
            if (wordToRemove.isNotEmpty())
            {
                if (isLastWordOnly)
                {
                    MatchCollection _matches = Regex.Matches(str, wordToRemove);
                    if (_matches.Count > 0)
                    {
                        int _i = _matches[_matches.Count - 1].Index;
                        _tmp = _tmp.Remove(_i, wordToRemove.Length);                        
                    }
                }
                else
                {
                    _tmp = _tmp.Replace(wordToRemove, string.Empty);
                }

                _tmp = _tmp.Trim();
            }
            return _tmp;
        }
        
        /// <summary>
        /// ไว้สำหรับ ลบ space ระหว่างคำของประโยคสุดท้ายที่ Match ได้
        /// <para>เช่น อำเภอ เมือง => อำเภอเมือง</para>
        /// </summary>
        /// <param name="addr"></param>
        /// <param name="pattern"></param>
        /// <returns>addr</returns>
        public static string removeSpaceBetweenWords(this string addr, string pattern)
        {
            string _addr = addr;
            string _pattern = pattern;
            MatchCollection _matches = Regex.Matches(_addr, _pattern);
            if (_matches.Count > 0)
            {
                string _tmp = _matches[_matches.Count - 1].Value; 
                if (_tmp.isNotEmpty())
                {
                    string _tmp2 = _tmp.Replace(" ", "").Trim();
                    _addr = _addr.Replace(_tmp, _tmp2);
                }
            }
            return _addr;
        }

        /// <summary>
        /// ไว้สำหรับ ลบ space ระหว่างคำของประโยคที่ Match ได้ทั้งหมด
        /// <para>เช่น อำเภอ เมือง => อำเภอเมือง</para>
        /// </summary>
        /// <param name="addr"></param>
        /// <param name="pattern"></param>
        /// <returns>addr</returns>
        public static string removeSpaceBetweenWordsAllMatches(this string addr, string pattern)
        {
            string _addr = addr;
            string _pattern = pattern;
            MatchCollection _matches = Regex.Matches(_addr, _pattern);
            if (_matches.Count > 0)
            {
                string _tmp, _tmp2;
                foreach (Match _m in _matches)
                {
                    _tmp = _m.Value;
                    if (_tmp.isNotEmpty())
                    {
                        _tmp2 = _tmp.Replace(" ", "").Trim();
                        _addr = _addr.Replace(_tmp, _tmp2);
                    }
                }
                
            }
            return _addr;
        }
    }
}
