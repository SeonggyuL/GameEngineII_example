# PointGenerator Core 시스템 명세서

## 개요

PointGenerator Core 시스템은 클릭 기반 방치형 게임의 핵심 로직을 담당하는 모듈입니다. 의존성 주입(Dependency Injection) 패턴과 인터페이스 기반 설계를 통해 확장 가능하고 테스트 가능한 아키텍처를 제공합니다.

## 폴더 구조

```
Core/
├── GameConstants.cs           # 게임 전역 상수 정의
├── Data/                      # 데이터 구조 정의
│   ├── AchievementData.cs     # 업적 관련 데이터 구조
│   ├── PrestigeData.cs        # 프레스티지 관련 데이터 구조
│   └── GameSaveData.cs        # 게임 저장 데이터 구조
├── Interfaces/                # 인터페이스 정의
│   ├── IGameManager.cs        # 게임 전체 관리 인터페이스
│   ├── IPointManager.cs       # 포인트 관리 인터페이스
│   ├── IUpgradeManager.cs     # 업그레이드 관리 인터페이스
│   ├── IAchievementManager.cs # 업적 관리 인터페이스
│   ├── IPrestigeManager.cs    # 프레스티지 관리 인터페이스
│   ├── IOfflineRewardsManager.cs # 오프라인 보상 관리 인터페이스
│   └── ISaveLoadManager.cs    # 저장/로드 관리 인터페이스
├── Services/                  # 서비스 구현체
│   ├── GameManager.cs         # 게임 전체 관리 서비스
│   ├── PointManager.cs        # 포인트 관리 서비스
│   ├── UpgradeManager.cs      # 업그레이드 관리 서비스
│   ├── AchievementManager.cs  # 업적 관리 서비스
│   ├── PrestigeManager.cs     # 프레스티지 관리 서비스
│   ├── OfflineRewardsManager.cs # 오프라인 보상 관리 서비스
│   └── SaveLoadManager.cs     # 저장/로드 관리 서비스
└── docs/                      # 문서
    ├── README.md              # 이 파일
    ├── Constants.md           # 상수 명세
    ├── Data-Structures.md     # 데이터 구조 명세
    ├── Interfaces.md          # 인터페이스 명세
    ├── Services.md            # 서비스 명세
    └── Architecture.md        # 아키텍처 설명
```

## 핵심 개념

### 1. 의존성 주입 (Dependency Injection)
- 모든 서비스는 `[Inject]` 어트리뷰트를 통해 의존성을 주입받습니다
- 인터페이스를 통한 느슨한 결합으로 테스트와 확장이 용이합니다

### 2. 인터페이스 기반 설계
- 각 관리자는 인터페이스와 구현체로 분리되어 있습니다
- 인터페이스를 통해 계약을 명확히 정의하고 구현 세부사항을 숨깁니다

### 3. 이벤트 기반 통신
- 매니저 간 통신은 주로 이벤트를 통해 이루어집니다
- 느슨한 결합을 유지하면서 상태 변화를 전파합니다

### 4. 데이터 영속성
- UserData와 GameData를 통해 게임 상태를 관리합니다
- JSON 직렬화를 통해 PlayerPrefs에 저장됩니다

## 주요 특징

- **모듈형 아키텍처**: 각 기능이 독립적인 매니저로 분리
- **타입 안전성**: 강타입 언어의 장점을 활용한 안전한 코드
- **확장성**: 새로운 기능 추가가 용이한 구조
- **테스트 가능성**: 인터페이스 기반으로 목(Mock) 객체 활용 가능
- **성능 최적화**: 필요에 따른 지연 초기화와 캐싱

## 문서 구성

각 문서는 다음과 같은 내용을 포함합니다:

- **Constants.md**: 게임에서 사용되는 모든 상수값들
- **Data-Structures.md**: 데이터 클래스들의 구조와 용도
- **Interfaces.md**: 각 인터페이스의 메서드와 이벤트 명세
- **Services.md**: 각 서비스 구현체의 상세 동작 방식
- **Architecture.md**: 전체 시스템의 아키텍처와 데이터 흐름

## 시작하기

1. GameManager를 통해 전체 시스템을 초기화합니다
2. 각 매니저는 Initialize() 메서드를 통해 초기화됩니다
3. 이벤트 구독을 통해 상태 변화를 감지합니다
4. SaveLoadManager를 통해 게임 상태를 저장/로드합니다

자세한 내용은 각 문서를 참고하세요. 