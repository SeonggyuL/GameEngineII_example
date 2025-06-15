using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using PointGenerator.Core;

namespace PointGenerator.Bootstrap
{
    /// <summary>
    /// Addressable을 이용한 데이터 로딩 클래스
    /// </summary>
    public static class DataLoader
    {
        /// <summary>
        /// GameData ScriptableObject 로딩
        /// </summary>
        /// <returns>로딩된 GameData</returns>
        public static async Task<GameData> LoadGameData()
        {
            try
            {
                Debug.Log($"[DataLoader] GameData 로딩 시작: {GameConstants.GAME_DATA_ADDRESS}");
                
                var handle = Addressables.LoadAssetAsync<GameData>(GameConstants.GAME_DATA_ADDRESS);
                var gameData = await handle.Task;
                
                if (gameData == null)
                {
                    Debug.LogError("[DataLoader] GameData를 로드할 수 없습니다.");
                    throw new Exception("GameData를 로드할 수 없습니다.");
                }
                
                Debug.Log($"[DataLoader] GameData 로딩 완료. 업그레이드 개수: {gameData.AllUpgradeDefinitions?.Count ?? 0}");
                return gameData;
            }
            catch (Exception e)
            {
                Debug.LogError($"[DataLoader] GameData 로딩 실패: {e.Message}");
                throw;
            }
        }
        
        /// <summary>
        /// 리소스 해제
        /// </summary>
        /// <param name="gameData">해제할 GameData</param>
        public static void ReleaseGameData(GameData gameData)
        {
            if (gameData != null)
            {
                Addressables.Release(gameData);
                Debug.Log("[DataLoader] GameData 리소스 해제 완료");
            }
        }
    }
}
