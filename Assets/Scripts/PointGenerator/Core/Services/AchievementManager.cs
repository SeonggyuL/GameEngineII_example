using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UJ.Attributes;
using PointGenerator.Core;

namespace PointGenerator.Core.Services
{
    /// <summary>
    /// 업적 관리 서비스
    /// </summary>
    public class AchievementManager : IAchievementManager
    {
        [Inject] private GameData gameData;
        [Inject] private UserData userData;
        [Inject] private IPointManager pointManager;
        
        private List<AchievementDefinition> achievementDefinitions;
        private Dictionary<string, UserAchievement> userAchievements;
        private bool isInitialized = false;
        
        public event Action<AchievementDefinition> OnAchievementUnlocked;
        public event Action<string, long, long> OnAchievementProgressChanged;
        
        public AchievementManager()
        {
            achievementDefinitions = new List<AchievementDefinition>();
            userAchievements = new Dictionary<string, UserAchievement>();
        }
        
        public void Initialize()
        {
            if (isInitialized) return;
            
            try
            {
                // 업적 정의 로드
                LoadAchievementDefinitions();
                
                // 사용자 업적 데이터 초기화
                InitializeUserAchievements();
                
                // 포인트 매니저 이벤트 구독
                SubscribeToEvents();
                
                isInitialized = true;
                Debug.Log($"[AchievementManager] 초기화 완료. 총 업적: {achievementDefinitions.Count}개");
            }
            catch (Exception e)
            {
                Debug.LogError($"[AchievementManager] 초기화 실패: {e.Message}");
                throw;
            }
        }
        
        public List<AchievementDefinition> GetAllAchievements()
        {
            return achievementDefinitions.ToList();
        }
        
        public List<UserAchievement> GetUserAchievements()
        {
            return userAchievements.Values.ToList();
        }
        
        public UserAchievement GetUserAchievement(string codeName)
        {
            return userAchievements.ContainsKey(codeName) ? userAchievements[codeName] : null;
        }
        
        public bool IsAchievementUnlocked(string codeName)
        {
            return userAchievements.ContainsKey(codeName) && userAchievements[codeName].isUnlocked;
        }
        
        public void UpdateProgress(AchievementType type, long value)
        {
            if (!isInitialized) return;
            
            var relevantAchievements = achievementDefinitions.Where(a => a.type == type && !IsAchievementUnlocked(a.codeName)).ToList();
            
            foreach (var achievement in relevantAchievements)
            {
                if (userAchievements.ContainsKey(achievement.codeName))
                {
                    var userAchievement = userAchievements[achievement.codeName];
                    userAchievement.currentProgress = value;
                    
                    // 진행도 변경 이벤트 발생
                    OnAchievementProgressChanged?.Invoke(achievement.codeName, value, achievement.targetValue);
                    
                    // 목표 달성 확인
                    if (value >= achievement.targetValue)
                    {
                        UnlockAchievement(achievement.codeName);
                    }
                }
            }
        }
        
        public void UnlockAchievement(string codeName)
        {
            if (!userAchievements.ContainsKey(codeName)) return;
            
            var userAchievement = userAchievements[codeName];
            if (userAchievement.isUnlocked) return;
            
            var definition = achievementDefinitions.FirstOrDefault(a => a.codeName == codeName);
            if (definition == null) return;
            
            // 업적 달성 처리
            userAchievement.isUnlocked = true;
            userAchievement.unlockedDate = DateTime.Now;
            
            // 보상 지급
            ApplyAchievementRewards(definition);
            
            // 이벤트 발생
            OnAchievementUnlocked?.Invoke(definition);
            
            Debug.Log($"[AchievementManager] 업적 달성: {definition.displayName}");
        }
        
        public int GetUnlockedCount()
        {
            return userAchievements.Values.Count(a => a.isUnlocked);
        }
        
        public int GetTotalCount()
        {
            return achievementDefinitions.Count;
        }
        
        /// <summary>
        /// 업적 정의 로드
        /// </summary>
        private void LoadAchievementDefinitions()
        {
            achievementDefinitions.Clear();
            
            // 기본 업적들 추가
            achievementDefinitions.AddRange(GetDefaultAchievements());
            
            Debug.Log($"[AchievementManager] 업적 정의 로드 완료: {achievementDefinitions.Count}개");
        }
        
        /// <summary>
        /// 기본 업적 목록 생성
        /// </summary>
        private List<AchievementDefinition> GetDefaultAchievements()
        {
            return new List<AchievementDefinition>
            {
                // 포인트 관련 업적
                new AchievementDefinition("first_point", "첫 포인트", "첫 번째 포인트를 획득하세요", 
                    AchievementType.TotalPoints, 1, 10, 1, 0),
                new AchievementDefinition("hundred_points", "백 포인트", "총 100 포인트를 획득하세요", 
                    AchievementType.TotalPoints, 100, 50, 2, 0),
                new AchievementDefinition("thousand_points", "천 포인트", "총 1,000 포인트를 획득하세요", 
                    AchievementType.TotalPoints, 1000, 200, 5, 1),
                new AchievementDefinition("million_points", "백만 포인트", "총 1,000,000 포인트를 획득하세요", 
                    AchievementType.TotalPoints, 1000000, 5000, 50, 10),
                
                // 클릭 관련 업적
                new AchievementDefinition("first_click", "첫 클릭", "첫 번째 클릭을 하세요", 
                    AchievementType.TotalClicks, 1, 5, 1, 0),
                new AchievementDefinition("hundred_clicks", "백 클릭", "총 100번 클릭하세요", 
                    AchievementType.TotalClicks, 100, 100, 3, 0),
                new AchievementDefinition("thousand_clicks", "천 클릭", "총 1,000번 클릭하세요", 
                    AchievementType.TotalClicks, 1000, 500, 10, 2),
                
                // 업그레이드 관련 업적
                new AchievementDefinition("first_upgrade", "첫 업그레이드", "첫 번째 업그레이드를 구매하세요", 
                    AchievementType.UpgradeCount, 1, 25, 2, 1),
                new AchievementDefinition("ten_upgrades", "업그레이드 마스터", "총 10개의 업그레이드를 구매하세요", 
                    AchievementType.UpgradeCount, 10, 1000, 20, 5),
                
                // 초당 포인트 관련 업적
                new AchievementDefinition("auto_generation", "자동화 시작", "초당 1 포인트를 달성하세요", 
                    AchievementType.PointsPerSecond, 1, 50, 0, 2),
                new AchievementDefinition("idle_master", "방치형 마스터", "초당 100 포인트를 달성하세요", 
                    AchievementType.PointsPerSecond, 100, 2000, 0, 20),
                
                // 클릭당 포인트 관련 업적
                new AchievementDefinition("power_click", "강력한 클릭", "클릭당 10 포인트를 달성하세요", 
                    AchievementType.PointsPerClick, 10, 200, 5, 0),
                new AchievementDefinition("mega_click", "메가 클릭", "클릭당 100 포인트를 달성하세요", 
                    AchievementType.PointsPerClick, 100, 1000, 25, 0),
                
                // 숨겨진 업적
                new AchievementDefinition("secret_achievement", "???", "숨겨진 업적을 발견하세요", 
                    AchievementType.TotalPoints, 999999, 10000, 100, 50, true)
            };
        }
        
        /// <summary>
        /// 사용자 업적 데이터 초기화
        /// </summary>
        private void InitializeUserAchievements()
        {
            userAchievements.Clear();
            
            foreach (var achievement in achievementDefinitions)
            {
                if (userData?.achievements != null && userData.achievements.ContainsKey(achievement.codeName))
                {
                    userAchievements[achievement.codeName] = userData.achievements[achievement.codeName];
                }
                else
                {
                    userAchievements[achievement.codeName] = new UserAchievement(achievement.codeName);
                }
            }
            
            // 사용자 데이터에 업적 정보 저장
            if (userData != null)
            {
                if (userData.achievements == null)
                    userData.achievements = new Dictionary<string, UserAchievement>();
                
                foreach (var kvp in userAchievements)
                {
                    userData.achievements[kvp.Key] = kvp.Value;
                }
            }
        }
        
        /// <summary>
        /// 이벤트 구독
        /// </summary>
        private void SubscribeToEvents()
        {
            if (pointManager != null)
            {
                pointManager.OnPointsChanged += (points) => UpdateProgress(AchievementType.TotalPoints, points);
                pointManager.OnClicked += () => UpdateProgress(AchievementType.TotalClicks, userData?.totalClicks ?? 0);
                pointManager.OnPointsPerClickChanged += (ppc) => UpdateProgress(AchievementType.PointsPerClick, ppc);
                pointManager.OnPointsPerSecondChanged += (pps) => UpdateProgress(AchievementType.PointsPerSecond, pps);
            }
        }
        
        /// <summary>
        /// 업적 보상 적용
        /// </summary>
        private void ApplyAchievementRewards(AchievementDefinition achievement)
        {
            if (pointManager != null)
            {
                // 포인트 보상
                if (achievement.rewardPoints > 0)
                {
                    pointManager.AddPoints(achievement.rewardPoints);
                }
                
                // 클릭 보너스
                if (achievement.rewardClickBonus > 0)
                {
                    pointManager.IncreasePointsPerClick(achievement.rewardClickBonus);
                }
                
                // 방치 보너스
                if (achievement.rewardIdleBonus > 0)
                {
                    pointManager.IncreasePointsPerSecond(achievement.rewardIdleBonus);
                }
            }
        }
    }
}
