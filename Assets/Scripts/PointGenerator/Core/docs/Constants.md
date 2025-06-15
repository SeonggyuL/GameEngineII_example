# 게임 상수 명세서

## 개요

`GameConstants.cs`에는 게임 전반에서 사용되는 상수값들이 정의되어 있습니다. 이 상수들은 게임 밸런스 조정과 설정 변경을 용이하게 합니다.

## 상수 분류

### 기본값 (Initial Values)

| 상수명 | 값 | 타입 | 설명 |
|--------|----|----- |------|
| `INITIAL_POINTS` | 0L | long | 게임 시작 시 초기 포인트 |
| `INITIAL_POINTS_PER_CLICK` | 1L | long | 게임 시작 시 클릭당 포인트 |
| `INITIAL_POINTS_PER_SECOND` | 0L | long | 게임 시작 시 초당 포인트 |

**용도**: 새 게임 시작 시 또는 프레스티지 리셋 시 사용되는 기본값들입니다.

### 자동 생성 관련 (Auto Generation)

| 상수명 | 값 | 타입 | 설명 |
|--------|----|----- |------|
| `AUTO_GENERATION_INTERVAL` | 1.0f | float | 자동 포인트 생성 간격 (초) |

**용도**: 방치형 게임의 핵심인 자동 포인트 생성 시스템의 업데이트 주기를 설정합니다.

### 저장 관련 (Save System)

| 상수명 | 값 | 타입 | 설명 |
|--------|----|----- |------|
| `SAVE_DATA_KEY` | "PointGenerator_SaveData" | string | PlayerPrefs 저장 키 |
| `AUTO_SAVE_INTERVAL` | 5.0f | float | 자동 저장 간격 (초) |

**용도**: 게임 데이터의 저장과 로드에 사용되는 설정값들입니다.

### UI 업데이트 관련 (UI Updates)

| 상수명 | 값 | 타입 | 설명 |
|--------|----|----- |------|
| `UI_UPDATE_INTERVAL` | 0.1f | float | UI 업데이트 간격 (초) |
| `BIG_NUMBER_THRESHOLD` | 1000 | int | 큰 숫자 표기 시작점 |

**용도**: UI의 성능과 사용자 경험을 위한 업데이트 주기와 표시 방식을 설정합니다.

### 리소스 경로 (Resource Paths)

| 상수명 | 값 | 타입 | 설명 |
|--------|----|----- |------|
| `GAME_DATA_ADDRESS` | "Assets/Prefabs/data.asset" | string | 게임 데이터 Addressable 경로 |

**용도**: Unity Addressable 시스템을 통해 게임 데이터를 로드할 때 사용되는 경로입니다.

### 업그레이드 관련 (Upgrades)

| 상수명 | 값 | 타입 | 설명 |
|--------|----|----- |------|
| `DEFAULT_COST_MULTIPLIER` | 1.15 | double | 기본 업그레이드 비용 배율 |

**용도**: 업그레이드 구매 시 다음 레벨의 비용을 계산하는 기본 배율입니다.

### 애니메이션 관련 (Animations)

| 상수명 | 값 | 타입 | 설명 |
|--------|----|----- |------|
| `CLICK_ANIMATION_DURATION` | 0.1f | float | 클릭 애니메이션 지속 시간 (초) |
| `UI_FADE_DURATION` | 0.3f | float | UI 페이드 애니메이션 지속 시간 (초) |

**용도**: 사용자 인터페이스의 시각적 피드백을 위한 애니메이션 지속 시간을 설정합니다.

### 프레스티지 관련 (Prestige System)

| 상수명 | 값 | 타입 | 설명 |
|--------|----|----- |------|
| `MIN_PRESTIGE_POINTS` | 1000000L | long | 프레스티지 최소 요구 포인트 |
| `PRESTIGE_POINT_RATIO` | 0.001 | double | 포인트 대비 프레스티지 포인트 비율 |

**용도**: 프레스티지 시스템의 진입 조건과 보상 계산에 사용됩니다.

### 오프라인 보상 관련 (Offline Rewards)

| 상수명 | 값 | 타입 | 설명 |
|--------|----|----- |------|
| `DEFAULT_MAX_OFFLINE_HOURS` | 24f | float | 기본 최대 오프라인 시간 (시간) |
| `DEFAULT_OFFLINE_EFFICIENCY` | 0.5f | float | 기본 오프라인 효율성 (50%) |
| `MIN_OFFLINE_TIME_SECONDS` | 60.0 | double | 최소 오프라인 시간 (초) |

**용도**: 플레이어가 게임을 종료한 후 다시 접속했을 때 주어지는 오프라인 보상을 계산합니다.

## 상수 활용 예시

```csharp
// 새 게임 시작 시
userData.CurrentPoints = GameConstants.INITIAL_POINTS;
userData.PointsPerClick = GameConstants.INITIAL_POINTS_PER_CLICK;

// 프레스티지 가능 여부 확인
bool canPrestige = userData.CurrentPoints >= GameConstants.MIN_PRESTIGE_POINTS;

// 업그레이드 비용 계산
long nextCost = baseCost * Math.Pow(GameConstants.DEFAULT_COST_MULTIPLIER, level);

// 오프라인 보상 계산
if (offlineSeconds >= GameConstants.MIN_OFFLINE_TIME_SECONDS)
{
    // 보상 계산 로직
}
```

## 주의사항

1. **밸런스 조정**: 이 상수들을 변경하면 게임 밸런스에 직접적인 영향을 미칩니다.
2. **저장 호환성**: `SAVE_DATA_KEY` 변경 시 기존 저장 데이터와의 호환성을 고려해야 합니다.
3. **성능 영향**: 업데이트 간격 관련 상수들은 게임 성능에 영향을 줄 수 있습니다.
4. **타입 안전성**: 상수의 타입을 변경할 때는 사용하는 모든 곳의 호환성을 확인해야 합니다. 