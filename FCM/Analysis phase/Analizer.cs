using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace FCM
{
    public static class Analizer
    {
        #region Constant Strings For Alternaive and parallel choise Iteration Nodes Lables
        const string AlternativeStart = "Alternative start";
        const string AlternativeEnd = "Alternative End";
        const string IterationStart = "Iteration start";
        const string IterationEnd = "Iteration End";
        const string ParallelStart = "Parallel Start";
        const string ParallelEnd = "Parallel End";
        const string Choice = "Choice";
        #endregion

        static readonly Queue<string> UnvisitedMethods = new Queue<string>();

        public static void AnalizeMethods(string inputFileContent, string AllContext)
        {
            //ClearEnumerables UnvisitedMethods And Functions And Flows And MethodsInfo to forget previous analysis
            ClearEnumerables();

            #region Start Method Info
            var startMethod = GetStartMethod(inputFileContent);

            if (! IsInputFileNotEmpty_and_StartMethodPresent(inputFileContent,startMethod)) return;

            var startMethodInfo = MethodInfo.GetMethodInfo(startMethod);

            if (startMethodInfo.CollapsedOrnull == null) // start method dosen't call any method
            {
                Dgml.Functions.Add(new MyFunction("@0", startMethodInfo.Name, startMethodInfo.Method));
                return;
            }

            #endregion

            #region Get All Method Info

            MethodInfo.MethodsInfo = FetchMethodsInStr(AllContext)
                .AsParallel()
                .Select(MethodInfo.GetMethodInfo)
                .ToList();

            #endregion

            UnvisitedMethods.Enqueue("@0");

            string totoalFuncId = UnvisitedMethods.First();
            Dgml.Functions.Add(new MyFunction(totoalFuncId, startMethodInfo.Name, startMethodInfo.Method, startMethodInfo.CollapsedOrnull, category : null));

            while (UnvisitedMethods.Count != 0)
            {
                totoalFuncId = UnvisitedMethods.Dequeue();

                string TotalMethodAsString = Dgml.Functions.First(f => f.Id == totoalFuncId).Info;

                var TotalMethod = MethodInfo.GetMethodInfo(TotalMethodAsString);

                bool NewMethod = true;

                StatementInfo
                    .GetStatementInfos(TotalMethod)
                    .ForEach(s => AnalyzeStatement(s.Type, s.Statement, totoalFuncId, ref NewMethod));
            }
        }
        static void ClearEnumerables() { Dgml.Functions.Clear();  Dgml.Flows.Clear();  UnvisitedMethods.Clear(); MethodInfo.MethodsInfo.Clear(); }
        static bool IsInputFileNotEmpty_and_StartMethodPresent(string inputFileContent , string startMethod)
        {
            if (inputFileContent == null)// start method unfounded
            {
                Dgml.Functions.Add(new MyFunction("@-1", "There is no code ", null));
                return false;
            }

            if (startMethod == null)
            {
                Dgml.Functions.Add(new MyFunction("@-1", "There is no Method in the code ", null));
                return false;
            }

            return true;
        }
        static List<string> GetInvocsIn(string statement)
        {
            var matchcollection = Regex.Matches(statement, RE.Invoc, RegexOptions.Multiline);
            return Enumerable.Range(0, matchcollection.Count).Select(i => matchcollection[i].Value).ToList();
        }
        static IEnumerable<string> FetchMethodsInStr(string inputFileContent)
        {

            var functionsAsMatchCollection = Regex
                .Matches(inputFileContent, RE.Function);

            var Methods = Enumerable
                .Range(0, functionsAsMatchCollection.Count)
                .Select(i => functionsAsMatchCollection[i].Value);

            return Methods;
        }
        static string GetStartMethod(string inputFileContent) => FetchMethodsInStr(inputFileContent).FirstOrDefault();

        #region Analize Statement
        static void AnalyzeStatement(string StatementType, string statement, string totoalFuncId, ref bool NewMethod)
        {
            // connect statements 
            if ((NewMethod != true || UnvisitedMethods.FirstOrDefault() == "@0") && Dgml.Functions.Count >= 2)
            {
                Dgml.Flows.Add(new Flow(Dgml.Functions.Last().Id, GetId()));
            }
            switch (StatementType)
            {
                case "NormalInvoc":
                    AnalyzeNormalInvocationStatement(statement, totoalFuncId);
                    break;
                case "IfElse":
                    AnalyzeIfElseStatement(statement, totoalFuncId);
                    break;
                case "ForOrWhile":
                    AnalyzeForOrWhileStatement(statement, totoalFuncId);
                    break;
                case "TryCatch":
                    AnalyzeTryCatchStatement(statement, totoalFuncId);
                    break;
                case "ThreadInvoc":
                    AnalyzeThreadStatement(statement, totoalFuncId, NewMethod);
                    break;
                case "Switch":
                    AnalyzeSwitchStatement(statement, totoalFuncId);
                    break;
                default:
                    break;
            }

            NewMethod = false;
        }
        static string GetId() => $"@{Dgml.Functions.Count}";
        static string GetNextId() => $"@{ Dgml.Functions.Count + 1}";
        private static void AnalyzeThreadStatement(string statement, string totoalFuncId, bool NewMethod)
        {
            // ex : new Thread (F1).Start() => InvocmethodNameInThreadInstance = F1
            var InvocmethodNameInThreadInstance = string.Join(null, statement.SkipWhile(c => c != '(').Skip(1).TakeWhile(c => c != ')'));

            MethodInfo ThmethodInfo = MethodInfo.GetOrgenalMethodInfo(InvocmethodNameInThreadInstance);

            MyFunction lastNode = Dgml.Functions.Last();

            if (lastNode.Lable == ParallelEnd && NewMethod != true)
            {
                var startpar = Dgml.Functions.Last(f => f.Lable == "Parallel Start").Id;
                var lastpar = lastNode.Id;

                var id = GetId();
                Dgml.Functions.Add(new MyFunction(id, ThmethodInfo.Name, ThmethodInfo.Method, ThmethodInfo.CollapsedOrnull, null));

                if (ThmethodInfo.CollapsedOrnull != null) UnvisitedMethods.Enqueue(id);

                Dgml.Flows.RemoveAt(Dgml.Flows.Count - 1); // to cancel Dgml.Flows.Add(new Dgml.Flow(Dgml.Functions.Last().Id, GetId())); in AnalizeStatement method

                Dgml.Flows.Add(new Flow(totoalFuncId, id, "Contains"));
                Dgml.Flows.Add(new Flow(startpar, id));
                Dgml.Flows.Add(new Flow(id, lastpar));

                //Re-Order the ParallelEnd Node to make it in last Function List 
                Dgml.Functions.Remove(lastNode);
                Dgml.Functions.Add(lastNode);
            }
            else
            {
                var startParId = GetId();
                Dgml.Functions.Add(new MyFunction(startParId, ParallelStart, "Par"));
                Dgml.Flows.Add(new Flow(totoalFuncId, startParId, "Contains"));



                var prev_id = startParId;
                var id = GetId();
                Dgml.Functions.Add(new MyFunction(id, ThmethodInfo.Name, ThmethodInfo.Method, ThmethodInfo.CollapsedOrnull, null));

                if (ThmethodInfo.CollapsedOrnull != null) UnvisitedMethods.Enqueue(id);

                Dgml.Flows.Add(new Flow(totoalFuncId, id, "Contains"));
                Dgml.Flows.Add(new Flow(prev_id, id));


                var endParId = GetId();

                Dgml.Functions.Add(new MyFunction(endParId, ParallelEnd, "Par"));
                Dgml.Flows.Add(new Flow(totoalFuncId, endParId, "Contains"));

                Dgml.Flows.Add(new Flow(id, endParId));

                Dgml.Flows.Add(new Flow(startParId, endParId));

            }


        }

        private static void AnalyzeTryCatchStatement(string statement, string totoalFuncId)
        {

            var startAltId = GetId();
            Dgml.Functions.Add(new MyFunction(startAltId, AlternativeStart, "Alt")); // we don have delay so we will use get id func not get next id
            Dgml.Flows.Add(new Flow(totoalFuncId, startAltId, "Contains"));

            var tryBlock = Regex.Match(statement, RE.Try).Value;
            var invocsInTryBlock = GetInvocsIn(tryBlock);

            var prev_id = startAltId;
            invocsInTryBlock.FillDGMLArrayesForSeriesOfInvocs(prev_id, GetId, totoalFuncId);

            var lastIdInVocationInTryStatement = GetIdForLastInvoc();

            var endAltId = GetId();

            Dgml.Flows.Add(new Flow(lastIdInVocationInTryStatement, endAltId));

            var catchBlock = Regex.Match(statement, RE.Catch).Value;
            var invocsInCatchBlock = GetInvocsIn(catchBlock);
            prev_id = startAltId;
            invocsInCatchBlock.FillDGMLArrayesForSeriesOfInvocs(prev_id, GetNextId, totoalFuncId);

            var lastIdInVocationInCatchStatement = GetIdForLastInvoc();

            Dgml.Flows.Add(new Flow(lastIdInVocationInCatchStatement, endAltId));

            Dgml.Functions.Add(new MyFunction(endAltId, AlternativeEnd, "Alt"));
            Dgml.Flows.Add(new Flow(totoalFuncId, endAltId, "Contains"));
        }

        private static void AnalyzeForOrWhileStatement(string statement, string totoalFuncId)
        {
            var startIterId = GetId();
            Dgml.Functions.Add(new MyFunction(startIterId, IterationStart, "Ite"));
            Dgml.Flows.Add(new Flow(totoalFuncId, startIterId, "Contains"));

            var InvocsInForOrWhileBlock = GetInvocsIn(statement);

            var prev_id = startIterId;
            InvocsInForOrWhileBlock.FillDGMLArrayesForSeriesOfInvocs(prev_id, GetId, totoalFuncId);

            var lastIdInVocationInForOrWhileStatement = GetIdForLastInvoc();
            var endAltId = GetId();
            Dgml.Functions.Add(new MyFunction(endAltId, IterationEnd, "Ite"));
            Dgml.Flows.Add(new Flow(totoalFuncId, endAltId, "Contains"));

            Dgml.Flows.Add(new Flow(lastIdInVocationInForOrWhileStatement, endAltId));

            Dgml.Flows.Add(new Flow(endAltId, startIterId));
        }

        private static string GetIdForLastInvoc() => Dgml.Functions.Last().Id;

        private static void AnalyzeIfElseStatement(string statement, string totoalFuncId)
        {
            var ChoId = GetId();  // delay add cho to function list just get id now


            var ifBlock = Regex.Match(statement, RE.If).Value; //get if_block from if_else_block
            var invocsInIfBlock = GetInvocsIn(ifBlock);

            var prev_id = ChoId;

            // we have delay so we will use get id next func not get id
            invocsInIfBlock.FillDGMLArrayesForSeriesOfInvocs(prev_id, GetNextId, totoalFuncId);


            var elseBlock = Regex.Match(statement, RE.Else).Value; //get else_block from if_else_block
            var invocsInElseBlock = GetInvocsIn(elseBlock);

            prev_id = ChoId;
            invocsInElseBlock.FillDGMLArrayesForSeriesOfInvocs(prev_id, GetNextId, totoalFuncId);

            Dgml.Functions.Add(new MyFunction(ChoId, Choice, "Cho"));
            Dgml.Flows.Add(new Flow(totoalFuncId, ChoId, "Contains"));
        }

        private static void AnalyzeSwitchStatement(string statement, string totoalFuncId)
        {
            var ChoId = GetId();  // delay add cho to function list just get id now


            var Switch = Regex.Match(statement, RE.Switch).Value; //get if_block from if_else_block
            var invocsInIfBlock = GetInvocsIn(Switch);

            invocsInIfBlock.ForEach(invoc =>
            {
                var id = GetNextId();
                var methodinfo = MethodInfo.GetOrgenalMethodInfo(invoc);

                Dgml.Functions.Add(new MyFunction(id, invoc, methodinfo.Method, methodinfo.CollapsedOrnull, null));
                Dgml.Flows.Add(new Flow(ChoId, id));
                Dgml.Flows.Add(new Flow(totoalFuncId, id, "Contains"));

                if (methodinfo.CollapsedOrnull != null) UnvisitedMethods.Enqueue(id);
            });

            Dgml.Functions.Add(new MyFunction(ChoId, Choice, "Cho"));
            Dgml.Flows.Add(new Flow(totoalFuncId, ChoId, "Contains"));
        }

        private static void AnalyzeNormalInvocationStatement(string NormalInvoc, string totoalFuncId)
        {
            var methodInfo = MethodInfo.GetOrgenalMethodInfo(NormalInvoc);
            var id = GetId();
            Dgml.Functions.Add(new MyFunction(id, NormalInvoc, methodInfo.Method, methodInfo.CollapsedOrnull, null));
            Dgml.Flows.Add(new Flow(totoalFuncId, id, "Contains"));

            if (methodInfo.CollapsedOrnull != null) UnvisitedMethods.Enqueue(id);

        }

        static void FillDGMLArrayesForSeriesOfInvocs(this List<string> invocs, string prev_id, Func<string> IdProvider, string totoalFuncId)
        {
            invocs.ForEach(invoc =>
            {
                var id = IdProvider();
                var methodinfo = MethodInfo.GetOrgenalMethodInfo(invoc);
                Dgml.Functions.Add(new MyFunction(id, invoc, methodinfo.Method, methodinfo.CollapsedOrnull, null));
                Dgml.Flows.Add(new Flow(prev_id, id));
                Dgml.Flows.Add(new Flow(totoalFuncId, id, "Contains"));
                prev_id = id;

                if (methodinfo.CollapsedOrnull != null) UnvisitedMethods.Enqueue(id);
            });
        }

#endregion

    }
}
