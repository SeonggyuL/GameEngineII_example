# 포인트 생성기 - 구현 계획서 (DI Framework 기반)

## 1. 프로젝트 폴더 구조

```
Assets/Scripts/PointGenerator/
├── Bootstrap/                     # 부트스트랩 및 초기화
│   ├── GameBootstrap.cs          # 게임 부트스트랩 (DI 컨테이너 설정)
│   ├── DataLoader.cs             # Addressable을 이용한 데이터 로딩
│   └── ServiceInstaller.cs       # 서비스 등록 및 DI 설정
├── Core/                          # 핵심 게임 로직
│   ├── Interfaces/               # 인터페이스 정의
│   │   ├── IGameManager.cs      # 게임 매니저 인터페이스
│   │   ├── IPointManager.cs     # 포인트 매니저 인터페이스
│   │   ├── IUpgradeManager.cs   # 업그레이드 매니저 인터페이스
│   │   └── ISaveLoadManager.cs  # 저장/로드 매니저 인터페이스
│   ├── Services/                # 서비스 구현
│   │   ├── GameManager.cs       # 게임 전체 상태 관리
│   │   ├── PointManager.cs      # 포인트 관리 (클릭, 자동생성)
│   │   ├── UpgradeManager.cs    # 업그레이드 시스템 관리
│   │   └── SaveLoadManager.cs   # 저장/로드 관리
│   └── GameConstants.cs         # 게임 상수 정의
├── Data/                         # 데이터 모델 (기존 구조 활용)
│   ├── GameData/                # 이미 존재하는 구조
│   │   ├── UpgradeDefinition.cs # 업그레이드 정의 (기존)
│   │   └── GameData.cs          # 게임 데이터 SO (기존)
│   └── UserData/                # 사용자 데이터
│       ├── UserData.cs          # 사용자 데이터 모델 (기존)
│       ├── UserUpgradeProgress.cs # 업그레이드 진행 데이터 (기존)
│       └── GameSaveData.cs      # 저장 데이터 래퍼
├── UI/                          # UI 관련 스크립트 (DIMono 상속)
│   ├── MainGameUI.cs           # 메인 게임 UI 컨트롤러
│   ├── PointDisplayUI.cs       # 포인트 표시 UI
│   ├── ClickButtonUI.cs        # 메인 클릭 버튼 UI
│   ├── UpgradeShopUI.cs        # 업그레이드 상점 UI
│   ├── UpgradeItemUI.cs        # 개별 업그레이드 아이템 UI
│   └── NumberFormatter.cs      # 숫자 포맷팅 유틸리티
├── Systems/                     # 게임 시스템
│   ├── UpgradeSystem.cs        # 업그레이드 로직
│   ├── AutoPointGenerator.cs   # 자동 포인트 생성 시스템
│   └── OfflineProgressSystem.cs # 오프라인 진행 계산
├── Events/                      # 이벤트 시스템
│   ├── GameEvents.cs           # 게임 이벤트 정의
│   └── EventManager.cs         # 이벤트 매니저
└── Utils/                       # 유틸리티
    ├── Extensions/              # 확장 메서드
    │   ├── NumberExtensions.cs # 숫자 관련 확장
    │   └── UIExtensions.cs     # UI 관련 확장
    └── Helpers/                 # 헬퍼 클래스
        ├── BigNumber.cs        # 큰 숫자 처리
        └── TimeHelper.cs       # 시간 관련 유틸리티
```

## 2. DI 프레임워크 기반 아키텍처

### 2.1 DI Container 구조
- **기존 UJ.DI 프레임워크 활용**: `DIContainer`, `DIMono`, `InjectObj` 클래스 활용
- **의존성 주입**: `[Inject]` 어트리뷰트를 통한 자동 의존성 주입
- **서비스 등록**: 게임 시작 시 모든 서비스를 DI 컨테이너에 등록

### 2.2 Bootstrap 프로세스
1. **GameBootstrap.cs**: 
   - `DIContainer` 초기화 및 설정
   - Addressable을 통한 GameData 로딩 ("Assets/Prefabs/data.asset")
   - 모든 서비스 인스턴스 생성 및 DI 컨테이너 등록
   
2. **DataLoader.cs**:
   - Addressable 시스템을 이용한 GameData ScriptableObject 로딩
   - 비동기 로딩 후 DI 컨테이너에 등록
   
3. **ServiceInstaller.cs**:
   - 모든 게임 서비스들의 등록 및 초기화 순서 관리

### 2.3 DI 주입 패턴
```csharp
// UI 컴포넌트에서 서비스 주입 예시
public class PointDisplayUI : DIMono
{
    [Inject] private IPointManager pointManager;
    [Inject] private GameData gameData;
    
    public override void Init()
    {
        // DIMono의 Init에서 의존성 주입 완료 후 초기화
    }
}

// 서비스에서 다른 서비스 주입 예시
public class UpgradeManager : IUpgradeManager
{
    [Inject] private GameData gameData;
    [Inject] private IPointManager pointManager;
    [Inject] private ISaveLoadManager saveLoadManager;
}
```

## 3. Addressable GameData 로딩 시스템

### 3.1 데이터 로딩 흐름
1. **게임 시작** → `GameBootstrap` 실행
2. **Addressable 로딩** → "Assets/Prefabs/data.asset" 경로에서 `GameData` ScriptableObject 로딩
3. **DI 등록** → 로딩된 `GameData`를 DI 컨테이너에 등록
4. **서비스 초기화** → 모든 서비스에 `GameData` 의존성 주입
5. **UI 초기화** → UI 컴포넌트들에 필요한 서비스들 주입

### 3.2 GameData 구조 활용
- **기존 구조 유지**: `GameData.cs`의 `AllUpgradeDefinitions` 리스트 활용
- **Excel 기반**: `[FromXlsx]` 어트리뷰트를 통한 엑셀 데이터 자동 변환
- **업그레이드 정의**: `UpgradeDefinition` 클래스의 모든 기능 활용

## 4. 제작해야 할 파일 목록 및 역할

### 4.1 Bootstrap 파일들 (최우선)

| 파일명 | 역할 | 의존성 |
|--------|------|--------|
| `GameBootstrap.cs` | 게임 전체 초기화, DI 컨테이너 설정, 부트스트랩 프로세스 관리 | 없음 |
| `DataLoader.cs` | Addressable을 통한 GameData 로딩, 비동기 로딩 관리 | Addressables |
| `ServiceInstaller.cs` | 모든 서비스 등록 및 DI 설정, 초기화 순서 관리 | DIContainer |

### 4.2 Core Interface 파일들 (높은 우선순위)

| 파일명 | 역할 | 우선순위 |
|--------|------|----------|
| `IGameManager.cs` | 게임 전체 상태 관리 인터페이스 | 높음 |
| `IPointManager.cs` | 포인트 관리 인터페이스 (클릭, 자동생성, 소모) | 높음 |
| `IUpgradeManager.cs` | 업그레이드 시스템 관리 인터페이스 | 높음 |
| `ISaveLoadManager.cs` | 저장/로드 관리 인터페이스 | 높음 |

### 4.3 Core Service 파일들 (높은 우선순위)

| 파일명 | 역할 | 의존성 |
|--------|------|--------|
| `GameManager.cs` | 게임 상태 관리, 게임 루프 제어 | `GameData`, `IPointManager` |
| `PointManager.cs` | 포인트 획득/소모/자동생성 로직 | `GameData`, `UserData` |
| `UpgradeManager.cs` | 업그레이드 구매/적용/가격 계산 | `GameData`, `IPointManager`, `UserData` |
| `SaveLoadManager.cs` | 사용자 데이터 저장/로드 (PlayerPrefs) | `UserData` |

### 4.4 Data 파일들 (기존 활용 + 추가)

| 파일명 | 역할 | 상태 |
|--------|------|------|
| `UpgradeDefinition.cs` | 업그레이드 정의 (기존) | 기존 활용 |
| `GameData.cs` | 게임 데이터 ScriptableObject (기존) | 기존 활용 |
| `UserData.cs` | 사용자 데이터 모델 (기존) | 기존 활용 |
| `UserUpgradeProgress.cs` | 업그레이드 진행 데이터 (기존) | 기존 활용 |
| `GameSaveData.cs` | 저장용 데이터 래퍼 | 신규 작성 |

### 4.5 UI 파일들 (DIMono 상속)

| 파일명 | 역할 | 의존성 |
|--------|------|--------|
| `MainGameUI.cs` | 메인 UI 전체 컨트롤러 | `IGameManager`, `IPointManager` |
| `PointDisplayUI.cs` | 포인트 정보 표시 | `IPointManager`, `GameData` |
| `ClickButtonUI.cs` | 메인 클릭 버튼 동작 및 애니메이션 | `IPointManager` |
| `UpgradeShopUI.cs` | 업그레이드 상점 ScrollView 관리 | `IUpgradeManager`, `GameData` |
| `UpgradeItemUI.cs` | 개별 업그레이드 아이템 UI | `IUpgradeManager` |

## 5. 구현 단계별 계획

### Phase 1: DI 기반 인프라 구축 (1주차)

1. **Bootstrap 시스템 구현**
   - `GameBootstrap.cs` - DI 컨테이너 초기화
   - `DataLoader.cs` - Addressable GameData 로딩
   - `ServiceInstaller.cs` - 서비스 등록 시스템

2. **핵심 인터페이스 정의**
   - `IGameManager.cs`, `IPointManager.cs` 등 인터페이스 작성
   - DI 주입 구조 설계

3. **데이터 구조 보완**
   - `GameSaveData.cs` - 저장용 래퍼 클래스 작성
   - 기존 데이터 클래스들과의 연동 확인

### Phase 2: 핵심 서비스 구현 (2주차)

1. **기본 서비스 구현**
   - `GameManager.cs` - 게임 상태 관리
   - `PointManager.cs` - 포인트 시스템
   - `SaveLoadManager.cs` - 저장/로드 시스템

2. **DI 주입 테스트**
   - 서비스 간 의존성 주입 테스트
   - Bootstrap 프로세스 검증

3. **기본 UI 프레임워크**
   - `DIMono` 기반 UI 베이스 클래스 구현
   - 기본 UI 컴포넌트 구조 설정

### Phase 3: 게임플레이 구현 (3주차)

1. **클릭 시스템**
   - `ClickButtonUI.cs` - 클릭 버튼 구현
   - 포인트 증가 로직 연동

2. **포인트 표시 시스템**
   - `PointDisplayUI.cs` - 실시간 포인트 표시
   - 숫자 포맷팅 및 UI 업데이트

3. **저장/로드 연동**
   - 게임 상태 자동 저장
   - 앱 재시작 시 상태 복원

### Phase 4: 업그레이드 시스템 (4주차)

1. **업그레이드 매니저**
   - `UpgradeManager.cs` - 업그레이드 로직 구현
   - GameData의 UpgradeDefinition 활용

2. **업그레이드 UI**
   - `UpgradeShopUI.cs` - 상점 UI 구현
   - `UpgradeItemUI.cs` - 개별 아이템 UI

3. **업그레이드 시스템 연동**
   - 구매 로직 및 효과 적용
   - 가격 계산 및 UI 업데이트

### Phase 5: 고급 기능 및 폴리싱 (5-6주차)

1. **자동 시스템**
   - `AutoPointGenerator.cs` - 자동 포인트 생성
   - `OfflineProgressSystem.cs` - 오프라인 진행

2. **이벤트 시스템**
   - `GameEvents.cs` - 게임 이벤트 정의
   - 서비스 간 이벤트 통신

3. **최적화 및 폴리싱**
   - 성능 최적화
   - UI/UX 개선
   - 버그 수정 및 테스트

## 6. DI 기반 서비스 아키텍처 예시

### 6.1 GameBootstrap.cs 구조
```csharp
public class GameBootstrap : MonoBehaviour
{
    private DIContainer container;
    
    private async void Start()
    {
        // 1. DI Container 초기화
        container = new DIContainer();
        DIContainer.AddContainer(container);
        
        // 2. GameData 로딩
        var gameData = await DataLoader.LoadGameData();
        container.Regist(gameData);
        
        // 3. 서비스 등록
        ServiceInstaller.InstallServices(container, gameData);
        
        // 4. 게임 시작
        var gameManager = container.GetValue<IGameManager>();
        gameManager.StartGame();
    }
}
```

### 6.2 서비스 등록 예시
```csharp
public static class ServiceInstaller
{
    public static void InstallServices(DIContainer container, GameData gameData)
    {
        // UserData 초기화
        var userData = SaveLoadManager.LoadUserData() ?? new UserData();
        container.Regist(userData);
        
        // 서비스 등록
        container.InjectAndRegist<IPointManager>(new PointManager());
        container.InjectAndRegist<IUpgradeManager>(new UpgradeManager());
        container.InjectAndRegist<ISaveLoadManager>(new SaveLoadManager());
        container.InjectAndRegist<IGameManager>(new GameManager());
    }
}
```

이 계획을 통해 기존 DI 프레임워크와 Addressable 시스템을 효과적으로 활용하면서, 확장 가능하고 유지보수가 용이한 클리커 게임을 구현할 수 있습니다.
```

## 2. 제작해야 할 파일 목록 및 역할

### 2.1 Core 파일들

| 파일명 | 역할 | 우선순위 |
|--------|------|----------|
| `GameManager.cs` | 게임 전체 상태 관리, 초기화, 게임 루프 제어 | 높음 |
| `PointManager.cs` | 포인트 획득, 소모, 자동 생성 로직 관리 | 높음 |
| `GameConstants.cs` | 게임 내 상수값들 (초기값, 배율 등) 정의 | 중간 |

### 2.2 Data 파일들

| 파일명 | 역할 | 우선순위 |
|--------|------|----------|
| `UserData.cs` | 플레이어 진행 상황 데이터 모델 | 높음 |
| `UserUpgradeData.cs` | 개별 업그레이드 진행 데이터 | 높음 |
| `UpgradeDefinition.cs` | 업그레이드 정의 ScriptableObject | 높음 |
| `UpgradeDatabase.cs` | 모든 업그레이드를 관리하는 데이터베이스 | 중간 |
| `GameConfig.cs` | 게임 전반적인 설정값들 | 중간 |
| `GameSaveData.cs` | 저장/로드용 데이터 래퍼 | 중간 |

### 2.3 UI 파일들

| 파일명 | 역할 | 우선순위 |
|--------|------|----------|
| `MainGameUI.cs` | 메인 UI 전체 컨트롤러 | 높음 |
| `PointDisplayUI.cs` | 포인트 정보 표시 (현재, 클릭당, 초당) | 높음 |
| `ClickButtonUI.cs` | 메인 클릭 버튼 동작 및 애니메이션 | 높음 |
| `UpgradeShopUI.cs` | 업그레이드 상점 ScrollView 관리 | 중간 |
| `UpgradeItemUI.cs` | 개별 업그레이드 아이템 UI | 중간 |
| `NumberFormatter.cs` | 큰 숫자를 K, M, B 등으로 표시 | 낮음 |

### 2.4 Manager 파일들

| 파일명 | 역할 | 우선순위 |
|--------|------|----------|
| `UpgradeManager.cs` | 업그레이드 구매, 적용, 가격 계산 | 높음 |
| `SaveLoadManager.cs` | PlayerPrefs를 이용한 저장/로드 | 중간 |
| `UIManager.cs` | UI 간의 상호작용 관리 | 중간 |
| `AudioManager.cs` | 효과음, 배경음 관리 | 낮음 |

### 2.5 System 파일들

| 파일명 | 역할 | 우선순위 |
|--------|------|----------|
| `UpgradeSystem.cs` | 업그레이드 로직 및 효과 적용 | 높음 |
| `AutoPointGenerator.cs` | 코루틴 기반 자동 포인트 생성 | 중간 |
| `OfflineProgressSystem.cs` | 오프라인 시간 계산 및 보상 | 낮음 |

### 2.6 Event 파일들

| 파일명 | 역할 | 우선순위 |
|--------|------|----------|
| `GameEvents.cs` | 게임 내 이벤트 정의 (UnityEvent 기반) | 중간 |
| `EventManager.cs` | 이벤트 구독/발행 관리 | 낮음 |

### 2.7 Utils 파일들

| 파일명 | 역할 | 우선순위 |
|--------|------|----------|
| `SingletonMono.cs` | MonoBehaviour Singleton 베이스 클래스 | 높음 |
| `BigNumber.cs` | 큰 숫자 처리 (ulong 한계 초과 시) | 낮음 |
| `NumberExtensions.cs` | 숫자 관련 확장 메서드 | 낮음 |
| `TimeHelper.cs` | 시간 계산 관련 유틸리티 | 낮음 |

## 3. 구현 단계별 계획

### Phase 1: 기본 프레임워크 구축 (1주차)

1. **프로젝트 구조 설정**
   - 폴더 구조 생성
   - 기본 씬 설정 (MainGame Scene)
   - 기본 UI 캔버스 구성

2. **핵심 데이터 구조 구현**
   - `UserData.cs` - 기본 사용자 데이터 모델
   - `UpgradeDefinition.cs` - 업그레이드 정의
   - `GameConstants.cs` - 기본 상수값들
   - `SingletonMono.cs` - Singleton 베이스 클래스

3. **기본 매니저 구현**
   - `GameManager.cs` - 게임 초기화 및 상태 관리
   - `PointManager.cs` - 기본 포인트 관리
   - `SaveLoadManager.cs` - 기본 저장/로드

### Phase 2: 기본 게임플레이 구현 (2주차)

1. **클릭 시스템 구현**
   - `ClickButtonUI.cs` - 클릭 버튼 동작
   - 클릭 시 포인트 증가 로직
   - 기본 클릭 효과 및 애니메이션

2. **포인트 표시 시스템**
   - `PointDisplayUI.cs` - 포인트 정보 표시
   - `NumberFormatter.cs` - 숫자 포맷팅
   - 실시간 UI 업데이트

3. **기본 저장/로드**
   - PlayerPrefs를 이용한 데이터 저장
   - 게임 재시작 시 상태 복원

### Phase 3: 업그레이드 시스템 구현 (3주차)

1. **업그레이드 데이터 구조**
   - `UpgradeDatabase.cs` - 업그레이드 데이터베이스
   - 기본 업그레이드 정의 생성 (ScriptableObject)
   - `UserUpgradeData.cs` - 사용자 업그레이드 진행

2. **업그레이드 매니저**
   - `UpgradeManager.cs` - 업그레이드 구매 로직
   - `UpgradeSystem.cs` - 업그레이드 효과 적용
   - 가격 계산 및 구매 가능 여부 확인

3. **업그레이드 UI**
   - `UpgradeShopUI.cs` - 상점 UI 관리
   - `UpgradeItemUI.cs` - 개별 아이템 UI
   - ScrollView 기반 상점 구현

### Phase 4: 자동 시스템 및 고급 기능 (4주차)

1. **자동 포인트 생성**
   - `AutoPointGenerator.cs` - 코루틴 기반 자동 생성
   - 초당 포인트 계산 및 적용
   - UI 실시간 업데이트

2. **이벤트 시스템**
   - `GameEvents.cs` - 게임 이벤트 정의
   - 매니저 간 통신 개선

3. **오프라인 진행**
   - `OfflineProgressSystem.cs` - 오프라인 시간 계산
   - 앱 복귀 시 누적 포인트 계산

### Phase 5: 폴리싱 및 최적화 (5-6주차)

1. **UI/UX 개선**
   - 애니메이션 효과 추가
   - 사운드 효과 (`AudioManager.cs`)
   - 시각적 피드백 강화

2. **성능 최적화**
   - UI 업데이트 최적화
   - 메모리 사용량 최적화
   - 프레임레이트 안정화

3. **테스트 및 버그 수정**
   - 기능 테스트
   - 다양한 디바이스 테스트
   - 엣지 케이스 처리

## 4. ScriptableObject 에셋 생성 계획

### 4.1 생성할 에셋들

1. **GameConfig.asset**
   - 게임 전반적인 설정값들
   - 초기 포인트, 기본 클릭 파워 등

2. **UpgradeDatabase.asset**
   - 모든 업그레이드 정의를 포함하는 데이터베이스
   - 4-5개의 기본 업그레이드 정의

3. **Individual Upgrade Assets**
   - EnhancedFinger.asset (강화된 손가락)
   - AutoClickMachine.asset (자동 클릭 머신)  
   - MassProduction.asset (대량 생산 시설)
   - SuperAmplifier.asset (슈퍼 에너지 증폭기)

### 4.2 에셋 생성 경로
```
Assets/Scripts/PointGenerator/Data/Assets/
├── GameConfig.asset
├── UpgradeDatabase.asset
└── Upgrades/
    ├── EnhancedFinger.asset
    ├── AutoClickMachine.asset
    ├── MassProduction.asset
    └── SuperAmplifier.asset
```

## 5. UI 프리팹 구조 계획

### 5.1 메인 UI 프리팹
```
MainGameCanvas (Canvas)
├── TopPanel (Panel)
│   ├── CurrentPointsText (TextMeshProUGUI)
│   ├── PointsPerClickText (TextMeshProUGUI)
│   └── PointsPerSecondText (TextMeshProUGUI)
├── MiddlePanel (Panel)
│   └── ClickButton (Button)
│       └── ButtonText (TextMeshProUGUI)
└── BottomPanel (Panel)
    ├── ShopTitle (TextMeshProUGUI)
    └── UpgradeScrollView (ScrollView)
        └── Content (Content)
            └── UpgradeItemPrefab (Prefab Instance)
```

### 5.2 업그레이드 아이템 프리팹
```
UpgradeItem (Panel)
├── UpgradeIcon (Image)
├── InfoPanel (Panel)
│   ├── NameText (TextMeshProUGUI)
│   ├── DescriptionText (TextMeshProUGUI)
│   └── PriceText (TextMeshProUGUI)
└── BuyButton (Button)
    └── ButtonText (TextMeshProUGUI)
```

## 6. 테스트 계획

### 6.1 단위 테스트
- 포인트 계산 로직 테스트
- 업그레이드 가격 계산 테스트
- 저장/로드 기능 테스트

### 6.2 통합 테스트
- 전체 게임 플로우 테스트
- UI 상호작용 테스트
- 성능 테스트 (메모리, 프레임레이트)

### 6.3 사용자 테스트
- 직관성 테스트
- 재미 요소 평가
- 밸런스 조정

## 7. Addressable 데이터 로딩 구조

### 7.1 데이터 파일 경로
- **GameData ScriptableObject**: `"Assets/Prefabs/data.asset"`
- **업그레이드 정의**: Excel에서 자동 변환된 `AllUpgradeDefinitions` 리스트

### 7.2 DataLoader.cs 구현 예시
```csharp
public static class DataLoader
{
    public static async Task<GameData> LoadGameData()
    {
        var handle = Addressables.LoadAssetAsync<GameData>("Assets/Prefabs/data.asset");
        var gameData = await handle.Task;
        
        if (gameData == null)
            throw new Exception("GameData를 로드할 수 없습니다.");
            
        return gameData;
    }
}
```

## 8. UI 프리팹 구조 계획

### 8.1 메인 UI 프리팹
```
MainGameCanvas (Canvas)
├── TopPanel (Panel)
│   ├── CurrentPointsText (TextMeshProUGUI) → PointDisplayUI
│   ├── PointsPerClickText (TextMeshProUGUI)
│   └── PointsPerSecondText (TextMeshProUGUI)
├── MiddlePanel (Panel)
│   └── ClickButton (Button) → ClickButtonUI
│       └── ButtonText (TextMeshProUGUI)
└── BottomPanel (Panel)
    ├── ShopTitle (TextMeshProUGUI)
    └── UpgradeScrollView (ScrollView) → UpgradeShopUI
        └── Content (Content)
            └── UpgradeItemPrefab → UpgradeItemUI
```

### 8.2 DIMono 기반 UI 컴포넌트
- 모든 UI 컴포넌트는 `DIMono`를 상속
- `[Inject]` 어트리뷰트로 필요한 서비스 주입
- `Init()` 메서드에서 초기화 로직 구현

## 9. 테스트 계획

### 9.1 DI 주입 테스트
- 모든 서비스가 올바르게 주입되는지 확인
- Bootstrap 프로세스 검증
- 의존성 순환 참조 방지 확인

### 9.2 Addressable 로딩 테스트
- GameData 로딩 성공/실패 케이스 테스트
- 비동기 로딩 중 에러 처리 테스트

### 9.3 게임플레이 테스트
- 클릭 시스템 동작 확인
- 업그레이드 구매 및 효과 적용 테스트
- 저장/로드 기능 검증

이 업데이트된 계획서는 현재 프로젝트의 DI 프레임워크와 Addressable 시스템을 완전히 활용하여 효율적이고 확장 가능한 포인트 생성기 게임을 구현할 수 있도록 설계되었습니다.
