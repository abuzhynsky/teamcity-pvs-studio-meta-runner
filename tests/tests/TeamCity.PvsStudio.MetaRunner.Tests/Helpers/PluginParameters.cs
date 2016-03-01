using System;
using System.IO;
using System.Reflection;

namespace TeamCity.PvsStudio.MetaRunner.Tests.Helpers
{
    public static class PluginParameters
    {
        public static string PvsStudioOutputPath
        {
            get { return Path.Combine(CurrentAssemblyPath, "temp", "result.xml"); }
        }

        public static string XsltOutputPath
        {
            get { return Path.Combine(CurrentAssemblyPath, "temp", "resharperResult.xml"); }
        }

        public static string PvsStudioConsolePath
        {
            get { return @"C:\Program Files (x86)\PVS-Studio\PVS-Studio_Cs.exe"; }
        }

        public static string MsXslPath
        {
            get { return Path.Combine(PluginPath, "msxsl.exe"); }
        }

        public static string PvsStudioParameters
        {
            get { return $"-r -o {PvsStudioOutputPath} -t {SolutionPath}"; }
        }

        public static string MsXslParameters
        {
            get { return GenerateXsltParameters(PvsStudioOutputPath); }
        }

        public static string GenerateXsltParameters(string inputPath)
        {
            return $"{inputPath} {XsltPath} -o {XsltOutputPath}";
        }

        public static string UnknownIssueTypeReportPath
        {
            get { return Path.Combine(CurrentAssemblyPath, "TestData", "UnknownIssueTypeReport.xml"); }
        }

        private static string SolutionPath
        {
            get
            {
                var solutionDirectoryPath = GetParentDirectoryPath(CurrentAssemblyPath, 4);

                return Path.Combine(solutionDirectoryPath, "TeamCity.PvsStudio.MetaRunner.Tests.sln");
            }
        }

        private static string PluginPath
        {
            get
            {
                var rootPath = GetParentDirectoryPath(CurrentAssemblyPath, 5);

                return Path.Combine(rootPath, "plugin", "agent", "bin");
            }
        }

        private static string XsltPath
        {
            get { return Path.Combine(PluginPath, "ResharperReport.xslt"); }
        }

        private static string CurrentAssemblyPath
        {
            get
            {
                var currentAssemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase);

                if (currentAssemblyPath == null)
                {
                    throw new DirectoryNotFoundException();
                }

                return new Uri(currentAssemblyPath).LocalPath;
            }
        }

        private static string GetParentDirectoryPath(string directoryPath, int levelsUp)
        {
            var resultPath = directoryPath;

            for (var i = 0; i < levelsUp; i++)
            {
                resultPath = Directory.GetParent(resultPath).FullName;
            }

            return resultPath;
        }
    }
}