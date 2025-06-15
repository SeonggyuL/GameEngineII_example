using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UJ.Attributes;
using PointGenerator.Core.Services;
using PointGenerator.Core;

namespace PointGenerator.UI
{
    /// <summary>
    /// 오프라인 보상 팝업 UI
    /// </summary>
    public class OfflineRewardsPopup : MonoBehaviour
    {
        [Inject] private IOfflineRewardsManager offlineRewardsManager;
        
        [Header("UI References")]
        [SerializeField] private GameObject popupPanel;
        [SerializeField] private TMPro.TMP_Text offlineTimeText;
        [SerializeField] private TMPro.TMP_Text rewardAmountText;
        [SerializeField] private Button claimButton;
        [SerializeField] private Button closeButton;
        [SerializeField] private Button multiplyButton; // 광고 시청으로 보상 배수
        
        [Header("Animation")]
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private Transform popupTransform;
        
        private long pendingReward;
        private double offlineSeconds;
        private bool isShowing = false;
        
        public event Action<long> OnRewardClaimed;
        
        private void Start()
        {
            // DI 주입 대기
            StartCoroutine(WaitForInjectionAndInitialize());
        }
        
        private System.Collections.IEnumerator WaitForInjectionAndInitialize()
        {
            yield return new WaitUntil(() => offlineRewardsManager != null);
            InitializePopup();
        }
        
        private void InitializePopup()
        {
            // 버튼 이벤트 설정
            if (claimButton != null)
                claimButton.onClick.AddListener(ClaimReward);
            
            if (closeButton != null)
                closeButton.onClick.AddListener(ClosePopup);
            
            if (multiplyButton != null)
                multiplyButton.onClick.AddListener(MultiplyReward);
            
            // 초기에는 팝업 숨김
            if (popupPanel != null)
                popupPanel.SetActive(false);
            
            // 오프라인 보상 이벤트 구독
            if (offlineRewardsManager != null)
            {
                offlineRewardsManager.OnOfflineRewardsApplied += OnOfflineRewardsCalculated;
            }
        }
        
        /// <summary>
        /// 오프라인 보상 팝업 표시
        /// </summary>
        public void ShowOfflineRewards(double seconds, long reward)
        {
            if (isShowing || reward <= 0) return;
            
            offlineSeconds = seconds;
            pendingReward = reward;
            
            // UI 업데이트
            UpdateRewardUI();
            
            // 팝업 표시
            ShowPopup();
        }
        
        private void UpdateRewardUI()
        {
            if (offlineTimeText != null)
            {
                offlineTimeText.text = $"오프라인 시간: {offlineRewardsManager.FormatOfflineTime(offlineSeconds)}";
            }
            
            if (rewardAmountText != null)
            {
                rewardAmountText.text = FormatNumber(pendingReward);
            }
            
            // 광고 배수 버튼 활성화 (광고 시스템이 있다면)
            if (multiplyButton != null)
            {
                multiplyButton.gameObject.SetActive(CanShowMultiplyOption());
            }
        }
        
        private void ShowPopup()
        {
            if (popupPanel != null)
            {
                popupPanel.SetActive(true);
                isShowing = true;
                
                // 애니메이션 재생
                StartCoroutine(PopupAppearAnimation());
            }
        }
        
        private void ClosePopup()
        {
            if (!isShowing) return;
            
            StartCoroutine(PopupDisappearAnimation());
        }
        
        private void ClaimReward()
        {
            if (pendingReward > 0)
            {
                OnRewardClaimed?.Invoke(pendingReward);
                Debug.Log($"[OfflineRewardsPopup] 보상 수령: {pendingReward} 포인트");
            }
            
            ClosePopup();
        }
        
        private void MultiplyReward()
        {
            // 광고 시청으로 보상 2배 (실제 광고 시스템 연동 필요)
            pendingReward *= 2;
            UpdateRewardUI();
            
            // 광고 버튼 비활성화
            if (multiplyButton != null)
                multiplyButton.gameObject.SetActive(false);
            
            Debug.Log($"[OfflineRewardsPopup] 보상 2배 적용: {pendingReward} 포인트");
        }
        
        private bool CanShowMultiplyOption()
        {
            // 광고 시스템이 있고, 보상이 충분히 클 때만 표시
            return pendingReward >= 1000; // 1000 포인트 이상일 때만
        }
        
        private System.Collections.IEnumerator PopupAppearAnimation()
        {
            if (canvasGroup != null && popupTransform != null)
            {
                // 초기 상태
                canvasGroup.alpha = 0f;
                popupTransform.localScale = Vector3.zero;
                
                float duration = 0.3f;
                float elapsed = 0f;
                
                while (elapsed < duration)
                {
                    elapsed += Time.deltaTime;
                    float progress = elapsed / duration;
                    
                    // Ease out scale
                    float scale = Mathf.Lerp(0f, 1f, EaseOutBack(progress));
                    popupTransform.localScale = Vector3.one * scale;
                    
                    // Fade in
                    canvasGroup.alpha = Mathf.Lerp(0f, 1f, progress);
                    
                    yield return null;
                }
                
                canvasGroup.alpha = 1f;
                popupTransform.localScale = Vector3.one;
            }
        }
        
        private System.Collections.IEnumerator PopupDisappearAnimation()
        {
            if (canvasGroup != null && popupTransform != null)
            {
                float duration = 0.2f;
                float elapsed = 0f;
                
                while (elapsed < duration)
                {
                    elapsed += Time.deltaTime;
                    float progress = elapsed / duration;
                    
                    // Scale down
                    float scale = Mathf.Lerp(1f, 0f, progress);
                    popupTransform.localScale = Vector3.one * scale;
                    
                    // Fade out
                    canvasGroup.alpha = Mathf.Lerp(1f, 0f, progress);
                    
                    yield return null;
                }
            }
            
            if (popupPanel != null)
                popupPanel.SetActive(false);
            
            isShowing = false;
        }
        
        private void OnOfflineRewardsCalculated(long reward, double seconds)
        {
            // 자동으로 팝업 표시
            ShowOfflineRewards(seconds, reward);
        }
        
        private float EaseOutBack(float t)
        {
            float c1 = 1.70158f;
            float c3 = c1 + 1f;
            return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
        }
        
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
        
        private void OnDestroy()
        {
            if (offlineRewardsManager != null)
            {
                offlineRewardsManager.OnOfflineRewardsApplied -= OnOfflineRewardsCalculated;
            }
        }
    }
}
