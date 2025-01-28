using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConnectPro.Tools
{
    public class AssemblyChecker : IDisposable
    {
        public List<string> CheckAssembliesInDirectory(string directoryPath)
        {
            List<string> _assemblies = new List<string>();

            if (!Directory.Exists(directoryPath))
            {
                Console.WriteLine("Directory does not exist.");
                return _assemblies;
            }

            string[] assemblyFiles = Directory.GetFiles(directoryPath, "*.dll");

            if (assemblyFiles.Length == 0)
            {
                Console.WriteLine("No assemblies found in the directory.");
                return _assemblies;
            }

            Console.WriteLine("Assemblies found in the directory:");
            foreach (string assemblyFile in assemblyFiles)
            {
                string assemblyName = Path.GetFileNameWithoutExtension(assemblyFile);
                _assemblies.Add(assemblyName);
            }

            return _assemblies;
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
