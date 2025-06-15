using System;

namespace PointGenerator.Core
{
    /// <summary>
    /// 오프라인 보상 관리 인터페이스
    /// </summary>
    public interface IOfflineRewardsManager
    {
        /// <summary>
        /// 오프라인 보상 계산 및 적용
        /// </summary>
        /// <param name="offlineSeconds">오프라인 시간(초)</param>
        /// <returns>획득한 포인트</returns>
        long CalculateAndApplyOfflineRewards(double offlineSeconds);
        
        /// <summary>
        /// 오프라인 보상 미리보기 계산
        /// </summary>
        /// <param name="offlineSeconds">오프라인 시간(초)</param>
        /// <returns>예상 포인트</returns>
        long CalculateOfflineRewards(double offlineSeconds);
        
        /// <summary>
        /// 최대 오프라인 시간 설정
        /// </summary>
        /// <param name="maxHours">최대 시간(시간)</param>
        void SetMaxOfflineHours(float maxHours);
        
        /// <summary>
        /// 오프라인 효율성 설정 (100%가 최대)
        /// </summary>
        /// <param name="efficiency">효율성 (0.0 ~ 1.0)</param>
        void SetOfflineEfficiency(float efficiency);
        
        /// <summary>
        /// 오프라인 시간을 읽기 쉬운 문자열로 포맷
        /// </summary>
        /// <param name="seconds">오프라인 시간(초)</param>
        /// <returns>포맷된 시간 문자열 (예: "2h 30m", "5m 20s")</returns>
        string FormatOfflineTime(double seconds);
        
        /// <summary>
        /// 오프라인 보상 적용 시 이벤트
        /// </summary>
        event Action<long, double> OnOfflineRewardsApplied;
    }
}
