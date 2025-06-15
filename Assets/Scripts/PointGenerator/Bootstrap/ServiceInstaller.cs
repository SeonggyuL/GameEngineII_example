using UnityEngine;
using UJ.DI;
using PointGenerator.Core;
using PointGenerator.Core.Services;

namespace PointGenerator.Bootstrap
{
    /// <summary>
    /// 모든 서비스 등록 및 DI 설정
    /// </summary>
    public static class ServiceInstaller
    {
        /// <summary>
        /// 모든 서비스를 DI 컨테이너에 등록
        /// </summary>
        /// <param name="container">DI 컨테이너</param>
        /// <param name="gameData">로딩된 게임 데이터</param>
        public static void InstallServices(DIContainer container, GameData gameData)
        {
            Debug.Log("[ServiceInstaller] 서비스 등록 시작");
            
            // GameData 등록
            container.Regist(gameData);
            Debug.Log("[ServiceInstaller] GameData 등록 완료");
            
            // UserData 초기화 및 등록
            var saveLoadManager = new SaveLoadManager();
            container.Regist<ISaveLoadManager>(saveLoadManager);
            
            var userData = saveLoadManager.LoadUserData() ?? new UserData();
            container.Regist(userData);
            Debug.Log("[ServiceInstaller] UserData 등록 완료");
              // 핵심 서비스들 등록 (의존성 순서대로)
            var pointManager = new PointManager();
            container.InjectAndRegist<IPointManager>(pointManager);
            Debug.Log("[ServiceInstaller] PointManager 등록 완료");
            
            var upgradeManager = new UpgradeManager();
            container.InjectAndRegist<IUpgradeManager>(upgradeManager);
            Debug.Log("[ServiceInstaller] UpgradeManager 등록 완료");
            
            var achievementManager = new AchievementManager();
            container.InjectAndRegist<IAchievementManager>(achievementManager);
            Debug.Log("[ServiceInstaller] AchievementManager 등록 완료");
              var prestigeManager = new PrestigeManager();
            container.InjectAndRegist<IPrestigeManager>(prestigeManager);
            Debug.Log("[ServiceInstaller] PrestigeManager 등록 완료");
            
            var offlineRewardsManager = new OfflineRewardsManager();
            container.InjectAndRegist<IOfflineRewardsManager>(offlineRewardsManager);
            Debug.Log("[ServiceInstaller] OfflineRewardsManager 등록 완료");
            
            var gameManager = new GameManager();
            container.InjectAndRegist<IGameManager>(gameManager);
            Debug.Log("[ServiceInstaller] GameManager 등록 완료");
            
            Debug.Log("[ServiceInstaller] 모든 서비스 등록 완료");
        }
        
        /// <summary>
        /// 서비스 해제
        /// </summary>
        /// <param name="container">DI 컨테이너</param>
        public static void UninstallServices(DIContainer container)
        {
            Debug.Log("[ServiceInstaller] 서비스 해제 시작");
            
            // 필요시 리소스 정리 로직 추가
            
            Debug.Log("[ServiceInstaller] 서비스 해제 완료");
        }
    }
}
