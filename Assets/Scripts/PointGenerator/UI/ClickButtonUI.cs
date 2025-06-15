using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UJ.Attributes;
using UJ.DI;
using PointGenerator.Core;

namespace PointGenerator.UI
{
    /// <summary>
    /// UI component that handles the main click button for generating points
    /// </summary>
    public class ClickButtonUI : DIMono
    {
        [Header("UI References")]
        [SerializeField] private Button clickButton;
        [SerializeField] private TMPro.TMP_Text buttonText;
        [SerializeField] private RectTransform buttonTransform;
        
        [Header("Visual Effects")]
        [SerializeField] private float clickScaleFactor = 0.95f;
        [SerializeField] private float scaleAnimationDuration = 0.1f;
        
        [Inject] private IPointManager pointManager;
        
        private Vector3 originalScale;
        private bool isAnimating = false;
        
        public override void Init()
        {
            if (clickButton == null)
            {
                Debug.LogError("ClickButtonUI: Click button is not assigned!");
                return;
            }
            
            // Store original scale
            originalScale = buttonTransform != null ? buttonTransform.localScale : Vector3.one;
            
            // Register click event
            clickButton.onClick.AddListener(OnClickButtonPressed);
            
            UpdateButtonText();
        }
        
        private void OnDestroy()
        {
            if (clickButton != null)
            {
                clickButton.onClick.RemoveListener(OnClickButtonPressed);
            }
        }
        
        private void Update()
        {
            if (!IsInitialized) return;
            
            UpdateButtonText();
        }
        
        private void OnClickButtonPressed()
        {
            if (!IsInitialized) return;
            
            // Process the click
            pointManager.PerformClick();
            
            // Play visual feedback
            PlayClickAnimation();
        }
        
        private void UpdateButtonText()
        {
            if (buttonText != null)
            {
                long pointsPerClick = pointManager.PointsPerClick;
                buttonText.text = $"Click!\n+{FormatNumber(pointsPerClick)} pts";
            }
        }
        
        private void PlayClickAnimation()
        {
            if (isAnimating || buttonTransform == null) return;
            
            StartCoroutine(AnimateClick());
        }
        
        private System.Collections.IEnumerator AnimateClick()
        {
            isAnimating = true;
            
            Vector3 targetScale = originalScale * clickScaleFactor;
            float elapsed = 0f;
            
            // Scale down
            while (elapsed < scaleAnimationDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / scaleAnimationDuration;
                buttonTransform.localScale = Vector3.Lerp(originalScale, targetScale, t);
                yield return null;
            }
            
            elapsed = 0f;
            
            // Scale back up
            while (elapsed < scaleAnimationDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / scaleAnimationDuration;
                buttonTransform.localScale = Vector3.Lerp(targetScale, originalScale, t);
                yield return null;
            }
            
            buttonTransform.localScale = originalScale;
            isAnimating = false;
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
