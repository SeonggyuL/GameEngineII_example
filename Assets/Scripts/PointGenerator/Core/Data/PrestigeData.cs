using System;
using System.Collections.Generic;

namespace PointGenerator.Core
{
    /// <summary>
    /// 프레스티지 데이터
    /// </summary>
    [Serializable]
    public class PrestigeData
    {
        public long prestigeLevel;           // 현재 프레스티지 레벨
        public long totalPrestigePoints;     // 총 프레스티지 포인트
        public long currentPrestigePoints;   // 현재 프레스티지 포인트
        public double globalMultiplier;      // 전역 포인트 배율
        public DateTime lastPrestigeTime;    // 마지막 프레스티지 시간
        public Dictionary<string, int> prestigeUpgradeLevels; // 프레스티지 업그레이드 레벨들
        
        public PrestigeData()
        {
            prestigeLevel = 0;
            totalPrestigePoints = 0;
            currentPrestigePoints = 0;
            globalMultiplier = 1.0;
            lastPrestigeTime = DateTime.MinValue;
            prestigeUpgradeLevels = new Dictionary<string, int>();
        }
    }
    
    /// <summary>
    /// 프레스티지 업그레이드 정의
    /// </summary>
    [Serializable]
    public class PrestigeUpgradeDefinition
    {
        public string codeName;
        public string displayName;
        public string description;
        public long baseCost;
        public double costMultiplier;
        public double baseEffectValue;
        public double effectIncrementPerLevel;
        public PrestigeUpgradeType upgradeType;
        public int maxLevel;
        
        // UI에서 사용하는 프로퍼티들
        public string id => codeName;
        public string name => displayName;
        
        public PrestigeUpgradeDefinition()
        {
            codeName = "";
            displayName = "";
            description = "";
            baseCost = 1;
            costMultiplier = 1.5;
            baseEffectValue = 1.0;
            effectIncrementPerLevel = 0.1;
            upgradeType = PrestigeUpgradeType.GlobalMultiplier;
            maxLevel = 100;
        }
    }
    
    /// <summary>
    /// 프레스티지 업그레이드 타입
    /// </summary>
    public enum PrestigeUpgradeType
    {
        GlobalMultiplier,    // 전역 포인트 배율
        ClickMultiplier,     // 클릭 포인트 배율
        IdleMultiplier,      // 방치 포인트 배율
        UpgradeDiscount,     // 업그레이드 할인
        OfflineBonus,        // 오프라인 보너스
        AutoClicker          // 자동 클릭
    }
}
