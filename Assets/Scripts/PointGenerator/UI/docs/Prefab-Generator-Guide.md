# UI 프리팹 자동 생성 도구 가이드

## 개요

`UIPrefabGenerator`는 PointGenerator UI 시스템에 필요한 프리팹들을 자동으로 생성해주는 에디터 도구입니다. 복잡한 UI 구조를 수동으로 만들 필요 없이 버튼 클릭만으로 완성된 프리팹을 생성할 수 있습니다.

**⚡ TextMeshPro UGUI 컴포넌트 사용**: 모든 텍스트는 Unity 기본 Text 대신 TextMeshPro UGUI 컴포넌트를 사용합니다.

**🎨 아이콘 자동 포함**: 모든 프리팹에 적절한 색상의 아이콘이 자동으로 생성됩니다.

## 사용 방법

### 1. 도구 실행
Unity 에디터에서 다음 메뉴를 선택하세요:
```
PointGenerator → Create UI Prefabs
```

### 2. 프리팹 생성 옵션

#### 모든 프리팹 한 번에 생성
- `Create All UI Prefabs` 버튼 클릭
- 다음 프리팹들이 자동으로 생성됩니다:
  - UpgradeItemPrefab.prefab
  - AchievementItemPrefab.prefab  
  - PrestigeUpgradeItemPrefab.prefab

#### 개별 프리팹 생성
필요한 프리팹만 선택적으로 생성할 수 있습니다:
- `Create UpgradeItem Prefab`: 업그레이드 아이템 프리팹
- `Create AchievementItem Prefab`: 업적 아이템 프리팹
- `Create PrestigeUpgradeItem Prefab`: 프레스티지 업그레이드 아이템 프리팹

## 생성되는 프리팹들

### 1. UpgradeItemPrefab
**경로**: `Assets/Prefabs/UI/UpgradeItemPrefab.prefab`

**구조**:
```
UpgradeItem
├── UpgradeIcon (Image - 파란색 아이콘)
│   └── IconBackground (Image - 배경)
├── InfoPanel
│   ├── NameText (TextMeshProUGUI - 업그레이드 이름)
│   ├── DescriptionText (TextMeshProUGUI - 설명)
│   ├── LevelText (TextMeshProUGUI - 현재 레벨)
│   └── CostText (TextMeshProUGUI - 구매 비용)
├── PurchaseButton (구매 버튼)
│   └── Text (TextMeshProUGUI - 버튼 텍스트)
└── MaxLevelIndicator (최대 레벨 표시)
    └── MaxText (TextMeshProUGUI - "MAX" 텍스트)
```

**특징**:
- 높이: 80px (가로는 부모에 맞춤)
- 배경색: 어두운 파란색 (0.2, 0.2, 0.3, 0.8)
- **아이콘**: 파란색 계열 (0.2, 0.6, 1.0)
- UpgradeItemUI 스크립트 자동 연결
- 모든 Inspector 참조 자동 설정

### 2. AchievementItemPrefab
**경로**: `Assets/Prefabs/UI/AchievementItemPrefab.prefab`

**구조**:
```
AchievementItem
├── AchievementIcon (Image - 금색 아이콘)
│   └── IconBackground (Image - 배경)
├── InfoPanel
│   ├── NameText (TextMeshProUGUI - 업적 이름)
│   ├── DescriptionText (TextMeshProUGUI - 업적 설명)
│   ├── ProgressText (TextMeshProUGUI - 진행도 텍스트)
│   └── RewardText (TextMeshProUGUI - 보상 정보)
├── ProgressSlider (진행도 바)
├── UnlockedIndicator (달성 표시)
└── NewIndicator (새로운 업적 표시)
```

**특징**:
- 높이: 100px
- 배경색: 어두운 보라색 (0.15, 0.15, 0.25, 0.9)
- **아이콘**: 금색 계열 (1.0, 0.8, 0.2) - 업적의 가치를 표현
- 진행도 슬라이더 완전 구성
- 상태 표시 인디케이터들 포함

### 3. PrestigeUpgradeItemPrefab  
**경로**: `Assets/Prefabs/UI/PrestigeUpgradeItemPrefab.prefab`

**구조**:
```
PrestigeUpgradeItem
├── PrestigeIcon (Image - 보라색 아이콘)
│   └── IconBackground (Image - 배경)
├── InfoPanel
│   ├── NameText (TextMeshProUGUI - 업그레이드 이름)
│   ├── DescriptionText (TextMeshProUGUI - 설명)
│   ├── LevelText (TextMeshProUGUI - 현재 레벨)
│   ├── CostText (TextMeshProUGUI - 프레스티지 포인트 비용)
│   └── EffectText (TextMeshProUGUI - 효과 설명)
├── UpgradeButton (업그레이드 버튼)
│   └── Text (TextMeshProUGUI - 버튼 텍스트)
└── MaxLevelIndicator (최대 레벨 표시)
    └── MaxText (TextMeshProUGUI - "MAX" 텍스트)
```

**특징**:
- 높이: 90px
- **아이콘**: 보라색 계열 (0.8, 0.2, 1.0) - 프레스티지의 특별함을 표현
- 프레스티지 전용 UI 요소들
- 효과 설명 텍스트 추가
- PrestigeUpgradeItemUI 스크립트 연결

## 자동 설정되는 내용

### 1. 폴더 생성
도구가 자동으로 다음 폴더들을 생성합니다:
- `Assets/Prefabs` (없을 경우)
- `Assets/Prefabs/UI` (없을 경우)

### 2. 컴포넌트 자동 연결
각 프리팹의 UI 스크립트에 모든 참조가 자동으로 연결됩니다:
- **TextMeshProUGUI** 컴포넌트들
- **Image** 컴포넌트들 (아이콘 포함)
- Button 컴포넌트들
- Slider 컴포넌트들 (해당되는 경우)

### 3. 아이콘 자동 생성
각 프리팹 타입별로 적절한 색상의 아이콘이 자동으로 생성됩니다:
- **업그레이드**: 파란색 계열 - 기능적 향상을 의미
- **업적**: 금색 계열 - 성취의 가치를 표현
- **프레스티지**: 보라색 계열 - 특별하고 고급스러운 느낌

### 4. TextMeshPro 설정
모든 텍스트 컴포넌트에 대해 자동으로 설정됩니다:
- **기본 폰트**: LiberationSans SDF (TextMeshPro 기본 폰트)
- **Word Wrapping**: 비활성화
- **Overflow Mode**: Ellipsis (말줄임표)
- **정렬**: MidlineLeft (기본), Center (버튼)

### 5. RectTransform 설정
모든 UI 요소의 위치, 크기, 앵커가 적절히 설정됩니다:
- 아이콘과 텍스트의 적절한 간격
- 텍스트 정렬 및 크기
- 버튼 위치 및 크기
- 레이아웃 그룹 설정

### 6. 스타일링
기본적인 색상 및 폰트 설정:
- 배경 이미지 색상
- 아이콘 색상 (타입별 구분)
- 텍스트 색상 (역할별로 다른 색상)
- 버튼 색상 및 전환 효과

## 사용 시 주의사항

### 1. TextMeshPro 의존성
생성된 프리팹들은 **TextMeshPro** 패키지가 설치되어 있어야 정상 작동합니다:
- Window → TextMeshPro → Import TMP Essential Resources (필요시)
- TextMeshPro 패키지가 프로젝트에 포함되어야 함

### 2. 기존 프리팹 덮어쓰기
같은 이름의 프리팹이 이미 존재하는 경우 덮어쓰여집니다. 기존 프리팹을 보존하려면 미리 백업하세요.

### 3. 스크립트 의존성
생성된 프리팹들은 다음 스크립트들이 존재해야 정상 작동합니다:
- `PointGenerator.UI.UpgradeItemUI`
- `PointGenerator.UI.AchievementItemUI`
- `PointGenerator.UI.PrestigeUpgradeItemUI`

### 4. UI 스크립트 필드 요구사항
생성된 프리팹을 사용하려면 UI 스크립트들이 다음 필드들을 가져야 합니다:

**모든 스크립트 공통**:
```csharp
[SerializeField] private TextMeshProUGUI nameText;
[SerializeField] private TextMeshProUGUI descriptionText;
[SerializeField] private Image iconImage; // 새롭게 추가된 필드
```

**업그레이드 및 프레스티지**:
```csharp
[SerializeField] private TextMeshProUGUI costText;
[SerializeField] private TextMeshProUGUI levelText;
[SerializeField] private Button purchaseButton; // 또는 upgradeButton
[SerializeField] private GameObject maxLevelIndicator;
```

**업적**:
```csharp
[SerializeField] private TextMeshProUGUI progressText;
[SerializeField] private TextMeshProUGUI rewardText;
[SerializeField] private Slider progressSlider;
[SerializeField] private GameObject unlockedIndicator;
[SerializeField] private GameObject newIndicator;
```

## 생성 후 작업

### 1. 아이콘 커스터마이징
생성된 아이콘을 프로젝트에 맞게 수정하세요:
- **스프라이트 교체**: 프로젝트의 실제 아이콘 이미지로 변경
- **색상 조정**: 게임의 컬러 테마에 맞게 조정
- **크기 조정**: 필요에 따라 아이콘 크기 변경
- **효과 추가**: 그림자, 외곽선 등 시각 효과

## 문제 해결

### TextMeshPro 관련 오류
```
The type or namespace name 'TextMeshProUGUI' could not be found
```
**해결방법**: 
1. Window → Package Manager → TextMeshPro 패키지 설치
2. `using TMPro;` 네임스페이스 추가
3. UI 스크립트 필드를 TextMeshProUGUI로 변경

### 폰트 에셋 누락 오류
```
Font Asset 'LiberationSans SDF' not found
```
**해결방법**:
1. Window → TextMeshPro → Import TMP Essential Resources
2. 또는 다른 TMP_FontAsset을 수동으로 할당

### Inspector 참조 타입 불일치
UI 스크립트의 필드 타입과 프리팹의 컴포넌트 타입이 맞지 않는 경우:
1. UI 스크립트에서 `Text` → `TextMeshProUGUI`로 변경
2. `using TMPro;` 네임스페이스 추가
3. 프리팹 재생성

## TextMeshPro 장점

### 1. 향상된 렌더링 품질
- **SDF (Signed Distance Field)** 기술로 확대해도 선명한 텍스트
- 다양한 해상도에서 일관된 품질

### 2. 고급 텍스트 효과
- Outline, Drop Shadow, Glow 효과
- 그라데이션 색상
- 텍스트 변형 및 애니메이션

### 3. 성능 최적화
- 배치 처리로 드로우 콜 감소
- GPU 친화적 렌더링

### 4. 유니코드 지원
- 다국어 지원 향상
- Emoji 및 특수 문자 지원

## 확장하기

### 새로운 프리팹 타입 추가
도구에 새로운 프리팹 생성 기능을 추가하려면:

1. `UIPrefabGenerator.cs`에 새로운 생성 메서드 추가:
```csharp
private void CreateNewItemPrefab()
{
    // TextMeshProUGUI 사용하여 새로운 프리팹 생성 로직
    var textComponent = CreateTextMeshPro(parent, "Text", "Hello", 14, Color.white);
}
```

2. `OnGUI()` 메서드에 버튼 추가:
```csharp
if (GUILayout.Button("Create NewItem Prefab"))
{
    CreateNewItemPrefab();
}
```

이 도구를 사용하면 TextMeshPro의 모든 장점을 활용한 고품질 UI 프리팹들을 빠르게 생성할 수 있습니다! 