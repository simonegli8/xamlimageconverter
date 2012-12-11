using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace SharpVectorXamlRenderingEngine
{
    public static class Utils
    {
        private static Regex regExName = new Regex(@"^(\p{Lu}|\p{Ll}|\p{Lo}|\p{Lt}|\p{Nl}|_)((\p{Lu}|\p{Ll}|\p{Lo}|\p{Lt}|\p{Nl}|_)|\p{Nd}|\p{Mc}|\p{Mn} )*", RegexOptions.Compiled);

        public static string FormatWPFName(string inkScapeLabel)
        {
            StringBuilder sb = new StringBuilder(inkScapeLabel);
            Match m;
            do
            {
                m = regExName.Match(sb.ToString());
                int failIdx = 0;
                if (!m.Success)
                    failIdx = 0;
                else
                {
                    failIdx = m.Length;
                }
                if (failIdx < sb.Length)
                    sb[failIdx] = '_';
            }
            while (m.Length != sb.Length);
            return sb.ToString();
        }
    }
}
