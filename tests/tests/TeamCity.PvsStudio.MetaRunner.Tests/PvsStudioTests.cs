﻿using System.Collections.Generic;
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
        private const string CategoryGeneralAnalysis = "General Analysis";
        private const string CategoryMissing = "Unknown Inspections";

        private const string ErrorCode3001 = "V3001";
        private const string ErrorCode3010 = "V3010";
        private const string ErrorCodeMissing = "V9999";

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
                new PvsStudioExpectedErrorCode(ErrorCode3001, CategoryGeneralAnalysis, 1, 1),
                new PvsStudioExpectedErrorCode(ErrorCode3010, CategoryGeneralAnalysis, 1, 1)
            };

            AssertValidPvsStudioAnalysisReport(PluginParameters.PvsStudioOutputPath, expectedErrorCodes);
        }

        [Fact]
        public async Task UnknownIssueTypeXsltOutputTest()
        {
            var xsltResult = await ProcessEx.RunAsync(PluginParameters.MsXslPath, PluginParameters.GenerateXsltParameters(PluginParameters.UnknownIssueTypeReportPath, false));
            AssertSuccessfulProcessCompletion(xsltResult);

            var expectedErrorCodes = new List<PvsStudioExpectedErrorCode>
            {
                new PvsStudioExpectedErrorCode(ErrorCode3001, CategoryGeneralAnalysis, 1, 1),
                new PvsStudioExpectedErrorCode(ErrorCodeMissing, CategoryMissing, 2, 2)
            };

            AssertValidResharperAnalysisReport(PluginParameters.XsltOutputPath, expectedErrorCodes, false);
        }

        [Fact]
        public async Task TreatPriority1IssuesAsErrorsXsltOutputTest()
        {
            var xsltResult = await ProcessEx.RunAsync(PluginParameters.MsXslPath, PluginParameters.GenerateXsltParameters(PluginParameters.UnknownIssueTypeReportPath, true));
            AssertSuccessfulProcessCompletion(xsltResult);

            var expectedErrorCodes = new List<PvsStudioExpectedErrorCode>
            {
                new PvsStudioExpectedErrorCode(ErrorCode3001, CategoryGeneralAnalysis, 1, 1),
                new PvsStudioExpectedErrorCode(ErrorCodeMissing, CategoryMissing, 2, 2)
            };

            AssertValidResharperAnalysisReport(PluginParameters.XsltOutputPath, expectedErrorCodes, true);
        }

        [Fact]
        public async Task XsltOutputTest()
        {
            var pvsStudioResult = await ProcessEx.RunAsync(PluginParameters.PvsStudioConsolePath, PluginParameters.PvsStudioParameters);
            AssertSuccessfulProcessCompletion(pvsStudioResult);

            var expectedErrorCodes = new List<PvsStudioExpectedErrorCode>
            {
                new PvsStudioExpectedErrorCode(ErrorCode3001, CategoryGeneralAnalysis, 1, 1),
                new PvsStudioExpectedErrorCode(ErrorCode3010, CategoryGeneralAnalysis, 1, 1)
            };

            AssertValidPvsStudioAnalysisReport(PluginParameters.PvsStudioOutputPath, expectedErrorCodes);

            var xsltResult = await ProcessEx.RunAsync(PluginParameters.MsXslPath, PluginParameters.MsXslParameters);
            AssertSuccessfulProcessCompletion(xsltResult);

            AssertValidResharperAnalysisReport(PluginParameters.XsltOutputPath, expectedErrorCodes, false);
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
        private static void AssertValidResharperAnalysisReport(string filePath, ICollection<PvsStudioExpectedErrorCode> expectedErrorCodes, bool treatPriority1IssuesAsErrors)
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

            foreach (var expectedErrorCode in expectedErrorCodes)
            {
                AssertResharperIssueTypes(issueTypeElements, expectedErrorCode, treatPriority1IssuesAsErrors);
            }

            var issuesElement = reportElement.Element("Issues");
            Assert.NotNull(issuesElement);

            var projectElement = issuesElement.Element("Project");
            Assert.NotNull(projectElement);

            var issueElements = projectElement.Descendants("Issue").ToList();

            Assert.NotNull(issueElements);
            Assert.Equal(expectedErrorCodes.Select(_ => _.OccurrenceCount).Sum(), issueElements.Count);
        }

        [AssertionMethod]
        private static void AssertResharperIssueTypes(IEnumerable<XElement> issueTypeElements, PvsStudioExpectedErrorCode expectedErrorCode, bool treatPriority1IssuesAsErrors)
        {
            var issueType = Assert.Single(issueTypeElements, _ => _.Attribute("Id").Value == expectedErrorCode.ErrorCode);
            Assert.NotNull(issueType);

            Assert.Equal($"PVS-Studio {expectedErrorCode.Category}. Priority: {expectedErrorCode.Priority}", issueType.Attribute("Category").Value);
            Assert.Equal($"{issueType.Attribute("Id").Value}. {issueType.Attribute("Description").Value}", issueType.Attribute("SubCategory").Value);

            AssertSeverity(expectedErrorCode, issueType, treatPriority1IssuesAsErrors);
        }

        [AssertionMethod]
        private static void AssertSeverity(PvsStudioExpectedErrorCode expectedErrorCode, XElement issueType, bool treatPriority1IssuesAsErrors)
        {
            var expectedSeverity = treatPriority1IssuesAsErrors && expectedErrorCode.Priority == 1 ? "ERROR" : "WARNING";
            Assert.Equal(expectedSeverity, issueType.Attribute("Severity").Value);
        }
    }
}
