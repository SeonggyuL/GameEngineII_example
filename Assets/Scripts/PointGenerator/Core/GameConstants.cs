namespace PointGenerator.Core
{
    /// <summary>
    /// 게임 상수 정의
    /// </summary>
    public static class GameConstants
    {
        // 기본값
        public const long INITIAL_POINTS = 0L;
        public const long INITIAL_POINTS_PER_CLICK = 1L;
        public const long INITIAL_POINTS_PER_SECOND = 0L;
        
        // 자동 생성 관련
        public const float AUTO_GENERATION_INTERVAL = 1.0f; // 초
        
        // 저장 관련
        public const string SAVE_DATA_KEY = "PointGenerator_SaveData";
        public const float AUTO_SAVE_INTERVAL = 5.0f; // 초
        
        // UI 업데이트 관련
        public const float UI_UPDATE_INTERVAL = 0.1f; // 초
        
        // 숫자 포맷팅
        public const int BIG_NUMBER_THRESHOLD = 1000;
        
        // Addressable 경로
        public const string GAME_DATA_ADDRESS = "Assets/Prefabs/data.asset";
        
        // 업그레이드 관련
        public const double DEFAULT_COST_MULTIPLIER = 1.15;
        
        // 애니메이션 관련
        public const float CLICK_ANIMATION_DURATION = 0.1f;
        public const float UI_FADE_DURATION = 0.3f;
        
        // 프레스티지 관련
        public const long MIN_PRESTIGE_POINTS = 1000000L; // 프레스티지 최소 요구 포인트
        public const double PRESTIGE_POINT_RATIO = 0.001; // 포인트 대비 프레스티지 포인트 비율
        
        // 오프라인 보상 관련
        public const float DEFAULT_MAX_OFFLINE_HOURS = 24f; // 기본 최대 오프라인 시간
        public const float DEFAULT_OFFLINE_EFFICIENCY = 0.5f; // 기본 오프라인 효율성
        public const double MIN_OFFLINE_TIME_SECONDS = 60.0; // 최소 오프라인 시간 (1분)
    }
}
