using System;
using System.IO;
using UnityEngine;

namespace MccDev260.Logger
{
    public class Logger : MonoBehaviour
    {
        [SerializeField] private LoggerGlobalSettings globalSettings;
        [Header("Config")]
        [SerializeField] private string logFileName = "Stats";
        [SerializeField] private string logNote;
        [SerializeField] private bool includeNoteInFileName;
        [SerializeField] private bool overwriteOutput;
        [SerializeField] private bool outputInUniqueIdFolder;
        [SerializeField] private bool includeHardwareInfo = true;
        [SerializeField] private bool generateConfigJsonInBuild;

        private static string logFilePath;
        
        private static bool isEditor;
        private static bool hasWrittenHeader;

        public delegate void LogSessionDataEvent();
        public static event LogSessionDataEvent LogSessionData;

        /// <summary>
        /// Writes multiple lines to the log file.
        /// </summary>
        /// <param name="messages"></param>
        /// <remarks>
        /// Each string is treated as a new line preceded by current time.
        /// Empty strings are treated as an empty line.
        /// </remarks>
        /// <returns>
        /// true if write is successful.
        /// </returns>
        public static bool Write(string[] messages)
        {
            if (!hasWrittenHeader) return false;

            try
            {
                using (StreamWriter writer = new StreamWriter(logFilePath, true))
                {
                    foreach (var m in messages)
                    {
                        Write(writer, m);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"{nameof(Logger)}: Failed to open writer! - {e.Message}" );
                return false;
            }

            return true;
        }

        /// <summary>
        /// Writes a single line to the log file preceded by current time.
        /// </summary>
        /// <param name="message"></param>
        /// <remarks>
        /// Use Write(string[]) if logging more than one message at a time.
        /// </remarks>
        /// <returns>
        /// true if write is successful.
        /// </returns>
        public static bool Write(string message)
        {
            if (!hasWrittenHeader) return false;

            try
            {
                using (StreamWriter writer = new StreamWriter(logFilePath, true))
                {
                    Write(writer, message);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"{nameof(Logger)}: Failed to write to open writer! - {e.Message}");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Formats message before writing to log.
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="message"></param>
        private static void Write(StreamWriter writer, string message)
        {
            if (message == string.Empty || string.IsNullOrWhiteSpace(message))
                writer.WriteLine();
            else
            {
                var timeStamp = "[" + DateTime.Now.ToString("HH:mm:ss") + "] ->";
                writer.WriteLine($"{timeStamp} {message}");
            }
        }

        private void Awake()
        {
            if (globalSettings.CanRecord == false) return;

            isEditor = Application.isEditor;

            LoggerSetup();

            // Subscribe to the application quit event
            Application.quitting += LogSessionInfo;

            LogHeader();

            Debug.Log($"{nameof(Logger)}: Setup @ {logFilePath}");
        }

        /// <summary>
        /// Creates output path, if it doesn't exist already and if able, sets up Config JSON.
        /// </summary>
        private void LoggerSetup()
        {
            // Define the folder path
            string rootPath = Application.dataPath + "/Logger";
            string logFolderPath = rootPath;

            if (outputInUniqueIdFolder)
            {
                string logFolderName = SystemInfo.deviceUniqueIdentifier == SystemInfo.unsupportedIdentifier ? "[Logs]" : SystemInfo.deviceUniqueIdentifier;
                logFolderPath = rootPath + "/" + logFolderName;
            }

            // Check if the folder exists, create it if it doesn't
            if (!Directory.Exists(logFolderPath))
            {
                Directory.CreateDirectory(logFolderPath);
            }

            if (!isEditor && generateConfigJsonInBuild)
                ConfigJson(rootPath);

            logFilePath = CreateLogFilePath(logFolderPath); 
        }

        /// <summary>
        /// Generates log file path according to settings.
        /// </summary>
        /// <param name="outputPath"></param>
        /// <returns></returns>
        private string CreateLogFilePath(string outputPath)
        {
            string partialLogPath;
            if (includeNoteInFileName)
            {
                partialLogPath = outputPath + "/" + logFileName + "-" + logNote;
            }
            else
            {
                partialLogPath = outputPath + "/" + logFileName;
            }

            string completeLogPath;
            if (overwriteOutput)
            {
                completeLogPath = partialLogPath + ".txt";
            }
            else
            {
                completeLogPath = partialLogPath + "_" + DateTime.Now.ToString("yyyy-MM-dd_HH-mm") + ".txt";
            }

            return completeLogPath;
        }

        /// <summary>
        /// (Builds only.) Configures the logger by reading from config.json file.
        /// </summary>
        /// <param name="path">Config file path.</param>
        /// <remarks>
        /// Will generate a new file if one doesn't already exist and 
        /// values will default to the same as in the logger class.
        /// </remarks>
        private void ConfigJson(string path)
        {
            // Create configuration JSON file if it doesn't exist
            string configPath = path + "/config.json";
            if (!File.Exists(configPath))
            {
                var defaultConfig = new Config
                {
                    logNote = this.logNote,
                    includeNoteInFileName = this.includeNoteInFileName.ToString(),
                    overwriteOutput = this.overwriteOutput.ToString(),
                    outputInUniqueIdFolder = this.outputInUniqueIdFolder.ToString(),
                    includeHardwareInfo = this.includeHardwareInfo.ToString(),
                };
                string defaultJson = JsonUtility.ToJson(defaultConfig);
                File.WriteAllText(configPath, defaultJson);

                // Don't bother reading from it as the default values are the same.
                return;
            }

            // Read configuration from JSON file
            string json = File.ReadAllText(configPath);
            var configData = JsonUtility.FromJson<Config>(json);
            if (configData != null)
            {
                logNote = configData.logNote;
                bool.TryParse(configData.includeNoteInFileName, out this.includeNoteInFileName);
                bool.TryParse(configData.overwriteOutput, out this.overwriteOutput);
                bool.TryParse(configData.includeHardwareInfo, out this.includeHardwareInfo);
            }
        }

        /// <summary>
        /// Logs header info before start of first frame.
        /// </summary>
        private void LogHeader()
        {
            using (StreamWriter writer = new StreamWriter(logFilePath))
            {
                writer.WriteLine(DateTime.Now);
                writer.WriteLine(Application.productName + " v" + Application.version);
                if (logNote != string.Empty && !string.IsNullOrEmpty(logNote))
                {
                    writer.WriteLine("Note: " + logNote);
                }

                string id = SystemInfo.deviceUniqueIdentifier == SystemInfo.unsupportedIdentifier ? "Unsupported" : SystemInfo.deviceUniqueIdentifier;
                writer.WriteLine($"Unique System Identifier: {id}");
                
                writer.WriteLine();

                if (isEditor)
                {
                    writer.WriteLine("===!#! EDITOR !#!===");
                }

                writer.WriteLine();
                writer.WriteLine("# System Info...");
                writer.WriteLine($"OS: {SystemInfo.operatingSystem}");
                writer.WriteLine($"Graphics Driver: {SystemInfo.graphicsDeviceVersion}");
                if (SystemInfo.batteryStatus != BatteryStatus.Unknown)
                {
                    writer.WriteLine($"Battery Status: {SystemInfo.batteryStatus}");
                }
                writer.WriteLine();
                
                if (includeHardwareInfo)
                {
                    writer.WriteLine("## Hardware...");
                    writer.WriteLine("- CPU -");
                    writer.WriteLine($"Model: {SystemInfo.processorType}");
                    writer.WriteLine($"Hardware Threads: {SystemInfo.processorCount}");
                    writer.WriteLine($"Frequency: {SystemInfo.processorFrequency} MHz");
                    writer.WriteLine("- GPU -");
                    writer.WriteLine($"Device Vendor: {SystemInfo.graphicsDeviceVendor}");
                    writer.WriteLine($"Device Vendor ID: {SystemInfo.graphicsDeviceVendorID}");
                    writer.WriteLine($"Model: {SystemInfo.graphicsDeviceName}");
                    writer.WriteLine($"Device ID: {SystemInfo.graphicsDeviceID}");
                    writer.WriteLine("- Memory -");
                    writer.WriteLine($"RAM: {SystemInfo.systemMemorySize} MB");
                    writer.WriteLine($"VRAM: {SystemInfo.graphicsMemorySize} MB");
                    writer.WriteLine();
                }

                writer.WriteLine("====== Update Loop Start ======");
            }

            hasWrittenHeader = true;
        }

        /// <summary>
        /// Logs session data on application quit.
        /// </summary>
        private void LogSessionInfo()
        {
            LogSessionData.Invoke();
            
            var timer = Time.realtimeSinceStartup;
            int hours = Mathf.FloorToInt(timer / 3600);
            int minutes = Mathf.FloorToInt((timer % 3600) / 60);
            int seconds = Mathf.FloorToInt(timer % 60);

            // Open or create the text file
            using (StreamWriter writer = new StreamWriter(logFilePath, true))
            {
                writer.WriteLine("====== Update Loop End   ======");
                writer.WriteLine();
                writer.WriteLine($"Session Length: {hours} hours : {minutes} mins : {seconds} secs");
                writer.WriteLine();

                if (SessionData.FPS.highest == 0)
                {
                    Write(writer, $"FPS counter did not appear to be enabled. If unintentional, check {nameof(FPSCounter)} is in the scene, active and its values are set correctly.");
                }
                else
                {
                    writer.WriteLine("# FPS...");
                    writer.WriteLine($"Average: {SessionData.FPS.mean}");
                    writer.WriteLine($"Median: {SessionData.FPS.median}");
                    writer.WriteLine($"Highest: {SessionData.FPS.highest}");
                    writer.WriteLine($"Lowest: {SessionData.FPS.lowest}");
                }
            }
        }

        private void OnDestroy()
        {
            // Unsubscribe from the application quit event
            Application.quitting -= LogSessionInfo;
        }

        /// <summary>
        /// Config data from json file.
        /// </summary>
        private class Config
        {
            public string logNote;
            public string includeNoteInFileName;
            public string overwriteOutput;
            public string outputInUniqueIdFolder;
            public string includeHardwareInfo;
        }
    }
}
