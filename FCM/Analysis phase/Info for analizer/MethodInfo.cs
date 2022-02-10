using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace FCM
{
    public class MethodInfo
    {
        public static List<MethodInfo> MethodsInfo = new List<MethodInfo>();

        private string name;

        public string Name
        {
            get { return name; }
            set { name = value; }
        }
        private string decleration;

        public string Decleration
        {
            get { return decleration; }
            set { decleration = value; }
        }
        private string method;

        public string Method
        {
            get { return method; }
            set { method = value; }
        }

        private string collapsedOrnull;

        public string CollapsedOrnull
        {
            get { return collapsedOrnull; }
            set { collapsedOrnull = value; }
        }

        public static MethodInfo GetMethodInfo(string Method)
        {
            MethodInfo methodInfo = new MethodInfo
            {
                Name = string.Join(null, Method.TakeWhile(c => c != '(')).Split(' ').Last().Trim(),
                Decleration = string.Join(null, Method.TakeWhile(c => c != '{')),
                Method = Method,
                collapsedOrnull = Regex.IsMatch(Method, RE.Invoc, RegexOptions.Multiline) ? "Collapsed" : null
            };

            return methodInfo;
        }
        public static MethodInfo GetOrgenalMethodInfo(string Invoc)
        {
            MethodInfo methodInfo;
            methodInfo = MethodInfo.MethodsInfo.Find(m => m.Name == string.Join(null, Invoc.TakeWhile(c => c != '(')).Trim().Split('.').Last());
            if (methodInfo != null)
            {
                return methodInfo;
            }
            else
            {
                return new MethodInfo
                {
                    Name = string.Join(null, Invoc.TakeWhile(c => c != '(')).Split('.').Last().Trim(),
                    Decleration = "",
                    Method = "We were unable to get this method decleration ",
                    collapsedOrnull = null
                };
            }

        }
    }
}
