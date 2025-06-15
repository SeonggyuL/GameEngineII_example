# PointGenerator Core 아키텍처

## 개요

PointGenerator Core는 클릭 기반 방치형 게임을 위한 확장 가능하고 테스트 가능한 아키텍처를 제공합니다. 의존성 주입, 인터페이스 기반 설계, 이벤트 기반 통신을 핵심으로 하는 모듈형 시스템입니다.

## 설계 원칙

### 1. 단일 책임 원칙 (Single Responsibility Principle)
각 클래스와 인터페이스는 하나의 명확한 책임을 가집니다:
- `IPointManager`: 포인트 시스템만 담당
- `IUpgradeManager`: 업그레이드 시스템만 담당
- `IAchievementManager`: 업적 시스템만 담당

### 2. 의존성 역전 원칙 (Dependency Inversion Principle)
구체 클래스가 아닌 인터페이스에 의존합니다:
```csharp
[Inject] private IPointManager pointManager; // ✓ 인터페이스 의존
[Inject] private PointManager pointManager;  // ✗ 구체 클래스 의존
```

### 3. 개방-폐쇄 원칙 (Open-Closed Principle)
확장에는 열려있고 수정에는 닫혀있습니다:
- 새로운 업적 타입 추가 시 기존 코드 수정 없이 확장 가능
- 새로운 프레스티지 업그레이드 타입 추가 가능

### 4. 인터페이스 분리 원칙 (Interface Segregation Principle)
클라이언트가 사용하지 않는 메서드에 의존하지 않도록 인터페이스를 분리:
- 읽기 전용 기능과 쓰기 기능을 분리
- 각 매니저별로 명확한 인터페이스 정의

## 아키텍처 레이어

```
┌─────────────────────────────────────────────────────────┐
│                    UI Layer                             │
│  (PointClickerUI, UpgradeUI, AchievementUI 등)         │
└─────────────────────┬───────────────────────────────────┘
                      │ 이벤트 구독 & 메서드 호출
┌─────────────────────▼───────────────────────────────────┐
│                Application Layer                        │
│              (GameManager)                              │
└─────────────────────┬───────────────────────────────────┘
                      │ 매니저 조율
┌─────────────────────▼───────────────────────────────────┐
│                 Service Layer                           │
│  (PointManager, UpgradeManager, AchievementManager...)  │
└─────────────────────┬───────────────────────────────────┘
                      │ 데이터 조작
┌─────────────────────▼───────────────────────────────────┐
│                  Data Layer                             │
│        (UserData, GameData, SaveData...)                │
└─────────────────────┬───────────────────────────────────┘
                      │ 영속성
┌─────────────────────▼───────────────────────────────────┐
│               Persistence Layer                         │
│            (PlayerPrefs, JSON 직렬화)                   │
└─────────────────────────────────────────────────────────┘
```

## 의존성 주입 아키텍처

### DI Container 역할
- UJ.DI 시스템을 통한 의존성 자동 주입
- 인터페이스와 구현체의 바인딩
- 객체 생명주기 관리

### 의존성 그래프
```
GameManager
├── IPointManager
├── IUpgradeManager
│   └── IPointManager
├── IAchievementManager
│   └── IPointManager
├── IPrestigeManager
│   ├── IPointManager
│   └── IUpgradeManager
├── IOfflineRewardsManager
│   ├── IPointManager
│   └── IPrestigeManager
└── ISaveLoadManager
    └── IOfflineRewardsManager
```

## 이벤트 기반 통신

### 이벤트 흐름
```
User Action (클릭)
     ↓
PointManager.PerformClick()
     ↓
OnClicked 이벤트 발생
     ↓
AchievementManager가 이벤트 수신
     ↓
업적 진행도 업데이트
     ↓
OnAchievementUnlocked 이벤트 발생 (조건 충족 시)
     ↓
UI가 업적 달성 알림 표시
```

### 이벤트 구독 패턴
```csharp
// 초기화 시 이벤트 구독
private void SubscribeToEvents()
{
    pointManager.OnPointsChanged += OnPointsUpdated;
    upgradeManager.OnUpgradePurchased += OnUpgradeBought;
}

// 정리 시 구독 해제
private void UnsubscribeFromEvents()
{
    if (pointManager != null)
        pointManager.OnPointsChanged -= OnPointsUpdated;
}
```

## 데이터 흐름

### 게임 시작 시 데이터 흐름
```
1. GameManager.StartGame()
   ↓
2. SaveLoadManager.LoadUserData()
   ↓
3. GameSaveData → UserData 복원
   ↓
4. 각 매니저에 UserData 주입
   ↓
5. 매니저별 Initialize() 호출
   ↓
6. 오프라인 보상 계산 및 적용
   ↓
7. UI 초기 상태 설정
```

### 게임 플레이 중 데이터 흐름
```
사용자 입력
   ↓
해당 매니저 메서드 호출
   ↓
UserData 상태 변경
   ↓
이벤트 발생
   ↓
다른 매니저들 상태 업데이트
   ↓
UI 갱신
   ↓
자동 저장 (주기적)
```

### 저장 시 데이터 흐름
```
자동 저장 타이머 또는 수동 저장 요청
   ↓
SaveLoadManager.SaveUserData()
   ↓
UserData → GameSaveData 래핑
   ↓
JSON 직렬화
   ↓
PlayerPrefs에 저장
```

## 상태 관리

### UserData 중심 상태 관리
모든 게임 상태는 `UserData` 객체에 중앙화되어 저장됩니다:

```csharp
public class UserData
{
    // 기본 게임 상태
    public long CurrentPoints;
    public long PointsPerClick;
    public long PointsPerSecond;
    
    // 업그레이드 상태
    public Dictionary<string, UserUpgradeProgress> UserUpgradeProgresses;
    
    // 업적 상태
    public Dictionary<string, UserAchievement> achievements;
    
    // 프레스티지 상태
    public PrestigeData prestigeData;
}
```

### 상태 동기화
- 모든 매니저가 동일한 UserData 인스턴스를 공유
- 상태 변경 시 이벤트를 통해 다른 컴포넌트에 알림
- UI는 이벤트를 구독하여 실시간 업데이트

## 확장성 설계

### 새로운 매니저 추가
1. 인터페이스 정의 (`INewManager`)
2. 구현체 작성 (`NewManager`)
3. DI 컨테이너에 바인딩 추가
4. 필요시 GameManager에 의존성 추가

### 새로운 게임 기능 추가
예: 멀티플레이어 기능
```csharp
public interface IMultiplayerManager
{
    void ConnectToServer();
    void SendScore(long score);
    event Action<PlayerData[]> OnLeaderboardUpdated;
}
```

## 성능 최적화

### 1. 이벤트 최적화
- 불필요한 이벤트 발생 방지
- 이벤트 구독자 수 제한
- 메모리 누수 방지를 위한 구독 해제

### 2. 계산 최적화
- 업그레이드 비용 계산 캐싱
- 프레스티지 효과 계산 최적화
- UI 업데이트 주기 조절

### 3. 메모리 관리
- 객체 풀링 (필요시)
- GC 압박 최소화
- 큰 데이터 구조 최적화

## 테스트 전략

### 단위 테스트
```csharp
[Test]
public void PointManager_AddPoints_ShouldUpdateCurrentPoints()
{
    // Arrange
    var userData = new UserData();
    var pointManager = new PointManager(userData);
    
    // Act
    pointManager.AddPoints(100);
    
    // Assert
    Assert.AreEqual(100, pointManager.CurrentPoints);
}
```

### 통합 테스트
```csharp
[Test]
public void UpgradeManager_PurchaseUpgrade_ShouldAffectPointGeneration()
{
    // 여러 매니저 간의 상호작용 테스트
}
```

### 목 객체 활용
```csharp
public class MockPointManager : IPointManager
{
    public long CurrentPoints { get; set; }
    // 테스트를 위한 목 구현
}
```

## 에러 처리 전략

### 1. 방어적 프로그래밍
```csharp
public bool SpendPoints(long amount)
{
    if (amount <= 0) return false;
    if (CurrentPoints < amount) return false;
    
    CurrentPoints -= amount;
    return true;
}
```

### 2. 예외 처리
```csharp
try
{
    var json = JsonUtility.ToJson(saveData);
    PlayerPrefs.SetString(SAVE_KEY, json);
}
catch (Exception e)
{
    Debug.LogError($"저장 실패: {e.Message}");
    // 대체 저장 방법 시도
}
```

### 3. 상태 복구
- 잘못된 저장 데이터 감지 시 기본값으로 복구
- 계산 오류 시 마지막 정상 상태로 롤백
- 치명적 오류 시 게임 재시작

## 보안 고려사항

### 1. 클라이언트 측 검증
- 사용자 입력 검증
- 계산 결과 유효성 검사
- 비정상적인 진행 속도 감지

### 2. 데이터 무결성
- 저장 데이터 체크섬
- 중요 데이터 암호화 (필요시)
- 백업 저장 시스템

## 향후 개선 방향

### 1. 모듈화 강화
- 기능별 어셈블리 분리
- 플러그인 시스템 도입
- 동적 로딩 지원

### 2. 성능 향상
- 비동기 처리 도입
- 멀티스레딩 활용
- 메모리 최적화

### 3. 개발자 경험 개선
- 에디터 툴 제작
- 자동화된 테스트
- 문서 자동 생성

이 아키텍처는 확장성, 유지보수성, 테스트 가능성을 중시하여 설계되었으며, 클릭 기반 방치형 게임의 모든 핵심 기능을 지원합니다. 