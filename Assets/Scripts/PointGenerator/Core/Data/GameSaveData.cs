using System;
using UnityEngine;

namespace PointGenerator.Core
{
    /// <summary>
    /// 게임 저장 데이터
    /// </summary>
    [Serializable]
    public class GameSaveData
    {
        public UserData userData;
        public string saveTimeString; // DateTime을 JSON으로 저장하기 위한 문자열
        public long saveTimeTicks; // DateTime의 Ticks 값 (백업용)
        
        /// <summary>
        /// 기본 생성자
        /// </summary>
        public GameSaveData()
        {
            userData = null;
            saveTimeString = DateTime.Now.ToString("O"); // ISO 8601 형식
            saveTimeTicks = DateTime.Now.Ticks;
        }
        
        /// <summary>
        /// UserData를 포함한 생성자
        /// </summary>
        /// <param name="userData">저장할 사용자 데이터</param>
        public GameSaveData(UserData userData)
        {
            this.userData = userData;
            var now = DateTime.Now;
            saveTimeString = now.ToString("O"); // ISO 8601 형식
            saveTimeTicks = now.Ticks;
        }
        
        /// <summary>
        /// 저장 시간 가져오기
        /// </summary>
        public DateTime GetSaveTime()
        {
            try
            {
                // 먼저 문자열에서 파싱 시도
                if (!string.IsNullOrEmpty(saveTimeString))
                {
                    return DateTime.Parse(saveTimeString);
                }
                
                // 문자열 파싱 실패 시 Ticks 값 사용
                if (saveTimeTicks > 0)
                {
                    return new DateTime(saveTimeTicks);
                }
                
                // 둘 다 실패하면 현재 시간 반환
                return DateTime.Now;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[GameSaveData] 저장 시간 파싱 실패: {e.Message}");
                return DateTime.Now;
            }
        }
        
        /// <summary>
        /// 오프라인 시간 계산 (초 단위)
        /// </summary>
        public double GetOfflineSeconds()
        {
            var saveTime = GetSaveTime();
            var currentTime = DateTime.Now;
            
            var offlineTime = currentTime - saveTime;
            return Math.Max(0, offlineTime.TotalSeconds);
        }
    }
} 