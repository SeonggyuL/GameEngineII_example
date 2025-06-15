# 데이터 구조 명세서

## 개요

Core/Data 폴더에는 게임의 주요 데이터 구조들이 정의되어 있습니다. 모든 데이터 클래스는 `[Serializable]` 어트리뷰트를 가지며 JSON 직렬화를 지원합니다.

## AchievementData.cs

### AchievementDefinition 클래스

**목적**: 업적의 정의와 보상 정보를 담는 클래스

| 필드명 | 타입 | 설명 |
|--------|------|------|
| `codeName` | string | 업적의 고유 식별자 |
| `displayName` | string | 화면에 표시되는 업적 이름 |
| `description` | string | 업적 설명 텍스트 |
| `type` | AchievementType | 업적 타입 (달성 조건 분류) |
| `targetValue` | long | 목표 달성값 |
| `rewardPoints` | long | 보상 포인트 |
| `rewardClickBonus` | long | 클릭 보너스 보상 |
| `rewardIdleBonus` | long | 방치 보너스 보상 |
| `isHidden` | bool | 숨겨진 업적 여부 |

**생성자**:
```csharp
AchievementDefinition() // 기본 생성자
AchievementDefinition(string code, string name, string desc, AchievementType achievementType, 
    long target, long pointReward = 0, long clickReward = 0, long idleReward = 0, bool hidden = false)
```

### AchievementType 열거형

**목적**: 업적의 달성 조건을 분류

| 값 | 설명 |
|----|------|
| `TotalPoints` | 총 포인트 획득량 |
| `TotalClicks` | 총 클릭 수 |
| `UpgradeCount` | 업그레이드 구매 횟수 |
| `TimeSpent` | 플레이 시간 |
| `PointsPerSecond` | 초당 포인트 생산량 |
| `PointsPerClick` | 클릭당 포인트 획득량 |
| `ReachLevel` | 특정 레벨 도달 |
| `MaxUpgradeLevel` | 최대 업그레이드 레벨 |

### UserAchievement 클래스

**목적**: 사용자의 업적 달성 현황을 저장

| 필드명 | 타입 | 설명 |
|--------|------|------|
| `codeName` | string | 업적 식별자 |
| `isUnlocked` | bool | 달성 여부 |
| `currentProgress` | long | 현재 진행도 |
| `unlockedDate` | DateTime | 달성 시간 |

**생성자**:
```csharp
UserAchievement() // 기본 생성자
UserAchievement(string code) // 코드명으로 초기화
```

## PrestigeData.cs

### PrestigeData 클래스

**목적**: 프레스티지 시스템의 모든 데이터를 관리

| 필드명 | 타입 | 설명 |
|--------|------|------|
| `prestigeLevel` | long | 현재 프레스티지 레벨 |
| `totalPrestigePoints` | long | 총 획득한 프레스티지 포인트 |
| `currentPrestigePoints` | long | 현재 보유 프레스티지 포인트 |
| `globalMultiplier` | double | 전역 포인트 배율 |
| `lastPrestigeTime` | DateTime | 마지막 프레스티지 실행 시간 |
| `prestigeUpgradeLevels` | Dictionary<string, int> | 프레스티지 업그레이드 레벨들 |

**생성자**:
```csharp
PrestigeData() // 모든 값을 기본값으로 초기화
```

### PrestigeUpgradeDefinition 클래스

**목적**: 프레스티지 업그레이드의 정의

| 필드명 | 타입 | 설명 |
|--------|------|------|
| `codeName` | string | 업그레이드 고유 식별자 |
| `displayName` | string | 화면 표시명 |
| `description` | string | 업그레이드 설명 |
| `baseCost` | long | 기본 비용 |
| `costMultiplier` | double | 비용 증가 배율 |
| `baseEffectValue` | double | 기본 효과값 |
| `effectIncrementPerLevel` | double | 레벨당 효과 증가값 |
| `upgradeType` | PrestigeUpgradeType | 업그레이드 타입 |
| `maxLevel` | int | 최대 레벨 |

**읽기 전용 프로퍼티**:
- `id` → `codeName`
- `name` → `displayName`

**효과값 계산**:
프레스티지 업그레이드의 실제 효과값은 다음 공식으로 계산됩니다:
```csharp
double effectValue = baseEffectValue + (effectIncrementPerLevel * (level - 1));
double totalEffect = effectValue * level;
```

### PrestigeUpgradeType 열거형

**목적**: 프레스티지 업그레이드의 효과 타입 분류

| 값 | 설명 |
|----|------|
| `GlobalMultiplier` | 전역 포인트 배율 증가 |
| `ClickMultiplier` | 클릭 포인트 배율 증가 |
| `IdleMultiplier` | 방치 포인트 배율 증가 |
| `UpgradeDiscount` | 업그레이드 비용 할인 |
| `OfflineBonus` | 오프라인 보너스 증가 |
| `AutoClicker` | 자동 클릭 기능 |

## GameSaveData.cs

### GameSaveData 클래스

**목적**: 게임 저장 데이터의 래퍼 클래스, 저장 시간 정보 포함

| 필드명 | 타입 | 설명 |
|--------|------|------|
| `userData` | UserData | 사용자 게임 데이터 |
| `saveTimeString` | string | 저장 시간 (ISO 8601 형식) |
| `saveTimeTicks` | long | 저장 시간 (Ticks, 백업용) |

**생성자**:
```csharp
GameSaveData() // 현재 시간으로 초기화
GameSaveData(UserData userData) // UserData와 현재 시간으로 초기화
```

**주요 메서드**:

#### GetSaveTime()
```csharp
public DateTime GetSaveTime()
```
저장 시간을 DateTime으로 반환합니다. 문자열 파싱을 우선 시도하고, 실패 시 Ticks 값을 사용합니다.

#### GetOfflineSeconds()
```csharp
public double GetOfflineSeconds()
```
저장 시간과 현재 시간의 차이를 초 단위로 반환합니다. 오프라인 보상 계산에 사용됩니다.

## UpgradeDefinition (외부 참조)

**위치**: `Assets/Scripts/Data/GameData/UpgradeDefinition.cs`

**목적**: 일반 업그레이드의 정의와 효과 정보를 담는 클래스

**주요 필드**:
- `code`: int - 업그레이드 고유 ID
- `codeName`: string - 업그레이드 고유 식별자
- `Name`: Str - 다국어 지원 업그레이드 이름
- `Description`: Str - 다국어 지원 업그레이드 설명
- `BaseCost`: long - 기본 구매 비용
- `CostMultiplier`: double - 레벨당 비용 증가 배율
- `PointsPerClickEffect`: long - 클릭당 포인트 증가 효과
- `PointsPerSecondEffect`: long - 초당 포인트 증가 효과
- `maxLevel`: int - 최대 구매 가능 레벨 (0이면 무제한)

**주요 메서드**:
- `CalculateCostForLevel(int currentLevel)`: 특정 레벨에서의 구매 비용 계산

## 데이터 흐름

```
게임 시작 → SaveLoadManager → GameSaveData → UserData → 각 매니저들
    ↓
게임 플레이 → 각 매니저들이 UserData 업데이트
    ↓
자동 저장 → SaveLoadManager → GameSaveData → PlayerPrefs
```

## 직렬화 특징

1. **JSON 호환성**: 모든 클래스는 `JsonUtility`로 직렬화 가능
2. **DateTime 처리**: 문자열과 Ticks 두 가지 방식으로 저장하여 안정성 확보
3. **Dictionary 지원**: `prestigeUpgradeLevels`는 Unity의 JSON 시스템 한계로 별도 처리 필요
4. **버전 관리**: 필드 추가 시 기존 저장 데이터와의 호환성 고려

## 사용 예시

```csharp
// 업적 정의 생성
var achievement = new AchievementDefinition(
    "first_click", "첫 클릭", "첫 번째 클릭을 하세요",
    AchievementType.TotalClicks, 1, 10, 1, 0
);

// 사용자 업적 진행도 업데이트
var userAchievement = new UserAchievement("first_click");
userAchievement.currentProgress = 1;
userAchievement.isUnlocked = true;

// 프레스티지 데이터 초기화
var prestigeData = new PrestigeData();
prestigeData.prestigeUpgradeLevels["global_multiplier"] = 5;

// 게임 저장
var saveData = new GameSaveData(userData);
var json = JsonUtility.ToJson(saveData);
```

## 확장 가이드

새로운 데이터 구조를 추가할 때는 다음을 고려하세요:

1. `[Serializable]` 어트리뷰트 추가
2. 기본 생성자 제공
3. JSON 직렬화 가능한 타입만 사용
4. 필요시 버전 정보 포함
5. 기존 저장 데이터와의 호환성 유지 