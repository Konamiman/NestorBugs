using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using System.IO;
using System.Reflection;
using System.Xml.Linq;
using System.Xml.XPath;
using System.Xml;
using System.Diagnostics;

namespace Konamiman.SignAllUnsignedReferences
{
    // This tool will sign all the unsigned referenced assemblies in all projects of the solution.
    // Uses info from: http://buffered.io/2008/07/09/net-fu-signing-an-unsigned-assembly-without-delay-signing/

    class Program
    {
        string MyExecutableFolder;
        string IlasmPath;
        string SolutionFilePath;
        string PublicKeyToken;

        //These are configurable via .exe.config and command line

        string SolutionFolder;
        string KeyFilePath;
        string IldasmPath;

        static int Main(string[] args)
        {
            try {
                (new Program()).Run(args);
                return 0;
            }
            catch(Exception ex) {
                Console.WriteLine("*** " + ex.Message);
                return 1;
            }
        }


        void Run(string[] args)
        {
            Write("SignAllUnsignedReferences - (c) 2011 by Konamiman");
            Write("");

            InitializeVariables();
            ConfigureFromConfigFile();
            ConfigureFromCommandLine(args);
            AdjustAndVerifyConfiguredValues();

            var projectFolders = GetSolutionFolders();
            foreach(var folder in projectFolders) {
                ProcessFolder(folder);
            }

            Write("");
            Write("Done!");
        }

        void InitializeVariables()
        {
            MyExecutableFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            IlasmPath = Path.Combine(
                System.Runtime.InteropServices.RuntimeEnvironment.GetRuntimeDirectory(),
                "ilasm.exe");
            if(!File.Exists(IlasmPath)) {
                throw new ApplicationException("Can't find ilasm.exe on folder " + System.Runtime.InteropServices.RuntimeEnvironment.GetRuntimeDirectory());
            }
        }

        void ConfigureFromConfigFile()
        {
            // Default values will be used if no .exe.config file is available

            SolutionFolder = Settings.Default.SolutionFolder;
            KeyFilePath = Settings.Default.KeyFilePath;
            IldasmPath = Settings.Default.IldasmPath;
        }

        void ConfigureFromCommandLine(string[] args)
        {
            foreach(var arg in args) {
                var tokens = arg.Split('=');
                if(tokens.Length != 2) {
                    throw new ApplicationException("Invalid command line argument: " + arg);
                }

                if(tokens[0].Equals("SolutionFolder", StringComparison.InvariantCultureIgnoreCase)) {
                    SolutionFolder = tokens[1];
                }
                else if(tokens[0].Equals("KeyFilePath", StringComparison.InvariantCultureIgnoreCase)) {
                    KeyFilePath = tokens[1];
                }
                else if(tokens[0].Equals("IldasmPath", StringComparison.InvariantCultureIgnoreCase)) {
                    IldasmPath = tokens[1];
                }
                else {
                    throw new ApplicationException("Invalid command line argument: " + arg);
                }
            }
        }

        void AdjustAndVerifyConfiguredValues()
        {
            if(IldasmPath == "") {
                IldasmPath = Path.Combine(MyExecutableFolder, "ildasm.exe");
            }

            SolutionFolder = Path.Combine(MyExecutableFolder, SolutionFolder);
            SolutionFilePath = Directory.EnumerateFiles(SolutionFolder, "*.sln", SearchOption.TopDirectoryOnly).FirstOrDefault();
            if(SolutionFilePath == null) {
                throw new ApplicationException("Could not find a solution file in path " + SolutionFolder);
            }

            var solutionFileName = Path.GetFileNameWithoutExtension(SolutionFilePath);
            KeyFilePath = KeyFilePath.Replace("|SolutionName|", solutionFileName);
            KeyFilePath = Path.Combine(MyExecutableFolder, KeyFilePath);
            if(!File.Exists(KeyFilePath)) {
                throw new ApplicationException("Could not find the key pair file " + KeyFilePath);
            }

            var publicKeyTokenBytes = GetPublicKeyTokenBytes(KeyFilePath);
            PublicKeyToken = BitConverter.ToString(publicKeyTokenBytes).Replace('-', ' ');

            IlasmPath = Environment.ExpandEnvironmentVariables(IlasmPath);
            IldasmPath = Environment.ExpandEnvironmentVariables(IldasmPath);
        }

        string[] GetSolutionFolders()
        {
            return Directory.EnumerateDirectories(SolutionFolder, "*.*", SearchOption.TopDirectoryOnly).ToArray();
        }

        void ProcessFolder(string folder)
        {
            var projectFilePath = Directory.EnumerateFiles(folder, "*.csproj", SearchOption.TopDirectoryOnly).FirstOrDefault();
            if(projectFilePath == null) {
                return;
            }

            Write("");
            Write(">>> Processing project {0}", Path.GetFileNameWithoutExtension(projectFilePath));

            var referencedAssemblies = GetReferencedAssemblies(projectFilePath);
            foreach(var assembly in referencedAssemblies) {
                ProcessAssembly(assembly);
            }
        }

        string[] GetReferencedAssemblies(string projectFilePath)
        {
            var referencedAssemblies = new List<string>();
            var projectFileFolder = Path.GetDirectoryName(projectFilePath);

            var doc = XDocument.Load(projectFilePath);

            RemoveXmlNamespaces(doc);

            var referenceNodes = doc.Root.XPathSelectElements("ItemGroup/Reference");
               
            foreach(var node in referenceNodes) {
                var assemblyName = new AssemblyName(node.Attributes().Where(a => a.Name == "Include").Single().Value);
                if(assemblyName.GetPublicKeyToken() != null) {
                    WriteIfNotSystem(assemblyName.Name, "    Skipping assembly {0} (already signed)", assemblyName.Name);
                    continue;
                }
                
                var pathNode = node.XPathSelectElement("HintPath");
                if(pathNode == null) {
                    WriteIfNotSystem(assemblyName.Name, "    Skipping assembly {0} (no path available)", assemblyName.Name);
                    continue;
                }

                var referencedAssemblyPath = Path.Combine(projectFileFolder, pathNode.Value);
                referencedAssemblies.Add(referencedAssemblyPath);
            }

            return referencedAssemblies.ToArray();
        }

        void RemoveXmlNamespaces(XDocument doc)
        {
            foreach(XElement e in doc.Root.DescendantsAndSelf()) {
                if(e.Name.Namespace != XNamespace.None) {
                    e.Name = XNamespace.None.GetName(e.Name.LocalName);
                }
                if(e.Attributes().Where(a => a.IsNamespaceDeclaration || a.Name.Namespace != XNamespace.None).Any()) {
                    e.ReplaceAttributes(e.Attributes().Select(a => a.IsNamespaceDeclaration ? null : a.Name.Namespace != XNamespace.None ? new XAttribute(XNamespace.None.GetName(a.Name.LocalName), a.Value) : a));
                }
            }
        }

        void ProcessAssembly(string assemblyPath)
        {
            if(!File.Exists(assemblyPath)) {
                Write("    * Assembly {0} not found! Skipping...", Path.GetFileNameWithoutExtension(assemblyPath));
                return;
            }

            Write("    ! Signing assembly {0}...", Path.GetFileNameWithoutExtension(assemblyPath));

            var ilPath = assemblyPath + ".il";
            try {
                GenerateIlFile(assemblyPath, ilPath);
                AddPublicKeyTokensToIlFile(ilPath);
                GenerateSignedAssembly(ilPath, assemblyPath);
            }
            finally {
                if(File.Exists(ilPath)) {
                    File.Delete(ilPath);
                }
            }
        }

        void GenerateIlFile(string assemblyPath, string ilPath)
        {
            var commandLine = string.Format(@"/nobar /all /out=""{0}"" ""{1}""", ilPath, assemblyPath);
            var result = RunProcess(IldasmPath, commandLine);
            if(result != 0) {
                throw new ApplicationException("ILDASM.EXE finished with error code " + result.ToString());
            }
            else if(!File.Exists(ilPath)) {
                throw new ApplicationException("ILDASM.EXE did not create the file " + ilPath);
            }
        }

        void AddPublicKeyTokensToIlFile(string ilPath)
        {
            //This would probably be shorter by using regular expressions...

            var publicKeyTokenLine = string.Format("  .publickeytoken = ({0} )", PublicKeyToken);
            var ilLines = File.ReadAllLines(ilPath).ToList();
            bool hasPublicToken = false;

            for(int i = 0; i < ilLines.Count; i++) {
                if(!ilLines[i].StartsWith(".assembly extern")) {
                    continue;
                }

                hasPublicToken = false;
                while(ilLines[i] != "}") {
                    if(ilLines[i].Contains(".publickeytoken")) {
                        hasPublicToken = true;
                        break;
                    }
                    i++;
                }

                if(!hasPublicToken) {
                    ilLines.Insert(i, publicKeyTokenLine);
                }
            }

            File.WriteAllLines(ilPath, ilLines.ToArray());
        }

        void GenerateSignedAssembly(string ilPath, string assemblyPath)
        {
            var backupPath = assemblyPath + ".backup";
            if(File.Exists(backupPath)) {
                File.Delete(backupPath);
            }

            File.Move(assemblyPath, backupPath);
            var commandLine = string.Format(@"/nologo /quiet /dll /key=""{0}"" /output=""{1}"" {2}", KeyFilePath, assemblyPath, ilPath);

            try {
                var result = RunProcess(IlasmPath, commandLine);
                if(result != 0) {
                    throw new ApplicationException("ILASM.EXE finished with error code " + result.ToString());
                }
                else if(!File.Exists(ilPath)) {
                    throw new ApplicationException("ILASM.EXE did not create the file " + ilPath);
                }
                File.Delete(backupPath);
            }
            catch {
                if(File.Exists(backupPath)) {
                    if(File.Exists(assemblyPath)) {
                        File.Delete(assemblyPath);
                    }
                    File.Move(backupPath, assemblyPath);
                }
                throw;
            }
        }

        int RunProcess(string exePath, string commandLine)
        {
            var startInfo = new ProcessStartInfo()
            {
                FileName = exePath,
                Arguments = commandLine,
                CreateNoWindow = true, 
                WindowStyle = ProcessWindowStyle.Hidden
            };

            using(var process = Process.Start(startInfo)) {
                var ok = process.WaitForExit(10000);
                if(!ok) {
                    throw new ApplicationException("The execution of " + Path.GetFileName(exePath) + " took too long.");
                }
                return process.ExitCode;
            }
        }

        byte[] GetPublicKeyTokenBytes(string keyPairFilePath)
        {
            var fs = File.Open(keyPairFilePath, FileMode.Open);
            StrongNameKeyPair k = new StrongNameKeyPair(fs);
            fs.Close();
            fs.Dispose();

            var sha1 = SHA1.Create();
            var publicKeyHashBytes = sha1.ComputeHash(k.PublicKey);
            sha1.Dispose();

            var publicKeyHashTokenBytes = publicKeyHashBytes.Reverse().Take(8);
            return publicKeyHashTokenBytes.ToArray();
        }

        void Write(string message, params object[] parameters)
        {
            Console.WriteLine(string.Format(message, parameters));
        }

        void WriteIfNotSystem(string assembly, string message, params object[] parameters)
        {
            if(!assembly.StartsWith("System") && !assembly.StartsWith("Microsoft")) {
                Console.WriteLine(string.Format(message, parameters));
            }
        }
    }
}
