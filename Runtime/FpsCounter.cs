using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MccDev260.Logger
{
    public class FPSCounter : MonoBehaviour
    {
        [SerializeField] private LoggerGlobalSettings globalSettings;
        [Header("Config")]
        [SerializeField] private float fpsUpdateInterval = 0.5f;
        [SerializeField] private int maxRecordedFPSCount = 100;

        private float fpsAccumulator = 0;
        private int frameCount = 0;
        
        private float timeLeft;
        private float highestFPS = 0;
        private float lowestFPS = 7000;
        private List<float> recordedFPS = new List<float>();
        
        private void Start()
        {
            if (globalSettings.CanRecord)
            {
                timeLeft = fpsUpdateInterval;
                Logger.LogSessionData += OnLogData;
                Logger.Write($"{nameof(FPSCounter)}: loaded!");
            }
        }

        private void Update()
        {
            if (!globalSettings.CanRecord) return;

            // Calculate current FPS
            timeLeft -= Time.deltaTime;
            fpsAccumulator += Time.timeScale / Time.deltaTime;
            frameCount++;

            // If interval has passed, update FPS and reset variables
            if (timeLeft <= 0.0)
            {
                float currentFPS = fpsAccumulator / frameCount;

                // Update highest recorded FPS
                if (currentFPS > highestFPS)
                {
                    highestFPS = currentFPS;
                }

                if (currentFPS < lowestFPS)
                {
                    lowestFPS = currentFPS;
                }

                // Store current FPS for average calculation
                recordedFPS.Add(currentFPS);

                // Reset variables
                fpsAccumulator = 0;
                frameCount = 0;
                timeLeft = fpsUpdateInterval;

                // Check if recorded FPS list exceeds the maximum size
                if (recordedFPS.Count > maxRecordedFPSCount)
                {
                    // Remove oldest recorded FPS values to keep the list size under control
                    recordedFPS.RemoveRange(0, recordedFPS.Count - maxRecordedFPSCount);
                }
            }
        }

        private void OnLogData()
        {
            SessionData.FPS.mean = CalculateAverage(recordedFPS);
            SessionData.FPS.median = CalculateMedian(recordedFPS);
            SessionData.FPS.highest  = highestFPS;
            SessionData.FPS.lowest = lowestFPS;
        }

        private float CalculateAverage(List<float> recordedFps)
        {
            // Calculate average FPS
            float totalFPS = 0;
            foreach (float fps in recordedFps)
            {
                totalFPS += fps;
            }

            return totalFPS / recordedFps.Count;
        }

        private float CalculateMedian(List<float> recordedFps)
        {
            int numberCount = recordedFps.Count;
            int halfIndex = numberCount / 2;
            var sortedNumbers = recordedFps.OrderBy(n => n).ToList();

            if (numberCount % 2 == 0)
            {
                // If even, average the two middle numbers
                float median = ((float)sortedNumbers[halfIndex] + (float)sortedNumbers[halfIndex - 1]) / 2;
                return median;
            }
            else
            {
                // If odd, return the middle number
                float median = sortedNumbers[halfIndex];
                return median;
            }
        }

        private void OnDestroy()
        {
            Logger.LogSessionData -= OnLogData;
        }
    }
}
