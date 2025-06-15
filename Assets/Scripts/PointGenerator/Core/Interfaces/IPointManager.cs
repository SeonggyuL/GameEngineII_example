using System;

namespace PointGenerator.Core
{
    /// <summary>
    /// 포인트 관리 인터페이스
    /// </summary>
    public interface IPointManager
    {
        /// <summary>
        /// 현재 포인트
        /// </summary>
        long CurrentPoints { get; }
        
        /// <summary>
        /// 클릭당 포인트
        /// </summary>
        long PointsPerClick { get; }
        
        /// <summary>
        /// 초당 포인트
        /// </summary>
        long PointsPerSecond { get; }
        
        /// <summary>
        /// 포인트 추가
        /// </summary>
        /// <param name="amount">추가할 포인트 수</param>
        void AddPoints(long amount);
        
        /// <summary>
        /// 포인트 소모 (구매 등)
        /// </summary>
        /// <param name="amount">소모할 포인트 수</param>
        /// <returns>소모 성공 여부</returns>
        bool SpendPoints(long amount);
        
        /// <summary>
        /// 클릭당 포인트 증가
        /// </summary>
        /// <param name="amount">증가량</param>
        void IncreasePointsPerClick(long amount);
        
        /// <summary>
        /// 초당 포인트 증가
        /// </summary>
        /// <param name="amount">증가량</param>
        void IncreasePointsPerSecond(long amount);
        
        /// <summary>
        /// 클릭 실행
        /// </summary>
        void PerformClick();
        
        /// <summary>
        /// 자동 포인트 생성 시작
        /// </summary>
        void StartAutoGeneration();
        
        /// <summary>
        /// 자동 포인트 생성 중지
        /// </summary>
        void StopAutoGeneration();
        
        /// <summary>
        /// 포인트 변경 이벤트
        /// </summary>
        event Action<long> OnPointsChanged;
        
        /// <summary>
        /// 클릭당 포인트 변경 이벤트
        /// </summary>
        event Action<long> OnPointsPerClickChanged;
        
        /// <summary>
        /// 초당 포인트 변경 이벤트
        /// </summary>
        event Action<long> OnPointsPerSecondChanged;
        
        /// <summary>
        /// 클릭 이벤트
        /// </summary>
        event Action OnClicked;
        
        /// <summary>
        /// 서비스 초기화
        /// </summary>
        void Initialize();

        /// <summary>
        /// 업그레이드 및 프레스티지 효과 등에 따른 스탯 재계산
        /// </summary>
        void RecalculateStats();
    }
}
