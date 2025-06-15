using System;
using System.Collections.Generic;
using PointGenerator.Core;

namespace PointGenerator.Core
{
    /// <summary>
    /// 프레스티지 관리 인터페이스
    /// </summary>
    public interface IPrestigeManager
    {
        /// <summary>
        /// 프레스티지 실행 가능 이벤트
        /// </summary>
        event Action<bool> OnPrestigeAvailabilityChanged;
        
        /// <summary>
        /// 프레스티지 실행 이벤트
        /// </summary>
        event Action<long> OnPrestigePerformed; // 획득한 프레스티지 포인트
        
        /// <summary>
        /// 프레스티지 포인트 변경 이벤트
        /// </summary>
        event Action<long> OnPrestigePointsChanged;
        
        /// <summary>
        /// 초기화
        /// </summary>
        void Initialize();
        
        /// <summary>
        /// 프레스티지 실행 가능 여부 확인
        /// </summary>
        bool CanPrestige();
        
        /// <summary>
        /// 프레스티지 실행 시 획득할 포인트 계산
        /// </summary>
        long CalculatePrestigePoints();
        
        /// <summary>
        /// 프레스티지 데이터 가져오기
        /// </summary>
        PrestigeData GetPrestigeData();
        
        /// <summary>
        /// 모든 프레스티지 업그레이드 정의 가져오기
        /// </summary>
        List<PrestigeUpgradeDefinition> GetAllPrestigeUpgradeDefinitions();
        
        /// <summary>
        /// 프레스티지 실행
        /// </summary>
        void PerformPrestige();
        
        /// <summary>
        /// 프레스티지 업그레이드 구매
        /// </summary>
        bool PurchasePrestigeUpgrade(string codeName);
        
        /// <summary>
        /// 프레스티지 업그레이드 구매 가능 여부 확인
        /// </summary>
        bool CanAffordPrestigeUpgrade(string codeName, long currentPrestigePoints);
        
        /// <summary>
        /// 프레스티지 업그레이드 비용 계산
        /// </summary>
        long CalculatePrestigeUpgradeCost(string codeName, int currentLevel);
        
        /// <summary>
        /// 프레스티지 업그레이드 레벨 가져오기
        /// </summary>
        int GetPrestigeUpgradeLevel(string codeName);
          /// <summary>
        /// 전역 배율 가져오기
        /// </summary>
        double GetGlobalMultiplier();
        
        /// <summary>
        /// 오프라인 보너스 배율 가져오기
        /// </summary>
        float GetOfflineBonus();
    }
}
