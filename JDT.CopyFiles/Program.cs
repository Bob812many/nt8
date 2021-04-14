using System;
using System.IO;

namespace JDT.CopyFiles
{
    public sealed class Program
    {
        #region Methods

        private static void Main(string[] args)
        {
            NT8FileHelper nt8FileHelper = new NT8FileHelper("JDT.NT8");
            int overwrittenCount = 0;

            FileInfo[] systemFiles = nt8FileHelper.GetNinjaTraderSystemFiles();
            FileInfo[] projectFiles = nt8FileHelper.GetNinjaTraderProjectFiles();

            Console.WriteLine("The following files are about to be overwritten from the system: ");
            for (int i = 0; i < systemFiles.Length; i++)
            {
                if (systemFiles[i] != null)
                {
                    Console.WriteLine(systemFiles[i].Name);
                }
            }
            Console.Write("Do you wish to override these files, Y or N? ");
            ConsoleKeyInfo input = Console.ReadKey();

            if (string.Equals(input.Key.ToString().ToUpper(), "Y"))
            {
                for (int i = 0; i < systemFiles.Length; i++)
                {
                    for (int j = 0; j < projectFiles.Length; j++)
                    {
                        if (systemFiles[i] != null && projectFiles[i] != null)
                        {
                            if (string.Equals(systemFiles[i].Name, projectFiles[j].Name))
                            {
                                File.Copy(systemFiles[i].FullName, projectFiles[j].FullName, true);
                                overwrittenCount++;
                            }
                        }
                    }
                }
            }
            else
            {
            }

            Console.WriteLine(string.Format("\n{0} files were overwritten.  Press any key to exit.", overwrittenCount));
            Console.ReadKey();
        }

        #endregion Methods
    }
}