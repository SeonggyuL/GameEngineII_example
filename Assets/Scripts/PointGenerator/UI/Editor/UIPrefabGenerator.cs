#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using System.IO;
using TMPro;

namespace PointGenerator.UI.Editor
{
    /// <summary>
    /// PointGenerator UI 프리팹 자동 생성 도구
    /// </summary>
    public class UIPrefabGenerator : EditorWindow
    {
        private const string PREFABS_FOLDER = "Assets/Prefabs";
        private const string UI_PREFABS_FOLDER = "Assets/Prefabs/UI";
        
        [MenuItem("PointGenerator/Create UI Prefabs")]
        public static void ShowWindow()
        {
            GetWindow<UIPrefabGenerator>("UI Prefab Generator");
        }
        
        private void OnGUI()
        {
            GUILayout.Label("PointGenerator UI Prefab Generator", EditorStyles.boldLabel);
            GUILayout.Space(10);
            
            GUILayout.Label("이 도구는 PointGenerator UI 시스템에 필요한 프리팹들을 자동으로 생성합니다.", EditorStyles.helpBox);
            GUILayout.Label("TextMeshPro UGUI 컴포넌트를 사용합니다.", EditorStyles.helpBox);
            GUILayout.Space(10);
            
            if (GUILayout.Button("Create All UI Prefabs", GUILayout.Height(40)))
            {
                CreateAllPrefabs();
            }
            
            GUILayout.Space(20);
            GUILayout.Label("개별 프리팹 생성:", EditorStyles.boldLabel);
            
            if (GUILayout.Button("Create UpgradeItem Prefab"))
            {
                CreateUpgradeItemPrefab();
            }
            
            if (GUILayout.Button("Create AchievementItem Prefab"))
            {
                CreateAchievementItemPrefab();
            }
            
            if (GUILayout.Button("Create PrestigeUpgradeItem Prefab"))
            {
                CreatePrestigeUpgradeItemPrefab();
            }
            
            GUILayout.Space(20);
            if (GUILayout.Button("Open Prefabs Folder"))
            {
                Selection.activeObject = AssetDatabase.LoadAssetAtPath<Object>(UI_PREFABS_FOLDER);
                EditorGUIUtility.PingObject(Selection.activeObject);
            }
        }
        
        private void CreateAllPrefabs()
        {
            EnsureFoldersExist();
            
            CreateUpgradeItemPrefab();
            CreateAchievementItemPrefab();
            CreatePrestigeUpgradeItemPrefab();
            
            AssetDatabase.Refresh();
            Debug.Log("[UIPrefabGenerator] 모든 UI 프리팹 생성 완료! (TextMeshPro UGUI 사용)");
        }
        
        private void EnsureFoldersExist()
        {
            if (!AssetDatabase.IsValidFolder(PREFABS_FOLDER))
            {
                AssetDatabase.CreateFolder("Assets", "Prefabs");
            }
            
            if (!AssetDatabase.IsValidFolder(UI_PREFABS_FOLDER))
            {
                AssetDatabase.CreateFolder(PREFABS_FOLDER, "UI");
            }
        }
        
        #region UpgradeItem Prefab Creation
        
        private void CreateUpgradeItemPrefab()
        {
            EnsureFoldersExist();
            
            // Root GameObject 생성
            var upgradeItem = new GameObject("UpgradeItem");
            var rectTransform = upgradeItem.AddComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(0, 80);
            
            // Background Image 추가
            var backgroundImage = upgradeItem.AddComponent<Image>();
            backgroundImage.color = new Color(0.2f, 0.2f, 0.3f, 0.8f);
            
            // UpgradeItemUI 스크립트 추가
            var upgradeItemUI = upgradeItem.AddComponent<PointGenerator.UI.UpgradeItemUI>();
            
            // Icon Image 생성
            var iconImage = CreateUpgradeIcon(upgradeItem.transform);
            SetRectTransform(iconImage, AnchorPresets.MiddleLeft, new Vector2(10, 0), new Vector2(50, 50));
            
            // InfoPanel 생성 (아이콘을 고려하여 위치 조정)
            var infoPanel = CreateInfoPanel(upgradeItem.transform, "InfoPanel");
            SetRectTransform(infoPanel, AnchorPresets.MiddleLeft, new Vector2(70, 0), new Vector2(280, 70));
            
            // InfoPanel 하위 텍스트들 생성
            var nameText = CreateTextMeshPro(infoPanel.transform, "NameText", "Upgrade Name", 16, Color.white);
            SetRectTransform(nameText, AnchorPresets.TopLeft, new Vector2(10, -10), new Vector2(180, 25));
            
            var descriptionText = CreateTextMeshPro(infoPanel.transform, "DescriptionText", "Upgrade description here", 12, new Color(0.8f, 0.8f, 0.8f));
            SetRectTransform(descriptionText, AnchorPresets.TopLeft, new Vector2(10, -35), new Vector2(180, 20));
            
            var levelText = CreateTextMeshPro(infoPanel.transform, "LevelText", "Level 0", 14, Color.yellow);
            SetRectTransform(levelText, AnchorPresets.TopRight, new Vector2(-10, -10), new Vector2(80, 25));
            
            var costText = CreateTextMeshPro(infoPanel.transform, "CostText", "Cost: 100", 12, Color.green);
            SetRectTransform(costText, AnchorPresets.TopRight, new Vector2(-10, -35), new Vector2(80, 20));
            
            // Purchase Button 생성
            var purchaseButton = CreateButton(upgradeItem.transform, "PurchaseButton", "BUY");
            SetRectTransform(purchaseButton, AnchorPresets.MiddleRight, new Vector2(-50, 0), new Vector2(80, 60));
            
            // Max Level Indicator 생성
            var maxLevelIndicator = CreateMaxLevelIndicator(upgradeItem.transform);
            
            // Inspector 참조 설정
            SetUpgradeItemUIReferences(upgradeItemUI, nameText, descriptionText, costText, levelText, purchaseButton, backgroundImage, maxLevelIndicator, iconImage);
            
            // 프리팹으로 저장
            string prefabPath = $"{UI_PREFABS_FOLDER}/UpgradeItemPrefab.prefab";
            var prefab = PrefabUtility.SaveAsPrefabAsset(upgradeItem, prefabPath);
            
            // 임시 오브젝트 삭제
            DestroyImmediate(upgradeItem);
            
            Debug.Log($"[UIPrefabGenerator] UpgradeItem 프리팹 생성 완료: {prefabPath}");
        }
        
        private void SetUpgradeItemUIReferences(UpgradeItemUI upgradeItemUI, GameObject nameText, GameObject descriptionText, 
            GameObject costText, GameObject levelText, GameObject purchaseButton, Image backgroundImage, GameObject maxLevelIndicator, GameObject iconImage)
        {
            var serializedObject = new SerializedObject(upgradeItemUI);
            
            // 각 컴포넌트가 존재하는지 확인하고 안전하게 설정
            var purchaseButtonProperty = serializedObject.FindProperty("purchaseButton");
            if (purchaseButtonProperty != null && purchaseButton != null)
                purchaseButtonProperty.objectReferenceValue = purchaseButton.GetComponent<Button>();
                
            var nameTextProperty = serializedObject.FindProperty("nameText");
            if (nameTextProperty != null && nameText != null)
                nameTextProperty.objectReferenceValue = nameText.GetComponent<TextMeshProUGUI>();
                
            var descriptionTextProperty = serializedObject.FindProperty("descriptionText");
            if (descriptionTextProperty != null && descriptionText != null)
                descriptionTextProperty.objectReferenceValue = descriptionText.GetComponent<TextMeshProUGUI>();
                
            var costTextProperty = serializedObject.FindProperty("costText");
            if (costTextProperty != null && costText != null)
                costTextProperty.objectReferenceValue = costText.GetComponent<TextMeshProUGUI>();
                
            var levelTextProperty = serializedObject.FindProperty("levelText");
            if (levelTextProperty != null && levelText != null)
                levelTextProperty.objectReferenceValue = levelText.GetComponent<TextMeshProUGUI>();
                
            var maxLevelIndicatorProperty = serializedObject.FindProperty("maxLevelIndicator");
            if (maxLevelIndicatorProperty != null && maxLevelIndicator != null)
                maxLevelIndicatorProperty.objectReferenceValue = maxLevelIndicator;
            
            // 아이콘이 생성되었을 때만 참조 설정
            if (iconImage != null)
            {
                var iconImageProperty = serializedObject.FindProperty("iconImage");
                if (iconImageProperty != null)
                {
                    iconImageProperty.objectReferenceValue = iconImage.GetComponent<Image>();
                }
            }
            
            serializedObject.ApplyModifiedProperties();
        }
        
        #endregion
        
        #region AchievementItem Prefab Creation
        
        private void CreateAchievementItemPrefab()
        {
            EnsureFoldersExist();
            
            // Root GameObject 생성
            var achievementItem = new GameObject("AchievementItem");
            var rectTransform = achievementItem.AddComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(0, 100);
            
            // Background Image 추가
            var backgroundImage = achievementItem.AddComponent<Image>();
            backgroundImage.color = new Color(0.15f, 0.15f, 0.25f, 0.9f);
            
            // AchievementItemUI 스크립트 추가
            var achievementItemUI = achievementItem.AddComponent<PointGenerator.UI.AchievementItemUI>();
            
            // Icon Image 생성
            var iconImage = CreateAchievementIcon(achievementItem.transform);
            SetRectTransform(iconImage, AnchorPresets.MiddleLeft, new Vector2(15, 0), new Vector2(60, 60));
            
            // InfoPanel 생성 (아이콘을 고려하여 위치 조정)
            var infoPanel = CreateInfoPanel(achievementItem.transform, "InfoPanel");
            SetRectTransform(infoPanel, AnchorPresets.TopStretch, new Vector2(85, 0), new Vector2(-20, 70));
            
            // InfoPanel 하위 텍스트들 생성
            var nameText = CreateTextMeshPro(infoPanel.transform, "NameText", "Achievement Name", 16, Color.white);
            SetRectTransform(nameText, AnchorPresets.TopLeft, new Vector2(10, -10), new Vector2(200, 25));
            
            var descriptionText = CreateTextMeshPro(infoPanel.transform, "DescriptionText", "Achievement description", 12, new Color(0.8f, 0.8f, 0.8f));
            SetRectTransform(descriptionText, AnchorPresets.TopLeft, new Vector2(10, -35), new Vector2(200, 20));
            
            var progressText = CreateTextMeshPro(infoPanel.transform, "ProgressText", "0/100", 14, Color.cyan);
            SetRectTransform(progressText, AnchorPresets.TopRight, new Vector2(-10, -10), new Vector2(100, 25));
            
            var rewardText = CreateTextMeshPro(infoPanel.transform, "RewardText", "Reward: 500 pts", 12, Color.yellow);
            SetRectTransform(rewardText, AnchorPresets.TopRight, new Vector2(-10, -35), new Vector2(120, 20));
            
            // Progress Slider 생성
            var progressSlider = CreateProgressSlider(achievementItem.transform);
            SetRectTransform(progressSlider, AnchorPresets.BottomStretch, new Vector2(85, 5), new Vector2(-20, 25));
            
            // Unlocked Indicator 생성
            var unlockedIndicator = CreateUnlockedIndicator(achievementItem.transform);
            
            // New Indicator 생성
            var newIndicator = CreateNewIndicator(achievementItem.transform);
            
            // Inspector 참조 설정
            SetAchievementItemUIReferences(achievementItemUI, nameText, descriptionText, progressText, rewardText, 
                backgroundImage, progressSlider, unlockedIndicator, newIndicator, iconImage);
            
            // 프리팹으로 저장
            string prefabPath = $"{UI_PREFABS_FOLDER}/AchievementItemPrefab.prefab";
            var prefab = PrefabUtility.SaveAsPrefabAsset(achievementItem, prefabPath);
            
            // 임시 오브젝트 삭제
            DestroyImmediate(achievementItem);
            
            Debug.Log($"[UIPrefabGenerator] AchievementItem 프리팹 생성 완료: {prefabPath}");
        }
        
        private void SetAchievementItemUIReferences(AchievementItemUI achievementItemUI, GameObject nameText, GameObject descriptionText,
            GameObject progressText, GameObject rewardText, Image backgroundImage, GameObject progressSlider, 
            GameObject unlockedIndicator, GameObject newIndicator, GameObject iconImage)
        {
            var serializedObject = new SerializedObject(achievementItemUI);
            
            // 각 컴포넌트가 존재하는지 확인하고 안전하게 설정
            var nameTextProperty = serializedObject.FindProperty("nameText");
            if (nameTextProperty != null && nameText != null)
                nameTextProperty.objectReferenceValue = nameText.GetComponent<TextMeshProUGUI>();
                
            var descriptionTextProperty = serializedObject.FindProperty("descriptionText");
            if (descriptionTextProperty != null && descriptionText != null)
                descriptionTextProperty.objectReferenceValue = descriptionText.GetComponent<TextMeshProUGUI>();
                
            var progressTextProperty = serializedObject.FindProperty("progressText");
            if (progressTextProperty != null && progressText != null)
                progressTextProperty.objectReferenceValue = progressText.GetComponent<TextMeshProUGUI>();
                
            var rewardTextProperty = serializedObject.FindProperty("rewardText");
            if (rewardTextProperty != null && rewardText != null)
                rewardTextProperty.objectReferenceValue = rewardText.GetComponent<TextMeshProUGUI>();
                
            var backgroundImageProperty = serializedObject.FindProperty("backgroundImage");
            if (backgroundImageProperty != null && backgroundImage != null)
                backgroundImageProperty.objectReferenceValue = backgroundImage;
                
            var progressSliderProperty = serializedObject.FindProperty("progressSlider");
            if (progressSliderProperty != null && progressSlider != null)
                progressSliderProperty.objectReferenceValue = progressSlider.GetComponent<Slider>();
                
            var unlockedIndicatorProperty = serializedObject.FindProperty("unlockedIndicator");
            if (unlockedIndicatorProperty != null && unlockedIndicator != null)
                unlockedIndicatorProperty.objectReferenceValue = unlockedIndicator;
                
            var newIndicatorProperty = serializedObject.FindProperty("newIndicator");
            if (newIndicatorProperty != null && newIndicator != null)
                newIndicatorProperty.objectReferenceValue = newIndicator;
            
            // 아이콘이 생성되었을 때만 참조 설정
            if (iconImage != null)
            {
                var iconImageProperty = serializedObject.FindProperty("iconImage");
                if (iconImageProperty != null)
                {
                    iconImageProperty.objectReferenceValue = iconImage.GetComponent<Image>();
                }
            }
            
            serializedObject.ApplyModifiedProperties();
        }
        
        #endregion
        
        #region PrestigeUpgradeItem Prefab Creation
        
        private void CreatePrestigeUpgradeItemPrefab()
        {
            EnsureFoldersExist();
            
            // Root GameObject 생성
            var prestigeUpgradeItem = new GameObject("PrestigeUpgradeItem");
            var rectTransform = prestigeUpgradeItem.AddComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(0, 90);
            
            // Background Image 추가
            var backgroundImage = prestigeUpgradeItem.AddComponent<Image>();
            backgroundImage.color = new Color(0.15f, 0.15f, 0.25f, 0.9f);
            
            // PrestigeUpgradeItemUI 스크립트 추가
            var prestigeUpgradeItemUI = prestigeUpgradeItem.AddComponent<PointGenerator.UI.PrestigeUpgradeItemUI>();
            
            // Icon Image 생성
            var iconImage = CreatePrestigeIcon(prestigeUpgradeItem.transform);
            SetRectTransform(iconImage, AnchorPresets.MiddleLeft, new Vector2(10, 0), new Vector2(55, 55));
            
            // InfoPanel 생성 (아이콘을 고려하여 위치 조정)
            var infoPanel = CreateInfoPanel(prestigeUpgradeItem.transform, "InfoPanel");
            SetRectTransform(infoPanel, AnchorPresets.MiddleLeft, new Vector2(75, 0), new Vector2(265, 80));
            
            // InfoPanel 하위 텍스트들 생성
            var nameText = CreateTextMeshPro(infoPanel.transform, "NameText", "Prestige Upgrade", 16, Color.white);
            SetRectTransform(nameText, AnchorPresets.TopLeft, new Vector2(10, -10), new Vector2(180, 25));
            
            var descriptionText = CreateTextMeshPro(infoPanel.transform, "DescriptionText", "Prestige upgrade description", 12, new Color(0.8f, 0.8f, 0.8f));
            SetRectTransform(descriptionText, AnchorPresets.TopLeft, new Vector2(10, -30), new Vector2(180, 20));
            
            var levelText = CreateTextMeshPro(infoPanel.transform, "LevelText", "Lv. 0", 14, Color.yellow);
            SetRectTransform(levelText, AnchorPresets.TopRight, new Vector2(-10, -10), new Vector2(60, 25));
            
            var costText = CreateTextMeshPro(infoPanel.transform, "CostText", "Cost: 10 PP", 12, Color.green);
            SetRectTransform(costText, AnchorPresets.TopRight, new Vector2(-10, -30), new Vector2(80, 20));
            
            var effectText = CreateTextMeshPro(infoPanel.transform, "EffectText", "Effect: x1.0", 12, Color.cyan);
            SetRectTransform(effectText, AnchorPresets.BottomLeft, new Vector2(10, 10), new Vector2(180, 20));
            
            // Upgrade Button 생성
            var upgradeButton = CreateButton(prestigeUpgradeItem.transform, "UpgradeButton", "UPGRADE");
            SetRectTransform(upgradeButton, AnchorPresets.MiddleRight, new Vector2(-50, 0), new Vector2(80, 70));
            
            // Max Level Indicator 생성
            var maxLevelIndicator = CreateMaxLevelIndicator(prestigeUpgradeItem.transform);
            
            // Inspector 참조 설정
            SetPrestigeUpgradeItemUIReferences(prestigeUpgradeItemUI, nameText, descriptionText, costText, levelText, 
                effectText, upgradeButton, backgroundImage, maxLevelIndicator, iconImage);
            
            // 프리팹으로 저장
            string prefabPath = $"{UI_PREFABS_FOLDER}/PrestigeUpgradeItemPrefab.prefab";
            var prefab = PrefabUtility.SaveAsPrefabAsset(prestigeUpgradeItem, prefabPath);
            
            // 임시 오브젝트 삭제
            DestroyImmediate(prestigeUpgradeItem);
            
            Debug.Log($"[UIPrefabGenerator] PrestigeUpgradeItem 프리팹 생성 완료: {prefabPath}");
        }
        
        private void SetPrestigeUpgradeItemUIReferences(PrestigeUpgradeItemUI prestigeUpgradeItemUI, GameObject nameText, 
            GameObject descriptionText, GameObject costText, GameObject levelText, GameObject effectText, 
            GameObject upgradeButton, Image backgroundImage, GameObject maxLevelIndicator, GameObject iconImage)
        {
            var serializedObject = new SerializedObject(prestigeUpgradeItemUI);
            
            Debug.Log("[UIPrefabGenerator] 프레스티지 프리팹 참조 설정 시작");
            
            // 각 컴포넌트가 존재하는지 확인하고 안전하게 설정
            var nameTextProperty = serializedObject.FindProperty("nameText");
            if (nameTextProperty != null && nameText != null)
            {
                nameTextProperty.objectReferenceValue = nameText.GetComponent<TextMeshProUGUI>();
                Debug.Log("[UIPrefabGenerator] nameText 연결 완료");
            }
            else
            {
                Debug.LogWarning($"[UIPrefabGenerator] nameText 연결 실패 - Property: {nameTextProperty != null}, GameObject: {nameText != null}");
            }
                
            var descriptionTextProperty = serializedObject.FindProperty("descriptionText");
            if (descriptionTextProperty != null && descriptionText != null)
            {
                descriptionTextProperty.objectReferenceValue = descriptionText.GetComponent<TextMeshProUGUI>();
                Debug.Log("[UIPrefabGenerator] descriptionText 연결 완료");
            }
            else
            {
                Debug.LogWarning($"[UIPrefabGenerator] descriptionText 연결 실패 - Property: {descriptionTextProperty != null}, GameObject: {descriptionText != null}");
            }
                
            var costTextProperty = serializedObject.FindProperty("costText");
            if (costTextProperty != null && costText != null)
            {
                costTextProperty.objectReferenceValue = costText.GetComponent<TextMeshProUGUI>();
                Debug.Log("[UIPrefabGenerator] costText 연결 완료");
            }
            else
            {
                Debug.LogWarning($"[UIPrefabGenerator] costText 연결 실패 - Property: {costTextProperty != null}, GameObject: {costText != null}");
            }
                
            var levelTextProperty = serializedObject.FindProperty("levelText");
            if (levelTextProperty != null && levelText != null)
            {
                levelTextProperty.objectReferenceValue = levelText.GetComponent<TextMeshProUGUI>();
                Debug.Log("[UIPrefabGenerator] levelText 연결 완료");
            }
            else
            {
                Debug.LogWarning($"[UIPrefabGenerator] levelText 연결 실패 - Property: {levelTextProperty != null}, GameObject: {levelText != null}");
            }
                
            var effectTextProperty = serializedObject.FindProperty("effectText");
            if (effectTextProperty != null && effectText != null)
            {
                effectTextProperty.objectReferenceValue = effectText.GetComponent<TextMeshProUGUI>();
                Debug.Log("[UIPrefabGenerator] effectText 연결 완료");
            }
            else
            {
                Debug.LogWarning($"[UIPrefabGenerator] effectText 연결 실패 - Property: {effectTextProperty != null}, GameObject: {effectText != null}");
            }
                
            var upgradeButtonProperty = serializedObject.FindProperty("upgradeButton");
            if (upgradeButtonProperty != null && upgradeButton != null)
            {
                upgradeButtonProperty.objectReferenceValue = upgradeButton.GetComponent<Button>();
                Debug.Log("[UIPrefabGenerator] upgradeButton 연결 완료");
            }
            else
            {
                Debug.LogWarning($"[UIPrefabGenerator] upgradeButton 연결 실패 - Property: {upgradeButtonProperty != null}, GameObject: {upgradeButton != null}");
            }
                
            var backgroundImageProperty = serializedObject.FindProperty("backgroundImage");
            if (backgroundImageProperty != null && backgroundImage != null)
            {
                backgroundImageProperty.objectReferenceValue = backgroundImage;
                Debug.Log("[UIPrefabGenerator] backgroundImage 연결 완료");
            }
            else
            {
                Debug.LogWarning($"[UIPrefabGenerator] backgroundImage 연결 실패 - Property: {backgroundImageProperty != null}, Image: {backgroundImage != null}");
            }
                
            var maxLevelIndicatorProperty = serializedObject.FindProperty("maxLevelIndicator");
            if (maxLevelIndicatorProperty != null && maxLevelIndicator != null)
            {
                maxLevelIndicatorProperty.objectReferenceValue = maxLevelIndicator;
                Debug.Log("[UIPrefabGenerator] maxLevelIndicator 연결 완료");
            }
            else
            {
                Debug.LogWarning($"[UIPrefabGenerator] maxLevelIndicator 연결 실패 - Property: {maxLevelIndicatorProperty != null}, GameObject: {maxLevelIndicator != null}");
            }
            
            // 아이콘이 생성되었을 때만 참조 설정
            if (iconImage != null)
            {
                var iconImageProperty = serializedObject.FindProperty("iconImage");
                if (iconImageProperty != null)
                {
                    iconImageProperty.objectReferenceValue = iconImage.GetComponent<Image>();
                    Debug.Log("[UIPrefabGenerator] iconImage 연결 완료");
                }
                else
                {
                    Debug.LogWarning("[UIPrefabGenerator] iconImage 프로퍼티를 찾을 수 없음");
                }
            }
            
            serializedObject.ApplyModifiedProperties();
            Debug.Log("[UIPrefabGenerator] 프레스티지 프리팹 참조 설정 완료");
        }
        
        #endregion
        
        #region Helper Methods
        
        private GameObject CreateInfoPanel(Transform parent, string name)
        {
            var infoPanel = new GameObject(name);
            infoPanel.transform.SetParent(parent);
            var rectTransform = infoPanel.AddComponent<RectTransform>();
            return infoPanel;
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
            
            // 기본 폰트는 TextMeshPro 기본 폰트 사용 (Resources에서 로드)
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
        
        private GameObject CreateProgressSlider(Transform parent)
        {
            var sliderObj = new GameObject("ProgressSlider");
            sliderObj.transform.SetParent(parent);
            var rectTransform = sliderObj.AddComponent<RectTransform>();
            var slider = sliderObj.AddComponent<Slider>();
            
            // Background
            var background = new GameObject("Background");
            background.transform.SetParent(sliderObj.transform);
            var bgRectTransform = background.AddComponent<RectTransform>();
            var bgImage = background.AddComponent<Image>();
            bgImage.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
            
            bgRectTransform.anchorMin = Vector2.zero;
            bgRectTransform.anchorMax = Vector2.one;
            bgRectTransform.offsetMin = Vector2.zero;
            bgRectTransform.offsetMax = Vector2.zero;
            
            // Fill Area
            var fillArea = new GameObject("Fill Area");
            fillArea.transform.SetParent(sliderObj.transform);
            var fillAreaRectTransform = fillArea.AddComponent<RectTransform>();
            
            fillAreaRectTransform.anchorMin = Vector2.zero;
            fillAreaRectTransform.anchorMax = Vector2.one;
            fillAreaRectTransform.offsetMin = Vector2.zero;
            fillAreaRectTransform.offsetMax = Vector2.zero;
            
            // Fill
            var fill = new GameObject("Fill");
            fill.transform.SetParent(fillArea.transform);
            var fillRectTransform = fill.AddComponent<RectTransform>();
            var fillImage = fill.AddComponent<Image>();
            fillImage.color = new Color(0.2f, 0.8f, 0.2f, 1f);
            
            fillRectTransform.anchorMin = Vector2.zero;
            fillRectTransform.anchorMax = Vector2.one;
            fillRectTransform.offsetMin = Vector2.zero;
            fillRectTransform.offsetMax = Vector2.zero;
            
            // Slider 설정
            slider.fillRect = fillRectTransform;
            slider.value = 0f;
            slider.minValue = 0f;
            slider.maxValue = 1f;
            
            return sliderObj;
        }
        
        private GameObject CreateMaxLevelIndicator(Transform parent)
        {
            var indicator = new GameObject("MaxLevelIndicator");
            indicator.transform.SetParent(parent);
            var rectTransform = indicator.AddComponent<RectTransform>();
            var image = indicator.AddComponent<Image>();
            
            image.color = new Color(1f, 0.8f, 0.2f, 0.8f);
            indicator.SetActive(false);
            
            SetRectTransform(indicator, AnchorPresets.TopRight, new Vector2(-5, -5), new Vector2(30, 30));
            
            // MAX 텍스트 추가 (TextMeshPro 사용)
            var maxText = CreateTextMeshPro(indicator.transform, "MaxText", "MAX", 10, Color.black);
            SetRectTransform(maxText, AnchorPresets.MiddleCenter, Vector2.zero, new Vector2(25, 25));
            var maxTextComponent = maxText.GetComponent<TextMeshProUGUI>();
            maxTextComponent.alignment = TextAlignmentOptions.Center;
            
            return indicator;
        }
        
        private GameObject CreateUnlockedIndicator(Transform parent)
        {
            var indicator = new GameObject("UnlockedIndicator");
            indicator.transform.SetParent(parent);
            var rectTransform = indicator.AddComponent<RectTransform>();
            var image = indicator.AddComponent<Image>();
            
            image.color = new Color(0.2f, 0.8f, 0.2f, 0.8f);
            indicator.SetActive(false);
            
            SetRectTransform(indicator, AnchorPresets.TopRight, new Vector2(-5, -5), new Vector2(25, 25));
            
            return indicator;
        }
        
        private GameObject CreateNewIndicator(Transform parent)
        {
            var indicator = new GameObject("NewIndicator");
            indicator.transform.SetParent(parent);
            var rectTransform = indicator.AddComponent<RectTransform>();
            var image = indicator.AddComponent<Image>();
            
            image.color = new Color(1f, 0.2f, 0.2f, 0.9f);
            indicator.SetActive(false);
            
            SetRectTransform(indicator, AnchorPresets.TopLeft, new Vector2(5, -5), new Vector2(20, 20));
            
            return indicator;
        }
        
        private GameObject CreateUpgradeIcon(Transform parent)
        {
            var iconObj = new GameObject("UpgradeIcon");
            iconObj.transform.SetParent(parent);
            var rectTransform = iconObj.AddComponent<RectTransform>();
            var image = iconObj.AddComponent<Image>();
            
            // 업그레이드 아이콘 설정 (파란색 계열) - 스프라이트 없이 색상만 사용
            image.color = new Color(0.2f, 0.6f, 1f, 1f); // 파란색
            image.sprite = null; // 스프라이트 없이 기본 사각형
            
            return iconObj;
        }
        
        private GameObject CreateAchievementIcon(Transform parent)
        {
            var iconObj = new GameObject("AchievementIcon");
            iconObj.transform.SetParent(parent);
            var rectTransform = iconObj.AddComponent<RectTransform>();
            var image = iconObj.AddComponent<Image>();
            
            // 업적 아이콘 설정 (금색 계열) - 스프라이트 없이 색상만 사용
            image.color = new Color(1f, 0.8f, 0.2f, 1f); // 금색
            image.sprite = null; // 스프라이트 없이 기본 사각형
            
            return iconObj;
        }
        
        private GameObject CreatePrestigeIcon(Transform parent)
        {
            var iconObj = new GameObject("PrestigeIcon");
            iconObj.transform.SetParent(parent);
            var rectTransform = iconObj.AddComponent<RectTransform>();
            var image = iconObj.AddComponent<Image>();
            
            // 프레스티지 아이콘 설정 (보라색 계열) - 스프라이트 없이 색상만 사용
            image.color = new Color(0.8f, 0.2f, 1f, 1f); // 보라색
            image.sprite = null; // 스프라이트 없이 기본 사각형
            
            return iconObj;
        }
        
        private void SetRectTransform(GameObject obj, AnchorPresets preset, Vector2 position, Vector2 size)
        {
            var rectTransform = obj.GetComponent<RectTransform>();
            
            switch (preset)
            {
                case AnchorPresets.TopLeft:
                    rectTransform.anchorMin = new Vector2(0, 1);
                    rectTransform.anchorMax = new Vector2(0, 1);
                    rectTransform.pivot = new Vector2(0, 1);
                    break;
                case AnchorPresets.TopRight:
                    rectTransform.anchorMin = new Vector2(1, 1);
                    rectTransform.anchorMax = new Vector2(1, 1);
                    rectTransform.pivot = new Vector2(1, 1);
                    break;
                case AnchorPresets.BottomLeft:
                    rectTransform.anchorMin = new Vector2(0, 0);
                    rectTransform.anchorMax = new Vector2(0, 0);
                    rectTransform.pivot = new Vector2(0, 0);
                    break;
                case AnchorPresets.MiddleLeft:
                    rectTransform.anchorMin = new Vector2(0, 0.5f);
                    rectTransform.anchorMax = new Vector2(0, 0.5f);
                    rectTransform.pivot = new Vector2(0, 0.5f);
                    break;
                case AnchorPresets.MiddleRight:
                    rectTransform.anchorMin = new Vector2(1, 0.5f);
                    rectTransform.anchorMax = new Vector2(1, 0.5f);
                    rectTransform.pivot = new Vector2(1, 0.5f);
                    break;
                case AnchorPresets.MiddleCenter:
                    rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
                    rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
                    rectTransform.pivot = new Vector2(0.5f, 0.5f);
                    break;
                case AnchorPresets.TopStretch:
                    rectTransform.anchorMin = new Vector2(0, 1);
                    rectTransform.anchorMax = new Vector2(1, 1);
                    rectTransform.pivot = new Vector2(0.5f, 1);
                    break;
                case AnchorPresets.BottomStretch:
                    rectTransform.anchorMin = new Vector2(0, 0);
                    rectTransform.anchorMax = new Vector2(1, 0);
                    rectTransform.pivot = new Vector2(0.5f, 0);
                    break;
            }
            
            rectTransform.anchoredPosition = position;
            rectTransform.sizeDelta = size;
        }
        
        private enum AnchorPresets
        {
            TopLeft, TopRight, BottomLeft, MiddleLeft, MiddleRight, MiddleCenter, TopStretch, BottomStretch
        }
        
        #endregion
    }
}
#endif 