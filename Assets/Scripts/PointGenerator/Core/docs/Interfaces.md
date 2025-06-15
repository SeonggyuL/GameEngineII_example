# 인터페이스 명세서

## 개요

Core/Interfaces 폴더에는 시스템의 모든 핵심 인터페이스가 정의되어 있습니다. 각 인터페이스는 특정 도메인의 책임을 명확히 정의하고, 구현체와의 계약을 제공합니다.

## IGameManager.cs

**목적**: 게임 전체 상태와 라이프사이클을 관리하는 최상위 인터페이스

### 프로퍼티
| 이름 | 타입 | 설명 |
|------|------|------|
| `IsInitialized` | bool | 게임 초기화 완료 여부 |

### 메서드
| 메서드명 | 반환 타입 | 설명 |
|----------|----------|------|
| `GetGameData()` | GameData | 게임 설정 데이터 반환 |
| `StartGame()` | void | 게임 시작 및 초기화 |
| `PauseGame()` | void | 게임 일시정지 |
| `ResumeGame()` | void | 게임 재개 |
| `SaveGame()` | void | 게임 상태 저장 |
| `UpdateGame()` | void | 매 프레임 게임 업데이트 |
| `OnApplicationFocusChanged(bool)` | void | 애플리케이션 포커스 변경 처리 |
| `OnApplicationPauseChanged(bool)` | void | 애플리케이션 일시정지 변경 처리 |
| `OnApplicationQuit()` | void | 애플리케이션 종료 처리 |

### 이벤트
- `OnGameInitialized`: 게임 초기화 완료 시 발생
- `OnGameStarted`: 게임 시작 시 발생

## IPointManager.cs

**목적**: 포인트 시스템과 클릭/방치 메커니즘을 관리

### 프로퍼티
| 이름 | 타입 | 설명 |
|------|------|------|
| `CurrentPoints` | long | 현재 보유 포인트 |
| `PointsPerClick` | long | 클릭당 획득 포인트 |
| `PointsPerSecond` | long | 초당 자동 생성 포인트 |

### 메서드
| 메서드명 | 반환 타입 | 매개변수 | 설명 |
|----------|----------|----------|------|
| `AddPoints` | void | long amount | 포인트 추가 |
| `SpendPoints` | bool | long amount | 포인트 소모 (성공 여부 반환) |
| `IncreasePointsPerClick` | void | long amount | 클릭당 포인트 증가 |
| `IncreasePointsPerSecond` | void | long amount | 초당 포인트 증가 |
| `PerformClick` | void | - | 클릭 실행 |
| `StartAutoGeneration` | void | - | 자동 생성 시작 |
| `StopAutoGeneration` | void | - | 자동 생성 중지 |
| `Initialize` | void | - | 서비스 초기화 |
| `RecalculateStats` | void | - | 스탯 재계산 |

### 이벤트
- `OnPointsChanged(long)`: 포인트 변경 시 발생
- `OnPointsPerClickChanged(long)`: 클릭당 포인트 변경 시 발생
- `OnPointsPerSecondChanged(long)`: 초당 포인트 변경 시 발생
- `OnClicked()`: 클릭 실행 시 발생

## IUpgradeManager.cs

**목적**: 업그레이드 시스템과 효과 적용을 관리

### 메서드
| 메서드명 | 반환 타입 | 매개변수 | 설명 |
|----------|----------|----------|------|
| `PurchaseUpgrade` | bool | string upgradeId | 업그레이드 구매 |
| `CanPurchaseUpgrade` | bool | string upgradeId | 구매 가능 여부 확인 |
| `GetUpgradeLevel` | int | string upgradeId | 현재 레벨 조회 |
| `GetUpgradeCurrentCost` | long | string upgradeId | 현재 가격 조회 |
| `GetAllUpgradeDefinitions` | List<UpgradeDefinition> | - | 모든 업그레이드 정의 조회 |
| `GetUpgradeDefinition` | UpgradeDefinition | string upgradeId | 특정 업그레이드 정의 조회 |
| `Initialize` | void | - | 서비스 초기화 |
| `RecalculateAllUpgradeEffects` | void | - | 모든 업그레이드 효과 재계산 |
| `GetAffordableUpgrades` | List<string> | - | 구매 가능한 업그레이드 목록 |
| `GetTotalInvestedCost` | long | string upgradeId | 총 투자 비용 계산 |

### 이벤트
- `OnUpgradePurchased(string, int)`: 업그레이드 구매 성공 시 발생
- `OnUpgradePurchaseFailed(string)`: 업그레이드 구매 실패 시 발생

## IAchievementManager.cs

**목적**: 업적 시스템과 진행도 추적을 관리

### 메서드
| 메서드명 | 반환 타입 | 매개변수 | 설명 |
|----------|----------|----------|------|
| `Initialize` | void | - | 서비스 초기화 |
| `GetAllAchievements` | List<AchievementDefinition> | - | 모든 업적 정의 조회 |
| `GetUserAchievements` | List<UserAchievement> | - | 사용자 업적 데이터 조회 |
| `GetUserAchievement` | UserAchievement | string codeName | 특정 업적 데이터 조회 |
| `IsAchievementUnlocked` | bool | string codeName | 업적 달성 여부 확인 |
| `UpdateProgress` | void | AchievementType type, long value | 진행도 업데이트 |
| `UnlockAchievement` | void | string codeName | 업적 강제 달성 |
| `GetUnlockedCount` | int | - | 달성한 업적 수 |
| `GetTotalCount` | int | - | 총 업적 수 |

### 이벤트
- `OnAchievementUnlocked(AchievementDefinition)`: 업적 달성 시 발생
- `OnAchievementProgressChanged(string, long, long)`: 진행도 변경 시 발생

## IPrestigeManager.cs

**목적**: 프레스티지 시스템과 장기 진행을 관리

### 메서드
| 메서드명 | 반환 타입 | 매개변수 | 설명 |
|----------|----------|----------|------|
| `Initialize` | void | - | 서비스 초기화 |
| `CanPrestige` | bool | - | 프레스티지 실행 가능 여부 |
| `CalculatePrestigePoints` | long | - | 획득 예상 프레스티지 포인트 |
| `GetPrestigeData` | PrestigeData | - | 프레스티지 데이터 조회 |
| `GetAllPrestigeUpgradeDefinitions` | List<PrestigeUpgradeDefinition> | - | 모든 프레스티지 업그레이드 정의 |
| `PerformPrestige` | void | - | 프레스티지 실행 |
| `CanAffordPrestigeUpgrade` | bool | string codeName, long currentPrestigePoints | 프레스티지 업그레이드 구매 가능 여부 확인 |
| `PurchasePrestigeUpgrade` | bool | string codeName | 프레스티지 업그레이드 구매 |
| `CalculatePrestigeUpgradeCost` | long | string codeName, int currentLevel | 프레스티지 업그레이드 비용 계산 |
| `GetPrestigeUpgradeLevel` | int | string codeName | 프레스티지 업그레이드 레벨 조회 |
| `GetGlobalMultiplier` | double | - | 전역 배율 조회 |
| `GetOfflineBonus` | float | - | 오프라인 보너스 배율 조회 |

### 이벤트
- `OnPrestigeAvailabilityChanged(bool)`: 프레스티지 가능 여부 변경 시 발생
- `OnPrestigePerformed(long)`: 프레스티지 실행 시 발생
- `OnPrestigePointsChanged(long)`: 프레스티지 포인트 변경 시 발생

## IOfflineRewardsManager.cs

**목적**: 오프라인 보상 시스템을 관리

### 메서드
| 메서드명 | 반환 타입 | 매개변수 | 설명 |
|----------|----------|----------|------|
| `CalculateAndApplyOfflineRewards` | long | double offlineSeconds | 오프라인 보상 계산 및 적용 |
| `CalculateOfflineRewards` | long | double offlineSeconds | 오프라인 보상 미리보기 계산 |
| `SetMaxOfflineHours` | void | float maxHours | 최대 오프라인 시간 설정 |
| `SetOfflineEfficiency` | void | float efficiency | 오프라인 효율성 설정 |
| `FormatOfflineTime` | string | double seconds | 오프라인 시간을 읽기 쉬운 문자열로 포맷 |

### 이벤트
- `OnOfflineRewardsApplied(long, double)`: 오프라인 보상 적용 시 발생

## ISaveLoadManager.cs

**목적**: 게임 데이터의 저장과 로드를 관리

### 메서드
| 메서드명 | 반환 타입 | 매개변수 | 설명 |
|----------|----------|----------|------|
| `SaveUserData` | void | UserData userData | 사용자 데이터 저장 |
| `LoadUserData` | UserData | - | 사용자 데이터 로드 |
| `HasSaveData` | bool | - | 저장 데이터 존재 여부 확인 |
| `DeleteSaveData` | void | - | 저장 데이터 삭제 |
| `EnableAutoSave` | void | - | 자동 저장 활성화 |
| `DisableAutoSave` | void | - | 자동 저장 비활성화 |
| `UpdateAutoSave` | void | UserData userData | 자동 저장 업데이트 |

### 이벤트
- `OnSaveCompleted()`: 저장 완료 시 발생
- `OnLoadCompleted(UserData)`: 로드 완료 시 발생

## 인터페이스 간 의존성 관계

```
IGameManager (최상위)
├── IPointManager (포인트 시스템)
├── IUpgradeManager (업그레이드)
│   └── IPointManager (포인트 소모)
├── IAchievementManager (업적)
│   └── IPointManager (진행도 추적)
├── IPrestigeManager (프레스티지)
│   ├── IPointManager (포인트 확인)
│   └── IUpgradeManager (리셋)
├── IOfflineRewardsManager (오프라인 보상)
│   ├── IPointManager (보상 지급)
│   └── IPrestigeManager (보너스 적용)
└── ISaveLoadManager (저장/로드)
    └── IOfflineRewardsManager (오프라인 시간 계산)
```

## 사용 패턴

### 1. 의존성 주입
```csharp
public class SomeService
{
    [Inject] private IPointManager pointManager;
    [Inject] private IUpgradeManager upgradeManager;
}
```

### 2. 이벤트 구독
```csharp
pointManager.OnPointsChanged += OnPointsUpdated;
upgradeManager.OnUpgradePurchased += OnUpgradeBought;
```

### 3. 계층적 초기화
```csharp
// 1. 기본 시스템 초기화
pointManager.Initialize();
upgradeManager.Initialize();

// 2. 의존성이 있는 시스템 초기화
achievementManager.Initialize();
prestigeManager.Initialize();

// 3. 최상위 시스템 초기화
gameManager.StartGame();
```

## 확장 가이드

새로운 인터페이스를 추가할 때는 다음을 고려하세요:

1. **단일 책임 원칙**: 하나의 명확한 도메인만 담당
2. **의존성 방향**: 상위 레벨이 하위 레벨에 의존하도록 설계
3. **이벤트 설계**: 상태 변화를 외부에 알릴 필요가 있는 경우 이벤트 제공
4. **비동기 고려**: 필요시 Task 기반 비동기 메서드 제공
5. **테스트 가능성**: Mock 객체로 테스트할 수 있도록 설계 