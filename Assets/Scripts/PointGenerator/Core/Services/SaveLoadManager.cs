using System;
using UnityEngine;
using UJ.Attributes;
using PointGenerator.Core;

namespace PointGenerator.Core.Services
{    /// <summary>
    /// 저장/로드 관리 서비스
    /// </summary>
    public class SaveLoadManager : ISaveLoadManager
    {
        [Inject] private IOfflineRewardsManager offlineRewardsManager;
        
        // private const float AUTO_SAVE_INTERVAL = 30f; // 30초마다 자동 저장
        private bool autoSaveEnabled = true;
        private float lastSaveTime = 0f;
        
        public event Action OnSaveCompleted;
        public event Action<UserData> OnLoadCompleted;
        
        public SaveLoadManager()
        {
            // 자동 저장 활성화
            EnableAutoSave();
        }
        
        public void SaveUserData(UserData userData)
        {
            try
            {
                var saveData = new GameSaveData(userData);
                var json = JsonUtility.ToJson(saveData, true);
                
                PlayerPrefs.SetString(GameConstants.SAVE_DATA_KEY, json);
                PlayerPrefs.Save();
                
                lastSaveTime = Time.time;
                OnSaveCompleted?.Invoke();
                
                Debug.Log("[SaveLoadManager] 사용자 데이터 저장 완료");
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveLoadManager] 저장 실패: {e.Message}");
            }
        }
        
        public UserData LoadUserData()
        {
            try
            {
                if (!HasSaveData())
                {
                    Debug.Log("[SaveLoadManager] 저장 데이터가 없습니다. 새 데이터를 생성합니다.");
                    return null;
                }
                
                var json = PlayerPrefs.GetString(GameConstants.SAVE_DATA_KEY);
                var saveData = JsonUtility.FromJson<GameSaveData>(json);
                
                if (saveData?.userData != null)
                {                    Debug.Log($"[SaveLoadManager] 사용자 데이터 로드 완료. 포인트: {saveData.userData.CurrentPoints}");
                    
                    // 오프라인 시간 계산 및 보상
                    var offlineSeconds = saveData.GetOfflineSeconds();
                    if (offlineSeconds > 60) // 1분 이상 오프라인
                    {
                        Debug.Log($"[SaveLoadManager] 오프라인 시간: {offlineSeconds:F0}초");
                        
                        // 오프라인 보상 적용
                        if (offlineRewardsManager != null)
                        {
                            var offlineReward = offlineRewardsManager.CalculateAndApplyOfflineRewards(offlineSeconds);
                            if (offlineReward > 0)
                            {
                                Debug.Log($"[SaveLoadManager] 오프라인 보상 적용: {offlineReward} 포인트");
                            }
                        }
                    }
                    
                    OnLoadCompleted?.Invoke(saveData.userData);
                    return saveData.userData;
                }
                
                Debug.LogWarning("[SaveLoadManager] 저장 데이터가 유효하지 않습니다.");
                return null;
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveLoadManager] 로드 실패: {e.Message}");
                return null;
            }
        }
        
        public bool HasSaveData()
        {
            return PlayerPrefs.HasKey(GameConstants.SAVE_DATA_KEY);
        }
        
        public void DeleteSaveData()
        {
            if (HasSaveData())
            {
                PlayerPrefs.DeleteKey(GameConstants.SAVE_DATA_KEY);
                PlayerPrefs.Save();
                Debug.Log("[SaveLoadManager] 저장 데이터 삭제 완료");
            }
        }
        
        public void EnableAutoSave()
        {
            autoSaveEnabled = true;
            Debug.Log("[SaveLoadManager] 자동 저장 활성화");
        }
        
        public void DisableAutoSave()
        {
            autoSaveEnabled = false;
            Debug.Log("[SaveLoadManager] 자동 저장 비활성화");
        }
        
        /// <summary>
        /// 자동 저장 업데이트 (GameManager에서 호출)
        /// </summary>
        /// <param name="userData">저장할 사용자 데이터</param>
        public void UpdateAutoSave(UserData userData)
        {
            if (!autoSaveEnabled) return;
            
            if (Time.time - lastSaveTime >= GameConstants.AUTO_SAVE_INTERVAL)
            {
                SaveUserData(userData);
            }
        }
    }
}
