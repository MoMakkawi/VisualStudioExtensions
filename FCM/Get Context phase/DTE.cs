using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.IO;
using System.Linq;

namespace FCM
{
    public class DTE
    {
        public static string GetContext()
        {
            string CustomDirectory = GetCustomDirectory();

            //there is at least 1 cs file , that makes us soure that the the tool only appears on cs files 
            var PathsAllCsFiles = Directory
                .GetFiles(CustomDirectory, "*.cs", SearchOption.AllDirectories)
                .Where(path => !path.Contains("Debug"))
                .ToList();

            //from all cs files  collect as set of strings chars all codes  
            var contextAsASetOfStrings =
                PathsAllCsFiles
                .Select(PathcsFile => File.ReadAllText(PathcsFile));

            //collect all codes in 1 string
            var contextAsString = String.Concat(contextAsASetOfStrings);

            return contextAsString;
        }
        static string GetCustomDirectory()
        {
            // By GetCustomSlnPath() : if Custom Sln Path = "C:/x/y..../z/Bace Directory/solution.sln"   
            // => By Path.GetDirectoryName(CustomSlnPath) : Custom Directory = "C:/x/y..../z/Bace Directory" 

            var CustomSlnPath = GetCustomSlnPath();
            var CustomDirectory = Path.GetDirectoryName(CustomSlnPath);
            return CustomDirectory;
        }
        static string GetCustomSlnPath()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            // DTE = Development Tools Environoment
            EnvDTE.DTE dte = Package.GetGlobalService(typeof(SDTE)) as EnvDTE.DTE;
            string SlnPath = Path.GetFullPath(dte.Solution.FullName);

            if (SlnPath == null) throw new Exception("Sln File Cannot be Found");

            return SlnPath;
        }
    }
}
