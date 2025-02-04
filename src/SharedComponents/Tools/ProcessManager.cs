using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Reflection;

namespace ConnectPro.Tools
{
    /// <summary>
    /// Manages processes, including the main application process and child processes, 
    /// with functionality for graceful shutdown.
    /// </summary>
    public class ProcessManager
    {
        #region Fields

        private readonly Process currentProcess;
        private readonly List<Process> childProcesses = new List<Process>();
        private readonly string customProcessName;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="ProcessManager"/> class with a custom process name.
        /// </summary>
        /// <param name="customName">The custom process name.</param>
        public ProcessManager(string customName)
        {
            customProcessName = customName;
            currentProcess = Process.GetCurrentProcess();
            SetProcessName(customProcessName);
        }

        #endregion

        #region Process Management

        /// <summary>
        /// Registers a child process under the current process.
        /// </summary>
        /// <param name="process">The child process to register.</param>
        public void RegisterChildProcess(Process process)
        {
            childProcesses.Add(process);
        }

        /// <summary>
        /// Attempts to set the process name using reflection.
        /// </summary>
        /// <param name="name">The custom name to assign to the process.</param>
        private void SetProcessName(string name)
        {
            FieldInfo processField = currentProcess.GetType()
                .GetField("processName", BindingFlags.NonPublic | BindingFlags.Instance);

            processField?.SetValue(currentProcess, name);
        }

        #endregion

        #region Application Shutdown

        /// <summary>
        /// Closes the application and all registered child processes gracefully.
        /// </summary>
        public void CloseApplication()
        {
            try
            {
                foreach (var process in childProcesses)
                {
                    TerminateProcessGracefully(process);
                }

                SaveApplicationState();
                CloseOpenResources();
                ApplicationShutdown();
                PostShutdownTasks();
            }
            catch (Exception ex)
            {
                HandleShutdownException(ex);
            }
        }

        /// <summary>
        /// Attempts to terminate a child process gracefully.
        /// </summary>
        /// <param name="process">The process to terminate.</param>
        private void TerminateProcessGracefully(Process process)
        {
            if (process != null && !process.HasExited)
            {
                process.CloseMainWindow();
                process.WaitForExit(5000); // Wait 5 seconds before forcing termination
                if (!process.HasExited)
                {
                    process.Kill();
                }
            }
        }

        /// <summary>
        /// Saves the application state before shutdown (e.g., user preferences, data).
        /// </summary>
        private void SaveApplicationState()
        {
            // Implement state-saving logic (e.g., saving preferences, session data)
        }

        /// <summary>
        /// Closes open resources such as files, database connections, and network sockets.
        /// </summary>
        private void CloseOpenResources()
        {
            // Implement logic for closing resources before shutdown
        }

        /// <summary>
        /// Performs the actual application shutdown.
        /// </summary>
        private void ApplicationShutdown()
        {
            // Implement application shutdown logic (e.g., Application.Current.Shutdown() for WPF)
        }

        /// <summary>
        /// Executes any necessary tasks after the application shutdown.
        /// </summary>
        private void PostShutdownTasks()
        {
            // Implement post-shutdown tasks if required
        }

        /// <summary>
        /// Handles exceptions that occur during the shutdown process.
        /// </summary>
        /// <param name="ex">The exception that occurred.</param>
        private void HandleShutdownException(Exception ex)
        {
            // Implement logging or error-handling for shutdown failures
        }

        #endregion
    }
}
