using System;
using System.Collections.Generic;
using UJ.Attributes;
using UnityEngine;

[FromXlsx]
[Serializable]
public class GameData :ScriptableObject
{
    // 모든 게임 업그레이드 정의 목록
    public List<UpgradeDefinition> AllUpgradeDefinitions;
    
    // UI에서 사용하는 프로퍼티 (배열 형태)
    public UpgradeDefinition[] upgradeDefinitions => AllUpgradeDefinitions?.ToArray();
}