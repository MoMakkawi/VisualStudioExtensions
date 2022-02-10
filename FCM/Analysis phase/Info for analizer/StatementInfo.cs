using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace FCM
{
    public class StatementInfo
    {
        private string statement;

        public string Statement
        {
            get { return statement; }
            set { statement = value; }
        }
        private string type;

        public string Type
        {
            get { return type; }
            set { type = value; }
        }
        private int Start;
        private int Length;


        public static List<StatementInfo> GetStatementInfos(MethodInfo method)
        {
            string StartMethod = method.Method;

            var MultStatements = GetStatements_IfElseOrSwitch_OR_ForOrWhile_OR_TryCatch(StartMethod);

            //put spaces in place of ifelse for while trycatch stetement
            MultStatements.ForEach(stat =>
            {
                StartMethod = StartMethod.Replace(stat.Statement, string.Join(null, Enumerable.Range(0, stat.Length).Select(i => ' ')));
            });


            List<StatementInfo> StatementsInvocs = GetStatements_Invocations(StartMethod);

            var Statements = MultStatements
                .Concat(StatementsInvocs)
                .OrderBy(s => s.Start).ToList();


            return Statements;
        }


        static List<StatementInfo> GetStatements_IfElseOrSwitch_OR_ForOrWhile_OR_TryCatch(string StartMethod)
        {
            MatchCollection matchCollection = Regex.Matches(StartMethod, RE.IfElseOrSwitch_OR_ForOrWhile_OR_TryCatch, RegexOptions.Multiline);

            var Statements_IfElseOrSwitch_OR_ForOrWhile_OR_TryCatch =
                Enumerable.Range(0, matchCollection.Count)
                .Select(i => new
                 StatementInfo
                {
                    Statement = matchCollection[i].Value,

                    Type = Regex.IsMatch(matchCollection[i].Value, RE.IfElse) ? "IfElse"
                             : Regex.IsMatch(matchCollection[i].Value, RE.ForOrForEachOrWhileOrDoWhile) ? "ForOrWhile"
                             : Regex.IsMatch(matchCollection[i].Value, RE.TryCatch) ? "TryCatch"
                             : "Switch",

                    Start = matchCollection[i].Index,
                    Length = matchCollection[i].Length
                })
                .Where(Try => Regex.IsMatch(Try.Statement, RE.NormalInvoc, RegexOptions.Multiline))
                .ToList();
            return Statements_IfElseOrSwitch_OR_ForOrWhile_OR_TryCatch;
        }
        static List<StatementInfo> GetStatements_Invocations(string StartMethod)
        {
            var invocsMatchCollections = Regex.Matches(StartMethod, RE.Invoc, RegexOptions.Multiline);
            List<StatementInfo> Statementsivocs =
                Enumerable.Range(0, invocsMatchCollections.Count)
                .Select(i => new
                StatementInfo
                {
                    Statement = invocsMatchCollections[i].Value.Split('=').Last(),
                    Type = Regex.IsMatch(invocsMatchCollections[i].Value, RE.NormalInvoc) ? "NormalInvoc" : "ThreadInvoc",
                    Start = invocsMatchCollections[i].Index,
                    Length = invocsMatchCollections[i].Length
                })
                .ToList();

            return Statementsivocs;
        }
    }
}
