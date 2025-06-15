using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using PointGenerator.Core;
using UJ.DI;
using UJ.Attributes;

namespace PointGenerator.UI
{
    /// <summary>
    /// 업적 패널 UI 컴포넌트
    /// </summary>
    public class AchievementPanelUI : DIMono
    {
        [Header("UI References")]
        [SerializeField] private Transform achievementItemParent;
        [SerializeField] private GameObject achievementItemPrefab;
        [SerializeField] private Button closeButton;
        [SerializeField] private TMPro.TMP_Text titleText;
        [SerializeField] private ScrollRect scrollRect;
        
        [Inject] private IAchievementManager achievementManager;
        
        private List<AchievementItemUI> achievementItems = new List<AchievementItemUI>();

        public override void Init()
        {

            // 닫기 버튼 이벤트 등록
            if (closeButton != null)
                closeButton.onClick.AddListener(ClosePanel);

            // 타이틀 설정
            if (titleText != null)
                titleText.text = "Achievements";
            // 업적 시스템 이벤트 구독
            if (achievementManager != null)
            {
                achievementManager.OnAchievementUnlocked += OnAchievementUnlocked;
                achievementManager.OnAchievementProgressChanged += OnAchievementProgressChanged;
            }
            
            CreateAchievementItems();
        }
        
        /// <summary>
        /// 업적 아이템들을 생성
        /// </summary>
        private void CreateAchievementItems()
        {
            if (achievementManager == null || achievementItemPrefab == null || achievementItemParent == null)
                return;
            
            // 기존 아이템들 제거
            foreach (var item in achievementItems)
            {
                if (item != null)
                    Destroy(item.gameObject);
            }
            achievementItems.Clear();
            
            // 모든 업적 정의 가져오기
            var allAchievements = achievementManager.GetAllAchievements();
            
            foreach (var achievement in allAchievements)
            {
                // 업적 아이템 생성
                var itemObj = Instantiate(achievementItemPrefab, achievementItemParent);
                var itemUI = itemObj.GetComponent<AchievementItemUI>();
                
                if (itemUI != null)
                {
                    itemUI.Initialize(achievement, achievementManager);
                    achievementItems.Add(itemUI);
                }
            }
        }
        
        /// <summary>
        /// 패널 열기
        /// </summary>
        public void OpenPanel()
        {
            gameObject.SetActive(true);
            RefreshAllItems();
        }
        
        /// <summary>
        /// 패널 닫기
        /// </summary>
        public void ClosePanel()
        {
            gameObject.SetActive(false);
        }
        
        /// <summary>
        /// 모든 업적 아이템 새로고침
        /// </summary>
        public void RefreshAllItems()
        {
            foreach (var item in achievementItems)
            {
                if (item != null)
                    item.RefreshUI();
            }
        }
        
        /// <summary>
        /// 업적 해금 이벤트 처리
        /// </summary>
        private void OnAchievementUnlocked(AchievementDefinition achievement)
        {
            // 해당 업적 아이템 찾아서 업데이트
            foreach (var item in achievementItems)
            {
                if (item != null && item.GetAchievementId() == achievement.codeName)
                {
                    item.RefreshUI();
                    item.PlayUnlockEffect();
                    break;
                }
            }
        }
        
        /// <summary>
        /// 업적 진행도 변경 이벤트 처리
        /// </summary>
        private void OnAchievementProgressChanged(string achievementId, long current, long target)
        {
            // 해당 업적 아이템 찾아서 업데이트
            foreach (var item in achievementItems)
            {
                if (item != null && item.GetAchievementId() == achievementId)
                {
                    item.RefreshUI();
                    break;
                }
            }
        }
        
        private void OnDestroy()
        {
            // 이벤트 구독 해제
            if (achievementManager != null)
            {
                achievementManager.OnAchievementUnlocked -= OnAchievementUnlocked;
                achievementManager.OnAchievementProgressChanged -= OnAchievementProgressChanged;
            }
            
            // 버튼 이벤트 해제
            if (closeButton != null)
                closeButton.onClick.RemoveAllListeners();
        }
    }
}
