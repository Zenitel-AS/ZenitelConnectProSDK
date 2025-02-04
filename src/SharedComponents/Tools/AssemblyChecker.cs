using System;
using System.Collections.Generic;
using System.IO;

namespace ConnectPro.Tools
{
    /// <summary>
    /// Provides functionality to check and list assemblies within a specified directory.
    /// Implements <see cref="IDisposable"/> for resource management.
    /// </summary>
    public class AssemblyChecker : IDisposable
    {
        #region Public Methods

        /// <summary>
        /// Scans the specified directory for .NET assemblies (.dll files) and returns a list of their names.
        /// </summary>
        /// <param name="directoryPath">The absolute path of the directory to scan.</param>
        /// <returns>A list of assembly names found in the directory.</returns>
        /// <remarks>
        /// If the directory does not exist or contains no assemblies, an empty list is returned.
        /// </remarks>
        public List<string> CheckAssembliesInDirectory(string directoryPath)
        {
            List<string> assemblies = new List<string>();

            if (!Directory.Exists(directoryPath))
            {
                Console.WriteLine("Directory does not exist.");
                return assemblies;
            }

            string[] assemblyFiles = Directory.GetFiles(directoryPath, "*.dll");

            if (assemblyFiles.Length == 0)
            {
                Console.WriteLine("No assemblies found in the directory.");
                return assemblies;
            }

            Console.WriteLine("Assemblies found in the directory:");
            foreach (string assemblyFile in assemblyFiles)
            {
                string assemblyName = Path.GetFileNameWithoutExtension(assemblyFile);
                assemblies.Add(assemblyName);
            }

            return assemblies;
        }

        #endregion

        #region IDisposable Implementation

        /// <summary>
        /// Releases resources used by the <see cref="AssemblyChecker"/> class.
        /// </summary>
        public void Dispose()
        {
            // Implement cleanup logic if necessary
            throw new NotImplementedException();
        }

        #endregion
    }
}
