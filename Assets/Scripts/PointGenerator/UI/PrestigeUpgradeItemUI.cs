using UnityEngine;
using UnityEngine.UI;
using PointGenerator.Core;

namespace PointGenerator.UI
{
    /// <summary>
    /// 프레스티지 업그레이드 아이템 UI 컴포넌트
    /// </summary>
    public class PrestigeUpgradeItemUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TMPro.TMP_Text nameText;
        [SerializeField] private TMPro.TMP_Text descriptionText;
        [SerializeField] private TMPro.TMP_Text costText;
        [SerializeField] private TMPro.TMP_Text levelText;
        [SerializeField] private TMPro.TMP_Text effectText;
        [SerializeField] private Button upgradeButton;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private GameObject maxLevelIndicator;
        
        private PrestigeUpgradeDefinition upgradeDefinition;
        private IPrestigeManager prestigeManager;
        
        /// <summary>
        /// 프레스티지 업그레이드 아이템 초기화
        /// </summary>
        public void Initialize(PrestigeUpgradeDefinition definition, IPrestigeManager manager)
        {
            upgradeDefinition = definition;
            prestigeManager = manager;
            
            // 버튼 이벤트 등록
            if (upgradeButton != null)
                upgradeButton.onClick.AddListener(OnUpgradeButtonClicked);
            
            // 기본 정보 설정
            if (nameText != null)
                nameText.text = definition.name;
            
            if (descriptionText != null)
                descriptionText.text = definition.description;
            
            RefreshUI();
        }
        
        /// <summary>
        /// UI 상태 새로고침
        /// </summary>
        public void RefreshUI()
        {
            if (upgradeDefinition == null || prestigeManager == null)
                return;
            
            var currentLevel = prestigeManager.GetPrestigeUpgradeLevel(upgradeDefinition.id);
            var cost = prestigeManager.CalculatePrestigeUpgradeCost(upgradeDefinition.id, currentLevel);
            var canAfford = prestigeManager.CanAffordPrestigeUpgrade(upgradeDefinition.id, prestigeManager.GetPrestigeData().currentPrestigePoints);
            var isMaxLevel = upgradeDefinition.maxLevel > 0 && currentLevel >= upgradeDefinition.maxLevel;
            
            // 레벨 표시
            if (levelText != null)
            {
                if (isMaxLevel)
                {
                    levelText.text = "MAX";
                    levelText.color = Color.yellow;
                }
                else
                {
                    levelText.text = $"Lv. {currentLevel}";
                    levelText.color = Color.white;
                }
            }
            
            // 비용 표시
            if (costText != null)
            {
                if (isMaxLevel)
                {
                    costText.text = "MAX LEVEL";
                    costText.color = Color.yellow;
                }
                else
                {
                    costText.text = $"Cost: {cost} PP";
                    costText.color = canAfford ? Color.green : Color.red;
                }
            }
            
            // 효과 표시
            if (effectText != null)
            {
                var currentEffect = CalculateCurrentEffect(currentLevel);
                var nextEffect = CalculateCurrentEffect(currentLevel + 1);
                
                if (isMaxLevel)
                {
                    effectText.text = $"Effect: x{currentEffect:F2}";
                }
                else
                {
                    effectText.text = $"Effect: x{currentEffect:F2} → x{nextEffect:F2}";
                }
            }
            
            // 버튼 상태
            if (upgradeButton != null)
            {
                upgradeButton.interactable = canAfford && !isMaxLevel;
                
                var colors = upgradeButton.colors;
                if (isMaxLevel)
                {
                    colors.normalColor = new Color(0.8f, 0.8f, 0.2f, 0.8f); // 노란색 (최대 레벨)
                }
                else if (canAfford)
                {
                    colors.normalColor = new Color(0.2f, 0.8f, 0.2f, 1f); // 녹색 (구매 가능)
                    colors.highlightedColor = new Color(0.3f, 0.9f, 0.3f, 1f);
                }
                else
                {
                    colors.normalColor = new Color(0.5f, 0.5f, 0.5f, 0.5f); // 회색 (구매 불가)
                    colors.highlightedColor = new Color(0.6f, 0.6f, 0.6f, 0.5f);
                }
                upgradeButton.colors = colors;
            }
            
            // 배경 색상
            if (backgroundImage != null)
            {
                if (isMaxLevel)
                {
                    backgroundImage.color = new Color(0.3f, 0.3f, 0.1f, 0.9f); // 노란색 톤
                }
                else if (canAfford)
                {
                    backgroundImage.color = new Color(0.1f, 0.3f, 0.1f, 0.9f); // 녹색 톤
                }
                else
                {
                    backgroundImage.color = new Color(0.15f, 0.15f, 0.25f, 0.9f); // 기본 톤
                }
            }
            
            // 최대 레벨 표시
            if (maxLevelIndicator != null)
                maxLevelIndicator.SetActive(isMaxLevel);
        }
        
        /// <summary>
        /// 업그레이드 버튼 클릭 이벤트
        /// </summary>
        private void OnUpgradeButtonClicked()
        {
            if (prestigeManager.PurchasePrestigeUpgrade(upgradeDefinition.id))
            {
                RefreshUI();
                StartCoroutine(PurchaseSuccessEffect());
            }
        }
        
        /// <summary>
        /// 현재 효과값 계산
        /// </summary>
        private double CalculateCurrentEffect(int level)
        {
            // 레벨 0: 기본 효과값만 적용
            // 레벨 1 이상: 기본 효과값 + (레벨 * 증가분)
            return upgradeDefinition.baseEffectValue + (upgradeDefinition.effectIncrementPerLevel * level);
        }
        
        /// <summary>
        /// 구매 성공 시 시각적 효과
        /// </summary>
        private System.Collections.IEnumerator PurchaseSuccessEffect()
        {
            var originalScale = transform.localScale;
            var targetScale = originalScale * 1.1f;
            
            // 확대
            float timer = 0f;
            float duration = 0.15f;
            while (timer < duration)
            {
                timer += Time.deltaTime;
                float t = timer / duration;
                transform.localScale = Vector3.Lerp(originalScale, targetScale, t);
                yield return null;
            }
            
            // 축소
            timer = 0f;
            while (timer < duration)
            {
                timer += Time.deltaTime;
                float t = timer / duration;
                transform.localScale = Vector3.Lerp(targetScale, originalScale, t);
                yield return null;
            }
            
            transform.localScale = originalScale;
            
            // 반짝임 효과
            if (backgroundImage != null)
            {
                var originalColor = backgroundImage.color;
                var flashColor = Color.yellow;
                
                timer = 0f;
                duration = 0.3f;
                while (timer < duration)
                {
                    timer += Time.deltaTime;
                    float t = Mathf.PingPong(timer * 8, 1);
                    backgroundImage.color = Color.Lerp(originalColor, flashColor, t * 0.4f);
                    yield return null;
                }
                
                backgroundImage.color = originalColor;
            }
        }
        
        private void OnDestroy()
        {
            // 메모리 누수 방지
            if (upgradeButton != null)
                upgradeButton.onClick.RemoveAllListeners();
        }
    }
}
