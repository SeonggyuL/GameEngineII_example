using System;
using System.Collections;
using UnityEngine;
using UJ.Attributes;
using PointGenerator.Core;

namespace PointGenerator.Core.Services
{
    /// <summary>
    /// 게임 전체 상태 관리 서비스
    /// </summary>
    public class GameManager : IGameManager
    {        [Inject] private GameData gameData;
        [Inject] private UserData userData;
        [Inject] private IPointManager pointManager;
        [Inject] private IUpgradeManager upgradeManager;
        [Inject] private ISaveLoadManager saveLoadManager;
        [Inject] private IAchievementManager achievementManager;
        [Inject] private IPrestigeManager prestigeManager;
        
        private bool isInitialized = false;
        private bool isGamePaused = false;
        private Coroutine autoGenerationCoroutine;
        private Coroutine autoSaveCoroutine;
        
        public bool IsInitialized => isInitialized;
        
        public event Action OnGameInitialized;
        public event Action OnGameStarted;
        
        public GameManager()
        {
            // DI 주입 후 초기화될 것임
        }
        
        public GameData GetGameData()
        {
            return gameData;
        }
        
        public void StartGame()
        {
            if (isInitialized) return;
            
            try
            {
                Debug.Log("[GameManager] 게임 시작");
                
                // 각 매니저 초기화
                InitializeManagers();
                
                // 자동 시스템 시작
                StartAutoSystems();
                
                isInitialized = true;
                OnGameInitialized?.Invoke();
                OnGameStarted?.Invoke();
                
                Debug.Log("[GameManager] 게임 초기화 완료");
            }
            catch (Exception e)
            {
                Debug.LogError($"[GameManager] 게임 시작 실패: {e.Message}");
                throw;
            }
        }
        
        public void PauseGame()
        {
            if (!isInitialized || isGamePaused) return;
            
            isGamePaused = true;
            StopAutoSystems();
            SaveGame();
            
            Debug.Log("[GameManager] 게임 일시정지");
        }
        
        public void ResumeGame()
        {
            if (!isInitialized || !isGamePaused) return;
            
            isGamePaused = false;
            StartAutoSystems();
            
            Debug.Log("[GameManager] 게임 재개");
        }
        
        public void SaveGame()
        {
            if (!isInitialized) return;
            
            saveLoadManager?.SaveUserData(userData);
            Debug.Log("[GameManager] 게임 저장 완료");
        }
          /// <summary>
        /// 각 매니저 초기화
        /// </summary>
        private void InitializeManagers()
        {
            // PointManager 초기화
            if (pointManager is PointManager pm)
            {
                pm.Initialize();
            }
            
            // UpgradeManager 초기화
            if (upgradeManager is UpgradeManager um)
            {
                um.Initialize();
                um.RecalculateAllUpgradeEffects(); // 로드된 데이터 기반으로 효과 재계산
            }
            
            // AchievementManager 초기화
            if (achievementManager is AchievementManager am)
            {
                am.Initialize();
            }
            
            // PrestigeManager 초기화
            if (prestigeManager is PrestigeManager prm)
            {
                prm.Initialize();
            }
            
            Debug.Log("[GameManager] 모든 매니저 초기화 완료");
        }
        
        /// <summary>
        /// 자동 시스템들 시작
        /// </summary>
        private void StartAutoSystems()
        {
            // MonoBehaviour가 필요한 코루틴은 별도 컴포넌트에서 처리
            // 여기서는 요청만 보냄
            StartAutoPointGeneration();
            StartAutoSave();
        }
        
        /// <summary>
        /// 자동 시스템들 중지
        /// </summary>
        private void StopAutoSystems()
        {
            StopAutoPointGeneration();
            StopAutoSave();
        }
        
        /// <summary>
        /// 자동 포인트 생성 시작
        /// </summary>
        private void StartAutoPointGeneration()
        {
            // 실제 구현은 별도 MonoBehaviour에서 처리
            pointManager?.StartAutoGeneration();
        }
        
        /// <summary>
        /// 자동 포인트 생성 중지
        /// </summary>
        private void StopAutoPointGeneration()
        {
            pointManager?.StopAutoGeneration();
        }
        
        /// <summary>
        /// 자동 저장 시작
        /// </summary>
        private void StartAutoSave()
        {
            saveLoadManager?.EnableAutoSave();
        }
        
        /// <summary>
        /// 자동 저장 중지
        /// </summary>
        private void StopAutoSave()
        {
            saveLoadManager?.DisableAutoSave();
        }
        
        /// <summary>
        /// 게임 상태 업데이트 (매 프레임 호출용)
        /// </summary>
        public void UpdateGame()
        {
            if (!isInitialized || isGamePaused) return;
            
            // 자동 저장 업데이트
            if (saveLoadManager is SaveLoadManager slm)
            {
                slm.UpdateAutoSave(userData);
            }
        }
        
        /// <summary>
        /// 애플리케이션 포커스 변경 시 호출
        /// </summary>
        /// <param name="hasFocus">포커스 여부</param>
        public void OnApplicationFocusChanged(bool hasFocus)
        {
            if (!isInitialized) return;
            
            if (hasFocus)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
        
        /// <summary>
        /// 애플리케이션 일시정지 시 호출
        /// </summary>
        /// <param name="pauseStatus">일시정지 상태</param>
        public void OnApplicationPauseChanged(bool pauseStatus)
        {
            if (!isInitialized) return;
            
            if (pauseStatus)
            {
                PauseGame();
            }
            else
            {
                ResumeGame();
            }
        }
        
        /// <summary>
        /// 게임 종료 시 호출
        /// </summary>
        public void OnApplicationQuit()
        {
            if (isInitialized)
            {
                SaveGame();
                Debug.Log("[GameManager] 게임 종료 시 저장 완료");
            }
        }
    }
}
