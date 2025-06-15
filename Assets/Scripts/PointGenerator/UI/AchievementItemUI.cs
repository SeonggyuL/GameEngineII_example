using UnityEngine;
using UnityEngine.UI;
using PointGenerator.Core;

namespace PointGenerator.UI
{
    /// <summary>
    /// 개별 업적 아이템 UI 컴포넌트
    /// </summary>
    public class AchievementItemUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TMPro.TMP_Text nameText;
        [SerializeField] private TMPro.TMP_Text descriptionText;
        [SerializeField] private TMPro.TMP_Text progressText;
        [SerializeField] private TMPro.TMP_Text rewardText;
        [SerializeField] private Image iconImage;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Slider progressSlider;
        [SerializeField] private GameObject unlockedIndicator;
        [SerializeField] private GameObject newIndicator;
        
        private AchievementDefinition achievementDefinition;
        private IAchievementManager achievementManager;
        
        /// <summary>
        /// 업적 아이템 초기화
        /// </summary>
        public void Initialize(AchievementDefinition definition, IAchievementManager manager)
        {
            achievementDefinition = definition;
            achievementManager = manager;
            
            // 기본 UI 설정
            if (nameText != null)
                nameText.text = definition.displayName;
            
            if (descriptionText != null)
                descriptionText.text = definition.description;
            
            if (rewardText != null)
                rewardText.text = $"Reward: {definition.rewardPoints} points";
            
            // 초기 UI 상태 설정
            RefreshUI();
        }
        
        /// <summary>
        /// UI 상태 새로고침
        /// </summary>
        public void RefreshUI()
        {
            if (achievementDefinition == null || achievementManager == null)
                return;
            
            var userData = achievementManager.GetUserAchievement(achievementDefinition.codeName);
            var isUnlocked = userData?.isUnlocked ?? false;
            var progress = userData?.currentProgress ?? 0;
            var target = achievementDefinition.targetValue;
            
            // 진행도 표시
            if (progressText != null)
            {
                if (isUnlocked)
                {
                    progressText.text = "COMPLETED";
                    progressText.color = Color.green;
                }
                else
                {
                    progressText.text = $"{FormatNumber(progress)}/{FormatNumber(target)}";
                    progressText.color = Color.white;
                }
            }
            
            // 진행도 슬라이더
            if (progressSlider != null)
            {
                progressSlider.value = target > 0 ? (float)((double)progress / target) : 0f;
                progressSlider.fillRect.GetComponent<Image>().color = isUnlocked ? Color.green : Color.blue;
            }
            
            // 배경 색상 변경
            if (backgroundImage != null)
            {
                if (isUnlocked)
                {
                    backgroundImage.color = new Color(0.2f, 0.4f, 0.2f, 0.8f); // 녹색 톤
                }
                else
                {
                    backgroundImage.color = new Color(0.15f, 0.15f, 0.25f, 0.9f); // 기본 톤
                }
            }
            
            // 해금 표시
            if (unlockedIndicator != null)
                unlockedIndicator.SetActive(isUnlocked);
            
            // 새 업적 표시 (최근에 해금된 경우)
            if (newIndicator != null)
            {
                bool isNew = userData?.isUnlocked == true && 
                            (System.DateTime.Now - userData.unlockedDate).TotalMinutes < 5;
                newIndicator.SetActive(isNew);
            }
        }
        
        /// <summary>
        /// 업적 해금 효과 재생
        /// </summary>
        public void PlayUnlockEffect()
        {
            StartCoroutine(UnlockEffectCoroutine());
        }
        
        /// <summary>
        /// 업적 해금 시 시각적 효과
        /// </summary>
        private System.Collections.IEnumerator UnlockEffectCoroutine()
        {
            var originalScale = transform.localScale;
            var targetScale = originalScale * 1.2f;
            
            // 확대 애니메이션
            float timer = 0f;
            float duration = 0.2f;
            while (timer < duration)
            {
                timer += Time.deltaTime;
                float t = Mathf.Sin((timer / duration) * Mathf.PI);
                transform.localScale = Vector3.Lerp(originalScale, targetScale, t);
                yield return null;
            }
            
            // 복원
            transform.localScale = originalScale;
            
            // 색상 플래시 효과
            if (backgroundImage != null)
            {
                var originalColor = backgroundImage.color;
                var flashColor = Color.yellow;
                
                timer = 0f;
                duration = 0.5f;
                while (timer < duration)
                {
                    timer += Time.deltaTime;
                    float t = Mathf.PingPong(timer * 4, 1);
                    backgroundImage.color = Color.Lerp(originalColor, flashColor, t * 0.3f);
                    yield return null;
                }
                
                backgroundImage.color = originalColor;
            }
        }
        
        /// <summary>
        /// 업적 ID 반환
        /// </summary>
        public string GetAchievementId()
        {
            return achievementDefinition?.codeName;
        }
        
        /// <summary>
        /// 숫자를 읽기 쉬운 형태로 포맷
        /// </summary>
        private string FormatNumber(long number)
        {
            if (number >= 1000000000000) // Trillion
                return $"{number / 1000000000000.0:F1}T";
            if (number >= 1000000000) // Billion
                return $"{number / 1000000000.0:F1}B";
            if (number >= 1000000) // Million
                return $"{number / 1000000.0:F1}M";
            if (number >= 1000) // Thousand
                return $"{number / 1000.0:F1}K";
            
            return number.ToString("F0");
        }
    }
}
