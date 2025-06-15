using System;

namespace PointGenerator.Core
{
    /// <summary>
    /// 게임 전체 상태 관리 인터페이스
    /// </summary>
    public interface IGameManager
    {
        /// <summary>
        /// 게임 초기화 완료 여부
        /// </summary>
        bool IsInitialized { get; }
        
        /// <summary>
        /// 게임 데이터 가져오기
        /// </summary>
        GameData GetGameData();
        
        /// <summary>
        /// 게임 시작
        /// </summary>
        void StartGame();
        
        /// <summary>
        /// 게임 일시정지
        /// </summary>
        void PauseGame();
        
        /// <summary>
        /// 게임 재개
        /// </summary>
        void ResumeGame();
        
        /// <summary>
        /// 게임 상태 저장
        /// </summary>
        void SaveGame();
        
        /// <summary>
        /// 매 프레임 게임 업데이트 (자동 저장 등)
        /// </summary>
        void UpdateGame();
        
        /// <summary>
        /// 애플리케이션 포커스 변경 시 처리
        /// </summary>
        /// <param name="hasFocus">포커스 상태</param>
        void OnApplicationFocusChanged(bool hasFocus);
        
        /// <summary>
        /// 애플리케이션 일시정지 변경 시 처리
        /// </summary>
        /// <param name="pauseStatus">일시정지 상태</param>
        void OnApplicationPauseChanged(bool pauseStatus);
        
        /// <summary>
        /// 애플리케이션 종료 시 처리
        /// </summary>
        void OnApplicationQuit();
        
        /// <summary>
        /// 게임 초기화 완료 이벤트
        /// </summary>
        event Action OnGameInitialized;
        
        /// <summary>
        /// 게임 시작 이벤트
        /// </summary>
        event Action OnGameStarted;
    }
}
