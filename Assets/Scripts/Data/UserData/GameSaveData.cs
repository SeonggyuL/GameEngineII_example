using System;

/// <summary>
/// 저장/로드용 데이터 래퍼 클래스
/// </summary>
[Serializable]
public class GameSaveData
{
    /// <summary>
    /// 사용자 데이터
    /// </summary>
    public UserData userData;
    
    /// <summary>
    /// 저장 시간
    /// </summary>
    public string saveTime;
    
    /// <summary>
    /// 게임 버전
    /// </summary>
    public string gameVersion;
    
    /// <summary>
    /// 오프라인 시간 계산을 위한 마지막 플레이 시간
    /// </summary>
    public string lastPlayTime;
    
    public GameSaveData()
    {
        userData = new UserData();
        saveTime = DateTime.Now.ToBinary().ToString();
        gameVersion = "1.0.0";
        lastPlayTime = DateTime.Now.ToBinary().ToString();
    }
    
    public GameSaveData(UserData userData)
    {
        this.userData = userData;
        saveTime = DateTime.Now.ToBinary().ToString();
        gameVersion = "1.0.0";
        lastPlayTime = DateTime.Now.ToBinary().ToString();
    }
    
    /// <summary>
    /// 저장 시간을 DateTime으로 변환
    /// </summary>
    public DateTime GetSaveTime()
    {
        if (long.TryParse(saveTime, out long binary))
        {
            return DateTime.FromBinary(binary);
        }
        return DateTime.Now;
    }
    
    /// <summary>
    /// 마지막 플레이 시간을 DateTime으로 변환
    /// </summary>
    public DateTime GetLastPlayTime()
    {
        if (long.TryParse(lastPlayTime, out long binary))
        {
            return DateTime.FromBinary(binary);
        }
        return DateTime.Now;
    }
    
    /// <summary>
    /// 오프라인 시간 계산 (초 단위)
    /// </summary>
    public double GetOfflineSeconds()
    {
        var lastPlay = GetLastPlayTime();
        var now = DateTime.Now;
        return (now - lastPlay).TotalSeconds;
    }
}
