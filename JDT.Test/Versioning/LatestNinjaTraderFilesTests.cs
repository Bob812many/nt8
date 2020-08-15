using JDT.CopyFiles;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Linq;

namespace JDT.Test.Versioning
{
    [TestClass]
    public class LatestNinjaTraderFilesTests
    {
        private readonly NT8FileHelper nt8FileHelper = new NT8FileHelper("JDT.NT8");

        private FileInfo fileProject;

        private FileInfo fileSystem;

        private Version versionProject;

        private Version versionSystem;

        [TestMethod]
        public void NinjaTraderCoreAssemblyVersionIsLatestTest()
        {
            bool isMatch = nt8FileHelper.IsProjectVersionMatchSystemVersion(NT8FileHelper.NT8_CORE_ASSEMBLY, out versionSystem, out versionProject, out fileSystem, out fileProject);

            Assert.IsTrue(isMatch, VersionMismatchMessage(NT8FileHelper.NT8_CORE_ASSEMBLY, versionProject, versionSystem));
        }

        [TestMethod]
        public void NinjaTraderCustomAssemblyVersionIsLatestTest()
        {
            bool isMatch = nt8FileHelper.IsProjectVersionMatchSystemVersion(NT8FileHelper.NT8_CUSTOM_ASSEMBLY, out versionSystem, out versionProject, out fileSystem, out fileProject);

            Assert.IsTrue(isMatch, VersionMismatchMessage(NT8FileHelper.NT8_CUSTOM_ASSEMBLY, versionProject, versionSystem));
        }

        [TestMethod]
        public void NinjaTraderGuiAssemblyVersionIsLatestTest()
        {
            bool isMatch = nt8FileHelper.IsProjectVersionMatchSystemVersion(NT8FileHelper.NT8_GUI_ASSEMBLY, out versionSystem, out versionProject, out fileSystem, out fileProject);

            Assert.IsTrue(isMatch, VersionMismatchMessage(NT8FileHelper.NT8_GUI_ASSEMBLY, versionProject, versionSystem));
        }

        [TestMethod]
        public void NinjaTraderSharpDXAssemblyVersionIsLatestTest()
        {
            bool isMatch = nt8FileHelper.IsProjectVersionMatchSystemVersion(NT8FileHelper.NT8_SHARPDX_ASSEMBLY, out versionSystem, out versionProject, out fileSystem, out fileProject);

            Assert.IsTrue(isMatch, VersionMismatchMessage(NT8FileHelper.NT8_SHARPDX_ASSEMBLY, versionProject, versionSystem));
        }

        [TestMethod]
        public void NinjaTraderSharpDXDirect2D1AssemblyVersionIsLatestTest()
        {
            bool isMatch = nt8FileHelper.IsProjectVersionMatchSystemVersion(NT8FileHelper.NT8_SHARPDX_DIRECT2D1_ASSEMBLY, out versionSystem, out versionProject, out fileSystem, out fileProject);

            Assert.IsTrue(isMatch, VersionMismatchMessage(NT8FileHelper.NT8_SHARPDX_DIRECT2D1_ASSEMBLY, versionProject, versionSystem));
        }

        [TestMethod]
        public void NT8InstallDefaultlLocationTest()
        {
            DirectoryInfo installDirectory = nt8FileHelper.GetNT8InstallDirectory(NT8FileHelper.NT8_DEFAULT_REGISTRY_KEY);

            Assert.IsNotNull(installDirectory);
            Assert.IsTrue(string.Equals(installDirectory.FullName, NT8FileHelper.NT8_DEFAULT_INSTALL_DIRECTORY));
        }

        [TestMethod]
        [DataRow("JDT.NT8")]
        public void ProjectFolderLocationTest(string projectFolderName)
        {
            string project = this.nt8FileHelper.GetProjectDirectory();

            DirectoryInfo directoryInfo = new DirectoryInfo(project);
            FileInfo[] projectFile = directoryInfo.GetFiles($"{directoryInfo.Name}.csproj");

            Assert.IsTrue(string.Equals(directoryInfo.Name, projectFolderName));
            Assert.IsTrue(string.Equals($"{directoryInfo.Name}.csproj", projectFile.FirstOrDefault().Name));
        }

        private static string VersionMismatchMessage(string assemblyFileName, Version versionProject, Version versionSystem)
        {
            return $"{assemblyFileName} current version {versionProject} does not match.  System version is {versionSystem}.";
        }
    }
}