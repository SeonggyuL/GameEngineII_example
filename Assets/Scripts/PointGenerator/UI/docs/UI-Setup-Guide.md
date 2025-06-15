# PointGenerator UI 설정 가이드

## 개요

PointGenerator UI 시스템은 의존성 주입(DI) 기반으로 설계된 모듈형 UI 시스템입니다. 각 UI 컴포넌트는 `DIMono`를 상속받아 Core 시스템과 자동으로 연동됩니다.

## Canvas 구조

### 메인 Canvas 설정
```
MainCanvas (Canvas)
├── Header (상단 정보 영역)
│   ├── PointDisplayUI
│   └── HeaderButtons
├── MainContent (중앙 게임 영역)
│   ├── ClickButtonUI
│   └── PointClickerUI
├── Shop (하단 상점 영역)
│   └── UpgradeShopUI
├── Panels (팝업 패널들)
│   ├── AchievementPanelUI
│   └── PrestigePanelUI
└── Popups (오버레이 팝업들)
    └── OfflineRewardsPopup
```

### Canvas 컴포넌트 설정
- **Render Mode**: Screen Space - Overlay
- **Canvas Scaler**: Scale With Screen Size
- **Reference Resolution**: 1920 x 1080
- **Match**: 0.5 (Width와 Height 균형)

## UI 컴포넌트별 배치 가이드

### 1. PointDisplayUI (포인트 표시)
**위치**: Canvas 상단
**역할**: 현재 포인트, 초당 포인트 표시

#### 필수 UI 요소:
- `Text pointCountText`: "Points: 1,234"
- `Text pointsPerSecondText`: "Per Second: 56"
- `Text totalEarnedText`: "Current: 1,234"

#### 배치 설정:
```
RectTransform:
- Anchor: Top-Center
- Position: (0, -50, 0)
- Size: (400, 100)
```

### 2. ClickButtonUI (메인 클릭 버튼)
**위치**: Canvas 중앙
**역할**: 포인트 생성을 위한 메인 클릭 버튼

#### 필수 UI 요소:
- `Button clickButton`: 클릭 가능한 버튼
- `Text buttonText`: "Click!\n+10 pts"
- `RectTransform buttonTransform`: 애니메이션용 Transform

#### 배치 설정:
```
RectTransform:
- Anchor: Center
- Position: (0, 0, 0)
- Size: (200, 200)
```

### 3. UpgradeShopUI (업그레이드 상점)
**위치**: Canvas 하단 또는 사이드
**역할**: 업그레이드 구매 인터페이스

#### 필수 UI 요소:
- `Transform upgradeContainer`: 업그레이드 아이템들의 부모
- `GameObject upgradeItemPrefab`: UpgradeItemUI 프리팹
- `ScrollRect scrollRect`: 스크롤 가능한 영역
- `Text shopTitleText`: "Upgrade Shop"

#### 배치 설정:
```
RectTransform:
- Anchor: Bottom-Stretch
- Position: (0, 150, 0)
- Size: (Full Width, 300)
```

### 4. AchievementPanelUI (업적 패널)
**위치**: 전체 화면 오버레이
**역할**: 업적 목록 및 진행도 표시

#### 필수 UI 요소:
- `Transform achievementItemParent`: 업적 아이템 컨테이너
- `GameObject achievementItemPrefab`: AchievementItemUI 프리팹
- `Button closeButton`: 패널 닫기 버튼
- `Text titleText`: "Achievements"
- `ScrollRect scrollRect`: 스크롤 영역

#### 배치 설정:
```
RectTransform:
- Anchor: Full-Screen
- Position: (0, 0, 0)
- Size: (Full Screen)
- 초기 상태: SetActive(false)
```

### 5. PrestigePanelUI (프레스티지 패널)
**위치**: 전체 화면 오버레이
**역할**: 프레스티지 시스템 관리

#### 필수 UI 요소:
- `Button closeButton`: 닫기 버튼
- `Button prestigeButton`: 프레스티지 실행 버튼
- `Text currentLevelText`: "Current Level: 1"
- `Text prestigePointsText`: "Prestige Points: 0"
- `Text gainedPointsText`: "Points to Gain: 10"
- `Transform upgradeItemParent`: 프레스티지 업그레이드 컨테이너
- `GameObject prestigeUpgradeItemPrefab`: PrestigeUpgradeItemUI 프리팹

### 6. OfflineRewardsPopup (오프라인 보상 팝업)
**위치**: 화면 중앙 오버레이
**역할**: 오프라인 보상 표시 및 수령

#### 필수 UI 요소:
- `GameObject popupPanel`: 팝업 패널
- `TextMeshProUGUI offlineTimeText`: "오프라인 시간: 2시간"
- `TextMeshProUGUI rewardAmountText`: "1,234"
- `Button claimButton`: 보상 수령 버튼
- `Button closeButton`: 팝업 닫기 버튼
- `CanvasGroup canvasGroup`: 애니메이션용
- `Transform popupTransform`: 스케일 애니메이션용

## 프리팹 구조

### 1. UpgradeItemUI 프리팹
```
UpgradeItem (GameObject)
├── Background (Image)
├── Icon (Image) [선택사항]
├── InfoPanel
│   ├── NameText (Text)
│   ├── DescriptionText (Text)
│   ├── LevelText (Text)
│   └── CostText (Text)
├── PurchaseButton (Button)
└── MaxLevelIndicator (GameObject) [선택사항]
```

### 2. AchievementItemUI 프리팹
```
AchievementItem (GameObject)
├── Background (Image)
├── Icon (Image) [선택사항]
├── InfoPanel
│   ├── NameText (Text)
│   ├── DescriptionText (Text)
│   ├── ProgressText (Text)
│   └── RewardText (Text)
├── ProgressBar (Slider)
├── UnlockedIndicator (GameObject)
└── NewIndicator (GameObject)
```

### 3. PrestigeUpgradeItemUI 프리팹
```
PrestigeUpgradeItem (GameObject)
├── Background (Image)
├── InfoPanel
│   ├── NameText (Text)
│   ├── DescriptionText (Text)
│   ├── LevelText (Text)
│   ├── CostText (Text)
│   └── EffectText (Text)
├── UpgradeButton (Button)
└── MaxLevelIndicator (GameObject)
```

## 메인 UI 컨트롤러 설정

### PointClickerUI (메인 컨트롤러)
이 컴포넌트는 다른 UI 요소들을 조율하는 메인 컨트롤러입니다.

#### 필수 연결 요소:
- `Button clickButton`: ClickButtonUI의 버튼
- `TextMeshProUGUI pointsText`: 포인트 표시 텍스트
- `Transform upgradeContainer`: 업그레이드 컨테이너
- `GameObject upgradeItemPrefab`: 업그레이드 아이템 프리팹
- `Button achievementsButton`: 업적 버튼
- `Button prestigeButton`: 프레스티지 버튼
- `AchievementPanelUI achievementPanel`: 업적 패널
- `PrestigePanelUI prestigePanel`: 프레스티지 패널

## 씬 설정 단계별 가이드

### 1단계: Canvas 생성
1. 빈 GameObject에 Canvas 컴포넌트 추가
2. Canvas Scaler 설정 (위 설정 참조)
3. GraphicRaycaster 컴포넌트 확인

### 2단계: 기본 UI 레이아웃 생성
1. Header 영역 생성 → PointDisplayUI 추가
2. MainContent 영역 생성 → ClickButtonUI 추가
3. Shop 영역 생성 → UpgradeShopUI 추가

### 3단계: 패널 및 팝업 설정
1. Panels 부모 오브젝트 생성
2. AchievementPanelUI, PrestigePanelUI 추가
3. Popups 부모 오브젝트 생성
4. OfflineRewardsPopup 추가

### 4단계: 프리팹 연결
1. 각 UI 컴포넌트에 필요한 프리팹 연결
2. 참조 관계 설정 (Inspector에서)

### 5단계: 메인 컨트롤러 설정
1. PointClickerUI 컴포넌트를 메인 Canvas에 추가
2. 모든 UI 요소들을 PointClickerUI에 연결

## 의존성 주입 설정

### DIMono 상속 컴포넌트들
모든 UI 컴포넌트는 `DIMono`를 상속받아 자동으로 의존성이 주입됩니다.

#### 주입되는 서비스들:
- `IPointManager`: 포인트 관리
- `IUpgradeManager`: 업그레이드 관리
- `IAchievementManager`: 업적 관리
- `IPrestigeManager`: 프레스티지 관리
- `IOfflineRewardsManager`: 오프라인 보상 관리

### 초기화 순서
1. GameBootstrap에서 서비스 등록
2. DI 컨테이너 초기화
3. UI 컴포넌트 생성 시 자동 주입
4. `Init()` 메서드 자동 호출

## 이벤트 시스템 연결

### 자동 연결되는 이벤트들
UI 컴포넌트들은 `Init()` 메서드에서 자동으로 Core 시스템의 이벤트들을 구독합니다:

- `OnPointsChanged`: 포인트 변경 시
- `OnUpgradePurchased`: 업그레이드 구매 시
- `OnAchievementUnlocked`: 업적 달성 시
- `OnPrestigePerformed`: 프레스티지 수행 시

## 주의사항

### 1. 초기화 순서
- UI 컴포넌트는 GameBootstrap 완료 후에 활성화되어야 합니다
- `WaitForInjectionAndInitialize()` 패턴 사용 권장

### 2. 메모리 관리
- `OnDestroy()`에서 이벤트 구독 해제 필수
- 버튼 이벤트 리스너 정리

### 3. 성능 최적화
- UI 업데이트는 필요할 때만 수행
- 대량의 업그레이드/업적 아이템은 오브젝트 풀링 고려

## 테스트 및 디버깅

### 의존성 주입 확인
```csharp
private void Start()
{
    if (pointManager == null)
        Debug.LogError("PointManager 주입 실패!");
}
```

### UI 상태 확인
- Inspector에서 각 UI 컴포넌트의 참조가 올바르게 설정되었는지 확인
- Console에서 초기화 로그 확인
- 이벤트 구독 상태 확인

이 가이드를 따라 설정하면 PointGenerator UI 시스템이 정상적으로 작동하며, Core 시스템과 완벽하게 연동됩니다. 