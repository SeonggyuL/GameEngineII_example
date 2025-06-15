using System;
using System.Collections.Generic;
using PointGenerator.Core;

namespace PointGenerator.Core
{
    /// <summary>
    /// 업적 관리 인터페이스
    /// </summary>
    public interface IAchievementManager
    {
        /// <summary>
        /// 업적 달성 이벤트
        /// </summary>
        event Action<AchievementDefinition> OnAchievementUnlocked;
        
        /// <summary>
        /// 업적 진행도 변경 이벤트
        /// </summary>
        event Action<string, long, long> OnAchievementProgressChanged; // codeName, current, target
        
        /// <summary>
        /// 초기화
        /// </summary>
        void Initialize();
        
        /// <summary>
        /// 모든 업적 정의 가져오기
        /// </summary>
        List<AchievementDefinition> GetAllAchievements();
        
        /// <summary>
        /// 사용자 업적 데이터 가져오기
        /// </summary>
        List<UserAchievement> GetUserAchievements();
        
        /// <summary>
        /// 특정 업적의 사용자 데이터 가져오기
        /// </summary>
        UserAchievement GetUserAchievement(string codeName);
        
        /// <summary>
        /// 특정 업적 달성 여부 확인
        /// </summary>
        bool IsAchievementUnlocked(string codeName);
        
        /// <summary>
        /// 업적 진행도 업데이트
        /// </summary>
        void UpdateProgress(AchievementType type, long value);
        
        /// <summary>
        /// 업적 강제 달성 (치트/테스트용)
        /// </summary>
        void UnlockAchievement(string codeName);
        
        /// <summary>
        /// 달성한 업적 수 가져오기
        /// </summary>
        int GetUnlockedCount();
        
        /// <summary>
        /// 총 업적 수 가져오기
        /// </summary>
        int GetTotalCount();
    }
}
