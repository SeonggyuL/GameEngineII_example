using System;
using UnityEngine;
using UJ.DI;
using PointGenerator.Core;

namespace PointGenerator.Bootstrap
{
    /// <summary>
    /// 게임 부트스트랩 - 게임 전체 초기화 및 DI 컨테이너 설정
    /// </summary>
    public static class GameBootstrap
    {
        private static DIContainer container;
        private static bool isInitialized = false;
        
        /// <summary>
        /// 초기화 완료 이벤트
        /// </summary>
        public static event Action OnBootstrapCompleted;
        
        /// <summary>
        /// 씬 로드 시 자동으로 실행되는 초기화 메서드
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static async void Initialize()
        {
            try
            {
                Debug.Log("[GameBootstrap] 게임 부트스트랩 시작 (자동 초기화)");
                
                // 1. DI Container 초기화
                await InitializeDIContainer();
                
                // 2. GameData 로딩
                var gameData = await DataLoader.LoadGameData();
                
                // 3. 서비스 등록
                ServiceInstaller.InstallServices(container, gameData);
                
                // 4. 애플리케이션 생명주기 관리자 생성
                CreateLifecycleManager();
                
                // 5. 게임 매니저 초기화 및 시작
                var gameManager = container.GetValue<IGameManager>();
                gameManager.StartGame();
                
                isInitialized = true;
                OnBootstrapCompleted?.Invoke();
                
                Debug.Log("[GameBootstrap] 게임 부트스트랩 완료 (자동 초기화)");
            }
            catch (Exception e)
            {
                Debug.LogError($"[GameBootstrap] 부트스트랩 실패: {e.Message}");
                Debug.LogException(e);
            }
        }
        
        private static async System.Threading.Tasks.Task InitializeDIContainer()
        {
            container = new DIContainer();
            DIContainer.AddContainer(container);
            
            Debug.Log("[GameBootstrap] DI Container 초기화 완료");
            
            // 필요시 추가 초기화 작업
            await System.Threading.Tasks.Task.Yield();
        }
        
        /// <summary>
        /// 애플리케이션 생명주기 관리자 생성
        /// </summary>
        private static void CreateLifecycleManager()
        {
            var lifecycleManagerGO = new GameObject("[GameLifecycleManager]");
            var lifecycleManager = lifecycleManagerGO.AddComponent<GameLifecycleManager>();
            lifecycleManager.Initialize(container);
            
            // 씬 전환 시 파괴되지 않도록 설정
            UnityEngine.Object.DontDestroyOnLoad(lifecycleManagerGO);
            
            Debug.Log("[GameBootstrap] 게임 생명주기 관리자 생성 완료");
        }
        
        /// <summary>
        /// 수동으로 서비스 정리 (필요시 호출)
        /// </summary>
        public static void Cleanup()
        {
            if (container != null)
            {
                ServiceInstaller.UninstallServices(container);
                DIContainer.RemoveContainer(container);
                container = null;
                isInitialized = false;
                
                Debug.Log("[GameBootstrap] 서비스 정리 완료");
            }
        }
        
        /// <summary>
        /// 초기화 완료 여부
        /// </summary>
        public static bool IsInitialized => isInitialized;
        
        /// <summary>
        /// DI 컨테이너 접근
        /// </summary>
        public static DIContainer Container => container;
    }
    
    /// <summary>
    /// 애플리케이션 생명주기 관리 컴포넌트
    /// </summary>
    internal class GameLifecycleManager : MonoBehaviour
    {
        private DIContainer container;
        private IGameManager gameManager;
        
        public void Initialize(DIContainer diContainer)
        {
            container = diContainer;
            gameManager = container?.GetValue<IGameManager>();
        }
        
        private void OnApplicationPause(bool pauseStatus)
        {
            if (!GameBootstrap.IsInitialized) return;
            
            gameManager?.OnApplicationPauseChanged(pauseStatus);
            
            if (pauseStatus)
            {
                // 앱이 일시정지될 때 자동 저장
                gameManager?.SaveGame();
            }
        }
        
        private void OnApplicationFocus(bool hasFocus)
        {
            if (!GameBootstrap.IsInitialized) return;
            
            gameManager?.OnApplicationFocusChanged(hasFocus);
            
            if (!hasFocus)
            {
                // 앱이 포커스를 잃을 때 자동 저장
                gameManager?.SaveGame();
            }
        }
        
        private void OnDestroy()
        {
            if (GameBootstrap.IsInitialized)
            {
                gameManager?.OnApplicationQuit();
                GameBootstrap.Cleanup();
            }
        }
        
        private void Update()
        {
            if (GameBootstrap.IsInitialized)
            {
                gameManager?.UpdateGame();
            }
        }
    }
}
