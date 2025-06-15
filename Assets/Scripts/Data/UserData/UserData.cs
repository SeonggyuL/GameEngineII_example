using System;
using System.Collections.Generic;
using PointGenerator.Core;


/// <summary>
/// 플레이어의 현재 진행 상황을 저장하는 클래스 (세이브/로드 대상)
/// </summary>
[Serializable]
public class UserData
{
    // 현재 보유 포인트 (클리커 게임 특성상 매우 커질 수 있으므로 long 사용)
    public long CurrentPoints;


    // 클릭 한 번당 획득하는 포인트
    public long PointsPerClick;


    // 초당 자동으로 획득하는 포인트
    public long PointsPerSecond;
    
    
    // 총 클릭 횟수 (업적용)
    public long totalClicks;
    
    
    // 총 업그레이드 구매 횟수 (업적용)
    public long totalUpgradesPurchased;
    
    
    // 플레이 시작 시간 (업적용)
    public DateTime gameStartTime;
    
    
    // 총 플레이 시간 (초 단위)
    public double totalPlayTimeSeconds;


    // 플레이어가 구매한 각 업그레이드의 현재 레벨 목록
    // 각 UserUpgradeProgress는 UpgradeDefinition의 Id와 연결됩니다.
    public List<UserUpgradeProgress> UserUpgradeProgresses;
    
      // 업적 데이터
    public Dictionary<string, UserAchievement> achievements;
    
    
    // 프레스티지 데이터
    public PrestigeData prestigeData;
    
    
    public UserData()
    {
        // 초기 상태 설정
        CurrentPoints = 0;
        PointsPerClick = 1; // 기획서 초기 상태에 따라
        PointsPerSecond = 0;
        totalClicks = 0;
        totalUpgradesPurchased = 0;
        gameStartTime = DateTime.Now;        totalPlayTimeSeconds = 0;
        UserUpgradeProgresses = new List<UserUpgradeProgress>();
        achievements = new Dictionary<string, UserAchievement>();
        prestigeData = new PrestigeData();
    }


    /// <summary>
    /// 특정 업그레이드의 현재 레벨을 가져옵니다. 없으면 0을 반환합니다.
    /// </summary>
    /// <param name="upgradeId">업그레이드 ID</param>
    /// <returns>해당 업그레이드의 현재 레벨</returns>
    public int GetUpgradeLevel(string upgradeId)
    {
        foreach (var progress in UserUpgradeProgresses)
        {
            if (progress.UpgradeId == upgradeId)
            {
                return progress.CurrentLevel;
            }
        }
        return 0; // 아직 구매하지 않은 업그레이드
    }


    /// <summary>
    /// 특정 업그레이드의 레벨을 업데이트하거나 새로 추가합니다.
    /// </summary>
    /// <param name="upgradeId">업그레이드 ID</param>
    /// <param name="newLevel">설정할 새 레벨</param>
    public void SetUpgradeLevel(string upgradeId, int newLevel)
    {
        UserUpgradeProgress progressToUpdate = null;
        foreach (var progress in UserUpgradeProgresses)
        {
            if (progress.UpgradeId == upgradeId)
            {
                progressToUpdate = progress;
                break;
            }
        }


        if (progressToUpdate != null)
        {
            progressToUpdate.CurrentLevel = newLevel;
        }
        else
        {
            UserUpgradeProgresses.Add(new UserUpgradeProgress(upgradeId, newLevel));
        }
    }
}