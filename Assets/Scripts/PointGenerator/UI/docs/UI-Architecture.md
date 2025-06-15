# PointGenerator UI 아키텍처

## UI 시스템 개요

PointGenerator UI는 **MVP(Model-View-Presenter)** 패턴을 기반으로 한 계층형 아키텍처입니다. Core 시스템을 Model로, UI 컴포넌트를 View로, 그리고 각 Manager들을 Presenter로 활용합니다.

## 아키텍처 다이어그램

```
┌─────────────────────────────────────────────────────────┐
│                     UI Layer                            │
│  ┌─────────────┐ ┌─────────────┐ ┌─────────────┐       │
│  │ PointDisplayUI│ │ClickButtonUI│ │UpgradeShopUI│       │
│  └─────────────┘ └─────────────┘ └─────────────┘       │
│          │               │               │             │
│          └───────────────┼───────────────┘             │
│                          │                             │
│  ┌─────────────────────────────────────────────────────┐ │
│  │         PointClickerUI (Main Controller)           │ │
│  └─────────────────────────────────────────────────────┘ │
└─────────────────────┬───────────────────────────────────┘
                      │ DI + Events
┌─────────────────────▼───────────────────────────────────┐
│                Core Services                            │
│  ┌─────────────┐ ┌─────────────┐ ┌─────────────┐       │
│  │PointManager │ │UpgradeManager│ │AchievementMgr│      │
│  └─────────────┘ └─────────────┘ └─────────────┘       │
└─────────────────────────────────────────────────────────┘
```

## UI 컴포넌트 분류

### 1. 디스플레이 컴포넌트 (Display Components)
실시간으로 게임 상태를 표시하는 컴포넌트들:

- **PointDisplayUI**: 포인트 정보 표시
- **ClickButtonUI**: 클릭 버튼 및 클릭당 포인트 표시

```csharp
// 실시간 업데이트 패턴
private void Update()
{
    if (IsInitialized)
        UpdateDisplay();
}
```

### 2. 인터랙션 컴포넌트 (Interaction Components)
사용자 입력을 받아 Core 시스템과 상호작용하는 컴포넌트들:

- **UpgradeShopUI**: 업그레이드 구매
- **UpgradeItemUI**: 개별 업그레이드 아이템

```csharp
// 사용자 액션 처리 패턴
private void OnPurchaseClicked()
{
    upgradeManager.PurchaseUpgrade(upgradeId);
}
```

### 3. 패널 컴포넌트 (Panel Components)
전체 화면 또는 큰 영역을 차지하는 UI 패널들:

- **AchievementPanelUI**: 업적 관리 패널
- **PrestigePanelUI**: 프레스티지 관리 패널

```csharp
// 패널 표시/숨김 패턴
public void OpenPanel()
{
    gameObject.SetActive(true);
    RefreshUI();
}
```

### 4. 팝업 컴포넌트 (Popup Components)
특정 상황에서 나타나는 임시 UI들:

- **OfflineRewardsPopup**: 오프라인 보상 팝업

```csharp
// 팝업 애니메이션 패턴
public void ShowOfflineRewards(double seconds, long reward)
{
    StartCoroutine(PopupAppearAnimation());
}
```

## UI 통신 아키텍처

### 1. 의존성 주입 (Dependency Injection)
```csharp
public class PointDisplayUI : DIMono
{
    [Inject] private IPointManager pointManager;
    
    public override void Init()
    {
        // 자동 주입 완료 후 초기화
    }
}
```

### 2. 이벤트 기반 통신 (Event-Driven Communication)
```csharp
// 서비스에서 이벤트 발생
pointManager.OnPointsChanged += OnPointsUpdated;

// UI에서 이벤트 수신
private void OnPointsUpdated(long newPoints)
{
    UpdatePointsDisplay(newPoints);
}
```

### 3. 직접 메서드 호출 (Direct Method Call)
```csharp
// 사용자 액션 → 서비스 메서드 직접 호출
private void OnClickButtonPressed()
{
    pointManager.PerformClick();
}
```

## UI 생명주기 관리

### 1. 초기화 (Initialization)
```
GameBootstrap.Initialize()
     ↓
DI Container 설정
     ↓
UI 컴포넌트 생성
     ↓
DIMono.CheckInjectAndInit()
     ↓
UI.Init() 호출
     ↓
이벤트 구독
```

### 2. 업데이트 (Update)
```
Unity Update Loop
     ↓
UI 컴포넌트의 Update()
     ↓
실시간 데이터 갱신
     ↓
UI 요소 업데이트
```

### 3. 종료 (Cleanup)
```
OnDestroy() 호출
     ↓
이벤트 구독 해제
     ↓
버튼 리스너 제거
     ↓
메모리 정리
```

## UI 상태 관리

### 1. 로컬 상태 (Local State)
각 UI 컴포넌트가 자체적으로 관리하는 상태:
- 애니메이션 진행 상태
- UI 요소의 가시성
- 임시 표시 데이터

### 2. 공유 상태 (Shared State)
Core 시스템에서 관리되는 전역 상태:
- 현재 포인트
- 업그레이드 레벨
- 업적 진행도

### 3. 상태 동기화 패턴
```csharp
// 이벤트 기반 상태 동기화
private void SubscribeToEvents()
{
    pointManager.OnPointsChanged += SyncPointsDisplay;
    upgradeManager.OnUpgradePurchased += SyncUpgradeDisplay;
}

// 수동 상태 동기화
public void RefreshUI()
{
    var currentPoints = pointManager.CurrentPoints;
    UpdatePointsDisplay(currentPoints);
}
```

## UI 성능 최적화

### 1. 업데이트 최적화
```csharp
// 조건부 업데이트
private void Update()
{
    if (hasDataChanged)
    {
        UpdateDisplay();
        hasDataChanged = false;
    }
}
```

### 2. UI 풀링 (Object Pooling)
```csharp
// 대량의 UI 아이템이 필요한 경우
public class UIItemPool : MonoBehaviour
{
    private Queue<UpgradeItemUI> pool = new Queue<UpgradeItemUI>();
    
    public UpgradeItemUI GetItem()
    {
        return pool.Count > 0 ? pool.Dequeue() : CreateNewItem();
    }
}
```

### 3. 레이지 로딩 (Lazy Loading)
```csharp
// 필요할 때만 UI 생성
private void CreateUpgradeItems()
{
    if (upgradeItems.Count == 0)
    {
        // 업그레이드 아이템들 생성
        InitializeUpgradeItems();
    }
}
```

## UI 테스트 전략

### 1. 단위 테스트 (Unit Testing)
```csharp
[Test]
public void PointDisplayUI_UpdatePoints_ShouldShowCorrectFormat()
{
    // Arrange
    var pointDisplay = CreatePointDisplayUI();
    
    // Act
    pointDisplay.UpdatePointsDisplay(1500);
    
    // Assert
    Assert.AreEqual("1.5K", pointDisplay.GetDisplayText());
}
```

### 2. 통합 테스트 (Integration Testing)
```csharp
[Test]
public void UpgradeShopUI_PurchaseUpgrade_ShouldUpdateDisplay()
{
    // UI와 서비스 간의 상호작용 테스트
}
```

### 3. UI 자동화 테스트
```csharp
[UnityTest]
public IEnumerator ClickButton_ShouldTriggerPointIncrease()
{
    // UI 상호작용 시뮬레이션 테스트
    yield return new WaitForSeconds(0.1f);
}
```

## 확장성 고려사항

### 1. 새로운 UI 컴포넌트 추가
1. `DIMono` 상속
2. 필요한 서비스 `[Inject]`로 주입
3. `Init()` 메서드에서 초기화
4. 이벤트 구독/해제 구현

### 2. 다국어 지원
```csharp
public class LocalizedText : MonoBehaviour
{
    [SerializeField] private string localizationKey;
    
    private void Start()
    {
        UpdateText(LocalizationManager.GetText(localizationKey));
    }
}
```

### 3. 테마 시스템
```csharp
public class ThemeManager : MonoBehaviour
{
    public void ApplyTheme(UITheme theme)
    {
        foreach (var component in GetComponentsInChildren<IThemeable>())
        {
            component.ApplyTheme(theme);
        }
    }
}
```

## 디버깅 및 개발 도구

### 1. UI 상태 모니터링
```csharp
[System.Serializable]
public class UIDebugInfo
{
    public bool isInitialized;
    public string currentState;
    public int activeEventSubscriptions;
}
```

### 2. 에디터 확장
```csharp
#if UNITY_EDITOR
[CustomEditor(typeof(PointClickerUI))]
public class PointClickerUIEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        if (GUILayout.Button("Test UI State"))
        {
            ((PointClickerUI)target).RefreshUI();
        }
    }
}
#endif
```

이 아키텍처를 통해 확장 가능하고 유지보수가 용이한 UI 시스템을 구축할 수 있습니다. 