using System;
using UnityEngine; // [Serializable] 때문이 아니라, 혹시 나중에 MonoBehaviour나 ScriptableObject 상속을 염두에 둔다면 필요. 지금은 System.Serializable로 충분.
using UJ.Language;

/// <summary>
/// 각 업그레이드의 고정된 정의 (정적 데이터, 엑셀 테이블 등에서 불러옴)
/// </summary>
[Serializable]
public class UpgradeDefinition
{
    // 각 업그레이드의 고유 ID. 사용자 진행 상황과 매칭시키기 위해 필요합니다.
    public int code;
    public string codeName;



    // 업그레이드 이름 (예: "강화된 손가락")
    public Str Name;


    // 업그레이드 효과 설명 (예: "클릭당 +1 포인트")
    public Str Description;


    // 초기 구매 가격 (클리커 게임 특성상 숫자가 커질 수 있으므로 long 사용)
    public long BaseCost;


    // 가격 상승률 (예: 1.1)
    public double CostMultiplier;


    // 이 업그레이드가 증가시키는 클릭당 포인트 (0이면 영향 없음)
    public long PointsPerClickEffect;


    // 이 업그레이드가 증가시키는 초당 포인트 (0이면 영향 없음)
    public long PointsPerSecondEffect;

    // 최대 구매 가능 레벨 (0이면 무제한)
    public int maxLevel;

     


    /// <summary>
    /// 특정 레벨에서의 이 업그레이드의 현재 구매 가격을 계산합니다.
    /// (이 메서드는 정의 자체에 속하는 계산 로직이므로 여기에 포함)
    /// </summary>
    /// <param name="currentLevel">현재 업그레이드의 레벨 (구매 횟수)</param>
    /// <returns>다음 레벨 구매에 필요한 비용</returns>
    public long CalculateCostForLevel(int currentLevel)
    {
        if (currentLevel < 0) return BaseCost; // 에러 방지 또는 첫 구매 가격
        return (long)Math.Round(BaseCost * Math.Pow(CostMultiplier, currentLevel));
    }
}