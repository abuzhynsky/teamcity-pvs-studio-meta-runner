using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using JetBrains.Annotations;
using RunProcessAsTask;
using TeamCity.PvsStudio.MetaRunner.Tests.Helpers;
using Xunit;

namespace TeamCity.PvsStudio.MetaRunner.Tests
{
    public class PvsStudioTests
    {
        public PvsStudioTests()
        {
            DeleteExistingFile(PluginParameters.PvsStudioOutputPath);
            DeleteExistingFile(PluginParameters.XsltOutputPath);
        }

        [Fact]
        public async Task PvsStudioOutputTest()
        {
            var result = await ProcessEx.RunAsync(PluginParameters.PvsStudioConsolePath, PluginParameters.PvsStudioParameters);
            AssertSuccessfulProcessCompletion(result);

            var expectedErrorCodes = new List<PvsStudioExpectedErrorCode>
            {
                new PvsStudioExpectedErrorCode("V3001", 1),
                new PvsStudioExpectedErrorCode("V3010", 1)
            };

            AssertValidPvsStudioAnalysisReport(PluginParameters.PvsStudioOutputPath, expectedErrorCodes);
        }

        [Fact]
        public async Task UnknownIssueTypeXsltOutputTest()
        {
            var xsltResult = await ProcessEx.RunAsync(PluginParameters.MsXslPath, PluginParameters.GenerateXsltParameters(PluginParameters.UnknownIssueTypeReportPath));
            AssertSuccessfulProcessCompletion(xsltResult);

            var expectedErrorCodes = new List<PvsStudioExpectedErrorCode>
            {
                new PvsStudioExpectedErrorCode("V3001", 1),
                new PvsStudioExpectedErrorCode("V9999", 2)
            };

            AssertValidResharperAnalysisReport(PluginParameters.XsltOutputPath, expectedErrorCodes);
        }

        [Fact]
        public async Task XsltOutputTest()
        {
            var pvsStudioResult = await ProcessEx.RunAsync(PluginParameters.PvsStudioConsolePath, PluginParameters.PvsStudioParameters);
            AssertSuccessfulProcessCompletion(pvsStudioResult);

            var expectedErrorCodes = new List<PvsStudioExpectedErrorCode>
            {
                new PvsStudioExpectedErrorCode("V3001", 1),
                new PvsStudioExpectedErrorCode("V3010", 1)
            };

            AssertValidPvsStudioAnalysisReport(PluginParameters.PvsStudioOutputPath, expectedErrorCodes);

            var xsltResult = await ProcessEx.RunAsync(PluginParameters.MsXslPath, PluginParameters.MsXslParameters);
            AssertSuccessfulProcessCompletion(xsltResult);

            AssertValidResharperAnalysisReport(PluginParameters.XsltOutputPath, expectedErrorCodes);
        }

        private static void DeleteExistingFile(string filePath)
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }

        [AssertionMethod]
        private static void AssertFileExists(string filePath)
        {
            Assert.True(File.Exists(filePath));
        }

        [AssertionMethod]
        private static void AssertSuccessfulProcessCompletion(ProcessResults result)
        {
            Assert.NotNull(result);
            Assert.NotEqual(1, result.Process.ExitCode);
            Assert.Empty(result.StandardError);
        }

        [AssertionMethod]
        private static void AssertValidPvsStudioAnalysisReport(string filePath, ICollection<PvsStudioExpectedErrorCode> expectedErrorCodes)
        {
            AssertFileExists(filePath);

            var outputDocument = XDocument.Load(filePath);
            Assert.NotNull(outputDocument);

            var analysisLogs = outputDocument.Descendants("PVS-Studio_Analysis_Log").ToList();

            Assert.NotNull(analysisLogs);
            Assert.Equal(expectedErrorCodes.Count, analysisLogs.Count);

            var errorCodes = analysisLogs.Select(analysisLog => analysisLog.Element("ErrorCode")).ToList();

            var groupedCodesInReport = errorCodes.GroupBy(_ => _).ToList();

            foreach (var expectedErrorCode in expectedErrorCodes)
            {
                var grouping = Assert.Single(groupedCodesInReport, _ => _.Key.Value == expectedErrorCode.ErrorCode);
                Assert.NotNull(grouping);
                Assert.Equal(expectedErrorCode.OccurrenceCount, grouping.Count());
            }
        }

        [AssertionMethod]
        private static void AssertValidResharperAnalysisReport(string filePath, ICollection<PvsStudioExpectedErrorCode> expectedErrorCodes)
        {
            AssertFileExists(filePath);

            var resharperOutputDocument = XDocument.Load(filePath);
            Assert.NotNull(resharperOutputDocument);

            var reportElement = resharperOutputDocument.Element("Report");
            Assert.NotNull(reportElement);

            var issueTypesElement = reportElement.Element("IssueTypes");
            Assert.NotNull(issueTypesElement);

            var issueTypeElements = issueTypesElement.Descendants("IssueType").ToList();

            Assert.NotNull(issueTypeElements);
            Assert.Equal(expectedErrorCodes.Count, issueTypeElements.Count);

            var issuesElement = reportElement.Element("Issues");
            Assert.NotNull(issuesElement);

            var projectElement = issuesElement.Element("Project");
            Assert.NotNull(projectElement);

            var issueElements = projectElement.Descendants("Issue").ToList();

            Assert.NotNull(issueElements);
            Assert.Equal(expectedErrorCodes.Select(_ => _.OccurrenceCount).Sum(), issueElements.Count);
        }
    }
}
