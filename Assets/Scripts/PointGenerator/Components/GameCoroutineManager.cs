using System.Collections;
using UnityEngine;
using UJ.Attributes;
using PointGenerator.Core.Services;
using PointGenerator.Core;

namespace PointGenerator.Components
{
    /// <summary>
    /// 게임 코루틴 관리 MonoBehaviour 컴포넌트
    /// 자동 포인트 생성과 기타 코루틴들을 처리
    /// </summary>
    public class GameCoroutineManager : MonoBehaviour
    {
        [Inject] private IPointManager pointManager;
        [Inject] private IGameManager gameManager;
        
        private Coroutine autoGenerationCoroutine;
        private bool isAutoGenerationActive = false;
        
        [Header("Auto Generation Settings")]
        [SerializeField] private float autoGenerationInterval = 1f; // 1초마다 실행
        
        private void Start()
        {
            // DI 주입 완료 후 초기화
            StartCoroutine(WaitForInjectionAndStart());
        }
        
        private IEnumerator WaitForInjectionAndStart()
        {
            // DI 주입이 완료될 때까지 대기
            yield return new WaitUntil(() => pointManager != null && gameManager != null);
            
            // 포인트 매니저의 자동 생성 상태 변경 이벤트 구독
            if (pointManager is PointManager pm)
            {
                // 자동 생성 시작/중지를 위한 이벤트 리스너 추가 (필요시)
            }
            
            Debug.Log("[GameCoroutineManager] 초기화 완료");
        }
        
        /// <summary>
        /// 자동 포인트 생성 시작
        /// </summary>
        public void StartAutoGeneration()
        {
            if (isAutoGenerationActive || autoGenerationCoroutine != null) return;
            
            isAutoGenerationActive = true;
            autoGenerationCoroutine = StartCoroutine(AutoGenerationCoroutine());
            
            Debug.Log("[GameCoroutineManager] 자동 포인트 생성 코루틴 시작");
        }
        
        /// <summary>
        /// 자동 포인트 생성 중지
        /// </summary>
        public void StopAutoGeneration()
        {
            isAutoGenerationActive = false;
            
            if (autoGenerationCoroutine != null)
            {
                StopCoroutine(autoGenerationCoroutine);
                autoGenerationCoroutine = null;
            }
            
            Debug.Log("[GameCoroutineManager] 자동 포인트 생성 코루틴 중지");
        }
        
        /// <summary>
        /// 자동 포인트 생성 코루틴
        /// </summary>
        private IEnumerator AutoGenerationCoroutine()
        {
            while (isAutoGenerationActive)
            {
                yield return new WaitForSeconds(autoGenerationInterval);
                
                // 게임이 일시정지되지 않았고 포인트 매니저가 유효할 때만 실행
                if (gameManager?.IsInitialized == true && pointManager != null)
                {
                    if (pointManager is PointManager pm)
                    {
                        pm.ExecuteAutoGeneration();
                    }
                }
            }
        }
        
        /// <summary>
        /// 애플리케이션 포커스 변경 시 처리
        /// </summary>
        private void OnApplicationFocus(bool hasFocus)
        {
            if (gameManager != null)
            {
                gameManager.OnApplicationFocusChanged(hasFocus);
            }
        }
        
        /// <summary>
        /// 애플리케이션 일시정지 시 처리
        /// </summary>
        private void OnApplicationPause(bool pauseStatus)
        {
            if (gameManager != null)
            {
                gameManager.OnApplicationPauseChanged(pauseStatus);
            }
        }
        
        /// <summary>
        /// 애플리케이션 종료 시 처리
        /// </summary>
        private void OnApplicationQuit()
        {
            StopAutoGeneration();
            
            if (gameManager != null)
            {
                gameManager.OnApplicationQuit();
            }
        }
        
        /// <summary>
        /// 매 프레임 업데이트
        /// </summary>
        private void Update()
        {
            // 게임 매니저 업데이트 (자동 저장 등)
            if (gameManager != null)
            {
                gameManager.UpdateGame();
            }
        }
    }
}
