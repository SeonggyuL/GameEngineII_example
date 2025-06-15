using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UJ.Attributes;
using PointGenerator.Core;

namespace PointGenerator.Core.Services
{
    /// <summary>
    /// 프레스티지 관리 서비스
    /// </summary>
    public class PrestigeManager : IPrestigeManager
    {
        [Inject] private GameData gameData;
        [Inject] private UserData userData;
        [Inject] private IPointManager pointManager;
        [Inject] private IUpgradeManager upgradeManager;
        [Inject] private ISaveLoadManager saveLoadManager;
        
        private PrestigeData prestigeData;
        private List<PrestigeUpgradeDefinition> prestigeUpgrades;
        private bool isInitialized = false;
        
        public event Action<bool> OnPrestigeAvailabilityChanged;
        public event Action<long> OnPrestigePerformed;
        public event Action<long> OnPrestigePointsChanged;
        
        public PrestigeManager()
        {
            prestigeUpgrades = new List<PrestigeUpgradeDefinition>();
        }
        
        public void Initialize()
        {
            if (isInitialized) return;
            
            try
            {
                // 프레스티지 데이터 초기화
                InitializePrestigeData();
                
                // 프레스티지 업그레이드 정의 로드
                LoadPrestigeUpgrades();
                
                // 포인트 변경 이벤트 구독
                SubscribeToEvents();
                
                isInitialized = true;
                Debug.Log("[PrestigeManager] 초기화 완료");
            }
            catch (Exception e)
            {
                Debug.LogError($"[PrestigeManager] 초기화 실패: {e.Message}");
                throw;
            }
        }
        
        public bool CanPrestige()
        {
            return userData?.CurrentPoints >= GameConstants.MIN_PRESTIGE_POINTS;
        }
        
        public long CalculatePrestigePoints()
        {
            if (!CanPrestige()) return 0;
            
            // 현재 포인트 기반으로 프레스티지 포인트 계산
            var basePoints = (long)(userData.CurrentPoints * GameConstants.PRESTIGE_POINT_RATIO);
            
            // 프레스티지 레벨에 따른 보너스 적용
            var levelBonus = prestigeData.prestigeLevel * 0.1;
            
            return (long)(basePoints * (1.0 + levelBonus));
        }
        
        public void PerformPrestige()
        {
            if (!CanPrestige())
            {
                Debug.LogWarning("[PrestigeManager] 프레스티지 조건을 만족하지 않습니다.");
                return;
            }
            
            var prestigePoints = CalculatePrestigePoints();
            
            // 프레스티지 데이터 업데이트
            prestigeData.prestigeLevel++;
            prestigeData.totalPrestigePoints += prestigePoints;
            prestigeData.currentPrestigePoints += prestigePoints;
            prestigeData.lastPrestigeTime = DateTime.Now;
            
            // 게임 상태 초기화
            ResetGameState();
            
            // 프레스티지 배율 재계산
            RecalculatePrestigeEffects();
            
            // 이벤트 발생
            OnPrestigePerformed?.Invoke(prestigePoints);
            OnPrestigePointsChanged?.Invoke(prestigeData.currentPrestigePoints);
            
            // 자동 저장
            saveLoadManager?.SaveUserData(userData);
            
            Debug.Log($"[PrestigeManager] 프레스티지 실행! 레벨: {prestigeData.prestigeLevel}, 획득 포인트: {prestigePoints}");
        }
        
        public bool PurchasePrestigeUpgrade(string codeName)
        {
            var upgrade = prestigeUpgrades.FirstOrDefault(u => u.codeName == codeName);
            if (upgrade == null) return false;
            
            var cost = CalculatePrestigeUpgradeCost(codeName, GetPrestigeUpgradeLevel(codeName));
            if (prestigeData.currentPrestigePoints < cost) return false;
            
            var currentLevel = GetPrestigeUpgradeLevel(codeName);
            if (currentLevel >= upgrade.maxLevel) return false;
            
            // 프레스티지 포인트 소모
            prestigeData.currentPrestigePoints -= cost;
            
            // 업그레이드 레벨 증가
            prestigeData.prestigeUpgradeLevels[codeName] = currentLevel + 1;
            
            // 효과 재계산
            RecalculatePrestigeEffects();
            
            // 이벤트 발생
            OnPrestigePointsChanged?.Invoke(prestigeData.currentPrestigePoints);
            
            Debug.Log($"[PrestigeManager] 프레스티지 업그레이드 구매: {upgrade.displayName} Lv.{currentLevel + 1}");
            return true;
        }
        
        public bool CanAffordPrestigeUpgrade(string codeName, long currentPrestigePoints)
        {
            var cost = CalculatePrestigeUpgradeCost(codeName, GetPrestigeUpgradeLevel(codeName));
            return currentPrestigePoints >= cost;
        }
        
        public long CalculatePrestigeUpgradeCost(string codeName, int currentLevel)
        {
            var upgrade = prestigeUpgrades.FirstOrDefault(u => u.codeName == codeName);
            if (upgrade == null) return long.MaxValue;
            
            return (long)(upgrade.baseCost * Math.Pow(upgrade.costMultiplier, currentLevel));
        }
        
        public int GetPrestigeUpgradeLevel(string codeName)
        {
            return prestigeData.prestigeUpgradeLevels.ContainsKey(codeName) ? prestigeData.prestigeUpgradeLevels[codeName] : 0;
        }
        
        public PrestigeData GetPrestigeData()
        {
            return prestigeData;
        }
        
        public double GetGlobalMultiplier()
        {
            return prestigeData?.globalMultiplier ?? 1.0;
        }
        
        public float GetOfflineBonus()
        {
            float offlineBonus = 1.0f;
            
            foreach (var upgrade in prestigeUpgrades)
            {
                var level = GetPrestigeUpgradeLevel(upgrade.codeName);
                if (level > 0 && upgrade.upgradeType == PrestigeUpgradeType.OfflineBonus)
                {
                    var effectValue = upgrade.baseEffectValue + (upgrade.effectIncrementPerLevel * (level - 1));
                    offlineBonus += (float)(effectValue * level);
                }
            }
            
            return offlineBonus;
        }
        
        public List<PrestigeUpgradeDefinition> GetAllPrestigeUpgradeDefinitions()
        {
            return prestigeUpgrades.ToList();
        }
        
        /// <summary>
        /// 프레스티지 데이터 초기화
        /// </summary>
        private void InitializePrestigeData()
        {
            // UserData에서 프레스티지 데이터 로드하거나 새로 생성
            if (userData != null)
            {
                // TODO: UserData에 PrestigeData 필드 추가 필요
                // prestigeData = new PrestigeData(); // 임시로 새로 생성
                if (userData.prestigeData == null)
                {
                    userData.prestigeData = new PrestigeData();
                }
                prestigeData = userData.prestigeData;
            }
            else
            {
                // UserData가 없는 비정상적인 상황이지만, 기본값으로 초기화
                prestigeData = new PrestigeData();
                Debug.LogWarning("[PrestigeManager] UserData가 주입되지 않아 새 PrestigeData를 생성합니다.");
            }
        }
        
        /// <summary>
        /// 프레스티지 업그레이드 정의 로드
        /// </summary>
        private void LoadPrestigeUpgrades()
        {
            prestigeUpgrades.Clear();
            
            // 기본 프레스티지 업그레이드 추가
            prestigeUpgrades.AddRange(GetDefaultPrestigeUpgrades());
            
            // 사용자 레벨 데이터 초기화
            foreach (var upgrade in prestigeUpgrades)
            {
                if (!prestigeData.prestigeUpgradeLevels.ContainsKey(upgrade.codeName))
                {
                    prestigeData.prestigeUpgradeLevels[upgrade.codeName] = 0;
                }
            }
        }
        
        /// <summary>
        /// 기본 프레스티지 업그레이드 목록
        /// </summary>
        private List<PrestigeUpgradeDefinition> GetDefaultPrestigeUpgrades()
        {
            return new List<PrestigeUpgradeDefinition>
            {
                new PrestigeUpgradeDefinition
                {
                    codeName = "global_multiplier",
                    displayName = "전역 배율",
                    description = "모든 포인트 획득량이 10% 증가합니다",
                    baseCost = 1,
                    costMultiplier = 2.0,
                    baseEffectValue = 0.1,
                    effectIncrementPerLevel = 0.0,
                    upgradeType = PrestigeUpgradeType.GlobalMultiplier,
                    maxLevel = 50
                },
                new PrestigeUpgradeDefinition
                {
                    codeName = "click_multiplier",
                    displayName = "클릭 배율",
                    description = "클릭 포인트가 20% 증가합니다",
                    baseCost = 2,
                    costMultiplier = 2.5,
                    baseEffectValue = 0.2,
                    effectIncrementPerLevel = 0.0,
                    upgradeType = PrestigeUpgradeType.ClickMultiplier,
                    maxLevel = 25
                },
                new PrestigeUpgradeDefinition
                {
                    codeName = "idle_multiplier",
                    displayName = "방치 배율",
                    description = "초당 포인트가 15% 증가합니다",
                    baseCost = 3,
                    costMultiplier = 2.2,
                    baseEffectValue = 0.15,
                    effectIncrementPerLevel = 0.0,
                    upgradeType = PrestigeUpgradeType.IdleMultiplier,
                    maxLevel = 30
                },
                new PrestigeUpgradeDefinition
                {
                    codeName = "upgrade_discount",
                    displayName = "업그레이드 할인",
                    description = "모든 업그레이드 비용이 5% 감소합니다",
                    baseCost = 5,
                    costMultiplier = 3.0,
                    baseEffectValue = 0.05,
                    effectIncrementPerLevel = 0.0,
                    upgradeType = PrestigeUpgradeType.UpgradeDiscount,
                    maxLevel = 20
                }
            };
        }
        
        /// <summary>
        /// 이벤트 구독
        /// </summary>
        private void SubscribeToEvents()
        {
            if (pointManager != null)
            {
                pointManager.OnPointsChanged += (points) => 
                {
                    var canPrestige = CanPrestige();
                    OnPrestigeAvailabilityChanged?.Invoke(canPrestige);
                };
            }
        }
        
        /// <summary>
        /// 게임 상태 초기화 (프레스티지 시)
        /// </summary>
        private void ResetGameState()
        {
            if (userData == null) return;
            
            // 포인트와 생산량 초기화
            userData.CurrentPoints = 0;
            userData.PointsPerClick = GameConstants.INITIAL_POINTS_PER_CLICK;
            userData.PointsPerSecond = GameConstants.INITIAL_POINTS_PER_SECOND;
            
            // 업그레이드 초기화
            userData.UserUpgradeProgresses.Clear();
            
            // 매니저들에게 알림
            if (pointManager is PointManager pm)
            {
                pm.RecalculateStats();
            }
            
            if (upgradeManager is UpgradeManager um)
            {
                um.RecalculateAllUpgradeEffects();
            }
        }
        
        /// <summary>
        /// 프레스티지 효과 재계산
        /// </summary>
        private void RecalculatePrestigeEffects()
        {
            if (prestigeData == null) return;
            
            // 전역 배율 계산
            double globalMultiplier = 1.0;
            
            foreach (var upgrade in prestigeUpgrades)
            {
                var level = GetPrestigeUpgradeLevel(upgrade.codeName);
                if (level > 0)
                {
                    switch (upgrade.upgradeType)
                    {
                        case PrestigeUpgradeType.GlobalMultiplier:
                            globalMultiplier += upgrade.baseEffectValue * level;
                            break;
                        // 다른 타입들은 필요시 구현
                    }
                }
            }
            
            prestigeData.globalMultiplier = globalMultiplier;
            
            Debug.Log($"[PrestigeManager] 프레스티지 효과 재계산. 전역 배율: {globalMultiplier:F2}x");
        }
    }
}
