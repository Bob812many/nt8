using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace JDT.CopyFiles
{
    public class NT8FileHelper
    {
        #region Fields

        public const string NT8_CORE_ASSEMBLY = "NinjaTrader.Core.dll";

        public const string NT8_CUSTOM_ASSEMBLY = "NinjaTrader.Custom.dll";

        public const string NT8_DEFAULT_INSTALL_DIRECTORY = @"C:\Program Files (x86)\NinjaTrader 8\";

        public const string NT8_DEFAULT_REGISTRY_KEY = @"Software\Wow6432Node\NinjaTrader, LLC\NinjaTrader 8";

        public const string NT8_GUI_ASSEMBLY = "NinjaTrader.Gui.dll";

        public const string NT8_SHARPDX_ASSEMBLY = "SharpDX.dll";

        public const string NT8_SHARPDX_DIRECT2D1_ASSEMBLY = "SharpDX.Direct2D1.dll";

        private readonly string projectFolderName;

        #endregion Fields

        public NT8FileHelper(string projectFolderName)
        {
            this.projectFolderName = projectFolderName;
        }

        #region Methods

        public string GetCurrentUser()
        {
            string currentUserPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

            return currentUserPath;
        }

        public Version GetFileVersion(string fullName)
        {
            Version version = null;

            AssemblyName assemblyName = AssemblyName.GetAssemblyName(fullName);

            if (assemblyName != null)
            {
                version = assemblyName.Version;
            }

            return version;
        }

        public string GetProjectLibsDirectory(string projectFolderName)
        {
            string directory = Path.Combine(new DirectoryInfo(Environment.CurrentDirectory).Parent.Parent.Parent.FullName, $"{projectFolderName}\\Libs");

            return directory;
        }

        public string GetProjectDirectory()
        {
            string directory = Path.Combine(new DirectoryInfo(Environment.CurrentDirectory).Parent.Parent.Parent.FullName, this.projectFolderName);

            return directory;
        }

        public FileInfo[] GetNinjaTraderProjectFiles()
        {
            FileInfo[] ninjaTraderProjectFiles = new DirectoryInfo(this.GetProjectLibsDirectory(this.projectFolderName)).GetFiles("*.dll");

            return ninjaTraderProjectFiles;
        }

        public FileInfo[] GetNinjaTraderSystemFiles()
        {
            List<FileInfo> systemNT8Files = new List<FileInfo>();

            DirectoryInfo ntSystemDirectory = this.GetNT8SystemDirectory();
            DirectoryInfo ntCustomDirectory = new DirectoryInfo(Path.Combine(this.GetCurrentUser(), @"Documents\NinjaTrader 8\bin\Custom"));

            if (ntSystemDirectory != null)
            {
                systemNT8Files.Add(ntSystemDirectory.GetFiles("NinjaTrader.Core.dll").FirstOrDefault());
                systemNT8Files.Add(ntCustomDirectory.GetFiles("NinjaTrader.Custom.dll").FirstOrDefault());
                systemNT8Files.Add(ntSystemDirectory.GetFiles("NinjaTrader.Gui.dll").FirstOrDefault());
                systemNT8Files.Add(ntSystemDirectory.GetFiles("SharpDX.Direct2D1.dll").FirstOrDefault());
                systemNT8Files.Add(ntSystemDirectory.GetFiles("SharpDX.dll").FirstOrDefault());
            }

            return systemNT8Files.ToArray();
        }

        public DirectoryInfo GetNT8InstallDirectory(string registryKey)
        {
            DirectoryInfo installDirectoryInfo = null;

            using (RegistryKey key = Registry.LocalMachine.OpenSubKey(registryKey))
            {
                if (key != null)
                {
                    object o = key.GetValue("InstallDir");
                    if (o != null)
                    {
                        string path = o as string;

                        if (!string.IsNullOrEmpty(path))
                        {
                            installDirectoryInfo = new DirectoryInfo(path);
                        }
                    }
                }
            }

            return installDirectoryInfo;
        }

        public DirectoryInfo GetNT8SystemDirectory()
        {
            DirectoryInfo path = this.GetNT8InstallDirectory(NT8_DEFAULT_REGISTRY_KEY);

            string location = Environment.Is64BitOperatingSystem ? Path.Combine(path.FullName, "bin64") : Path.Combine(path.FullName, "bin");

            return new DirectoryInfo(location);
        }

        public DirectoryInfo GetSolutionNTDirectory()
        {
            DirectoryInfo solutionDirectoryInfo = null;

            return solutionDirectoryInfo;
        }

        public bool IsProjectVersionMatchSystemVersion(string assemblyName, out Version versionSystem, out Version versionProject, out FileInfo fileSystem, out FileInfo fileProject)
        {
            bool isMatch = true;

            versionSystem = null;
            versionProject = null;
            fileSystem = null;
            fileProject = null;

            FileInfo[] ntSystemFiles = this.GetNinjaTraderSystemFiles();
            FileInfo[] ntProjectFiles = this.GetNinjaTraderProjectFiles();

            for (int i = 0; i < ntSystemFiles.Length; i++)
            {
                string nameSystem = ntSystemFiles[i].Name;
                versionSystem = this.GetFileVersion(ntSystemFiles[i].FullName);

                if (nameSystem == assemblyName)
                {
                    fileSystem = ntSystemFiles[i];
                    for (int j = 0; j < ntProjectFiles.Length; j++)
                    {
                        string nameProject = ntProjectFiles[j].Name;

                        if (nameProject == nameSystem)
                        {
                            versionProject = this.GetFileVersion(ntProjectFiles[j].FullName);
                            fileProject = ntProjectFiles[j];
                            if (versionProject != versionSystem)
                            {
                                isMatch = false;
                                return isMatch;
                            }
                        }
                    }
                }
            }

            return isMatch;
        }

        #endregion Methods
    }
}