#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEngine.EventSystems;
using TMPro;
using System.IO;

namespace PointGenerator.UI.Editor
{
    /// <summary>
    /// PointGenerator 씬 자동 셋업 도구
    /// </summary>
    public class SceneSetupGenerator : EditorWindow
    {
        private const string PREFABS_FOLDER = "Assets/Prefabs";
        private const string UI_PREFABS_FOLDER = "Assets/Prefabs/UI";
        
        private bool setupEventSystem = true;
        private bool setupMainCamera = true;
        private bool setupUICanvas = true;
        private bool setupHeader = true;
        private bool setupMainContent = true;
        private bool setupPanels = true;
        private bool setupPopups = true;
        
        [MenuItem("PointGenerator/Setup Game Scene")]
        public static void ShowWindow()
        {
            GetWindow<SceneSetupGenerator>("Scene Setup Generator");
        }
        
        private void OnGUI()
        {
            GUILayout.Label("PointGenerator Scene Setup Generator", EditorStyles.boldLabel);
            GUILayout.Space(10);
            
            GUILayout.Label("현재 씬에 PointGenerator UI 시스템을 자동으로 설정합니다.", EditorStyles.helpBox);
            GUILayout.Space(10);
            
            // 설정 옵션들
            GUILayout.Label("생성할 컴포넌트들:", EditorStyles.boldLabel);
            setupEventSystem = EditorGUILayout.Toggle("EventSystem", setupEventSystem);
            setupMainCamera = EditorGUILayout.Toggle("Main Camera (없을 경우만)", setupMainCamera);
            setupUICanvas = EditorGUILayout.Toggle("UI Canvas", setupUICanvas);
            setupHeader = EditorGUILayout.Toggle("Header (PointDisplay, Navigation)", setupHeader);
            setupMainContent = EditorGUILayout.Toggle("MainContent (ClickArea, UpgradeShop)", setupMainContent);
            setupPanels = EditorGUILayout.Toggle("Panels (Achievement, Prestige)", setupPanels);
            setupPopups = EditorGUILayout.Toggle("Popups (OfflineRewards)", setupPopups);
            
            GUILayout.Space(20);
            
            if (GUILayout.Button("Setup Complete Scene", GUILayout.Height(40)))
            {
                SetupCompleteScene();
            }
            
            GUILayout.Space(10);
            
            if (GUILayout.Button("Clear Current Scene", GUILayout.Height(30)))
            {
                if (EditorUtility.DisplayDialog("경고", "현재 씬의 모든 오브젝트를 삭제합니다. 계속하시겠습니까?", "삭제", "취소"))
                {
                    ClearScene();
                }
            }
        }
        
        private void SetupCompleteScene()
        {
            Debug.Log("[SceneSetupGenerator] PointGenerator 씬 설정을 시작합니다...");
            
            GameObject uiCanvas = null;
            
            // 1. EventSystem 설정
            if (setupEventSystem)
            {
                SetupEventSystem();
            }
            
            // 2. Main Camera 설정
            if (setupMainCamera)
            {
                SetupMainCamera();
            }
            
            // 3. UI Canvas 설정
            if (setupUICanvas)
            {
                uiCanvas = SetupUICanvas();
            }
            else
            {
                uiCanvas = GameObject.FindObjectOfType<Canvas>()?.gameObject;
            }
            
            if (uiCanvas == null)
            {
                Debug.LogError("[SceneSetupGenerator] UI Canvas를 찾을 수 없습니다!");
                return;
            }
            
            // 4. Header 설정
            if (setupHeader)
            {
                SetupHeader(uiCanvas);
            }
            
            // 5. MainContent 설정
            if (setupMainContent)
            {
                SetupMainContent(uiCanvas);
            }
            
            // 6. Panels 설정
            if (setupPanels)
            {
                SetupPanels(uiCanvas);
            }
            
            // 7. Popups 설정
            if (setupPopups)
            {
                SetupPopups(uiCanvas);
            }
            
            // 8. 메인 컨트롤러 설정
            SetupMainController(uiCanvas);
            
            Debug.Log("[SceneSetupGenerator] PointGenerator 씬 설정이 완료되었습니다!");
        }
        
        private void ClearScene()
        {
            var allObjects = FindObjectsOfType<GameObject>();
            foreach (var obj in allObjects)
            {
                if (obj.transform.parent == null) // 루트 오브젝트만
                {
                    DestroyImmediate(obj);
                }
            }
            Debug.Log("[SceneSetupGenerator] 씬이 초기화되었습니다.");
        }
        
        #region Setup Methods
        
        private void SetupEventSystem()
        {
            if (FindObjectOfType<EventSystem>() != null)
            {
                Debug.Log("[SceneSetupGenerator] EventSystem이 이미 존재합니다.");
                return;
            }
            
            var eventSystemObj = new GameObject("EventSystem");
            eventSystemObj.AddComponent<EventSystem>();
            eventSystemObj.AddComponent<StandaloneInputModule>();
            
            Debug.Log("[SceneSetupGenerator] EventSystem 생성 완료");
        }
        
        private void SetupMainCamera()
        {
            if (Camera.main != null)
            {
                Debug.Log("[SceneSetupGenerator] Main Camera가 이미 존재합니다.");
                return;
            }
            
            var cameraObj = new GameObject("Main Camera");
            var camera = cameraObj.AddComponent<Camera>();
            camera.tag = "MainCamera";
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.1f, 0.1f, 0.15f, 1f);
            
            cameraObj.transform.position = new Vector3(0, 0, -10);
            
            Debug.Log("[SceneSetupGenerator] Main Camera 생성 완료");
        }
        
        private GameObject SetupUICanvas()
        {
            var canvasObj = new GameObject("UI Canvas");
            var canvas = canvasObj.AddComponent<Canvas>();
            var canvasScaler = canvasObj.AddComponent<CanvasScaler>();
            var graphicRaycaster = canvasObj.AddComponent<GraphicRaycaster>();
            
            // Canvas 설정
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.pixelPerfect = true;
            canvas.sortingOrder = 0;
            
            // Canvas Scaler 설정 - 모바일 Portrait 최적화
            canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasScaler.referenceResolution = new Vector2(1080, 1920); // 모바일 Portrait 기준 해상도
            canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            canvasScaler.matchWidthOrHeight = 0.0f; // Width 기준으로 스케일링 (Portrait에서 중요)
            
            // Graphic Raycaster 설정
            graphicRaycaster.ignoreReversedGraphics = true;
            graphicRaycaster.blockingObjects = GraphicRaycaster.BlockingObjects.None;
            
            Debug.Log("[SceneSetupGenerator] UI Canvas 생성 완료 (모바일 Portrait 최적화)");
            return canvasObj;
        }
        
        private void SetupHeader(GameObject canvas)
        {
            var header = new GameObject("Header");
            header.transform.SetParent(canvas.transform);
            
            // UI 오브젝트에는 RectTransform이 필요
            if (header.GetComponent<RectTransform>() == null)
                header.AddComponent<RectTransform>();
                
            // 모바일 Portrait: 상단 안전 영역 고려하여 높이 증가
            SetRectTransform(header, AnchorPresets.TopStretch, new Vector2(20, -140), new Vector2(-20, 120));
            
            // PointDisplay 생성
            SetupPointDisplay(header);
            
            // Navigation Buttons 생성
            SetupNavigationButtons(header);
            
            Debug.Log("[SceneSetupGenerator] Header 설정 완료");
        }
        
        private void SetupPointDisplay(GameObject header)
        {
            var pointDisplay = new GameObject("PointDisplay");
            pointDisplay.transform.SetParent(header.transform);
            
            // UI 오브젝트에는 RectTransform이 필요
            if (pointDisplay.GetComponent<RectTransform>() == null)
                pointDisplay.AddComponent<RectTransform>();
                
            // 모바일: 왼쪽 영역 전체 사용, 네비게이션 버튼 공간 확보
            SetRectTransform(pointDisplay, AnchorPresets.StretchAll, new Vector2(10, 10), new Vector2(-120, -10));
            
            // CanvasGroup 추가
            var canvasGroup = pointDisplay.AddComponent<CanvasGroup>();
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
            
            // PointsText - 더 큰 폰트 크기
            var pointsText = CreateTextMeshPro(pointDisplay.transform, "PointsText", "Points: 0", 28, Color.white);
            SetRectTransform(pointsText, AnchorPresets.TopLeft, new Vector2(0, -5), new Vector2(250, 35));
            
            // PerSecondText - 적절한 위치
            var perSecondText = CreateTextMeshPro(pointDisplay.transform, "PerSecondText", "Per Second: 0", 18, new Color(0.8f, 0.8f, 0.8f));
            SetRectTransform(perSecondText, AnchorPresets.TopLeft, new Vector2(0, -45), new Vector2(250, 25));
            
            // TotalEarnedText - 세 번째 줄로 배치
            var totalText = CreateTextMeshPro(pointDisplay.transform, "TotalEarnedText", "Total: 0", 16, new Color(0.7f, 0.7f, 0.7f));
            SetRectTransform(totalText, AnchorPresets.TopLeft, new Vector2(0, -75), new Vector2(250, 25));
        }
        
        private void SetupNavigationButtons(GameObject header)
        {
            var navButtons = new GameObject("Navigation Buttons");
            navButtons.transform.SetParent(header.transform);
            
            // UI 오브젝트에는 RectTransform이 필요
            if (navButtons.GetComponent<RectTransform>() == null)
                navButtons.AddComponent<RectTransform>();
                
            // 모바일: 오른쪽에 세로로 배치
            SetRectTransform(navButtons, AnchorPresets.TopRight, new Vector2(-10, -10), new Vector2(100, 100));
            
            // Achievements Button - 위쪽
            var achievementsBtn = CreateButton(navButtons.transform, "AchievementsButton", "Achieve");
            SetRectTransform(achievementsBtn, AnchorPresets.TopStretch, new Vector2(0, -45), new Vector2(0, 40));
            
            // Prestige Button - 아래쪽
            var prestigeBtn = CreateButton(navButtons.transform, "PrestigeButton", "Prestige");
            SetRectTransform(prestigeBtn, AnchorPresets.TopStretch, new Vector2(0, -90), new Vector2(0, 40));
        }
        
        private void SetupMainContent(GameObject canvas)
        {
            var mainContent = new GameObject("MainContent");
            mainContent.transform.SetParent(canvas.transform);
            
            // UI 오브젝트에는 RectTransform이 필요
            if (mainContent.GetComponent<RectTransform>() == null)
                mainContent.AddComponent<RectTransform>();
                
            // 모바일: Header와 하단 업그레이드 영역 사이의 중앙 공간 사용
            SetRectTransform(mainContent, AnchorPresets.StretchAll, new Vector2(20, 140), new Vector2(-20, -300));
            
            // ClickArea 생성
            SetupClickArea(mainContent);
            
            // UpgradeShop 생성 - 하단으로 이동
            SetupUpgradeShop(canvas); // canvas에 직접 추가
            
            Debug.Log("[SceneSetupGenerator] MainContent 설정 완료");
        }
        
        private void SetupClickArea(GameObject mainContent)
        {
            var clickArea = new GameObject("ClickArea");
            clickArea.transform.SetParent(mainContent.transform);
            
            // UI 오브젝트에는 RectTransform이 필요
            if (clickArea.GetComponent<RectTransform>() == null)
                clickArea.AddComponent<RectTransform>();
                
            // 모바일: 중앙 전체 영역 사용
            SetRectTransform(clickArea, AnchorPresets.StretchAll, new Vector2(20, 20), new Vector2(-20, -20));
            
            // ClickButton - 화면 중앙에 적절한 크기
            var clickButton = CreateButton(clickArea.transform, "ClickButton", "Click!\n+1 pts");
            SetRectTransform(clickButton, AnchorPresets.MiddleCenter, Vector2.zero, new Vector2(250, 250));
            
            // Button 이미지 스타일 설정
            var buttonImage = clickButton.GetComponent<Image>();
            buttonImage.color = new Color(0.2f, 0.6f, 1f, 1f);
            
            // ButtonText 스타일 조정
            var buttonText = clickButton.transform.Find("Text").GetComponent<TextMeshProUGUI>();
            buttonText.fontSize = 24; // 모바일에 적합한 크기
            buttonText.alignment = TextAlignmentOptions.Center;
        }
        
        private void SetupUpgradeShop(GameObject canvas)
        {
            var upgradeShop = new GameObject("UpgradeShop");
            upgradeShop.transform.SetParent(canvas.transform);
            
            // UI 오브젝트에는 RectTransform이 필요
            if (upgradeShop.GetComponent<RectTransform>() == null)
                upgradeShop.AddComponent<RectTransform>();
                
            // 모바일: 화면 하단에 고정, 안전 영역 고려
            SetRectTransform(upgradeShop, AnchorPresets.BottomStretch, new Vector2(20, 20), new Vector2(-20, 280));
            
            // Background Image
            var backgroundImage = upgradeShop.AddComponent<Image>();
            backgroundImage.color = new Color(0.1f, 0.1f, 0.2f, 0.8f);
            
            // Title Text - 위치 조정
            var titleText = CreateTextMeshPro(upgradeShop.transform, "TitleText", "Upgrades", 20, Color.white);
            SetRectTransform(titleText, AnchorPresets.TopLeft, new Vector2(15, -15), new Vector2(150, 30));
            
            // ScrollView 생성 - 제목 아래 여백 확보
            SetupScrollView(upgradeShop, "UpgradeScrollView", new Vector2(10, 50), new Vector2(-30, -10));
        }
        
        private void SetupPanels(GameObject canvas)
        {
            var panels = new GameObject("Panels");
            panels.transform.SetParent(canvas.transform);
            
            // UI 오브젝트에는 RectTransform이 필요
            if (panels.GetComponent<RectTransform>() == null)
                panels.AddComponent<RectTransform>();
                
            SetRectTransform(panels, AnchorPresets.StretchAll, Vector2.zero, Vector2.zero);
            
            // Achievement Panel
            SetupAchievementPanel(panels);
            
            // Prestige Panel
            SetupPrestigePanel(panels);
            
            Debug.Log("[SceneSetupGenerator] Panels 설정 완료");
        }
        
        private void SetupAchievementPanel(GameObject panels)
        {
            var achievementPanel = new GameObject("AchievementPanel");
            achievementPanel.transform.SetParent(panels.transform);
            
            // UI 오브젝트에는 RectTransform이 필요
            if (achievementPanel.GetComponent<RectTransform>() == null)
                achievementPanel.AddComponent<RectTransform>();
                
            SetRectTransform(achievementPanel, AnchorPresets.StretchAll, Vector2.zero, Vector2.zero);
            
            // Canvas Group
            var canvasGroup = achievementPanel.AddComponent<CanvasGroup>();
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
            
            // Background
            var backgroundImage = achievementPanel.AddComponent<Image>();
            backgroundImage.color = new Color(0, 0, 0, 0.7f);
            
            // Panel Container - 모바일에 맞는 크기와 위치
            var panelContainer = new GameObject("PanelContainer");
            panelContainer.transform.SetParent(achievementPanel.transform);
            
            // UI 오브젝트에는 RectTransform이 필요
            if (panelContainer.GetComponent<RectTransform>() == null)
                panelContainer.AddComponent<RectTransform>();
                
            // 모바일: 화면의 대부분을 차지하되 여백 확보
            SetRectTransform(panelContainer, AnchorPresets.StretchAll, new Vector2(40, 100), new Vector2(-40, -100));
            
            var containerImage = panelContainer.AddComponent<Image>();
            containerImage.color = new Color(0.15f, 0.15f, 0.25f, 0.95f);
            
            // Header Panel
            var headerPanel = new GameObject("HeaderPanel");
            headerPanel.transform.SetParent(panelContainer.transform);
            
            // UI 오브젝트에는 RectTransform이 필요
            if (headerPanel.GetComponent<RectTransform>() == null)
                headerPanel.AddComponent<RectTransform>();
                
            SetRectTransform(headerPanel, AnchorPresets.TopStretch, new Vector2(15, -60), new Vector2(-15, 50));
            
            var titleText = CreateTextMeshPro(headerPanel.transform, "TitleText", "Achievements", 22, Color.white);
            SetRectTransform(titleText, AnchorPresets.MiddleLeft, new Vector2(10, 0), new Vector2(200, 30));
            
            var closeButton = CreateButton(headerPanel.transform, "CloseButton", "X");
            SetRectTransform(closeButton, AnchorPresets.MiddleRight, new Vector2(-10, 0), new Vector2(50, 40));
            
            // Content ScrollView - 헤더 아래 여백 확보
            SetupScrollView(panelContainer, "AchievementScrollView", new Vector2(15, -70), new Vector2(-15, 15));
            
            achievementPanel.SetActive(false);
        }
        
        private void SetupPrestigePanel(GameObject panels)
        {
            var prestigePanel = new GameObject("PrestigePanel");
            prestigePanel.transform.SetParent(panels.transform);
            
            // UI 오브젝트에는 RectTransform이 필요
            if (prestigePanel.GetComponent<RectTransform>() == null)
                prestigePanel.AddComponent<RectTransform>();
                
            SetRectTransform(prestigePanel, AnchorPresets.StretchAll, Vector2.zero, Vector2.zero);
            
            // Canvas Group
            var canvasGroup = prestigePanel.AddComponent<CanvasGroup>();
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
            
            // Background
            var backgroundImage = prestigePanel.AddComponent<Image>();
            backgroundImage.color = new Color(0, 0, 0, 0.7f);
            
            // Panel Container - 모바일에 맞는 크기
            var panelContainer = new GameObject("PanelContainer");
            panelContainer.transform.SetParent(prestigePanel.transform);
            
            // UI 오브젝트에는 RectTransform이 필요
            if (panelContainer.GetComponent<RectTransform>() == null)
                panelContainer.AddComponent<RectTransform>();
                
            // 모바일: 세로가 더 길어도 적절한 크기 유지
            SetRectTransform(panelContainer, AnchorPresets.StretchAll, new Vector2(40, 80), new Vector2(-40, -80));
            
            var containerImage = panelContainer.AddComponent<Image>();
            containerImage.color = new Color(0.1f, 0.05f, 0.2f, 0.95f);
            
            // Header Panel
            var headerPanel = new GameObject("HeaderPanel");
            headerPanel.transform.SetParent(panelContainer.transform);
            
            // UI 오브젝트에는 RectTransform이 필요
            if (headerPanel.GetComponent<RectTransform>() == null)
                headerPanel.AddComponent<RectTransform>();
                
            SetRectTransform(headerPanel, AnchorPresets.TopStretch, new Vector2(15, -60), new Vector2(-15, 50));
            
            var titleText = CreateTextMeshPro(headerPanel.transform, "TitleText", "Prestige", 22, Color.white);
            SetRectTransform(titleText, AnchorPresets.MiddleLeft, new Vector2(10, 0), new Vector2(200, 30));
            
            var closeButton = CreateButton(headerPanel.transform, "CloseButton", "X");
            SetRectTransform(closeButton, AnchorPresets.MiddleRight, new Vector2(-10, 0), new Vector2(50, 40));
            
            // Info Panel
            var infoPanel = new GameObject("InfoPanel");
            infoPanel.transform.SetParent(panelContainer.transform);
            
            // UI 오브젝트에는 RectTransform이 필요
            if (infoPanel.GetComponent<RectTransform>() == null)
                infoPanel.AddComponent<RectTransform>();
                
            SetRectTransform(infoPanel, AnchorPresets.TopStretch, new Vector2(15, -130), new Vector2(-15, 70));
            
            var prestigeInfoText = CreateTextMeshPro(infoPanel.transform, "PrestigeInfoText", "Prestige Points: 0", 18, Color.yellow);
            SetRectTransform(prestigeInfoText, AnchorPresets.TopLeft, new Vector2(10, -10), new Vector2(250, 30));
            
            var prestigeButton = CreateButton(infoPanel.transform, "PrestigeButton", "PRESTIGE");
            SetRectTransform(prestigeButton, AnchorPresets.TopRight, new Vector2(-10, -10), new Vector2(140, 50));
            
            // Content ScrollView
            SetupScrollView(panelContainer, "PrestigeScrollView", new Vector2(15, -140), new Vector2(-15, 15));
            
            prestigePanel.SetActive(false);
        }
        
        private void SetupPopups(GameObject canvas)
        {
            var popups = new GameObject("Popups");
            popups.transform.SetParent(canvas.transform);
            
            // UI 오브젝트에는 RectTransform이 필요
            if (popups.GetComponent<RectTransform>() == null)
                popups.AddComponent<RectTransform>();
                
            SetRectTransform(popups, AnchorPresets.StretchAll, Vector2.zero, Vector2.zero);
            
            // Offline Rewards Popup
            SetupOfflineRewardsPopup(popups);
            
            Debug.Log("[SceneSetupGenerator] Popups 설정 완료");
        }
        
        private void SetupOfflineRewardsPopup(GameObject popups)
        {
            var offlineRewardsPopup = new GameObject("OfflineRewardsPopup");
            offlineRewardsPopup.transform.SetParent(popups.transform);
            
            // UI 오브젝트에는 RectTransform이 필요
            if (offlineRewardsPopup.GetComponent<RectTransform>() == null)
                offlineRewardsPopup.AddComponent<RectTransform>();
                
            SetRectTransform(offlineRewardsPopup, AnchorPresets.StretchAll, Vector2.zero, Vector2.zero);
            
            // Canvas Group
            var canvasGroup = offlineRewardsPopup.AddComponent<CanvasGroup>();
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
            
            // Background
            var backgroundImage = offlineRewardsPopup.AddComponent<Image>();
            backgroundImage.color = new Color(0, 0, 0, 0.7f);
            
            // Popup Panel - 모바일에 맞는 크기
            var popupPanel = new GameObject("PopupPanel");
            popupPanel.transform.SetParent(offlineRewardsPopup.transform);
            
            // UI 오브젝트에는 RectTransform이 필요
            if (popupPanel.GetComponent<RectTransform>() == null)
                popupPanel.AddComponent<RectTransform>();
                
            // 모바일: 적절한 크기의 팝업, 세로 비율 고려
            SetRectTransform(popupPanel, AnchorPresets.MiddleCenter, Vector2.zero, new Vector2(400, 350));
            
            var popupPanelCanvasGroup = popupPanel.AddComponent<CanvasGroup>();
            var popupImage = popupPanel.AddComponent<Image>();
            popupImage.color = new Color(0.15f, 0.15f, 0.25f, 0.95f);
            
            // Header Panel
            var headerPanel = new GameObject("HeaderPanel");
            headerPanel.transform.SetParent(popupPanel.transform);
            
            // UI 오브젝트에는 RectTransform이 필요
            if (headerPanel.GetComponent<RectTransform>() == null)
                headerPanel.AddComponent<RectTransform>();
                
            SetRectTransform(headerPanel, AnchorPresets.TopStretch, new Vector2(15, -50), new Vector2(-15, 40));
            
            var titleText = CreateTextMeshPro(headerPanel.transform, "TitleText", "Welcome Back!", 20, Color.white);
            SetRectTransform(titleText, AnchorPresets.MiddleLeft, new Vector2(10, 0), new Vector2(250, 30));
            
            var closeButton = CreateButton(headerPanel.transform, "CloseButton", "X");
            SetRectTransform(closeButton, AnchorPresets.MiddleRight, new Vector2(-10, 0), new Vector2(45, 35));
            
            // Info Panel - 정보 표시 영역
            var infoPanel = new GameObject("InfoPanel");
            infoPanel.transform.SetParent(popupPanel.transform);
            
            // UI 오브젝트에는 RectTransform이 필요
            if (infoPanel.GetComponent<RectTransform>() == null)
                infoPanel.AddComponent<RectTransform>();
                
            SetRectTransform(infoPanel, AnchorPresets.MiddleStretch, new Vector2(20, -80), new Vector2(-20, 80));
            
            var timeText = CreateTextMeshPro(infoPanel.transform, "TimeText", "You were away for: 0h 0m", 16, Color.cyan);
            SetRectTransform(timeText, AnchorPresets.TopCenter, new Vector2(0, -20), new Vector2(320, 25));
            
            var rewardText = CreateTextMeshPro(infoPanel.transform, "RewardText", "You earned: 0 points", 18, Color.yellow);
            SetRectTransform(rewardText, AnchorPresets.TopCenter, new Vector2(0, -60), new Vector2(320, 30));
            
            // Button Panel - 버튼 영역
            var buttonPanel = new GameObject("ButtonPanel");
            buttonPanel.transform.SetParent(popupPanel.transform);
            
            // UI 오브젝트에는 RectTransform이 필요
            if (buttonPanel.GetComponent<RectTransform>() == null)
                buttonPanel.AddComponent<RectTransform>();
                
            SetRectTransform(buttonPanel, AnchorPresets.BottomStretch, new Vector2(20, 15), new Vector2(-20, 60));
            
            var horizontalLayout = buttonPanel.AddComponent<HorizontalLayoutGroup>();
            horizontalLayout.spacing = 15;
            horizontalLayout.childAlignment = TextAnchor.MiddleCenter;
            horizontalLayout.childControlWidth = false;
            horizontalLayout.childControlHeight = false;
            
            var claimButton = CreateButton(buttonPanel.transform, "ClaimButton", "CLAIM");
            SetRectTransform(claimButton, AnchorPresets.MiddleCenter, Vector2.zero, new Vector2(120, 45));
            
            offlineRewardsPopup.SetActive(false);
        }
        
        private void SetupMainController(GameObject canvas)
        {
            // PointClickerUI 컴포넌트 추가 시도
            if (canvas.GetComponent<PointGenerator.UI.PointClickerUI>() == null)
            {
                Debug.Log("[SceneSetupGenerator] PointClickerUI 컴포넌트를 찾을 수 없어 참조 설정을 건너뜁니다.");
                Debug.Log("[SceneSetupGenerator] 수동으로 PointClickerUI 컴포넌트를 추가하고 참조를 설정해주세요.");
            }
        }
        
        #endregion
        
        #region Helper Methods
        
        private GameObject SetupScrollView(GameObject parent, string name, Vector2? offsetMin = null, Vector2? offsetMax = null)
        {
            var scrollView = new GameObject(name);
            scrollView.transform.SetParent(parent.transform);
            
            // UI 오브젝트에는 RectTransform이 필요
            if (scrollView.GetComponent<RectTransform>() == null)
                scrollView.AddComponent<RectTransform>();
            
            var scrollRect = scrollView.AddComponent<ScrollRect>();
            var scrollViewRect = scrollView.GetComponent<RectTransform>();
            
            // ScrollView RectTransform 설정
            if (offsetMin.HasValue && offsetMax.HasValue)
            {
                SetRectTransform(scrollView, AnchorPresets.StretchAll, offsetMin.Value, offsetMax.Value);
            }
            else
            {
                SetRectTransform(scrollView, AnchorPresets.StretchAll, new Vector2(10, 40), new Vector2(-30, -10));
            }
            
            // Viewport
            var viewport = new GameObject("Viewport");
            viewport.transform.SetParent(scrollView.transform);
            
            // UI 오브젝트에는 RectTransform이 필요
            if (viewport.GetComponent<RectTransform>() == null)
                viewport.AddComponent<RectTransform>();
                
            SetRectTransform(viewport, AnchorPresets.StretchAll, new Vector2(0, 0), new Vector2(-20, 0));
            
            var mask = viewport.AddComponent<Mask>();
            var viewportImage = viewport.AddComponent<Image>();
            viewportImage.color = new Color(1, 1, 1, 0.01f);
            mask.showMaskGraphic = false;
            
            // Content
            var content = new GameObject("Content");
            content.transform.SetParent(viewport.transform);
            
            // UI 오브젝트에는 RectTransform이 필요
            if (content.GetComponent<RectTransform>() == null)
                content.AddComponent<RectTransform>();
                
            SetRectTransform(content, AnchorPresets.TopStretch, Vector2.zero, Vector2.zero);
            
            var contentRect = content.GetComponent<RectTransform>();
            contentRect.pivot = new Vector2(0.5f, 1f);
            
            var verticalLayout = content.AddComponent<VerticalLayoutGroup>();
            verticalLayout.padding = new RectOffset(10, 10, 10, 10);
            verticalLayout.spacing = 5;
            verticalLayout.childAlignment = TextAnchor.UpperCenter;
            verticalLayout.childControlWidth = true;
            verticalLayout.childControlHeight = false;
            verticalLayout.childForceExpandWidth = true;
            verticalLayout.childForceExpandHeight = false;
            
            var contentSizeFitter = content.AddComponent<ContentSizeFitter>();
            contentSizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            
            // Scrollbar
            var scrollbar = new GameObject("Scrollbar");
            scrollbar.transform.SetParent(scrollView.transform);
            
            // UI 오브젝트에는 RectTransform이 필요
            if (scrollbar.GetComponent<RectTransform>() == null)
                scrollbar.AddComponent<RectTransform>();
                
            SetRectTransform(scrollbar, AnchorPresets.RightStretch, new Vector2(-20, 0), new Vector2(0, 0));
            
            var scrollbarComponent = scrollbar.AddComponent<Scrollbar>();
            var scrollbarImage = scrollbar.AddComponent<Image>();
            scrollbarImage.color = new Color(0.3f, 0.3f, 0.3f, 0.8f);
            
            var slidingArea = new GameObject("Sliding Area");
            slidingArea.transform.SetParent(scrollbar.transform);
            
            // UI 오브젝트에는 RectTransform이 필요
            if (slidingArea.GetComponent<RectTransform>() == null)
                slidingArea.AddComponent<RectTransform>();
                
            SetRectTransform(slidingArea, AnchorPresets.StretchAll, Vector2.zero, Vector2.zero);
            
            var handle = new GameObject("Handle");
            handle.transform.SetParent(slidingArea.transform);
            
            // UI 오브젝트에는 RectTransform이 필요
            if (handle.GetComponent<RectTransform>() == null)
                handle.AddComponent<RectTransform>();
                
            SetRectTransform(handle, AnchorPresets.StretchAll, Vector2.zero, Vector2.zero);
            
            var handleImage = handle.AddComponent<Image>();
            handleImage.color = new Color(0.6f, 0.6f, 0.6f, 1f);
            
            // ScrollRect 설정
            scrollRect.content = contentRect;
            scrollRect.viewport = viewport.GetComponent<RectTransform>();
            scrollRect.verticalScrollbar = scrollbarComponent;
            scrollRect.movementType = ScrollRect.MovementType.Elastic;
            scrollRect.elasticity = 0.1f;
            scrollRect.scrollSensitivity = 1f;
            scrollRect.vertical = true;
            scrollRect.horizontal = false;
            
            // Scrollbar 설정
            scrollbarComponent.handleRect = handle.GetComponent<RectTransform>();
            scrollbarComponent.direction = Scrollbar.Direction.TopToBottom;
            
            return scrollView;
        }
        
        private GameObject CreateTextMeshPro(Transform parent, string name, string text, int fontSize, Color color)
        {
            var textObj = new GameObject(name);
            textObj.transform.SetParent(parent);
            var rectTransform = textObj.AddComponent<RectTransform>();
            var textComponent = textObj.AddComponent<TextMeshProUGUI>();
            
            textComponent.text = text;
            textComponent.fontSize = fontSize;
            textComponent.color = color;
            textComponent.alignment = TextAlignmentOptions.MidlineLeft;
            
            // TextMeshPro 기본 설정
            textComponent.enableWordWrapping = false;
            textComponent.overflowMode = TextOverflowModes.Ellipsis;
            
            // 기본 폰트는 TextMeshPro 기본 폰트 사용
            var defaultFont = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
            if (defaultFont != null)
            {
                textComponent.font = defaultFont;
            }
            
            return textObj;
        }
        
        private GameObject CreateButton(Transform parent, string name, string buttonText)
        {
            var buttonObj = new GameObject(name);
            buttonObj.transform.SetParent(parent);
            var rectTransform = buttonObj.AddComponent<RectTransform>();
            var image = buttonObj.AddComponent<Image>();
            var button = buttonObj.AddComponent<Button>();
            
            image.color = new Color(0.2f, 0.6f, 1f, 1f);
            button.targetGraphic = image;
            
            // Button Text 추가 (TextMeshPro 사용)
            var textObj = new GameObject("Text");
            textObj.transform.SetParent(buttonObj.transform);
            var textRectTransform = textObj.AddComponent<RectTransform>();
            var text = textObj.AddComponent<TextMeshProUGUI>();
            
            text.text = buttonText;
            text.fontSize = 14;
            text.color = Color.white;
            text.alignment = TextAlignmentOptions.Center;
            text.enableWordWrapping = false;
            
            // 기본 폰트 설정
            var defaultFont = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
            if (defaultFont != null)
            {
                text.font = defaultFont;
            }
            
            // Text RectTransform 설정
            textRectTransform.anchorMin = Vector2.zero;
            textRectTransform.anchorMax = Vector2.one;
            textRectTransform.offsetMin = Vector2.zero;
            textRectTransform.offsetMax = Vector2.zero;
            
            return buttonObj;
        }
        
        private void SetRectTransform(GameObject obj, AnchorPresets preset, Vector2 offsetMin, Vector2 offsetMax)
        {
            var rectTransform = obj.GetComponent<RectTransform>();
            
            switch (preset)
            {
                case AnchorPresets.TopLeft:
                    rectTransform.anchorMin = new Vector2(0, 1);
                    rectTransform.anchorMax = new Vector2(0, 1);
                    rectTransform.pivot = new Vector2(0, 1);
                    rectTransform.anchoredPosition = offsetMin;
                    rectTransform.sizeDelta = offsetMax;
                    break;
                case AnchorPresets.TopRight:
                    rectTransform.anchorMin = new Vector2(1, 1);
                    rectTransform.anchorMax = new Vector2(1, 1);
                    rectTransform.pivot = new Vector2(1, 1);
                    rectTransform.anchoredPosition = offsetMin;
                    rectTransform.sizeDelta = offsetMax;
                    break;
                case AnchorPresets.BottomLeft:
                    rectTransform.anchorMin = new Vector2(0, 0);
                    rectTransform.anchorMax = new Vector2(0, 0);
                    rectTransform.pivot = new Vector2(0, 0);
                    rectTransform.anchoredPosition = offsetMin;
                    rectTransform.sizeDelta = offsetMax;
                    break;
                case AnchorPresets.MiddleLeft:
                    rectTransform.anchorMin = new Vector2(0, 0.5f);
                    rectTransform.anchorMax = new Vector2(0, 0.5f);
                    rectTransform.pivot = new Vector2(0, 0.5f);
                    rectTransform.anchoredPosition = offsetMin;
                    rectTransform.sizeDelta = offsetMax;
                    break;
                case AnchorPresets.MiddleRight:
                    rectTransform.anchorMin = new Vector2(1, 0.5f);
                    rectTransform.anchorMax = new Vector2(1, 0.5f);
                    rectTransform.pivot = new Vector2(1, 0.5f);
                    rectTransform.anchoredPosition = offsetMin;
                    rectTransform.sizeDelta = offsetMax;
                    break;
                case AnchorPresets.MiddleCenter:
                    rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
                    rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
                    rectTransform.pivot = new Vector2(0.5f, 0.5f);
                    rectTransform.anchoredPosition = offsetMin;
                    rectTransform.sizeDelta = offsetMax;
                    break;
                case AnchorPresets.TopCenter:
                    rectTransform.anchorMin = new Vector2(0.5f, 1);
                    rectTransform.anchorMax = new Vector2(0.5f, 1);
                    rectTransform.pivot = new Vector2(0.5f, 1);
                    rectTransform.anchoredPosition = offsetMin;
                    rectTransform.sizeDelta = offsetMax;
                    break;
                case AnchorPresets.TopStretch:
                    rectTransform.anchorMin = new Vector2(0, 1);
                    rectTransform.anchorMax = new Vector2(1, 1);
                    rectTransform.pivot = new Vector2(0.5f, 1);
                    rectTransform.offsetMin = offsetMin;
                    rectTransform.offsetMax = offsetMax;
                    break;
                case AnchorPresets.BottomStretch:
                    rectTransform.anchorMin = new Vector2(0, 0);
                    rectTransform.anchorMax = new Vector2(1, 0);
                    rectTransform.pivot = new Vector2(0.5f, 0);
                    rectTransform.offsetMin = offsetMin;
                    rectTransform.offsetMax = offsetMax;
                    break;
                case AnchorPresets.MiddleStretch:
                    rectTransform.anchorMin = new Vector2(0, 0.5f);
                    rectTransform.anchorMax = new Vector2(1, 0.5f);
                    rectTransform.pivot = new Vector2(0.5f, 0.5f);
                    rectTransform.offsetMin = offsetMin;
                    rectTransform.offsetMax = offsetMax;
                    break;
                case AnchorPresets.RightStretch:
                    rectTransform.anchorMin = new Vector2(1, 0);
                    rectTransform.anchorMax = new Vector2(1, 1);
                    rectTransform.pivot = new Vector2(1, 0.5f);
                    rectTransform.offsetMin = offsetMin;
                    rectTransform.offsetMax = offsetMax;
                    break;
                case AnchorPresets.StretchAll:
                    rectTransform.anchorMin = Vector2.zero;
                    rectTransform.anchorMax = Vector2.one;
                    rectTransform.pivot = new Vector2(0.5f, 0.5f);
                    rectTransform.offsetMin = offsetMin;
                    rectTransform.offsetMax = offsetMax;
                    break;
            }
        }
        
        private enum AnchorPresets
        {
            TopLeft, TopRight, BottomLeft, MiddleLeft, MiddleRight, MiddleCenter, TopCenter,
            TopStretch, BottomStretch, MiddleStretch, RightStretch, StretchAll
        }
        
        #endregion
    }
}
#endif 