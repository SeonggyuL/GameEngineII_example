using UnityEngine;
using UnityEngine.UI;
using UJ.Attributes;
using UJ.DI;
using System.Collections.Generic;
using PointGenerator.Core;

namespace PointGenerator.UI
{
    /// <summary>
    /// UI component that manages the upgrade shop interface
    /// </summary>
    public class UpgradeShopUI : DIMono
    {
        [Header("UI References")]
        [SerializeField] private Transform upgradeContainer;
        [SerializeField] private GameObject upgradeItemPrefab;
        [SerializeField] private ScrollRect scrollRect;
        [SerializeField] private TMPro.TMP_Text shopTitleText;
        
        [Inject] private IUpgradeManager upgradeManager;
        [Inject] private IGameManager gameManager;
        
        private List<UpgradeItemUI> upgradeItems = new List<UpgradeItemUI>();
        
        public override void Init()
        {
            if (shopTitleText != null)
            {
                shopTitleText.text = "Upgrade Shop";
            }
            
            CreateUpgradeItems();
        }
        
        private void CreateUpgradeItems()
        {
            if (upgradeContainer == null || upgradeItemPrefab == null)
            {
                Debug.LogError("UpgradeShopUI: Missing required references!");
                return;
            }
            
            // Clear existing items
            ClearUpgradeItems();
            
            // Get game data
            var gameData = gameManager.GetGameData();
            if (gameData?.upgradeDefinitions == null)
            {
                Debug.LogWarning("UpgradeShopUI: No upgrade definitions found!");
                return;
            }
            
            // Create upgrade item UI for each upgrade definition
            for (int i = 0; i < gameData.upgradeDefinitions.Length; i++)
            {
                var upgradeDefinition = gameData.upgradeDefinitions[i];
                CreateUpgradeItem(upgradeDefinition, i);
            }
        }
        
        private void CreateUpgradeItem(UpgradeDefinition definition, int index)
        {
            GameObject itemObject = Instantiate(upgradeItemPrefab, upgradeContainer);
            UpgradeItemUI itemUI = itemObject.GetComponent<UpgradeItemUI>();
            
            if (itemUI == null)
            {
                Debug.LogError($"UpgradeShopUI: Upgrade item prefab must have UpgradeItemUI component!");
                Destroy(itemObject);
                return;
            }
            
            // Initialize the item with dependency injection
            itemUI.CheckInjectAndInit();
            itemUI.Initialize(definition, index);
            
            upgradeItems.Add(itemUI);
        }
        
        private void ClearUpgradeItems()
        {
            foreach (var item in upgradeItems)
            {
                if (item != null && item.gameObject != null)
                {
                    Destroy(item.gameObject);
                }
            }
            upgradeItems.Clear();
        }
        
        /// <summary>
        /// Refresh all upgrade items display
        /// </summary>
        public void RefreshUpgrades()
        {
            foreach (var item in upgradeItems)
            {
                if (item != null)
                {
                    // The individual items will update themselves in their Update methods
                    // This method can be called if we need to force an immediate refresh
                }
            }
        }
        
        /// <summary>
        /// Scroll to a specific upgrade item
        /// </summary>
        /// <param name="index">Index of the upgrade to scroll to</param>
        public void ScrollToUpgrade(int index)
        {
            if (scrollRect == null || index < 0 || index >= upgradeItems.Count)
                return;
            
            var targetItem = upgradeItems[index];
            if (targetItem == null) return;
            
            // Calculate scroll position to show the target item
            RectTransform content = scrollRect.content;
            RectTransform itemRect = targetItem.GetComponent<RectTransform>();
            
            if (content != null && itemRect != null)
            {
                float contentHeight = content.rect.height;
                float viewportHeight = scrollRect.viewport.rect.height;
                float itemY = -itemRect.anchoredPosition.y;
                
                float normalizedPosition = Mathf.Clamp01(itemY / (contentHeight - viewportHeight));
                scrollRect.verticalNormalizedPosition = 1f - normalizedPosition;
            }
        }
    }
}
