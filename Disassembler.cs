using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Web;

namespace Anonymous
{
    /// <summary>
    /// Credit to Daniel Williams from https://stackoverflow.com/questions/2735643/how-can-i-tell-whether-two-net-dlls-are-the-same
    /// </summary>
    public class Disassembler
    {
        public static Regex regexMVID = new Regex("//\\s*MVID\\:\\s*\\{[a-zA-Z0-9\\-]+\\}", RegexOptions.Multiline | RegexOptions.Compiled);
        public static Regex regexImageBase = new Regex("//\\s*Image\\s+base\\:\\s0x[0-9A-Fa-f]*", RegexOptions.Multiline | RegexOptions.Compiled);
        public static Regex regexTimeStamp = new Regex("//\\s*Time-date\\s+stamp\\:\\s*0x[0-9A-Fa-f]*", RegexOptions.Multiline | RegexOptions.Compiled);

        private static readonly Lazy<Assembly> currentAssembly = new Lazy<Assembly>(() =>
        {
            return MethodBase.GetCurrentMethod().DeclaringType.Assembly;
        });

        private static readonly Lazy<string> executingAssemblyPath = new Lazy<string>(() =>
        {
            return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        });

        private static readonly Lazy<string> currentAssemblyFolder = new Lazy<string>(() =>
        {
            return Path.GetDirectoryName(currentAssembly.Value.Location);
        });

        private static readonly Lazy<string[]> arrResources = new Lazy<string[]>(() =>
        {
            return currentAssembly.Value.GetManifestResourceNames();
        });

        private const string ildasmArguments = "/all /text \"{0}\"";

        public static string ILDasmFileLocation
        {
            get
            {
                return Path.Combine(ConfigurationManager.AppSettings["ildasmFolder"], "ildasm.exe");

                //return Path.Combine(executingAssemblyPath.Value, "ildasm.exe");
            }
        }

        static Disassembler()
        {
            //extract the ildasm file to the executing assembly location
            //ExtractFileToLocation("ildasm.exe", ILDasmFileLocation);
            ExtractFileToLocation(ConfigurationManager.AppSettings["AppPathInWwwroot"] + @"bin\Anonymous.dll", ConfigurationManager.AppSettings["AppPathInWwwroot"] + "anonymousDllDisassembled.dll");
        }

        /// <summary>
        /// Saves the file from embedded resource to a given location.
        /// </summary>
        /// <param name="embeddedResourceName">Name of the embedded resource.</param>
        /// <param name="fileName">Name of the file.</param>
        protected static void SaveFileFromEmbeddedResource(string embeddedResourceName, string fileName)
        {
            if (File.Exists(fileName))
            {
                //the file already exists, we can add deletion here if we want to change the version of the 7zip
                return;
            }
            FileInfo fileInfoOutputFile = new FileInfo(fileName);

            using (FileStream streamToOutputFile = fileInfoOutputFile.OpenWrite())
            using (Stream streamToResourceFile = currentAssembly.Value.GetManifestResourceStream(embeddedResourceName))
            {
                const int size = 4096;
                byte[] bytes = new byte[4096];
                int numBytes;
                while ((numBytes = streamToResourceFile.Read(bytes, 0, size)) > 0)
                {
                    streamToOutputFile.Write(bytes, 0, numBytes);
                }

                streamToOutputFile.Close();
                streamToResourceFile.Close();
            }
        }

        /// <summary>
        /// Searches the embedded resource and extracts it to the given location.
        /// </summary>
        /// <param name="fileNameInDll">The file name in DLL.</param>
        /// <param name="outFileName">Name of the out file.</param>
        protected static void ExtractFileToLocation(string fileNameInDll, string outFileName)
        {
            string resourcePath = arrResources.Value.Where(resource => resource.EndsWith(fileNameInDll, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
            if (resourcePath == null)
            {
                throw new Exception(string.Format("Cannot find {0} in the embedded resources of {1}", fileNameInDll, currentAssembly.Value.FullName));
            }
            SaveFileFromEmbeddedResource(resourcePath, outFileName);
        }

        public static string GetDisassembledFile(string assemblyFilePath = "")
        {
            if (assemblyFilePath == string.Empty)
            {
                assemblyFilePath = ConfigurationManager.AppSettings["AppPathInWwwroot"] + "/bin/Anonymous.dll";
            }
            if (!File.Exists(assemblyFilePath))
            {
                throw new InvalidOperationException(string.Format("The file {0} does not exist!", assemblyFilePath));
            }

            //string tempFileName = Path.GetTempFileName();
            string tempFileName = ConfigurationManager.AppSettings["AppPathInWwwroot"] + "anonymousDllDisassembled.dll";
            if (File.Exists(tempFileName))
            { 
                File.Delete(tempFileName);
            }

            var startInfo = new ProcessStartInfo(ILDasmFileLocation, string.Format(ildasmArguments, assemblyFilePath));
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            startInfo.CreateNoWindow = true;
            startInfo.UseShellExecute = false;
            startInfo.RedirectStandardOutput = true;

            using (var process = System.Diagnostics.Process.Start(startInfo))
            {
                string output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();

                if (process.ExitCode > 0)
                {
                    throw new InvalidOperationException(
                        string.Format("Generating IL code for file {0} failed with exit code - {1}. Log: {2}",
                        assemblyFilePath, process.ExitCode, output));
                }

                File.WriteAllText(tempFileName, output);
            }

            RemoveUnnededRows(tempFileName);
            return tempFileName;
        }

        private static void RemoveUnnededRows(string fileName)
        {
            string fileContent = File.ReadAllText(fileName);
            //remove MVID
            fileContent = regexMVID.Replace(fileContent, string.Empty);
            //remove Image Base
            fileContent = regexImageBase.Replace(fileContent, string.Empty);
            //remove Time Stamp
            fileContent = regexTimeStamp.Replace(fileContent, string.Empty);
            File.WriteAllText(fileName, fileContent);
        }

        public static string DisassembleFile(string assemblyFilePath = "")
        {
            string disassembledFile = GetDisassembledFile(assemblyFilePath);
            try
            {
                return File.ReadAllText(disassembledFile);
            }
            finally
            {
                if (File.Exists(disassembledFile))
                {
                    File.Delete(disassembledFile);
                }
            }
        }

    }
}