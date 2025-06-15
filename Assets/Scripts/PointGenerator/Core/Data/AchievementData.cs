using System;
using System.Collections.Generic;

namespace PointGenerator.Core
{
    /// <summary>
    /// 업적 정의
    /// </summary>
    [Serializable]
    public class AchievementDefinition
    {
        public string codeName;           // 업적 코드명
        public string displayName;        // 표시명
        public string description;        // 설명
        public AchievementType type;      // 업적 타입
        public long targetValue;          // 목표 값
        public long rewardPoints;         // 보상 포인트
        public long rewardClickBonus;     // 클릭 보너스
        public long rewardIdleBonus;      // 방치 보너스
        public bool isHidden;             // 숨겨진 업적 여부
        
        public AchievementDefinition()
        {
            codeName = "";
            displayName = "";
            description = "";
            type = AchievementType.TotalPoints;
            targetValue = 0;
            rewardPoints = 0;
            rewardClickBonus = 0;
            rewardIdleBonus = 0;
            isHidden = false;
        }
        
        public AchievementDefinition(string code, string name, string desc, AchievementType achievementType, 
            long target, long pointReward = 0, long clickReward = 0, long idleReward = 0, bool hidden = false)
        {
            codeName = code;
            displayName = name;
            description = desc;
            type = achievementType;
            targetValue = target;
            rewardPoints = pointReward;
            rewardClickBonus = clickReward;
            rewardIdleBonus = idleReward;
            isHidden = hidden;
        }
    }
    
    /// <summary>
    /// 업적 타입
    /// </summary>
    public enum AchievementType
    {
        TotalPoints,        // 총 포인트 획득
        TotalClicks,        // 총 클릭 수
        UpgradeCount,       // 업그레이드 횟수
        TimeSpent,          // 플레이 시간
        PointsPerSecond,    // 초당 포인트
        PointsPerClick,     // 클릭당 포인트
        ReachLevel,         // 특정 레벨 도달
        MaxUpgradeLevel     // 최대 업그레이드 레벨
    }
    
    /// <summary>
    /// 사용자 업적 데이터
    /// </summary>
    [Serializable]
    public class UserAchievement
    {
        public string codeName;
        public bool isUnlocked;
        public long currentProgress;
        public DateTime unlockedDate;
        
        public UserAchievement()
        {
            codeName = "";
            isUnlocked = false;
            currentProgress = 0;
            unlockedDate = DateTime.MinValue;
        }
        
        public UserAchievement(string code)
        {
            codeName = code;
            isUnlocked = false;
            currentProgress = 0;
            unlockedDate = DateTime.MinValue;
        }
    }
}
