# PointGenerator 씬 구성 예시

## 기본 씬 구조

### GameScene Hierarchy 예시
```
GameScene
├── EventSystem
│   └── Standalone Input Module (Component)
├── Main Camera
│   └── Camera (Component)
└── UI Canvas
    ├── Header
    │   ├── PointDisplay
    │   └── Navigation Buttons
    ├── MainContent
    │   ├── ClickArea
    │   └── UpgradeShop
    ├── Panels
    │   ├── AchievementPanel
    │   └── PrestigePanel
    └── Popups
        └── OfflineRewardsPopup
```

## 상세 컴포넌트 설정

### 1. UI Canvas 설정
```yaml
GameObject Name: "UI Canvas"
Components:
  - Canvas:
      Render Mode: Screen Space - Overlay
      Pixel Perfect: true
      Sort Order: 0
  - Canvas Scaler:
      UI Scale Mode: Scale With Screen Size
      Reference Resolution: (1920, 1080)
      Screen Match Mode: Match Width Or Height
      Match: 0.5
  - Graphic Raycaster:
      Ignore Reversed Graphics: true
      Blocking Objects: None
```

### 2. Header 영역 구성

#### PointDisplay GameObject
```yaml
GameObject Name: "PointDisplay"
Parent: Header
Components:
  - RectTransform:
      Anchor: Top-Stretch
      Anchor Min: (0, 1)
      Anchor Max: (1, 1)
      Offset Min: (20, -120)
      Offset Max: (-20, -20)
  - PointDisplayUI (Script):
      Point Count Text: PointsText
      Points Per Second Text: PerSecondText
      Total Earned Text: TotalText
  - Canvas Group:
      Alpha: 1
      Interactable: false
      Blocks Raycasts: false
```

#### PointDisplay 하위 오브젝트들
```yaml
# PointsText
GameObject Name: "PointsText"
Components:
  - Text:
      Text: "Points: 0"
      Font: Arial
      Font Size: 36
      Color: (1, 1, 1, 1)
      Alignment: Middle Left
  - RectTransform:
      Anchor: Top-Left
      Position: (10, -25, 0)
      Size: (300, 50)

# PerSecondText
GameObject Name: "PerSecondText"  
Components:
  - Text:
      Text: "Per Second: 0"
      Font: Arial
      Font Size: 24
      Color: (0.8, 0.8, 0.8, 1)
      Alignment: Middle Left
  - RectTransform:
      Anchor: Top-Left
      Position: (10, -75, 0)
      Size: (300, 40)
```

### 3. MainContent 영역 구성

#### ClickArea GameObject
```yaml
GameObject Name: "ClickArea"
Parent: MainContent
Components:
  - RectTransform:
      Anchor: Center
      Position: (0, 50, 0)
      Size: (400, 400)
  - ClickButtonUI (Script):
      Click Button: ClickButton
      Button Text: ButtonText
      Button Transform: ClickButton RectTransform
      Click Scale Factor: 0.95
      Scale Animation Duration: 0.1
```

#### ClickButton 하위 구조
```yaml
# ClickButton
GameObject Name: "ClickButton"
Components:
  - Button:
      Interactable: true
      Transition: Color Tint
      Target Graphic: ButtonImage
      Normal Color: (1, 1, 1, 1)
      Highlighted Color: (0.9, 0.9, 0.9, 1)
      Pressed Color: (0.7, 0.7, 0.7, 1)
  - Image (ButtonImage):
      Source Image: Circle (Unity Default)
      Color: (0.2, 0.6, 1, 1)
      Raycast Target: true
  - RectTransform:
      Anchor: Center
      Position: (0, 0, 0)
      Size: (300, 300)

# ButtonText
GameObject Name: "ButtonText"
Parent: ClickButton
Components:
  - Text:
      Text: "Click!\n+1 pts"
      Font: Arial
      Font Size: 32
      Color: (1, 1, 1, 1)
      Alignment: Middle Center
  - RectTransform:
      Anchor: Stretch
      Offset Min: (10, 10)
      Offset Max: (-10, -10)
```

### 4. UpgradeShop 구성

#### UpgradeShop GameObject
```yaml
GameObject Name: "UpgradeShop"
Parent: MainContent
Components:
  - RectTransform:
      Anchor: Bottom-Stretch
      Anchor Min: (0, 0)
      Anchor Max: (1, 0)
      Offset Min: (20, 20)
      Offset Max: (-20, 320)
  - UpgradeShopUI (Script):
      Upgrade Container: ScrollView/Viewport/Content
      Upgrade Item Prefab: UpgradeItemPrefab
      Scroll Rect: ScrollView
      Shop Title Text: TitleText
  - Image:
      Color: (0.1, 0.1, 0.2, 0.8)
```

#### UpgradeShop ScrollView 구조
```yaml
# ScrollView
GameObject Name: "ScrollView"
Components:
  - ScrollRect:
      Content: Content
      Viewport: Viewport
      Scrollbar Vertical: Scrollbar
      Movement Type: Elastic
      Elasticity: 0.1
      Scroll Sensitivity: 1
  - RectTransform:
      Anchor: Stretch
      Offset Min: (10, 40)
      Offset Max: (-10, -10)

# Viewport
GameObject Name: "Viewport"
Parent: ScrollView
Components:
  - Mask:
      Show Mask Graphic: false
  - Image:
      Color: (1, 1, 1, 0.01)
  - RectTransform:
      Anchor: Stretch
      Offset Min: (0, 0)
      Offset Max: (-20, 0)

# Content
GameObject Name: "Content"
Parent: Viewport
Components:
  - Vertical Layout Group:
      Padding: (10, 10, 10, 10)
      Spacing: 5
      Child Alignment: Upper Center
      Child Force Expand Width: true
      Child Force Expand Height: false
  - Content Size Fitter:
      Vertical Fit: Preferred Size
  - RectTransform:
      Anchor: Top-Stretch
      Pivot: (0.5, 1)
```

### 5. AchievementPanel 구성

#### AchievementPanel GameObject
```yaml
GameObject Name: "AchievementPanel"
Parent: Panels
Components:
  - RectTransform:
      Anchor: Stretch
      Offset Min: (0, 0)
      Offset Max: (0, 0)
  - AchievementPanelUI (Script):
      Achievement Item Parent: ScrollView/Viewport/Content
      Achievement Item Prefab: AchievementItemPrefab
      Close Button: HeaderPanel/CloseButton
      Title Text: HeaderPanel/TitleText
      Scroll Rect: ScrollView
  - Canvas Group:
      Alpha: 1
      Interactable: true
      Blocks Raycasts: true
  - Image:
      Color: (0, 0, 0, 0.7)
Active: false (초기에는 비활성화)
```

### 6. OfflineRewardsPopup 구성

#### OfflineRewardsPopup GameObject
```yaml
GameObject Name: "OfflineRewardsPopup"
Parent: Popups
Components:
  - RectTransform:
      Anchor: Stretch
      Offset Min: (0, 0)
      Offset Max: (0, 0)
  - OfflineRewardsPopup (Script):
      Popup Panel: PopupPanel
      Offline Time Text: PopupPanel/InfoPanel/TimeText
      Reward Amount Text: PopupPanel/InfoPanel/RewardText
      Claim Button: PopupPanel/ButtonPanel/ClaimButton
      Close Button: PopupPanel/HeaderPanel/CloseButton
      Canvas Group: PopupPanel CanvasGroup
      Popup Transform: PopupPanel RectTransform
  - Canvas Group:
      Alpha: 1
      Interactable: true
      Blocks Raycasts: true
Active: false (초기에는 비활성화)
```

#### PopupPanel 하위 구조
```yaml
# PopupPanel
GameObject Name: "PopupPanel"
Components:
  - RectTransform:
      Anchor: Center
      Position: (0, 0, 0)
      Size: (500, 400)
  - Image:
      Color: (0.15, 0.15, 0.25, 0.95)
      Raycast Target: true
  - Canvas Group:
      Alpha: 1
      Interactable: true
      Blocks Raycasts: true

# HeaderPanel
GameObject Name: "HeaderPanel"
Parent: PopupPanel
Components:
  - RectTransform:
      Anchor: Top-Stretch
      Offset Min: (10, -50)
      Offset Max: (-10, -10)

# InfoPanel  
GameObject Name: "InfoPanel"
Parent: PopupPanel
Components:
  - RectTransform:
      Anchor: Middle-Stretch
      Offset Min: (20, -50)
      Offset Max: (-20, 50)

# ButtonPanel
GameObject Name: "ButtonPanel"
Parent: PopupPanel
Components:
  - RectTransform:
      Anchor: Bottom-Stretch
      Offset Min: (20, 10)
      Offset Max: (-20, 60)
  - Horizontal Layout Group:
      Spacing: 20
      Child Alignment: Middle Center
```

## 프리팹 참조 설정

### UpgradeItemPrefab 구조
```yaml
Prefab Name: "UpgradeItemPrefab"
Root GameObject: "UpgradeItem"
Components:
  - RectTransform:
      Size: (0, 80) # Height 고정, Width는 부모에 맞춤
  - UpgradeItemUI (Script)
  - Image (Background):
      Color: (0.2, 0.2, 0.3, 0.8)

Children:
  - IconImage (Image) [선택사항]
  - InfoPanel (GameObject)
    - NameText (Text)
    - DescriptionText (Text) 
    - LevelText (Text)
    - CostText (Text)
  - PurchaseButton (Button)
  - MaxLevelIndicator (GameObject)
```

### AchievementItemPrefab 구조
```yaml
Prefab Name: "AchievementItemPrefab"
Root GameObject: "AchievementItem"
Components:
  - RectTransform:
      Size: (0, 100)
  - AchievementItemUI (Script)
  - Image (Background)

Children:
  - IconImage (Image) [선택사항]
  - InfoPanel
    - NameText (Text)
    - DescriptionText (Text)
    - ProgressText (Text)
    - RewardText (Text)
  - ProgressBar (Slider)
  - UnlockedIndicator (GameObject)
  - NewIndicator (GameObject)
```

## Navigation Buttons 설정

### Header에 추가되는 네비게이션 버튼들
```yaml
# AchievementsButton
GameObject Name: "AchievementsButton"
Parent: Header/Navigation Buttons
Components:
  - Button:
      On Click: PointClickerUI.OnAchievementsButtonPressed
  - RectTransform:
      Anchor: Top-Right
      Position: (-120, -30, 0)
      Size: (100, 40)

# PrestigeButton  
GameObject Name: "PrestigeButton"
Parent: Header/Navigation Buttons
Components:
  - Button:
      On Click: PointClickerUI.OnPrestigeButtonPressed
  - RectTransform:
      Anchor: Top-Right
      Position: (-10, -30, 0)
      Size: (100, 40)
```

## 메인 컨트롤러 연결

### PointClickerUI 설정 (메인 Canvas에 추가)
```yaml
Components:
  - PointClickerUI (Script):
      # Inject된 의존성들 (자동 주입됨)
      Point Manager: (자동 주입)
      Upgrade Manager: (자동 주입)
      Achievement Manager: (자동 주입)
      Prestige Manager: (자동 주입)
      
      # Inspector에서 설정할 참조들
      Click Button: ClickArea/ClickButton
      Points Text: Header/PointDisplay/PointsText
      Points Per Click Text: Header/PointDisplay/ClickText  
      Points Per Second Text: Header/PointDisplay/PerSecondText
      Upgrade Container: UpgradeShop/ScrollView/Viewport/Content
      Upgrade Item Prefab: UpgradeItemPrefab
      Achievements Button: Header/Navigation Buttons/AchievementsButton
      Prestige Button: Header/Navigation Buttons/PrestigeButton
      Achievement Panel: Panels/AchievementPanel
      Prestige Panel: Panels/PrestigePanel
      
      # 효과 관련 (선택사항)
      Click Effect: ClickArea/ParticleSystem
      Click Sound: ClickArea/AudioSource
```

## 초기 설정 체크리스트

### 필수 확인 사항
- [ ] EventSystem이 씬에 존재하는가?
- [ ] UI Canvas의 Canvas Scaler 설정이 올바른가?
- [ ] 모든 UI 컴포넌트에 필요한 참조가 설정되었는가?
- [ ] 프리팹들이 Resources 폴더 또는 Addressables에 등록되었는가?
- [ ] GameBootstrap이 씬 시작 시 자동 실행되도록 설정되었는가?

### 테스트 순서
1. 게임 실행 후 GameBootstrap 초기화 로그 확인
2. 클릭 버튼 동작 확인 (포인트 증가)
3. 업그레이드 상점 표시 확인
4. 업적/프레스티지 패널 열기/닫기 확인
5. 오프라인 보상 팝업 테스트 (시간 조작 후 재시작)

이 구성을 따라하면 완전히 동작하는 PointGenerator UI 시스템을 구축할 수 있습니다. 