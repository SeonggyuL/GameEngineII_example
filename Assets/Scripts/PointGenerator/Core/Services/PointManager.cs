using System;
using System.Collections;
using UnityEngine;
using UJ.Attributes;
using UJ.DI;
using PointGenerator.Core;

namespace PointGenerator.Core.Services
{
    /// <summary>
    /// 포인트 관리 서비스
    /// </summary>
    public class PointManager : IPointManager
    {
        [Inject] private UserData userData;
        [Inject] private GameData gameData;
        
        private Coroutine autoGenerationCoroutine;
        private bool isAutoGenerationActive = false;
        
        public long CurrentPoints => userData?.CurrentPoints ?? 0;
        public long PointsPerClick => userData?.PointsPerClick ?? GameConstants.INITIAL_POINTS_PER_CLICK;
        public long PointsPerSecond => userData?.PointsPerSecond ?? GameConstants.INITIAL_POINTS_PER_SECOND;
        
        public event Action<long> OnPointsChanged;
        public event Action<long> OnPointsPerClickChanged;
        public event Action<long> OnPointsPerSecondChanged;
        public event Action OnClicked;
        
        public PointManager()
        {
            // DI 주입 후 초기화될 것임
        }
        
        /// <summary>
        /// 서비스 초기화 (DI 주입 후 호출)
        /// </summary>
        public void Initialize()
        {
            if (userData == null)
            {
                Debug.LogError("[PointManager] UserData가 주입되지 않았습니다.");
                return;
            }
            
            Debug.Log($"[PointManager] 초기화 완료. 현재 포인트: {CurrentPoints}");
            
            // 이벤트 발생으로 UI 초기화
            OnPointsChanged?.Invoke(CurrentPoints);
            OnPointsPerClickChanged?.Invoke(PointsPerClick);
            OnPointsPerSecondChanged?.Invoke(PointsPerSecond);
            
            // 자동 생성 시작 (초당 포인트가 있다면)
            if (PointsPerSecond > 0)
            {
                StartAutoGeneration();
            }
        }
        
        public void AddPoints(long amount)
        {
            if (userData == null || amount <= 0) return;
            
            userData.CurrentPoints += amount;
            OnPointsChanged?.Invoke(userData.CurrentPoints);
            
            Debug.Log($"[PointManager] 포인트 추가: +{amount}, 현재: {userData.CurrentPoints}");
        }
        
        public bool SpendPoints(long amount)
        {
            if (userData == null || amount <= 0 || userData.CurrentPoints < amount)
            {
                Debug.Log($"[PointManager] 포인트 부족. 필요: {amount}, 보유: {userData.CurrentPoints}");
                return false;
            }
            
            userData.CurrentPoints -= amount;
            OnPointsChanged?.Invoke(userData.CurrentPoints);
            
            Debug.Log($"[PointManager] 포인트 소모: -{amount}, 현재: {userData.CurrentPoints}");
            return true;
        }
        
        public void IncreasePointsPerClick(long amount)
        {
            if (userData == null || amount <= 0) return;
            
            userData.PointsPerClick += amount;
            OnPointsPerClickChanged?.Invoke(userData.PointsPerClick);
            
            Debug.Log($"[PointManager] 클릭당 포인트 증가: +{amount}, 현재: {userData.PointsPerClick}");
        }
        
        public void IncreasePointsPerSecond(long amount)
        {
            if (userData == null || amount <= 0) return;
            
            userData.PointsPerSecond += amount;
            OnPointsPerSecondChanged?.Invoke(userData.PointsPerSecond);
            
            Debug.Log($"[PointManager] 초당 포인트 증가: +{amount}, 현재: {userData.PointsPerSecond}");
            
            // 자동 생성이 꺼져있다면 켜기
            if (!isAutoGenerationActive && userData.PointsPerSecond > 0)
            {
                StartAutoGeneration();
            }
        }
          public void PerformClick()
        {
            if (userData == null) return;
            
            AddPoints(userData.PointsPerClick);
            userData.totalClicks++;
            OnClicked?.Invoke();
            
            Debug.Log($"[PointManager] 클릭 수행: +{userData.PointsPerClick}, 총 클릭: {userData.totalClicks}");
        }
          public void StartAutoGeneration()
        {
            if (isAutoGenerationActive || userData?.PointsPerSecond <= 0) return;
            
            isAutoGenerationActive = true;
            
            // GameCoroutineManager에게 자동 생성 시작 요청
            var coroutineManager = UnityEngine.Object.FindObjectOfType<PointGenerator.Components.GameCoroutineManager>();
            if (coroutineManager != null)
            {
                coroutineManager.StartAutoGeneration();
            }
            
            Debug.Log("[PointManager] 자동 포인트 생성 시작 요청");
        }
          public void StopAutoGeneration()
        {
            isAutoGenerationActive = false;
            
            // GameCoroutineManager에게 자동 생성 중지 요청
            var coroutineManager = UnityEngine.Object.FindObjectOfType<PointGenerator.Components.GameCoroutineManager>();
            if (coroutineManager != null)
            {
                coroutineManager.StopAutoGeneration();
            }
            
            Debug.Log("[PointManager] 자동 포인트 생성 중지");
        }
        
        /// <summary>
        /// 자동 포인트 생성 실행 (외부에서 호출)
        /// </summary>
        public void ExecuteAutoGeneration()
        {
            if (!isAutoGenerationActive || userData?.PointsPerSecond <= 0) return;
            
            AddPoints(userData.PointsPerSecond);
        }
        
        /// <summary>
        /// 업그레이드 효과 재계산
        /// </summary>
        public void RecalculateStats()
        {
            if (userData == null || gameData?.AllUpgradeDefinitions == null) return;
            
            // 기본값으로 리셋
            userData.PointsPerClick = GameConstants.INITIAL_POINTS_PER_CLICK;
            userData.PointsPerSecond = GameConstants.INITIAL_POINTS_PER_SECOND;
            
            // 모든 업그레이드 효과 적용
            foreach (var upgradeDefinition in gameData.AllUpgradeDefinitions)
            {
                var level = userData.GetUpgradeLevel(upgradeDefinition.codeName);
                if (level > 0)
                {
                    userData.PointsPerClick += upgradeDefinition.PointsPerClickEffect * level;
                    userData.PointsPerSecond += upgradeDefinition.PointsPerSecondEffect * level;
                }
            }
            
            // 이벤트 발생
            OnPointsPerClickChanged?.Invoke(userData.PointsPerClick);
            OnPointsPerSecondChanged?.Invoke(userData.PointsPerSecond);
            
            Debug.Log($"[PointManager] 스탯 재계산 완료. PPC: {userData.PointsPerClick}, PPS: {userData.PointsPerSecond}");
        }
    }
}
