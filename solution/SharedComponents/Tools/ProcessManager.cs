using System;
using System.Diagnostics;
using System.Collections.Generic;

namespace ConnectPro.Tools
{
    public class ProcessManager
    {
        private Process currentProcess;
        private List<Process> childProcesses = new List<Process>();

        // Store the custom process name
        private string customProcessName;

        public ProcessManager(string customName)
        {
            customProcessName = customName;
            // Get the current process (your application's process)
            currentProcess = Process.GetCurrentProcess();
            // Set the process name to the custom name
            SetProcessName(customProcessName);
        }

        public void RegisterChildProcess(Process process)
        {
            childProcesses.Add(process);
        }

        // Method to set the process name to a custom name
        private void SetProcessName(string name)
        {
            // Change the process name of the current process using reflection
            var processField = currentProcess.GetType().GetField("processName", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (processField != null)
            {
                processField.SetValue(currentProcess, name);
            }
        }

        public void CloseApplication()
        {
            try
            {
                // Terminate child processes gracefully
                foreach (var process in childProcesses)
                {
                    TerminateProcessGracefully(process);
                }

                // Save application state (implement this)
                SaveApplicationState();

                // Close open resources (implement this)
                CloseOpenResources();

                // Close the main window or exit the application
                ApplicationShutdown();

                // Optionally, handle any post-shutdown tasks
                PostShutdownTasks();
            }
            catch (Exception ex)
            {
                // Handle exceptions during the shutdown process
                HandleShutdownException(ex);
            }
        }

        private void TerminateProcessGracefully(Process process)
        {
            // Implement graceful termination of the child process here
            // You may use process.CloseMainWindow() or process.Close() or other suitable methods
        }

        private void SaveApplicationState()
        {
            // Implement code to save the application state (e.g., user preferences, settings, data) here
        }

        private void CloseOpenResources()
        {
            // Implement code to close open resources (e.g., files, database connections, network sockets) here
        }

        private void ApplicationShutdown()
        {
            // Implement code to close the main window or exit the application here
            // For a WPF application, you can use Application.Current.Shutdown()
        }

        private void PostShutdownTasks()
        {
            // Implement any post-shutdown tasks here
        }

        private void HandleShutdownException(Exception ex)
        {
            // Implement code to handle exceptions during the shutdown process
            // You may log the exception, display an error message to the user, or take other appropriate actions
        }
    }
}
