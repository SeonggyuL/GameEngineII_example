using UnityEngine;
using UnityEngine.UI;
using PointGenerator.Core;
using UJ.DI;
using UJ.Attributes;

namespace PointGenerator.UI
{
    /// <summary>
    /// 프레스티지 패널 UI 컴포넌트
    /// </summary>
    public class PrestigePanelUI : DIMono
    {
        [Header("UI References")]
        [SerializeField] private Button closeButton;
        [SerializeField] private Button prestigeButton;
        [SerializeField] private TMPro.TMP_Text titleText;
        [SerializeField] private TMPro.TMP_Text currentLevelText;
        [SerializeField] private TMPro.TMP_Text prestigePointsText;
        [SerializeField] private TMPro.TMP_Text gainedPointsText;
        [SerializeField] private TMPro.TMP_Text multiplierText;
        [SerializeField] private TMPro.TMP_Text warningText;
        [SerializeField] private Transform upgradeItemParent;
        [SerializeField] private GameObject prestigeUpgradeItemPrefab;
        
        [Inject] private IPrestigeManager prestigeManager;
        [Inject] private IPointManager pointManager;
        
        private readonly System.Collections.Generic.List<PrestigeUpgradeItemUI> upgradeItems = 
            new();
        
        public override void Init()
        {
            
            // 버튼 이벤트 등록
            if (closeButton != null)
                closeButton.onClick.AddListener(ClosePanel);
            
            if (prestigeButton != null)
                prestigeButton.onClick.AddListener(OnPrestigeButtonClicked);
            
            // 타이틀 설정
            if (titleText != null)
                titleText.text = "Prestige";
            
            // 경고 텍스트 설정
            if (warningText != null)
                warningText.text = "Warning: Prestiging will reset your progress but grant permanent bonuses!";

            CreatePrestigeUpgradeItems();
        }

        /// <summary>
        /// 프레스티지 업그레이드 아이템들 생성
        /// </summary>
        private void CreatePrestigeUpgradeItems()
        {
            if (prestigeManager == null || prestigeUpgradeItemPrefab == null || upgradeItemParent == null)
                return;
            
            // 기존 아이템들 제거
            foreach (var item in upgradeItems)
            {
                if (item != null)
                    Destroy(item.gameObject);
            }
            upgradeItems.Clear();
            
            // 프레스티지 업그레이드 정의들 가져오기
            var upgradeDefinitions = prestigeManager.GetAllPrestigeUpgradeDefinitions();
            
            foreach (var upgrade in upgradeDefinitions)
            {
                var itemObj = Instantiate(prestigeUpgradeItemPrefab, upgradeItemParent);
                var itemUI = itemObj.GetComponent<PrestigeUpgradeItemUI>();
                
                if (itemUI != null)
                {
                    itemUI.Initialize(upgrade, prestigeManager);
                    upgradeItems.Add(itemUI);
                }
            }
        }
        
        /// <summary>
        /// 패널 열기
        /// </summary>
        public void OpenPanel()
        {
            gameObject.SetActive(true);
            RefreshUI();
        }
        
        /// <summary>
        /// 패널 닫기
        /// </summary>
        public void ClosePanel()
        {
            gameObject.SetActive(false);
        }
        
        /// <summary>
        /// UI 새로고침
        /// </summary>
        public void RefreshUI()
        {
            if (prestigeManager == null || pointManager == null)
                return;
            
            var prestigeData = prestigeManager.GetPrestigeData();
            var gainedPoints = prestigeManager.CalculatePrestigePoints();
            var canPrestige = prestigeManager.CanPrestige();
            
            // 현재 프레스티지 레벨
            if (currentLevelText != null)
                currentLevelText.text = $"Current Level: {prestigeData.prestigeLevel}";
            
            // 현재 프레스티지 포인트
            if (prestigePointsText != null)
                prestigePointsText.text = $"Prestige Points: {prestigeData.currentPrestigePoints}";
            
            // 획득할 프레스티지 포인트
            if (gainedPointsText != null)
            {
                gainedPointsText.text = $"Points to Gain: {gainedPoints}";
                gainedPointsText.color = gainedPoints > 0 ? Color.green : Color.gray;
            }
            
            // 현재 전역 배율
            if (multiplierText != null)
                multiplierText.text = $"Global Multiplier: x{prestigeData.globalMultiplier:F2}";
            
            // 프레스티지 버튼 상태
            if (prestigeButton != null)
            {
                prestigeButton.interactable = canPrestige;
                var colors = prestigeButton.colors;
                if (canPrestige)
                {
                    colors.normalColor = new Color(1f, 0.6f, 0.2f, 1f); // 오렌지
                    colors.highlightedColor = new Color(1f, 0.7f, 0.3f, 1f);
                }
                else
                {
                    colors.normalColor = new Color(0.5f, 0.5f, 0.5f, 0.5f); // 회색
                    colors.highlightedColor = new Color(0.6f, 0.6f, 0.6f, 0.5f);
                }
                prestigeButton.colors = colors;
            }
            
            // 업그레이드 아이템들 새로고침
            foreach (var item in upgradeItems)
            {
                if (item != null)
                    item.RefreshUI();
            }
        }
        
        /// <summary>
        /// 프레스티지 버튼 클릭 이벤트
        /// </summary>
        private void OnPrestigeButtonClicked()
        {
            if (prestigeManager.CanPrestige())
            {
                // 확인 대화상자 표시 (간단한 구현)
                if (ShowConfirmDialog())
                {
                    prestigeManager.PerformPrestige();
                    RefreshUI();
                    
                    // 프레스티지 완료 효과
                    StartCoroutine(PrestigeCompleteEffect());
                }
            }
        }
        
        /// <summary>
        /// 간단한 확인 대화상자 (실제 구현에서는 더 정교한 UI 사용 권장)
        /// </summary>
        private bool ShowConfirmDialog()
        {
            // 실제 게임에서는 별도의 확인 대화상자 UI를 사용하는 것이 좋습니다
            return true; // 현재는 항상 true 반환
        }
        
        /// <summary>
        /// 프레스티지 완료 시 시각적 효과
        /// </summary>
        private System.Collections.IEnumerator PrestigeCompleteEffect()
        {
            // 화면 플래시 효과
            var overlay = new GameObject("PrestigeFlash");
            var canvas = overlay.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 1000;
            
            var image = overlay.AddComponent<Image>();
            image.color = new Color(1f, 0.8f, 0.2f, 0f); // 투명한 골든 색상
            
            var rectTransform = overlay.GetComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
            
            // 페이드 인
            float timer = 0f;
            float duration = 0.3f;
            while (timer < duration)
            {
                timer += Time.deltaTime;
                float alpha = Mathf.Sin((timer / duration) * Mathf.PI) * 0.5f;
                image.color = new Color(1f, 0.8f, 0.2f, alpha);
                yield return null;
            }
            
            // 오버레이 제거
            Destroy(overlay);
        }
        
        private void OnDestroy()
        {
            // 버튼 이벤트 해제
            if (closeButton != null)
                closeButton.onClick.RemoveAllListeners();
            
            if (prestigeButton != null)
                prestigeButton.onClick.RemoveAllListeners();
        }
    }
}
