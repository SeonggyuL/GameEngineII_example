using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[Serializable]
public class UserUpgradeProgress
{
    // 업그레이드 정의와 매칭되는 고유 ID
    public string UpgradeId;


    // 플레이어가 이 업그레이드를 구매한 횟수 (레벨)
    public int CurrentLevel;


    public UserUpgradeProgress(string upgradeId, int currentLevel)
    {
        UpgradeId = upgradeId;
        CurrentLevel = currentLevel;
    }
}