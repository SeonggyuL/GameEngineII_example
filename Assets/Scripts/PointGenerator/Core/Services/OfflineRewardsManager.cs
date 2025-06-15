using System;
using UnityEngine;
using UJ.Attributes;
using PointGenerator.Core;

namespace PointGenerator.Core.Services
{
    /// <summary>
    /// 오프라인 보상 관리 서비스
    /// </summary>
    public class OfflineRewardsManager : IOfflineRewardsManager
    {
        [Inject] private IPointManager pointManager;
        [Inject] private IPrestigeManager prestigeManager;
        
        private float maxOfflineHours = 24f; // 최대 24시간
        private float offlineEfficiency = 0.5f; // 50% 효율성
        
        public event Action<long, double> OnOfflineRewardsApplied;
        
        public long CalculateAndApplyOfflineRewards(double offlineSeconds)
        {
            var rewardPoints = CalculateOfflineRewards(offlineSeconds);
            
            if (rewardPoints > 0 && pointManager != null)
            {
                pointManager.AddPoints(rewardPoints);
                OnOfflineRewardsApplied?.Invoke(rewardPoints, offlineSeconds);
                
                Debug.Log($"[OfflineRewardsManager] 오프라인 보상 적용: {rewardPoints} 포인트 ({offlineSeconds:F0}초)");
            }
            
            return rewardPoints;
        }
        
        public long CalculateOfflineRewards(double offlineSeconds)
        {
            if (pointManager == null || offlineSeconds < 60) // 1분 미만은 보상 없음
                return 0;
            
            // 최대 오프라인 시간 제한
            var maxOfflineSeconds = maxOfflineHours * 3600;
            var effectiveOfflineSeconds = Math.Min(offlineSeconds, maxOfflineSeconds);
            
            // 기본 초당 포인트
            var basePointsPerSecond = pointManager.PointsPerSecond;
            
            // 프레스티지 보너스 적용
            var prestigeBonus = prestigeManager?.GetOfflineBonus() ?? 1.0f;
            
            // 오프라인 효율성 적용
            var effectivePointsPerSecond = basePointsPerSecond * offlineEfficiency * prestigeBonus;
            
            // 총 보상 계산
            var totalReward = (long)(effectivePointsPerSecond * effectiveOfflineSeconds);
            
            return Math.Max(0, totalReward);
        }
        
        public void SetMaxOfflineHours(float maxHours)
        {
            maxOfflineHours = Mathf.Max(1f, maxHours);
            Debug.Log($"[OfflineRewardsManager] 최대 오프라인 시간 설정: {maxOfflineHours}시간");
        }
        
        public void SetOfflineEfficiency(float efficiency)
        {
            offlineEfficiency = Mathf.Clamp01(efficiency);
            Debug.Log($"[OfflineRewardsManager] 오프라인 효율성 설정: {offlineEfficiency * 100:F0}%");
        }
        
        /// <summary>
        /// 오프라인 시간 포맷팅
        /// </summary>
        public string FormatOfflineTime(double seconds)
        {
            if (seconds < 60)
                return $"{seconds:F0}초";
            
            if (seconds < 3600)
                return $"{seconds / 60:F0}분";
            
            if (seconds < 86400)
                return $"{seconds / 3600:F1}시간";
            
            return $"{seconds / 86400:F1}일";
        }
    }
}
