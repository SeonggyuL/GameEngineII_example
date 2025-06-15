using System;

namespace PointGenerator.Core
{
    /// <summary>
    /// 저장/로드 관리 인터페이스
    /// </summary>
    public interface ISaveLoadManager
    {
        /// <summary>
        /// 사용자 데이터 저장
        /// </summary>
        /// <param name="userData">저장할 사용자 데이터</param>
        void SaveUserData(UserData userData);
        
        /// <summary>
        /// 사용자 데이터 로드
        /// </summary>
        /// <returns>로드된 사용자 데이터 (없으면 null)</returns>
        UserData LoadUserData();
        
        /// <summary>
        /// 저장 데이터 존재 여부 확인
        /// </summary>
        /// <returns>저장 데이터 존재 여부</returns>
        bool HasSaveData();
        
        /// <summary>
        /// 저장 데이터 삭제
        /// </summary>
        void DeleteSaveData();
        
        /// <summary>
        /// 자동 저장 활성화
        /// </summary>
        void EnableAutoSave();
        
        /// <summary>
        /// 자동 저장 비활성화
        /// </summary>
        void DisableAutoSave();
        
        /// <summary>
        /// 저장 완료 이벤트
        /// </summary>
        event Action OnSaveCompleted;
        
        /// <summary>
        /// 로드 완료 이벤트
        /// </summary>
        event Action<UserData> OnLoadCompleted;

        /// <summary>
        /// 자동 저장 업데이트 (주기적으로 호출되어야 함)
        /// </summary>
        /// <param name="userData">저장할 사용자 데이터</param>
        void UpdateAutoSave(UserData userData);
    }
}
