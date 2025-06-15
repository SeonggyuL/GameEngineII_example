using UnityEngine;
using UnityEngine.UI;
using UJ.Attributes;
using UJ.DI;
using PointGenerator.Core;

namespace PointGenerator.UI
{
    /// <summary>
    /// UI component that displays a single upgrade item in the shop
    /// </summary>
    public class UpgradeItemUI : DIMono
    {
        [Header("UI References")]
        [SerializeField] private Button purchaseButton;
        [SerializeField] private TMPro.TMP_Text nameText;
        [SerializeField] private TMPro.TMP_Text descriptionText;
        [SerializeField] private TMPro.TMP_Text costText;
        [SerializeField] private TMPro.TMP_Text levelText;
        [SerializeField] private Image iconImage;
        [SerializeField] private GameObject maxLevelIndicator;
        
        [Inject] private IUpgradeManager upgradeManager;
        [Inject] private IPointManager pointManager;
        
        private UpgradeDefinition upgradeDefinition;
        private string upgradeId;
        
        public void Initialize(UpgradeDefinition definition)
        {
            upgradeDefinition = definition;
            upgradeId = definition.codeName;
            
            if (purchaseButton != null)
            {
                purchaseButton.onClick.AddListener(OnPurchaseClicked);
            }
            
            UpdateDisplay();
        }
        
        /// <summary>
        /// Initialize with index parameter
        /// </summary>
        public void Initialize(UpgradeDefinition definition, int index)
        {
            // index는 현재 사용하지 않지만, 향후 확장을 위해 유지
            Initialize(definition);
        }
        
        private void OnDestroy()
        {
            if (purchaseButton != null)
            {
                purchaseButton.onClick.RemoveListener(OnPurchaseClicked);
            }
        }
        
        private void Update()
        {
            if (!IsInitialized || upgradeDefinition == null) return;
            
            UpdateDisplay();
        }
        
        private void UpdateDisplay()
        {
            if (upgradeDefinition == null) return;
            
            int currentLevel = upgradeManager.GetUpgradeLevel(upgradeId);
            bool canAfford = upgradeManager.CanPurchaseUpgrade(upgradeId);
            bool isMaxLevel = upgradeDefinition.maxLevel > 0 && currentLevel >= upgradeDefinition.maxLevel;
            long cost = upgradeManager.GetUpgradeCurrentCost(upgradeId);
            
            // Update name
            if (nameText != null)
            {
                nameText.text = upgradeDefinition.Name;
            }
            
            // Update description
            if (descriptionText != null)
            {
                descriptionText.text = upgradeDefinition.Description;
            }
            
            // Update level
            if (levelText != null)
            {
                if (isMaxLevel)
                {
                    levelText.text = "MAX";
                }
                else
                {
                    levelText.text = $"Level {currentLevel}";
                }
            }
            
            // Update cost
            if (costText != null)
            {
                if (isMaxLevel)
                {
                    costText.text = "MAXED";
                }
                else
                {
                    costText.text = $"Cost: {FormatNumber(cost)}";
                }
            }
            
            // Update purchase button
            if (purchaseButton != null)
            {
                purchaseButton.interactable = canAfford && !isMaxLevel;
                
                // Change button color based on affordability
                var colors = purchaseButton.colors;
                if (isMaxLevel)
                {
                    colors.normalColor = Color.green;
                }
                else if (canAfford)
                {
                    colors.normalColor = Color.white;
                }
                else
                {
                    colors.normalColor = Color.gray;
                }
                purchaseButton.colors = colors;
            }
            
            // Update max level indicator
            if (maxLevelIndicator != null)
            {
                maxLevelIndicator.SetActive(isMaxLevel);
            }
            
            // Update icon if available (아이콘 필드 제거)
            // if (iconImage != null && upgradeDefinition.icon != null)
            // {
            //     iconImage.sprite = upgradeDefinition.icon;
            // }
        }
        
        private void OnPurchaseClicked()
        {
            if (!IsInitialized || upgradeDefinition == null) return;
            
            bool success = upgradeManager.PurchaseUpgrade(upgradeId);
            
            if (success)
            {
                // Play purchase sound or animation here if desired
                Debug.Log($"Purchased upgrade: {upgradeDefinition.Name}");
            }
            else
            {
                Debug.Log($"Cannot purchase upgrade: {upgradeDefinition.Name}");
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
