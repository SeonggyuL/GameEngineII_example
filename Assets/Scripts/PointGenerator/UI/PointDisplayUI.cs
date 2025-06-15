using UnityEngine;
using UnityEngine.UI;
using UJ.Attributes;
using UJ.DI;
using PointGenerator.Core;

namespace PointGenerator.UI
{
    /// <summary>
    /// UI component that displays the current point count and related statistics
    /// </summary>
    public class PointDisplayUI : DIMono
    {
        [Header("UI References")]
        [SerializeField] private TMPro.TMP_Text pointCountText;
        [SerializeField] private TMPro.TMP_Text pointsPerSecondText;
        [SerializeField] private TMPro.TMP_Text totalEarnedText;
        
        [Inject] private IPointManager pointManager;
        
        private void Update()
        {
            if (!IsInitialized) return;
            
            UpdateDisplay();
        }
        
        public override void Init()
        {
            UpdateDisplay();
        }
        
        private void UpdateDisplay()
        {
            // Display current points
            if (pointCountText != null)
            {
                pointCountText.text = $"Points: {FormatNumber(pointManager.CurrentPoints)}";
            }
            
            // Display points per second
            if (pointsPerSecondText != null)
            {
                pointsPerSecondText.text = $"Per Second: {FormatNumber(pointManager.PointsPerSecond)}";
            }
            
            // Display current points (total earned 대신 사용)
            if (totalEarnedText != null)
            {
                totalEarnedText.text = $"Current: {FormatNumber(pointManager.CurrentPoints)}";
            }
        }
        
        private string FormatNumber(long number)
        {
            if (number >= 1000000000000L) // Trillion
                return $"{number / 1000000000000.0:F2}T";
            if (number >= 1000000000L) // Billion
                return $"{number / 1000000000.0:F2}B";
            if (number >= 1000000L) // Million
                return $"{number / 1000000.0:F2}M";
            if (number >= 1000L) // Thousand
                return $"{number / 1000.0:F2}K";
            
            return number.ToString();
        }
    }
}
