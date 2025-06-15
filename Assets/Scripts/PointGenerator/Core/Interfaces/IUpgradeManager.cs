using System;
using System.Collections.Generic;

namespace PointGenerator.Core
{
    /// <summary>
    /// 업그레이드 시스템 관리 인터페이스
    /// </summary>
    public interface IUpgradeManager
    {
        /// <summary>
        /// 업그레이드 구매
        /// </summary>
        /// <param name="upgradeId">업그레이드 ID</param>
        /// <returns>구매 성공 여부</returns>
        bool PurchaseUpgrade(string upgradeId);
        
        /// <summary>
        /// 업그레이드 구매 가능 여부 확인
        /// </summary>
        /// <param name="upgradeId">업그레이드 ID</param>
        /// <returns>구매 가능 여부</returns>
        bool CanPurchaseUpgrade(string upgradeId);
        
        /// <summary>
        /// 업그레이드 현재 레벨 조회
        /// </summary>
        /// <param name="upgradeId">업그레이드 ID</param>
        /// <returns>현재 레벨</returns>
        int GetUpgradeLevel(string upgradeId);
        
        /// <summary>
        /// 업그레이드 현재 가격 조회
        /// </summary>
        /// <param name="upgradeId">업그레이드 ID</param>
        /// <returns>현재 가격</returns>
        long GetUpgradeCurrentCost(string upgradeId);
        
        /// <summary>
        /// 모든 업그레이드 정의 조회
        /// </summary>
        /// <returns>업그레이드 정의 리스트</returns>
        List<UpgradeDefinition> GetAllUpgradeDefinitions();
        
        /// <summary>
        /// 특정 업그레이드 정의 조회
        /// </summary>
        /// <param name="upgradeId">업그레이드 ID</param>
        /// <returns>업그레이드 정의</returns>
        UpgradeDefinition GetUpgradeDefinition(string upgradeId);
        
        /// <summary>
        /// 업그레이드 구매 성공 이벤트
        /// </summary>
        event Action<string, int> OnUpgradePurchased;
        
        /// <summary>
        /// 업그레이드 구매 실패 이벤트
        /// </summary>
        event Action<string> OnUpgradePurchaseFailed;

        /// <summary>
        /// 서비스 초기화
        /// </summary>
        void Initialize();

        /// <summary>
        /// 모든 업그레이드 효과 재계산 및 적용
        /// </summary>
        void RecalculateAllUpgradeEffects();

        /// <summary>
        /// 구매 가능한 업그레이드 ID 리스트 조회
        /// </summary>
        /// <returns>구매 가능한 업그레이드 ID 리스트</returns>
        List<string> GetAffordableUpgrades();

        /// <summary>
        /// 특정 업그레이드에 투자된 총 비용 계산
        /// </summary>
        /// <param name="upgradeId">업그레이드 ID</param>
        /// <returns>총 투자 비용</returns>
        long GetTotalInvestedCost(string upgradeId);
    }
}
