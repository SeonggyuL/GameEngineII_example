using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UJ.Attributes;
using PointGenerator.Core;

namespace PointGenerator.Core.Services
{
    /// <summary>
    /// 업그레이드 시스템 관리 서비스
    /// </summary>
    public class UpgradeManager : IUpgradeManager
    {
        [Inject] private GameData gameData;
        [Inject] private UserData userData;
        [Inject] private IPointManager pointManager;
        
        public event Action<string, int> OnUpgradePurchased;
        public event Action<string> OnUpgradePurchaseFailed;
        
        public UpgradeManager()
        {
            // DI 주입 후 초기화될 것임
        }
        
        /// <summary>
        /// 서비스 초기화 (DI 주입 후 호출)
        /// </summary>
        public void Initialize()
        {
            if (gameData?.AllUpgradeDefinitions == null)
            {
                Debug.LogError("[UpgradeManager] GameData 또는 UpgradeDefinitions가 없습니다.");
                return;
            }
            
            Debug.Log($"[UpgradeManager] 초기화 완료. 업그레이드 개수: {gameData.AllUpgradeDefinitions.Count}");
        }
        
        public bool PurchaseUpgrade(string upgradeId)
        {
            var definition = GetUpgradeDefinition(upgradeId);
            if (definition == null)
            {
                Debug.LogError($"[UpgradeManager] 업그레이드 정의를 찾을 수 없습니다: {upgradeId}");
                OnUpgradePurchaseFailed?.Invoke(upgradeId);
                return false;
            }
            
            var currentLevel = GetUpgradeLevel(upgradeId);
            var cost = definition.CalculateCostForLevel(currentLevel);
            
            if (!pointManager.SpendPoints(cost))
            {
                Debug.Log($"[UpgradeManager] 업그레이드 구매 실패 - 포인트 부족: {upgradeId}");
                OnUpgradePurchaseFailed?.Invoke(upgradeId);
                return false;
            }
            
            // 레벨 증가
            var newLevel = currentLevel + 1;
            userData.SetUpgradeLevel(upgradeId, newLevel);
            
            // 효과 적용
            ApplyUpgradeEffect(definition);
            
            OnUpgradePurchased?.Invoke(upgradeId, newLevel);
            
            Debug.Log($"[UpgradeManager] 업그레이드 구매 성공: {upgradeId} Lv.{newLevel}, 비용: {cost}");
            return true;
        }
        
        public bool CanPurchaseUpgrade(string upgradeId)
        {
            var definition = GetUpgradeDefinition(upgradeId);
            if (definition == null) return false;
            
            var currentLevel = GetUpgradeLevel(upgradeId);
            var cost = definition.CalculateCostForLevel(currentLevel);
            
            return pointManager.CurrentPoints >= cost;
        }
        
        public int GetUpgradeLevel(string upgradeId)
        {
            return userData?.GetUpgradeLevel(upgradeId) ?? 0;
        }
        
        public long GetUpgradeCurrentCost(string upgradeId)
        {
            var definition = GetUpgradeDefinition(upgradeId);
            if (definition == null) return long.MaxValue;
            
            var currentLevel = GetUpgradeLevel(upgradeId);
            return definition.CalculateCostForLevel(currentLevel);
        }
        
        public List<UpgradeDefinition> GetAllUpgradeDefinitions()
        {
            return gameData?.AllUpgradeDefinitions ?? new List<UpgradeDefinition>();
        }
        
        public UpgradeDefinition GetUpgradeDefinition(string upgradeId)
        {
            return gameData?.AllUpgradeDefinitions?.FirstOrDefault(u => u.codeName == upgradeId);
        }
        
        /// <summary>
        /// 업그레이드 효과 적용
        /// </summary>
        /// <param name="definition">업그레이드 정의</param>
        private void ApplyUpgradeEffect(UpgradeDefinition definition)
        {
            if (definition.PointsPerClickEffect > 0)
            {
                pointManager.IncreasePointsPerClick(definition.PointsPerClickEffect);
            }
            
            if (definition.PointsPerSecondEffect > 0)
            {
                pointManager.IncreasePointsPerSecond(definition.PointsPerSecondEffect);
            }
        }
        
        /// <summary>
        /// 모든 업그레이드 효과 재계산 및 적용
        /// </summary>
        public void RecalculateAllUpgradeEffects()
        {
            if (pointManager is PointManager pm)
            {
                pm.RecalculateStats();
            }
        }
        
        /// <summary>
        /// 업그레이드 구매 가능한 것들 조회
        /// </summary>
        /// <returns>구매 가능한 업그레이드 ID 리스트</returns>
        public List<string> GetAffordableUpgrades()
        {
            var affordable = new List<string>();
            
            foreach (var definition in GetAllUpgradeDefinitions())
            {
                if (CanPurchaseUpgrade(definition.codeName))
                {
                    affordable.Add(definition.codeName);
                }
            }
            
            return affordable;
        }
        
        /// <summary>
        /// 업그레이드 총 투자 비용 계산
        /// </summary>
        /// <param name="upgradeId">업그레이드 ID</param>
        /// <returns>총 투자 비용</returns>
        public long GetTotalInvestedCost(string upgradeId)
        {
            var definition = GetUpgradeDefinition(upgradeId);
            if (definition == null) return 0;
            
            var currentLevel = GetUpgradeLevel(upgradeId);
            long totalCost = 0;
            
            for (int i = 0; i < currentLevel; i++)
            {
                totalCost += definition.CalculateCostForLevel(i);
            }
            
            return totalCost;
        }
    }
}
