using Microsoft.VisualStudio.TextTemplating.VSHost;

namespace FCM
{
    class SFG : BaseCodeGeneratorWithSite
    {
        public const string Name = nameof(FCMCommand);
        public const string Discription = "Functhional Code Map Generator";
        public override string GetDefaultExtension() => ".dgml";
        protected override byte[] GenerateCode(string inputFileName, string inputFileContent)
        {
            var customCode = DTE.GetContext();

            Analizer.AnalizeMethods(inputFileContent, customCode);
            
            return Dgml.GeneratDGMLFile();
        }
    }
}
