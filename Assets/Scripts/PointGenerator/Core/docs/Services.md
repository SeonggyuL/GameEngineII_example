# 서비스 구현체 명세서

## 개요

Core/Services 폴더에는 각 인터페이스의 구체적인 구현체들이 정의되어 있습니다. 모든 서비스는 의존성 주입을 통해 협력하며, 이벤트 기반으로 통신합니다.

## GameManager.cs

**역할**: 게임 전체 라이프사이클과 매니저들의 조율을 담당하는 최상위 서비스

### 주요 특징
- **매니저 초기화 순서 관리**: 의존성에 따른 올바른 초기화 순서 보장
- **자동 시스템 관리**: 포인트 자동 생성, 자동 저장 등의 시스템 제어
- **애플리케이션 이벤트 처리**: 포커스 변경, 일시정지, 종료 시 적절한 대응

### 의존성
```csharp
[Inject] private GameData gameData;
[Inject] private UserData userData;
[Inject] private IPointManager pointManager;
[Inject] private IUpgradeManager upgradeManager;
[Inject] private ISaveLoadManager saveLoadManager;
[Inject] private IAchievementManager achievementManager;
[Inject] private IPrestigeManager prestigeManager;
```

### 핵심 메서드
- `StartGame()`: 전체 시스템 초기화 및 게임 시작
- `InitializeManagers()`: 각 매니저의 순차적 초기화
- `UpdateGame()`: 매 프레임 호출되어 자동 저장 등을 처리

## PointManager.cs

**역할**: 포인트 시스템의 핵심 로직과 클릭/방치 메커니즘 구현

### 주요 특징
- **실시간 포인트 관리**: 현재 포인트와 생산량 추적
- **자동 생성 시스템**: 코루틴 기반 초당 포인트 자동 생성
- **프레스티지 효과 적용**: 전역 배율 등 프레스티지 보너스 반영

### 핵심 기능
- 클릭당/초당 포인트 계산 및 적용
- 업그레이드 효과와 프레스티지 배율 종합 계산
- 이벤트를 통한 다른 시스템으로의 상태 전파

## UpgradeManager.cs

**역할**: 업그레이드 시스템과 효과 적용 로직 구현

### 주요 특징
- **동적 비용 계산**: 레벨에 따른 비용 증가 공식 적용
- **효과 누적 계산**: 여러 업그레이드의 효과를 종합하여 적용
- **구매 가능성 검증**: 포인트 부족 등의 제약 조건 확인

### 핵심 로직
```csharp
// 업그레이드 비용 계산
long cost = baseCost * Math.Pow(costMultiplier, currentLevel);

// 효과 적용
pointManager.IncreasePointsPerClick(upgradeEffect);
```

## AchievementManager.cs

**역할**: 업적 시스템과 진행도 추적 및 보상 지급

### 주요 특징
- **자동 진행도 추적**: 다른 매니저의 이벤트를 구독하여 자동 업데이트
- **동적 업적 생성**: 코드로 정의된 기본 업적들을 런타임에 생성
- **즉시 보상 지급**: 업적 달성 시 포인트나 보너스를 즉시 적용

### 기본 업적 카테고리
- 포인트 관련: 총 포인트 달성
- 클릭 관련: 총 클릭 수
- 업그레이드 관련: 업그레이드 구매 횟수
- 생산량 관련: 초당/클릭당 포인트 달성
- 숨겨진 업적: 특수 조건 달성

## PrestigeManager.cs

**역할**: 프레스티지 시스템과 장기 진행 메커니즘 구현

### 주요 특징
- **프레스티지 조건 검증**: 최소 포인트 요구량 확인
- **보상 계산**: 현재 포인트 기반 프레스티지 포인트 계산
- **게임 상태 리셋**: 프레스티지 시 포인트와 업그레이드 초기화
- **영구 효과 적용**: 프레스티지 업그레이드를 통한 지속 보너스
- **업그레이드 구매 관리**: 프레스티지 업그레이드의 구매 가능성 확인과 비용 계산

### 핵심 메서드
- `CanAffordPrestigeUpgrade(string codeName, long currentPrestigePoints)`: 현재 보유한 프레스티지 포인트로 특정 업그레이드 구매 가능 여부 확인
- `CalculatePrestigeUpgradeCost(string codeName, int currentLevel)`: 특정 레벨에서의 프레스티지 업그레이드 비용 계산
- `PurchasePrestigeUpgrade(string codeName)`: 프레스티지 업그레이드 구매 처리 및 효과 적용

### 프레스티지 업그레이드 타입
- 전역 배율: 모든 포인트 획득량 증가
- 클릭/방치 배율: 특정 획득 방식 강화
- 업그레이드 할인: 업그레이드 비용 감소
- 오프라인 보너스: 오프라인 효율성 증가

### 비용 계산 공식
```csharp
// 프레스티지 업그레이드 비용 계산
long cost = (long)(baseCost * Math.Pow(costMultiplier, currentLevel));
```

## OfflineRewardsManager.cs

**역할**: 오프라인 보상 계산과 지급 시스템

### 주요 특징
- **시간 기반 계산**: 저장 시간과 현재 시간의 차이 계산
- **효율성 제한**: 오프라인 시 100% 효율이 아닌 제한된 효율성 적용
- **최대 시간 제한**: 무제한 누적을 방지하는 상한선 설정
- **프레스티지 보너스**: 프레스티지 업그레이드에 따른 오프라인 보너스
- **시간 포맷팅**: 오프라인 시간을 사용자 친화적인 형태로 표시

### 계산 공식
```csharp
var effectivePointsPerSecond = basePointsPerSecond * offlineEfficiency * prestigeBonus;
var totalReward = effectivePointsPerSecond * Math.Min(offlineSeconds, maxOfflineSeconds);
```

### 시간 포맷팅
`FormatOfflineTime(double seconds)` 메서드는 초 단위 시간을 다음과 같이 변환합니다:
- 60초 미만: "30초"
- 60초~3600초: "5분"
- 3600초~86400초: "2.5시간"
- 86400초 이상: "1.2일"

## SaveLoadManager.cs

**역할**: 게임 데이터의 영속성 관리와 오프라인 시간 추적

### 주요 특징
- **JSON 직렬화**: Unity의 JsonUtility를 사용한 데이터 저장
- **자동 저장**: 일정 간격으로 자동 저장 수행
- **오프라인 시간 계산**: 저장 시간과 로드 시간 비교
- **에러 처리**: 저장/로드 실패 시 적절한 예외 처리

### 저장 데이터 구조
- GameSaveData 래퍼를 통한 메타데이터 포함
- 저장 시간 정보로 오프라인 시간 계산
- UserData의 완전한 상태 보존

## 서비스 간 협력 패턴

### 1. 이벤트 기반 통신
```csharp
// PointManager에서 발생한 이벤트를 AchievementManager가 구독
pointManager.OnPointsChanged += achievementManager.UpdateProgress;
```

### 2. 직접 메서드 호출
```csharp
// UpgradeManager가 PointManager의 메서드를 직접 호출
pointManager.IncreasePointsPerClick(upgradeEffect);
```

### 3. 데이터 공유
```csharp
// 모든 서비스가 UserData를 공유하여 상태 동기화
[Inject] private UserData userData;
```

## 초기화 순서와 의존성

```
1. GameManager (최상위)
   ↓
2. PointManager, SaveLoadManager (기본 시스템)
   ↓
3. UpgradeManager (PointManager 의존)
   ↓
4. AchievementManager, PrestigeManager (여러 시스템 의존)
   ↓
5. OfflineRewardsManager (PointManager, PrestigeManager 의존)
```

## 성능 고려사항

### 1. 이벤트 구독 정리
```csharp
// 메모리 누수 방지를 위한 이벤트 구독 해제
public void OnDestroy()
{
    pointManager.OnPointsChanged -= OnPointsUpdated;
}
```

### 2. 업데이트 주기 최적화
- UI 업데이트: 0.1초 간격
- 자동 저장: 5초 간격
- 포인트 자동 생성: 1초 간격

### 3. 계산 최적화
- 비용 계산 시 캐싱 활용
- 불필요한 재계산 방지
- 이벤트 발생 횟수 제한

## 확장 가이드

새로운 서비스를 추가할 때는 다음을 고려하세요:

1. **인터페이스 먼저 정의**: 구현 전에 인터페이스부터 설계
2. **의존성 주입 활용**: `[Inject]` 어트리뷰트로 의존성 선언
3. **이벤트 설계**: 상태 변화를 외부에 알릴 이벤트 정의
4. **초기화 순서**: 다른 서비스와의 의존성 고려
5. **에러 처리**: 예상 가능한 오류 상황에 대한 대응
6. **테스트 가능성**: 단위 테스트가 가능하도록 설계

## 디버깅과 로깅

모든 서비스는 일관된 로깅 패턴을 사용합니다:

```csharp
Debug.Log($"[{GetType().Name}] 작업 완료: {details}");
Debug.LogWarning($"[{GetType().Name}] 경고: {warning}");
Debug.LogError($"[{GetType().Name}] 오류: {error}");
```

이를 통해 게임 실행 중 각 시스템의 동작을 추적할 수 있습니다. 