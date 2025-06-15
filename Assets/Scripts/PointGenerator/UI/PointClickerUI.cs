using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UJ.Attributes;
using UJ.DI;
using PointGenerator.Core;

namespace PointGenerator.UI
{
    /// <summary>
    /// 포인트 클릭 UI 컨트롤러
    /// </summary>
    public class PointClickerUI : DIMono
    {
        [Inject] private IPointManager pointManager;
        [Inject] private IUpgradeManager upgradeManager;
        [Inject] private IAchievementManager achievementManager;
        [Inject] private IPrestigeManager prestigeManager;
        
        [Header("UI References")]
        [SerializeField] private Button clickButton;
        [SerializeField] private TextMeshProUGUI pointsText;
        [SerializeField] private TextMeshProUGUI pointsPerClickText;
        [SerializeField] private TextMeshProUGUI pointsPerSecondText;
        [SerializeField] private Transform upgradeContainer;
        [SerializeField] private GameObject upgradeItemPrefab;
        
        [Header("Panel Management")]
        [SerializeField] private Button achievementsButton;
        [SerializeField] private Button prestigeButton;
        [SerializeField] private AchievementPanelUI achievementPanel;
        [SerializeField] private PrestigePanelUI prestigePanel;
        
        [Header("Visual Effects")]
        [SerializeField] private ParticleSystem clickEffect;
        [SerializeField] private AudioSource clickSound;
        
        private void Start()
        {
            // DI 주입 대기 후 초기화
            StartCoroutine(WaitForInjectionAndInitialize());
        }
        
        private System.Collections.IEnumerator WaitForInjectionAndInitialize()
        {
            yield return new WaitUntil(() => pointManager != null);
            
            InitializeUI();
            SubscribeToEvents();
            UpdateUI();
        }
          /// <summary>
        /// UI 초기화
        /// </summary>
        private void InitializeUI()
        {
            // 클릭 버튼 설정
            if (clickButton != null)
            {
                clickButton.onClick.AddListener(OnClickButtonPressed);
            }
            
            // 패널 버튼 설정
            if (achievementsButton != null)
            {
                achievementsButton.onClick.AddListener(OnAchievementsButtonPressed);
            }
            
            if (prestigeButton != null)
            {
                prestigeButton.onClick.AddListener(OnPrestigeButtonPressed);
            }
            
            // 업그레이드 UI 생성
            CreateUpgradeUI();
            
            // 패널 초기화
            InitializePanels();
            
            Debug.Log("[PointClickerUI] UI 초기화 완료");
        }
          /// <summary>
        /// 이벤트 구독
        /// </summary>
        private void SubscribeToEvents()
        {
            if (pointManager != null)
            {
                pointManager.OnPointsChanged += OnPointsChanged;
                pointManager.OnPointsPerClickChanged += OnPointsPerClickChanged;
                pointManager.OnPointsPerSecondChanged += OnPointsPerSecondChanged;
                pointManager.OnClicked += OnClicked;
            }
            
            if (achievementManager != null)
            {
                achievementManager.OnAchievementUnlocked += OnAchievementUnlocked;
            }
            
            if (prestigeManager != null)
            {
                prestigeManager.OnPrestigePerformed += OnPrestigePerformed;
            }
        }
          /// <summary>
        /// 클릭 버튼 눌림 처리
        /// </summary>
        private void OnClickButtonPressed()
        {
            pointManager?.PerformClick();
        }
        
        /// <summary>
        /// 업적 버튼 눌림 처리
        /// </summary>
        private void OnAchievementsButtonPressed()
        {
            if (achievementPanel != null)
            {
                achievementPanel.gameObject.SetActive(!achievementPanel.gameObject.activeSelf);
                
                // 프레스티지 패널이 열려있으면 닫기
                if (prestigePanel != null && prestigePanel.gameObject.activeSelf)
                {
                    prestigePanel.gameObject.SetActive(false);
                }
            }
        }
        
        /// <summary>
        /// 프레스티지 버튼 눌림 처리
        /// </summary>
        private void OnPrestigeButtonPressed()
        {
            if (prestigePanel != null)
            {
                prestigePanel.gameObject.SetActive(!prestigePanel.gameObject.activeSelf);
                
                // 업적 패널이 열려있으면 닫기
                if (achievementPanel != null && achievementPanel.gameObject.activeSelf)
                {
                    achievementPanel.gameObject.SetActive(false);
                }
            }
        }
        
        /// <summary>
        /// 패널 초기화
        /// </summary>
        private void InitializePanels()
        {
            // 모든 패널을 초기에는 비활성화
            if (achievementPanel != null)
            {
                achievementPanel.gameObject.SetActive(false);
            }
            
            if (prestigePanel != null)
            {
                prestigePanel.gameObject.SetActive(false);
            }
        }
        
        /// <summary>
        /// 포인트 변경 시 UI 업데이트
        /// </summary>
        private void OnPointsChanged(long newPoints)
        {
            if (pointsText != null)
            {
                pointsText.text = FormatNumber(newPoints);
            }
        }
        
        /// <summary>
        /// 클릭당 포인트 변경 시 UI 업데이트
        /// </summary>
        private void OnPointsPerClickChanged(long newPointsPerClick)
        {
            if (pointsPerClickText != null)
            {
                pointsPerClickText.text = $"Per Click: {FormatNumber(newPointsPerClick)}";
            }
        }
        
        /// <summary>
        /// 초당 포인트 변경 시 UI 업데이트
        /// </summary>
        private void OnPointsPerSecondChanged(long newPointsPerSecond)
        {
            if (pointsPerSecondText != null)
            {
                pointsPerSecondText.text = $"Per Second: {FormatNumber(newPointsPerSecond)}";
            }
        }
        
        /// <summary>
        /// 클릭 시 시각/청각 효과
        /// </summary>
        private void OnClicked()
        {
            // 파티클 효과
            if (clickEffect != null)
            {
                clickEffect.Play();
            }
            
            // 클릭 사운드
            if (clickSound != null)
            {
                clickSound.Play();
            }
            
            // 버튼 애니메이션 (스케일 효과)
            if (clickButton != null)
            {
                StartCoroutine(ButtonClickAnimation());
            }
        }
          /// <summary>
        /// 업적 달성 알림
        /// </summary>
        private void OnAchievementUnlocked(PointGenerator.Core.AchievementDefinition achievement)
        {
            Debug.Log($"[PointClickerUI] 업적 달성: {achievement.displayName}");
            
            // 업적 달성 팝업 표시 (추후 구현)
            // ShowAchievementPopup(achievement);
        }
        
        /// <summary>
        /// 프레스티지 수행 알림
        /// </summary>
        private void OnPrestigePerformed(long prestigePoints)
        {
            Debug.Log($"[PointClickerUI] 프레스티지 수행: 포인트 {prestigePoints}");
            
            // 프레스티지 알림 팝업 표시 (추후 구현)
            // ShowPrestigeNotification(prestigePoints);
        }
        
        /// <summary>
        /// 업그레이드 UI 생성
        /// </summary>
        private void CreateUpgradeUI()
        {
            if (upgradeManager == null || upgradeContainer == null || upgradeItemPrefab == null)
                return;
            
            var upgrades = upgradeManager.GetAllUpgradeDefinitions();
            foreach (var upgrade in upgrades)
            {
                var upgradeItem = Instantiate(upgradeItemPrefab, upgradeContainer);
                var upgradeController = upgradeItem.GetComponent<UpgradeItemUI>();
                
                if (upgradeController != null)
                {
                    upgradeController.Initialize(upgrade);
                }
            }
        }
        
        /// <summary>
        /// 전체 UI 업데이트
        /// </summary>
        private void UpdateUI()
        {
            if (pointManager != null)
            {
                OnPointsChanged(pointManager.CurrentPoints);
                OnPointsPerClickChanged(pointManager.PointsPerClick);
                OnPointsPerSecondChanged(pointManager.PointsPerSecond);
            }
        }
        
        /// <summary>
        /// 버튼 클릭 애니메이션
        /// </summary>
        private System.Collections.IEnumerator ButtonClickAnimation()
        {
            var originalScale = clickButton.transform.localScale;
            var targetScale = originalScale * 1.1f;
            
            // 확대
            float time = 0;
            while (time < 0.1f)
            {
                time += Time.deltaTime;
                var scale = Vector3.Lerp(originalScale, targetScale, time / 0.1f);
                clickButton.transform.localScale = scale;
                yield return null;
            }
            
            // 축소
            time = 0;
            while (time < 0.1f)
            {
                time += Time.deltaTime;
                var scale = Vector3.Lerp(targetScale, originalScale, time / 0.1f);
                clickButton.transform.localScale = scale;
                yield return null;
            }
            
            clickButton.transform.localScale = originalScale;
        }
        
        /// <summary>
        /// 숫자 포맷팅 (K, M, B 등)
        /// </summary>
        private string FormatNumber(long number)
        {
            if (number >= 1000000000000) // 1T
                return $"{number / 1000000000000.0:F1}T";
            if (number >= 1000000000) // 1B
                return $"{number / 1000000000.0:F1}B";
            if (number >= 1000000) // 1M
                return $"{number / 1000000.0:F1}M";
            if (number >= 1000) // 1K
                return $"{number / 1000.0:F1}K";
            
            return number.ToString();
        }
          /// <summary>
        /// 컴포넌트 해제 시 이벤트 구독 해제
        /// </summary>
        private void OnDestroy()
        {
            if (pointManager != null)
            {
                pointManager.OnPointsChanged -= OnPointsChanged;
                pointManager.OnPointsPerClickChanged -= OnPointsPerClickChanged;
                pointManager.OnPointsPerSecondChanged -= OnPointsPerSecondChanged;
                pointManager.OnClicked -= OnClicked;
            }
            
            if (achievementManager != null)
            {
                achievementManager.OnAchievementUnlocked -= OnAchievementUnlocked;
            }
            
            if (prestigeManager != null)
            {
                prestigeManager.OnPrestigePerformed -= OnPrestigePerformed;
            }
        }
    }
}
