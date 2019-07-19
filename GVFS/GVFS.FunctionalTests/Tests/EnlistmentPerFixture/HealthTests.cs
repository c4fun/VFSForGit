﻿using GVFS.FunctionalTests.Tools;
using GVFS.Tests.Should;
using NUnit.Framework;
using NUnit.Framework.Internal;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;

namespace GVFS.FunctionalTests.Tests.EnlistmentPerFixture
{
    [TestFixture, Category(Categories.WindowsOnly)]
    public class HealthTests : TestsWithEnlistmentPerFixture
    {
        [TestCase, Order(0)]
        public void AfterCloningRepoIsPerfectlyHealthy()
        {
            // .gitignore is always a placeholder on creation
            // .gitconfig is always a modified path in functional tests since it is written at run time

            List<string> topHydratedDirectories = new List<string> { "GVFS", "GVFlt_BugRegressionTest", "GVFlt_DeleteFileTest", "GVFlt_DeleteFolderTest", "GVFlt_EnumTest" };
            List<int> directoryHydrationLevels = new List<int> { 0, 0, 0, 0, 0 };

            this.ValidateHealthOutputValues(
                directory: string.Empty,
                totalFiles: 1211,
                totalFilePercent: 100,
                fastFiles: 1,
                fastFilePercent: 0,
                slowFiles: 1,
                slowFilePercent: 0,
                totalPercent: 0,
                topHydratedDirectories: topHydratedDirectories,
                directoryHydrationLevels: directoryHydrationLevels,
                enlistmentHealthStatus: "OK");
        }

        [TestCase, Order(1)]
        public void PlaceholdersChangeHealthScores()
        {
            // Hydrate all files in the Scripts/ directory as placeholders
            // This creates 6 placeholders, 5 files along with the Scripts/ directory
            this.HydratePlaceholder(Path.Combine(this.Enlistment.RepoRoot, "Scripts/CreateCommonAssemblyVersion.bat"));
            this.HydratePlaceholder(Path.Combine(this.Enlistment.RepoRoot, "Scripts/CreateCommonCliAssemblyVersion.bat"));
            this.HydratePlaceholder(Path.Combine(this.Enlistment.RepoRoot, "Scripts/CreateCommonVersionHeader.bat"));
            this.HydratePlaceholder(Path.Combine(this.Enlistment.RepoRoot, "Scripts/RunFunctionalTests.bat"));
            this.HydratePlaceholder(Path.Combine(this.Enlistment.RepoRoot, "Scripts/RunUnitTests.bat"));

            List<string> topHydratedDirectories = new List<string> { "Scripts", "GVFS", "GVFlt_BugRegressionTest", "GVFlt_DeleteFileTest", "GVFlt_DeleteFolderTest" };
            List<int> directoryHydrationLevels = new List<int> { 5, 0, 0, 0, 0 };

            this.ValidateHealthOutputValues(
                directory: string.Empty,
                totalFiles: 1211,
                totalFilePercent: 100,
                fastFiles: 7,
                fastFilePercent: 1,
                slowFiles: 1,
                slowFilePercent: 0,
                totalPercent:1,
                topHydratedDirectories: topHydratedDirectories,
                directoryHydrationLevels: directoryHydrationLevels,
                enlistmentHealthStatus: "OK");
        }

        [TestCase, Order(2)]
        public void ModifiedPathsChangeHealthScores()
        {
            // Hydrate all files in GVFlt_FileOperationTest as modified paths
            // This creates 2 modified paths and one placeholder
            this.HydrateFullFile(Path.Combine(this.Enlistment.RepoRoot, "GVFlt_FileOperationTest/DeleteExistingFile.txt"));
            this.HydrateFullFile(Path.Combine(this.Enlistment.RepoRoot, "GVFlt_FileOperationTest/WriteAndVerify.txt"));

            List<string> topHydratedDirectories = new List<string> { "Scripts", "GVFlt_FileOperationTest", "GVFS", "GVFlt_BugRegressionTest", "GVFlt_DeleteFileTest" };
            List<int> directoryHydrationLevels = new List<int> { 5, 2, 0, 0, 0 };

            this.ValidateHealthOutputValues(
                directory: string.Empty,
                totalFiles: 1211,
                totalFilePercent: 100,
                fastFiles: 8,
                fastFilePercent: 1,
                slowFiles: 3,
                slowFilePercent: 0,
                totalPercent: 1,
                topHydratedDirectories: topHydratedDirectories,
                directoryHydrationLevels: directoryHydrationLevels,
                enlistmentHealthStatus: "OK");
        }

        [TestCase, Order(3)]
        public void TurnPlaceholdersIntoModifiedPaths()
        {
            // Hydrate the files in Scripts/ from placeholders to modified paths
            this.HydrateFullFile(Path.Combine(this.Enlistment.RepoRoot, "Scripts/CreateCommonAssemblyVersion.bat"));
            this.HydrateFullFile(Path.Combine(this.Enlistment.RepoRoot, "Scripts/CreateCommonCliAssemblyVersion.bat"));
            this.HydrateFullFile(Path.Combine(this.Enlistment.RepoRoot, "Scripts/CreateCommonVersionHeader.bat"));
            this.HydrateFullFile(Path.Combine(this.Enlistment.RepoRoot, "Scripts/RunFunctionalTests.bat"));
            this.HydrateFullFile(Path.Combine(this.Enlistment.RepoRoot, "Scripts/RunUnitTests.bat"));

            List<string> topHydratedDirectories = new List<string> { "Scripts", "GVFlt_FileOperationTest", "GVFS", "GVFlt_BugRegressionTest", "GVFlt_DeleteFileTest" };
            List<int> directoryHydrationLevels = new List<int> { 5, 2, 0, 0, 0 };

            this.ValidateHealthOutputValues(
                directory: string.Empty,
                totalFiles: 1211,
                totalFilePercent: 100,
                fastFiles: 3,
                fastFilePercent: 0,
                slowFiles: 8,
                slowFilePercent: 1,
                totalPercent: 1,
                topHydratedDirectories: topHydratedDirectories,
                directoryHydrationLevels: directoryHydrationLevels,
                enlistmentHealthStatus: "OK");
        }

        [TestCase, Order(4)]
        public void FilterIntoDirectory()
        {
            List<string> topHydratedDirectories = new List<string>();
            List<int> directoryHydrationLevels = new List<int>();

            this.ValidateHealthOutputValues(
                directory: "Scripts/",
                totalFiles: 5,
                totalFilePercent: 100,
                fastFiles: 0,
                fastFilePercent: 0,
                slowFiles: 5,
                slowFilePercent: 100,
                totalPercent: 100,
                topHydratedDirectories: topHydratedDirectories,
                directoryHydrationLevels: directoryHydrationLevels,
                enlistmentHealthStatus: "Highly Hydrated");
        }

        private void HydratePlaceholder(string filePath)
        {
            File.ReadAllText(filePath);
        }

        private void HydrateFullFile(string filePath)
        {
            File.OpenWrite(filePath).Close();
        }

        private void ValidateHealthOutputValues(
            string directory,
            int totalFiles,
            int totalFilePercent,
            int fastFiles,
            int fastFilePercent,
            int slowFiles,
            int slowFilePercent,
            int totalPercent,
            List<string> topHydratedDirectories,
            List<int> directoryHydrationLevels,
            string enlistmentHealthStatus)
        {
            List<string> healthOutputLines = new List<string>(this.Enlistment.Health(directory).Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries));

            healthOutputLines = this.GetHealthData(directory);

            // Strip the lines which have the status
            // These lines all have the format: "(<n>/3) <Status message>...Succeeded"
            while (healthOutputLines[0].StartsWith("("))
            {
                healthOutputLines.RemoveAt(0);
            }

            int numberOfExpectedSubdirectories = topHydratedDirectories.Count;

            this.ValidateTargetDirectory(healthOutputLines[0], directory);
            this.ValidateTotalFileInfo(healthOutputLines[1], totalFiles, totalFilePercent);
            this.ValidateFastFileInfo(healthOutputLines[2], fastFiles, fastFilePercent);
            this.ValidateSlowFileInfo(healthOutputLines[3], slowFiles, slowFilePercent);
            this.ValidateTotalHydration(healthOutputLines[4], totalPercent);
            this.ValidateSubDirectoryHealth(healthOutputLines.GetRange(6, numberOfExpectedSubdirectories), topHydratedDirectories, directoryHydrationLevels);
            this.ValidateEnlistmentStatus(healthOutputLines[6 + numberOfExpectedSubdirectories], enlistmentHealthStatus);
        }

        private List<string> GetHealthData(string directory = null)
        {
            ProcessStartInfo processInfo = new ProcessStartInfo(GVFSTestConfig.PathToGVFS);
            processInfo.Arguments = "health" + (!string.IsNullOrEmpty(directory) ? " -d " + directory : string.Empty);
            processInfo.WindowStyle = ProcessWindowStyle.Hidden;
            processInfo.WorkingDirectory = this.Enlistment.EnlistmentRoot;
            processInfo.UseShellExecute = false;
            processInfo.RedirectStandardOutput = true;

            List<string> healthOutputLines = new List<string>(ProcessHelper.Run(processInfo).Output.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries));

            while (healthOutputLines[0].StartsWith("("))
            {
                healthOutputLines.RemoveAt(0);
            }

            return healthOutputLines;
        }

        private void ValidateTargetDirectory(string outputLine, string targetDirectory)
        {
            // Regex to extract the target directory
            // "Health of directory: <directory>"
            Match lineMatch = Regex.Match(outputLine, @"^Health of directory:\s*(.*)$");

            string outputtedTargetDirectory = lineMatch.Groups[1].Value;

            outputtedTargetDirectory.ShouldEqual(targetDirectory);
        }

        private void ValidateTotalFileInfo(string outputLine, int totalFiles, int totalFilePercent)
        {
            // Regex to extract the total number of files and percentage they represent (should always be 100)
            // "Total files in HEAD commit:           <count> | <percentage>%"
            Match lineMatch = Regex.Match(outputLine, @"^Total files in HEAD commit:\s*([\d,]+)\s*\|\s*(\d+)%$");

            int.TryParse(lineMatch.Groups[1].Value, NumberStyles.AllowThousands, CultureInfo.CurrentCulture.NumberFormat, out int outputtedTotalFiles).ShouldBeTrue();
            int.TryParse(lineMatch.Groups[2].Value, out int outputtedTotalFilePercent).ShouldBeTrue();

            outputtedTotalFiles.ShouldEqual(totalFiles);
            outputtedTotalFilePercent.ShouldEqual(totalFilePercent);
        }

        private void ValidateFastFileInfo(string outputLine, int fastFiles, int fastFilesPercent)
        {
            // Regex to extract the total number of fast files and percentage they represent
            // "Files managed by VFS for Git (fast):    <count> | <percentage>%"
            Match lineMatch = Regex.Match(outputLine, @"^Files managed by VFS for Git \(fast\):\s*([\d,]+)\s*\|\s*(\d+)%$");

            int.TryParse(lineMatch.Groups[1].Value, NumberStyles.AllowThousands, CultureInfo.CurrentCulture.NumberFormat, out int outputtedFastFiles).ShouldBeTrue();
            int.TryParse(lineMatch.Groups[2].Value, out int outputtedFastFilesPercent).ShouldBeTrue();

            outputtedFastFiles.ShouldEqual(fastFiles);
            outputtedFastFilesPercent.ShouldEqual(fastFilesPercent);
        }

        private void ValidateSlowFileInfo(string outputLine, int slowFiles, int slowFilesPercent)
        {
            // Regex to extract the total number of slow files and percentage they represent
            // "Files managed by git (slow):                <count> | <percentage>%"
            Match lineMatch = Regex.Match(outputLine, @"^Files managed by git \(slow\):\s*([\d,]+)\s*\|\s*(\d+)%$");

            int.TryParse(lineMatch.Groups[1].Value, NumberStyles.AllowThousands, CultureInfo.CurrentCulture.NumberFormat, out int outputtedSlowFiles).ShouldBeTrue();
            int.TryParse(lineMatch.Groups[2].Value, out int outputtedSlowFilesPercent).ShouldBeTrue();

            outputtedSlowFiles.ShouldEqual(slowFiles);
            outputtedSlowFilesPercent.ShouldEqual(slowFilesPercent);
        }

        private void ValidateTotalHydration(string outputLine, int totalHydration)
        {
            // Regex to extract the total hydration percentage of the enlistment
            // "Total hydration percentage:            <percentage>%
            Match lineMatch = Regex.Match(outputLine, @"^Total hydration percentage:\s*(\d+)%$");

            int.TryParse(lineMatch.Groups[1].Value, out int outputtedTotalHydration).ShouldBeTrue();

            outputtedTotalHydration.ShouldEqual(totalHydration);
        }

        private void ValidateSubDirectoryHealth(List<string> outputLines, List<string> subdirectories, List<int> healthScores)
        {
            for (int i = 0; i < outputLines.Count; i++)
            {
                // Regex to extract the most hydrated subdirectory names and their hydration percentage
                // "  <hydrated-file-count> | <directory-name>" listed several times for different directories
                Match lineMatch = Regex.Match(outputLines[i], @"^\s*([\d,]+)\s*\|\s*(\S.*\S)\s*$");

                int.TryParse(lineMatch.Groups[1].Value, NumberStyles.AllowThousands, CultureInfo.CurrentCulture.NumberFormat, out int outputtedHealthScore).ShouldBeTrue();
                string outputtedSubdirectory = lineMatch.Groups[2].Value;

                outputtedHealthScore.ShouldEqual(healthScores[i]);
                outputtedSubdirectory.ShouldEqual(subdirectories[i]);
            }
        }

        private void ValidateEnlistmentStatus(string outputLine, string statusMessage)
        {
            // Regex to extract the status message for the enlistment
            // "Repository status: <status-message>"
            Match lineMatch = Regex.Match(outputLine, @"^Repository status:\s*(.*)$");

            string outputtedStatusMessage = lineMatch.Groups[1].Value;

            outputtedStatusMessage.ShouldEqual(statusMessage);
        }
    }
}
