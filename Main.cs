using System.Collections.Generic;
using System.Diagnostics;
using BepInEx;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using UnityEngine;
using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Linq;
using Il2CppInterop.Runtime.Injection;
using BepInEx.Logging;
using Utils;
using Il2CppSystem.Collections.Generic;
using System.Reflection;
using Il2CppSystem;
using SystemIntPtr = System.IntPtr;
using Il2CppIntPtr = Il2CppSystem.IntPtr;
using Il2CppInterop.Runtime;
using IntPtr = System.IntPtr;
using static MirrorDungeonSelectThemeUIPanel.UIResources;
using UnityEngine.Networking;
using SD;
using UnityEngine.UIElements;
using BattleUI;
using BattleUI.Dialog;
using StorySystem;
using System.Timers;
using BattleUI.Operation;
using UnityEngine.SceneManagement;

using HarmonyLib;
using UnityEngine;

using UnityEngine.EventSystems;
using static BattleUI.Abnormality.AbnormalityPartSkills;
using System.IO;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Rendering;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppSystem.Xml.Serialization;
using System.Drawing;
using static System.Net.Mime.MediaTypeNames;
using System.Numerics;

using Il2CppSystem.IO;
using System.IO.Compression;

using Lethe.Texture;
using Il2CppSystem.Collections;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using FMOD.Studio;
using Il2CppSystem.Reflection;

namespace BanAutoSelectButton
{
    public class VocabularyItem
    {
        public int Index { get; set; }
        public string Word { get; set; }
        public string Phonetic { get; set; }
        public string Definition { get; set; }
    }


    internal class ChangeAppearanceTemp : MonoBehaviour
    {
        public static ChangeAppearanceTemp Instance { get; private set; }
        private static ChangeAppearanceTemp _instance;
        private GameObject cachedTarget;

        // 新增的UI相关字段
        private static GameObject canvasRoot;
        private static UnityEngine.Font consoleFont;
        private static UnityEngine.Font defaultFont;
        private static Shader backupShader;
        private static AssetBundle uiBundle;

        private const float PANEL_SCREEN_RATIO = 0.6f; // 主面板占屏幕比例
        private const float BUTTON_ASPECT_RATIO = 3.0f; // 按钮宽高比（宽度/高度）
        private const float MARGIN_RATIO = 0.02f;       // 边距占屏幕比例
        private const float SPACING_RATIO = 0.01f;       // 间距占屏幕比例

        private static System.Collections.Generic.List<UnityEngine.UI.Text> textComponents = new System.Collections.Generic.List<UnityEngine.UI.Text>();

        private static System.Collections.Generic.List<VocabularyItem> vocabularyList = new System.Collections.Generic.List<VocabularyItem>();
        private static VocabularyItem currentWord;
        private static int correctButtonIndex;
        private static System.Collections.Generic.List<UnityEngine.UI.Text> optionTexts = new System.Collections.Generic.List<UnityEngine.UI.Text>();
        private static UnityEngine.UI.Text wordTextComponent;
        private static UnityEngine.UI.Text phoneticTextComponent;


        public static void Setup(Harmony harmony)
        {
            ClassInjector.RegisterTypeInIl2Cpp<ChangeAppearanceTemp>();
            LoadVocabularyData();

            // UI初始化核心逻辑
            CreateRootCanvas();


            GameObject obj = new GameObject("ChangeAppearanceController");
            GameObject.DontDestroyOnLoad(obj);
            obj.hideFlags = HideFlags.HideAndDontSave;
            _instance = obj.AddComponent<ChangeAppearanceTemp>();
            _instance.enabled = true;
            
        }


        
        private static void buffSinner(BUFF_UNIQUE_KEYWORD buf,int stack,int count)
        {
            Il2CppSystem.Collections.Generic.List<BattleUnitModel> list = new Il2CppSystem.Collections.Generic.List<BattleUnitModel>();
            Il2CppSystem.Collections.Generic.List<BattleUnitModel> enemylist = new Il2CppSystem.Collections.Generic.List<BattleUnitModel>();
            BattleObjectManager battleObjectManager = Singleton<SinManager>.Instance._battleObjectManager;
            
            AbilityBase dummyAbility = new AbilityBase();

            foreach (BattleUnitModel battleUnitModel in battleObjectManager.GetModelList())
            {
                Main.SharedLog.LogWarning($"model faction{battleUnitModel.Faction}");
                if (battleUnitModel.Faction == UNIT_FACTION.PLAYER)
                {
                    list.Add(battleUnitModel);
                }
          
            }
            foreach(BattleUnitModel model in list)
            {
                Main.SharedLog.LogWarning($"buff?{model.Faction}");
                dummyAbility.GiveBuff_Self(model, buf, stack, count, 0, BATTLE_EVENT_TIMING.ON_BATTLE_START, null);
            }
            /*
            foreach (BattleUnitModel model in list)
            {
                if (model._passiveDetail.Equals(__instance) && Don != null)
                {
                    int currentRound = Singleton<StageController>.Instance.GetCurrentRound();

                    Main.SharedLog.LogWarning(string.Format("Unit ID:{0},Unit data{1},Don faction{2}", model.GetUnitID(), model.Faction, Don.Faction));
                    HashSet<int> bodyParents = new HashSet<int>();

                    // 第一阶段：收集所有身体部件对应的本体ID
                    foreach (BattleUnitModel unit in list)
                    {
                        int unitId = unit.GetUnitID();
                        // 识别身体部件（6位数字）并提取本体ID
                        if (unitId >= 100000)
                        {
                            int parentId = unitId / 100;
                            bodyParents.Add(parentId);
                        }
                    }

                    // 第二阶段：构建最终敌人列表
                    foreach (BattleUnitModel battleUnitModel in list)
                    {
                        if (battleUnitModel.Faction != Don.Faction)
                        {
                            int currentId = battleUnitModel.GetUnitID();

                            // 检查是否是本体（4位数字）
                            if (currentId >= 1000 && currentId <= 9999)
                            {
                                // 当存在对应的身体部件时跳过添加本体
                                if (bodyParents.Contains(currentId))
                                {
                                    continue;
                                }
                            }

                            enemylist.Add(battleUnitModel);
                        }
                    }



                }

            }*/
        }
        public static void LoadVocabularyData()
        {
            vocabularyList.Clear(); // 清空原有数据

            try
            {
                string filePath = System.IO.Path.Combine(
                    System.IO.Path.GetDirectoryName(UnityEngine.Application.dataPath),
                    "BepInEx",
                    "plugins",
                    "CustomUI",
                    "wordTable",
                    "CET-6.csv"
                );

                if (!System.IO.File.Exists(filePath))
                {
                    Main.SharedLog.LogError($"CSV文件不存在: {filePath}");
                    return;
                }

                // 读取所有行并跳过标题行
                string[] allLines = System.IO.File.ReadAllLines(filePath);
                var validLines = allLines.Skip(1).Where(l => !string.IsNullOrWhiteSpace(l));

                foreach (string line in validLines)
                {
                    // 拆分字段并处理空白
                    string[] fields = line.Split(',')
                        .Select(f => f.Trim())
                        .ToArray();

                    // 验证字段完整性
                    if (fields.Length < 4)
                    {
                        Main.SharedLog.LogWarning($"无效行（字段不足）: {line}");
                        continue;
                    }

                    // 解析序号
                    if (!int.TryParse(fields[0], out int index))
                    {
                        Main.SharedLog.LogWarning($"序号格式错误: {fields[0]}");
                        continue;
                    }

                    // 创建词汇项
                    var item = new VocabularyItem
                    {
                        Index = index,
                        Word = fields[1],
                        Phonetic = fields[2],
                        Definition = string.Join(",", fields.Skip(3)) // 合并剩余字段为定义
                    };

                    vocabularyList.Add(item);
                }

                Main.SharedLog.LogWarning($"成功加载词汇数据: {vocabularyList.Count} 条");
            }
            catch (System.Exception ex)
            {
                Main.SharedLog.LogError($"词汇加载失败: {ex.ToString()}");
                vocabularyList.Clear(); // 确保失败时清空列表
            }
        }




        private static RectTransform GetRectTransformSafe(Transform target)
        {
            if (target == null) return null;
            var rectTransform = target.GetComponent<RectTransform>();
            if (rectTransform == null)
            {
                Main.SharedLog.LogError($"Missing RectTransform on {target.name}");
                return null;
            }
            return rectTransform;
        }
        // 新增的UI创建方法
        private static void CreateRootCanvas()
        {
            // 增强的Canvas创建逻辑
            if (canvasRoot != null) return;

            canvasRoot = new GameObject("RedUICanvas");
            DontDestroyOnLoad(canvasRoot);
            canvasRoot.SetActive(false);
            GameObject.DontDestroyOnLoad(canvasRoot);
            canvasRoot.hideFlags = HideFlags.HideAndDontSave;
            canvasRoot.layer = 5;

            // 添加并配置Canvas组件
            var canvas = canvasRoot.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            // 配置Canvas Scaler
            var canvasScaler = canvasRoot.AddComponent<CanvasScaler>();
            canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasScaler.referenceResolution = new UnityEngine.Vector2(1920, 1080);
            canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            canvasScaler.matchWidthOrHeight = 0.5f;

            // 添加必要的射线检测
            canvasRoot.AddComponent<GraphicRaycaster>();

            // 安全创建事件系统
            CreateEventSystem();
            // 修改事件订阅方式
            SceneManager.sceneLoaded += (UnityEngine.Events.UnityAction<Scene, LoadSceneMode>)OnSceneLoaded;





            // 创建背景面板（带异常处理）
            try
            {
                CreateRedBackground(canvasRoot.transform);
            }
            catch (System.Exception ex)
            {
                Main.SharedLog.LogError($"Background creation failed: {ex}");
            }

        }
        // 修改场景加载事件处理
        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            // 移除可能导致UI重复初始化的代码
            // 不再主动销毁canvasRoot，改为保持单例
            if (canvasRoot != null && canvasRoot.scene != scene)
            {
                // 使用安全方式重新父级化
                canvasRoot.transform.SetParent(null);
                GameObject.DontDestroyOnLoad(canvasRoot);
            }

            // 使用更可靠的协程启动方式
            if (_instance != null && canvasRoot != null)
            {
                _instance.StartCoroutine(SceneTransitionRoutine().WrapToIl2Cpp());
            }
        }
        private static System.Collections.IEnumerator SceneTransitionRoutine()
        {
            // 等待渲染线程完成
            yield return new WaitForEndOfFrame();
            yield return new WaitForFixedUpdate();

            // 增强的组件重绑定
            RebindingUIElements();

            // 强制刷新所有布局
            LayoutRebuilder.ForceRebuildLayoutImmediate(canvasRoot.GetComponent<RectTransform>());
            Canvas.ForceUpdateCanvases();

            // 重置文本显示
            if (vocabularyList.Count > 0)
            {
                RandomizeAllTextContents();
            }
            else
            {
                LoadVocabularyData();
            }

            // 确保字体资源可用
            ReinitializeFontResources();
        }
        private static void ReinitializeFontResources()
        {
            // 确保字体资源重新加载
            if (consoleFont == null || !consoleFont)
            {
                consoleFont = Resources.GetBuiltinResource<Font>("Arial.ttf");
                if (consoleFont == null)
                {
                    consoleFont = Font.CreateDynamicFontFromOSFont("Arial", 200);
                }
            }
            
            // 更新所有文本组件的字体引用
            foreach (var text in canvasRoot.GetComponentsInChildren<UnityEngine.UI.Text>(true))
            {
                text.font = consoleFont;
                //text.horizontalOverflow = HorizontalWrapMode.Overflow;
                //text.verticalOverflow = VerticalWrapMode.Overflow;
            }
        }

        // 使用C#原生迭代器接口（关键修改）
        private static System.Collections.IEnumerator DelayedUISetup()
        {
            yield return new WaitForEndOfFrame();

            // 确保在创建UI前加载数据
            LoadVocabularyData();

            CreateRootCanvas();
            yield return new WaitForEndOfFrame(); // 等待一帧确保UI元素生成

            RebindingUIElements();

            // 强制刷新文本显示
            if (canvasRoot != null)
            {
                RandomizeAllTextContents(); // 立即更新文本内容
                LayoutRebuilder.ForceRebuildLayoutImmediate(canvasRoot.GetComponent<RectTransform>());
                Canvas.ForceUpdateCanvases();
            }
        }


        private static void CreateEventSystem()
        {
            // 增强的事件系统创建逻辑
            if (GameObject.FindObjectOfType<EventSystem>() != null) return;

            var eventObject = new GameObject("EventSystem");
            eventObject.transform.SetParent(canvasRoot.transform);
            eventObject.AddComponent<EventSystem>();
            eventObject.AddComponent<StandaloneInputModule>();
        }



        private static void CreateRedBackground(Transform parent)
        {
            // 创建背景面板
            GameObject panelObj = new GameObject("CustomBackground");
            panelObj.transform.SetParent(parent, false);



            float dynamicMargin = Screen.height * MARGIN_RATIO;

            var rect = panelObj.AddComponent<RectTransform>();
            // 使用中心锚点实现动态居中
            rect.anchorMin = new UnityEngine.Vector2(0.5f, 0.5f);
            rect.anchorMax = new UnityEngine.Vector2(0.5f, 0.5f);
            // 动态计算面板尺寸
            rect.sizeDelta = new UnityEngine.Vector2(
                Screen.width * PANEL_SCREEN_RATIO,
                Screen.height * PANEL_SCREEN_RATIO
            );
            rect.anchoredPosition = UnityEngine.Vector2.zero;

            // 添加自适应布局组件
            var layoutGroup = panelObj.AddComponent<VerticalLayoutGroup>();
            layoutGroup.spacing = Screen.height * SPACING_RATIO;
            layoutGroup.padding = new RectOffset(
                (int)(dynamicMargin),
                (int)(dynamicMargin),
                (int)(dynamicMargin),
                (int)(dynamicMargin)
            );
            layoutGroup.childControlHeight = true;
            layoutGroup.childControlWidth = true;

            // 添加尺寸适配组件
            var contentSizeFitter = panelObj.AddComponent<ContentSizeFitter>();
            contentSizeFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            contentSizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            /*
            // 配置矩形变换（保持原半屏居中布局）
            var rect = panelObj.AddComponent<RectTransform>();
            rect.anchorMin = new UnityEngine.Vector2(0.5f, 0.5f);
            rect.anchorMax = new UnityEngine.Vector2(0.5f, 0.5f);
            rect.sizeDelta = new UnityEngine.Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
            rect.anchoredPosition = UnityEngine.Vector2.zero;
            */


            // 加载外部图片
            var image = panelObj.AddComponent<UnityEngine.UI.Image>();
            //try
            //{
            string imagePath = Il2CppSystem.IO.Path.Combine(
                Il2CppSystem.IO.Path.GetDirectoryName(UnityEngine.Application.dataPath), // 获取游戏根目录
                "BepInEx",
                "plugins",
                "CustomUI",
                "Images"

            );
            foreach (string item in Il2CppSystem.IO.Directory.GetFiles(imagePath, "*.png", Il2CppSystem.IO.SearchOption.AllDirectories))
            {
                Lethe.Texture.Texture.spritePaths.Add(item);
                Main.SharedLog.LogWarning(" ITEM " + item);
                if (item.Contains("background"))
                {
                    Sprite backgroundSprite = Lethe.Texture.Texture.LoadSpriteFromFile(item);
                    image.sprite = backgroundSprite;
                }

            }
            Main.SharedLog.LogWarning(string.Format(imagePath));

            var material = new Material(Shader.Find("UI/Default"));
            material.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
            material.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
            material.EnableKeyword("_ALPHABLEND_ON");
            image.material = material;

            // 轮廓设置保持不变
            var outline = panelObj.AddComponent<Outline>();
            outline.effectColor = new UnityEngine.Color(0, 0, 0, 0.5f);
            outline.effectDistance = new UnityEngine.Vector2(2, -2);

            GraphicRegistry.RegisterGraphicForCanvas(canvasRoot.GetComponent<Canvas>(), image);

            /*
            //垂直布局
            var verticalLayout = panelObj.AddComponent<VerticalLayoutGroup>();
            verticalLayout.spacing = 10f;
            verticalLayout.padding = new RectOffset(20, 20, 20, 20);
            verticalLayout.childControlHeight = true;
            verticalLayout.childControlWidth = true;
            verticalLayout.childForceExpandHeight = false;
            verticalLayout.childForceExpandWidth = false;*/

            // 创建上半部分文本框
            CreateTextField(panelObj.transform);

            // 创建下半部分按钮容器
            CreateButtonContainer(panelObj.transform);
        }
        private static void CreateTextField(Transform parent)
        {
            // 保留原有滚动视图结构
            GameObject textField = new GameObject("TextField");
            textField.transform.SetParent(parent, false);

            // 保持原有布局设置
            var layoutElement = textField.AddComponent<LayoutElement>();
            layoutElement.flexibleHeight = 1;
            layoutElement.preferredHeight = Screen.height * 0.5f - 20;

            // 保留背景设置
            var bgImage = textField.AddComponent<UnityEngine.UI.Image>();
            bgImage.color = new UnityEngine.Color(0.2f, 0.2f, 0.2f, 0.8f);
            bgImage.enabled = false; // 直接禁用背景渲染

            // 保持滚动视图
            var scrollView = textField.AddComponent<ScrollRect>();
            scrollView.horizontal = true;
            scrollView.vertical = false;

            // 创建视口容器（保持原有参数）
            GameObject viewport = new GameObject("Viewport");
            viewport.transform.SetParent(textField.transform);
            var viewportImage = viewport.AddComponent<UnityEngine.UI.Image>();
            viewportImage.color = new UnityEngine.Color(0, 0, 0, 0.2f);
            viewportImage.enabled = false; // 直接禁用背景渲染
            var viewportRect = viewport.GetComponent<RectTransform>();
            viewportRect.anchorMin = UnityEngine.Vector2.zero;
            viewportRect.anchorMax = UnityEngine.Vector2.one;
            viewportRect.offsetMin = new UnityEngine.Vector2(25, 25);
            viewportRect.offsetMax = new UnityEngine.Vector2(-25, -25);

            // 创建垂直布局的内容容器
            GameObject contentContainer = new GameObject("ContentContainer");
            contentContainer.transform.SetParent(viewport.transform);
            var verticalLayout = contentContainer.AddComponent<VerticalLayoutGroup>();
            verticalLayout.spacing = 5f;
            verticalLayout.childControlHeight = false;
            verticalLayout.childForceExpandHeight = false;

            // 配置内容区域RectTransform
            var contentRect = contentContainer.GetComponent<RectTransform>();
            contentRect.anchorMin = new UnityEngine.Vector2(0, 0.5f);
            contentRect.anchorMax = new UnityEngine.Vector2(1, 0.5f);
            contentRect.sizeDelta = new UnityEngine.Vector2(Screen.width * 2f, 200); // 双倍宽度，更高高度
            contentRect.pivot = new UnityEngine.Vector2(0, 0.5f);
            contentRect.anchoredPosition = UnityEngine.Vector2.zero;

            // 创建单词文本组件
            GameObject wordObj = new GameObject("WordText");
            wordObj.transform.SetParent(contentContainer.transform);
            wordTextComponent = wordObj.AddComponent<UnityEngine.UI.Text>();
            wordTextComponent.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            wordTextComponent.color = UnityEngine.Color.black;
            wordTextComponent.fontSize = 200;
            wordTextComponent.alignment = TextAnchor.MiddleLeft;

            // 添加单词文本布局元素
            var wordLayout = wordObj.AddComponent<LayoutElement>();
            wordLayout.preferredHeight = 60f;

            // 创建音标文本组件
            GameObject phoneticObj = new GameObject("PhoneticText");
            phoneticObj.transform.SetParent(contentContainer.transform);
            phoneticTextComponent = phoneticObj.AddComponent<UnityEngine.UI.Text>();
            phoneticTextComponent.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            phoneticTextComponent.color = new UnityEngine.Color(0.4f, 0.4f, 0.4f, 1f);
            phoneticTextComponent.fontSize = 200;
            phoneticTextComponent.alignment = TextAnchor.MiddleLeft;
            phoneticTextComponent.fontStyle = FontStyle.Italic;

            // 连接滚动组件（保持原有滚动设置）
            //scrollView.content = contentRect;
            //scrollView.viewport = viewportRect;

            // 强制刷新布局
            //LayoutRebuilder.ForceRebuildLayoutImmediate(contentContainer.GetComponent<RectTransform>());
        }




        private static void CreateButtonContainer(Transform parent)
        {
            // 按钮容器
            GameObject buttonContainer = new GameObject("ButtonContainer");
            buttonContainer.transform.SetParent(parent, false);

            // 动态网格布局配置
            var gridLayout = buttonContainer.AddComponent<GridLayoutGroup>();
            gridLayout.cellSize = CalculateButtonSize(parent);
            gridLayout.spacing = new UnityEngine.Vector2(
                Screen.width * SPACING_RATIO,
                Screen.height * SPACING_RATIO
            );
            gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            gridLayout.constraintCount = 2;

            // 添加自适应布局组件
            var sizeFitter = buttonContainer.AddComponent<ContentSizeFitter>();
            sizeFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            sizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;


            // 创建四个按钮
            for (int i = 0; i < 4; i++)
            {
                CreateCustomButton(gridLayout.transform, i);
            }

        }
        private static UnityEngine.Vector2 CalculateButtonSize(Transform parent)
        {
            // 安全转换方法调用
            var parentRect = GetRectTransformSafe(parent);
            if (parentRect == null) return UnityEngine.Vector2.zero;

            float containerWidth = parentRect.rect.width;
            float spacing = Screen.width * SPACING_RATIO;
            float buttonWidth = Mathf.Clamp((containerWidth - spacing * 3) / 2, 100, 400);
            float buttonHeight = buttonWidth / BUTTON_ASPECT_RATIO;

            return new UnityEngine.Vector2(buttonWidth, buttonHeight);
        }
        private static void CreateCustomButton(Transform parent, int buttonIndex)
        {
            // 创建按钮对象
            GameObject buttonObj = new GameObject("CustomButton");
            buttonObj.transform.SetParent(parent, false);

            // 添加按钮组件
            var button = buttonObj.AddComponent<UnityEngine.UI.Button>();

            // 加载按钮背景图
            string buttonPath = Il2CppSystem.IO.Path.Combine(
                Il2CppSystem.IO.Path.GetDirectoryName(UnityEngine.Application.dataPath),
                "BepInEx",
                "plugins",
                "CustomUI",
                "Images",
                "button.png"
            );

            // 添加背景图
            var image = buttonObj.AddComponent<UnityEngine.UI.Image>();
            image.sprite = Lethe.Texture.Texture.LoadSpriteFromFile(buttonPath);
            image.type = UnityEngine.UI.Image.Type.Sliced;

            // 设置按钮过渡效果
            var colors = button.colors;
            colors.normalColor = UnityEngine.Color.white;
            colors.highlightedColor = new UnityEngine.Color(0.8f, 0.8f, 0.8f);
            colors.pressedColor = new UnityEngine.Color(0.6f, 0.6f, 0.6f);
            button.colors = colors;

            // 添加按钮文本
            GameObject textObj = new GameObject("ButtonText");
            textObj.transform.SetParent(buttonObj.transform);

            var textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = UnityEngine.Vector2.zero;
            textRect.anchorMax = UnityEngine.Vector2.one;
            textRect.offsetMin = new UnityEngine.Vector2(20, 20); // 左下方偏移
            textRect.offsetMax = new UnityEngine.Vector2(-20, -20); // 右上方偏移

            var textComponent = textObj.AddComponent<UnityEngine.UI.Text>();
            textComponent.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            textComponent.color = UnityEngine.Color.black;
            textComponent.fontSize = 200;
            textComponent.alignment = TextAnchor.MiddleCenter;

            // 新增文本换行设置
            textComponent.horizontalOverflow = HorizontalWrapMode.Wrap;
            textComponent.verticalOverflow = VerticalWrapMode.Truncate;
            textComponent.resizeTextForBestFit = true;
            textComponent.resizeTextMinSize = 20;
            textComponent.resizeTextMaxSize = 200;

            optionTexts.Add(textComponent);

            // 修改点击事件监听
            //button.onClick.AddListener(() => OnButtonClick(buttonIndex));
            button.onClick.AddListener((UnityEngine.Events.UnityAction)(() => OnButtonClick(buttonIndex)));
        }
        private static void givebuf(bool correct=true)
        {
            if (correct)
            {
                // 定义7个不同的代码块
                var actions = new System.Collections.Generic.List<System.Action>
                {
                    () => buffSinner(BUFF_UNIQUE_KEYWORD.AttackDmgUp,6,0),//伤害
                    () => buffSinner(BUFF_UNIQUE_KEYWORD.Enhancement,6,0),//强壮
                    () => buffSinner(BUFF_UNIQUE_KEYWORD.PlusCoinValueUp,4,0),//硬币威力
                    () => buffSinner(BUFF_UNIQUE_KEYWORD.Protection,7,0),//守护
                    () => buffSinner(BUFF_UNIQUE_KEYWORD.ParryingResultUp,7,0),//拼点威力
                    () => hpEdit(45),
                    () => mpEdit(25),
                };

                // 创建线程安全的随机源
                var rnd = System.Random.Shared;

                // 随机选择并执行3个不重复的操作
                foreach (var index in Enumerable.Range(0, 7).OrderBy(_ => rnd.Next()).Take(4))
                {
                    actions[index]();
                }
            }
            else
            {
                // 定义7个不同的代码块
                var actions = new System.Collections.Generic.List<System.Action>
                {
                    () => buffSinner(BUFF_UNIQUE_KEYWORD.AttackDmgDown,6,0),//伤害
                    () => buffSinner(BUFF_UNIQUE_KEYWORD.Reduction,6,0),//强壮
                    () => buffSinner(BUFF_UNIQUE_KEYWORD.PlusCoinValueDown,4,0),//硬币威力
                    () => buffSinner(BUFF_UNIQUE_KEYWORD.Vulnerable,7,0),//守护
                    () => buffSinner(BUFF_UNIQUE_KEYWORD.ParryingResultDown,7,0),//拼点威力
                    () => hpEdit(-30),
                    () => mpEdit(-20),
                };

                // 创建线程安全的随机源
                var rnd = System.Random.Shared;

                // 随机选择并执行3个不重复的操作
                foreach (var index in Enumerable.Range(0, 7).OrderBy(_ => rnd.Next()).Take(4))
                {
                    actions[index]();
                }
            }
        }
        private static void hpEdit(int hp)
        {
            if (hp > 0)
            {
                Il2CppSystem.Collections.Generic.List<BattleUnitModel> list = new Il2CppSystem.Collections.Generic.List<BattleUnitModel>();
                Il2CppSystem.Collections.Generic.List<BattleUnitModel> enemylist = new Il2CppSystem.Collections.Generic.List<BattleUnitModel>();
                BattleObjectManager battleObjectManager = Singleton<SinManager>.Instance._battleObjectManager;

                AbilityBase dummyAbility = new AbilityBase();

                foreach (BattleUnitModel battleUnitModel in battleObjectManager.GetModelList())
                {
                    Main.SharedLog.LogWarning($"model faction{battleUnitModel.Faction}");
                    if (battleUnitModel.Faction == UNIT_FACTION.PLAYER)
                    {
                        list.Add(battleUnitModel);
                    }

                }
                foreach (BattleUnitModel model in list)
                {
                    int num2;
                    Main.SharedLog.LogWarning($"hp?{model.Faction}");
                    model.TryRecoverHp(model, null, hp, ABILITY_SOURCE_TYPE.EVENT, BATTLE_EVENT_TIMING.ON_BATTLE_START, out num2);
                }
            }
            else
            {

                Il2CppSystem.Collections.Generic.List<BattleUnitModel> list = new Il2CppSystem.Collections.Generic.List<BattleUnitModel>();
                Il2CppSystem.Collections.Generic.List<BattleUnitModel> enemylist = new Il2CppSystem.Collections.Generic.List<BattleUnitModel>();
                BattleObjectManager battleObjectManager = Singleton<SinManager>.Instance._battleObjectManager;

                AbilityBase dummyAbility = new AbilityBase();

                foreach (BattleUnitModel battleUnitModel in battleObjectManager.GetModelList())
                {
                    Main.SharedLog.LogWarning($"model faction{battleUnitModel.Faction}");
                    if (battleUnitModel.Faction == UNIT_FACTION.PLAYER)
                    {
                        list.Add(battleUnitModel);
                    }

                }
                foreach (BattleUnitModel model in list)
                {
                    int num2;
                    int abilityMode;
                    Main.SharedLog.LogWarning($"hp?{model.Faction}");
                    model.TakeAbsHpDamage(null, hp * -1, out num2, out abilityMode, BATTLE_EVENT_TIMING.ON_BATTLE_START, DAMAGE_SOURCE_TYPE.NONE, null, null, (BUFF_UNIQUE_KEYWORD)1160, false);
                }

            }
        }
        private static void mpEdit(int mp)
        {
            if (mp > 0)
            {
                Il2CppSystem.Collections.Generic.List<BattleUnitModel> list = new Il2CppSystem.Collections.Generic.List<BattleUnitModel>();
                Il2CppSystem.Collections.Generic.List<BattleUnitModel> enemylist = new Il2CppSystem.Collections.Generic.List<BattleUnitModel>();
                BattleObjectManager battleObjectManager = Singleton<SinManager>.Instance._battleObjectManager;

                AbilityBase dummyAbility = new AbilityBase();

                foreach (BattleUnitModel battleUnitModel in battleObjectManager.GetModelList())
                {
                    Main.SharedLog.LogWarning($"model faction{battleUnitModel.Faction}");
                    if (battleUnitModel.Faction == UNIT_FACTION.PLAYER)
                    {
                        list.Add(battleUnitModel);
                    }

                }
                foreach (BattleUnitModel model in list)
                {
                    int num2;
                    Main.SharedLog.LogWarning($"hp?{model.Faction}");
                    model.HealTargetMp(model, mp, ABILITY_SOURCE_TYPE.SKILL, BATTLE_EVENT_TIMING.ON_BATTLE_START);
                }
            }
            else
            {

                Il2CppSystem.Collections.Generic.List<BattleUnitModel> list = new Il2CppSystem.Collections.Generic.List<BattleUnitModel>();
                Il2CppSystem.Collections.Generic.List<BattleUnitModel> enemylist = new Il2CppSystem.Collections.Generic.List<BattleUnitModel>();
                BattleObjectManager battleObjectManager = Singleton<SinManager>.Instance._battleObjectManager;

                AbilityBase dummyAbility = new AbilityBase();

                foreach (BattleUnitModel battleUnitModel in battleObjectManager.GetModelList())
                {
                    Main.SharedLog.LogWarning($"model faction{battleUnitModel.Faction}");
                    if (battleUnitModel.Faction == UNIT_FACTION.PLAYER)
                    {
                        list.Add(battleUnitModel);
                    }

                }
                foreach (BattleUnitModel model in list)
                {
                    int num=0;
                    int abilityMode;
                    Main.SharedLog.LogWarning($"hp?{model.Faction}");
                    model.GiveMpDamage(model, mp * -1, BATTLE_EVENT_TIMING.ON_BATTLE_START, DAMAGE_SOURCE_TYPE.PASSIVE, out num, null, (BUFF_UNIQUE_KEYWORD)1160, null);
                }

            }
        }
        private static void OnButtonClick(int buttonIndex)
        {
            try
            {
                // 添加静态变量跟踪答题状态
                if (!hasAnswered)
                {
                    // 第一次点击处理答案显示
                    if (buttonIndex == correctButtonIndex)
                    {
                        optionTexts[buttonIndex].color = UnityEngine.Color.green;
                        try
                        {
                            givebuf(true);
                        }
                        catch(System.Exception ex)
                        {

                        }
                        finally
                        {

                        }
                        
                    }
                    else
                    {
                        optionTexts[buttonIndex].color = UnityEngine.Color.red;
                        optionTexts[correctButtonIndex].color = UnityEngine.Color.green;
                        try
                        {
                            givebuf(false);
                        }
                        catch (System.Exception ex)
                        {

                        }
                        finally
                        {

                        }

                    }
                    hasAnswered = true;
                    //buffSinner(BUFF_UNIQUE_KEYWORD.Agility, 1, 1);
                    
                    //SingletonBehavior<BattleObjectManager>.Instance.RefreshPosition();
                    //SingletonBehavior<BattleObjectManager>.Instance.RefreshRandomValue();
                    
                }
                else
                {
                    // 第二次点击任何按钮时隐藏UI
                    SetUIVisibility(false);
                    hasAnswered = false;
                }
            }
            catch (System.Exception ex)
            {
                Main.SharedLog.LogError($"Button click error: {ex}");
            }
        }

        // 在类顶部添加状态变量
        public static bool hasAnswered = false;





        private static Sprite LoadPictureSprite(string path)
        {
            if (Il2CppSystem.IO.File.Exists(path))
            {
                Il2CppStructArray<byte> il2CppStructArray = Il2CppSystem.IO.File.ReadAllBytes(path);
                Texture2D tex = new Texture2D(2, 2);
                ImageConversion.LoadImage(tex, il2CppStructArray);
                return Sprite.Create(tex,
                    new Rect(0, 0, tex.width, tex.height),
                    new UnityEngine.Vector2(0.5f, 0.5f),
                    pixelsPerUnit: 100);
            }
            else
            {
                Texture2D tex = new Texture2D(2, 2);
                return Sprite.Create(tex,
                    new Rect(0, 0, tex.width, tex.height),
                    new UnityEngine.Vector2(0.5f, 0.5f),
                    pixelsPerUnit: 100);
            }
        }
        private static Sprite CreateDefaultSprite()
        {
            // 创建红色占位符纹理
            Texture2D tex = new Texture2D(2, 2);
            UnityEngine.Color[] pixels = { UnityEngine.Color.blue, UnityEngine.Color.blue, UnityEngine.Color.blue, UnityEngine.Color.blue };
            tex.SetPixels(pixels);
            tex.Apply();

            return Sprite.Create(tex,
                new Rect(0, 0, tex.width, tex.height),
                new UnityEngine.Vector2(0.5f, 0.5f),
                pixelsPerUnit: 100);
        }











        private void Update()
        {
            // 保留原有的注释代码
            /*
            if (cachedTarget == null)
            {
                cachedTarget = GameObject.Find("[Rect]AutoSelectButton");
            }

            if (cachedTarget != null && cachedTarget.activeInHierarchy)
            {
                cachedTarget.SetActive(false);
            }*/

            // 新增的UI更新逻辑
            if (canvasRoot && (lastScreenSize.x != Screen.width || lastScreenSize.y != Screen.height))
            {
                lastScreenSize = new UnityEngine.Vector2(Screen.width, Screen.height);
                RefreshLayout();
            }
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                hasAnswered = false;
                SetUIVisibility(false);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                LoadVocabularyData();
                hasAnswered = false;
                RandomizeAllTextContents();
                SetUIVisibility(true);
              
            }
        }
        public static void SetUIVisibility(bool visible)
        {
            if (canvasRoot != null)
            {
                // 激活前强制重新绑定组件
               

                canvasRoot.SetActive(visible);
                Main.SharedLog.LogWarning($"UI可见性已设置为: {visible}");

                if (visible)
                {
                    LayoutRebuilder.ForceRebuildLayoutImmediate(
                        canvasRoot.GetComponent<RectTransform>()
                    );
                }
                if (visible) RebindingUIElements();
            }
        }
        private static void RebindingUIElements()
        {
            if (canvasRoot == null) return;

            // 深度搜索所有子物体
            optionTexts = canvasRoot.GetComponentsInChildren<UnityEngine.UI.Text>(true)
                .Where(t => t.name == "ButtonText" && t.transform.parent.parent.name == "ButtonContainer")
                .ToList();

            // 精确匹配文本组件
            wordTextComponent = canvasRoot.GetComponentsInChildren<UnityEngine.UI.Text>(true)
                .FirstOrDefault(t => t.name == "WordText" && t.transform.parent.parent.parent.name == "TextField");

            phoneticTextComponent = canvasRoot.GetComponentsInChildren<UnityEngine.UI.Text>(true)
                .FirstOrDefault(t => t.name == "PhoneticText" && t.transform.parent.parent.parent.name == "TextField");

            // 强制激活文本组件
            if (wordTextComponent != null) wordTextComponent.gameObject.SetActive(true);
            if (phoneticTextComponent != null) phoneticTextComponent.gameObject.SetActive(true);
            optionTexts.ForEach(t => t.gameObject.SetActive(true));

            Main.SharedLog.LogWarning($"组件重绑定 - 单词: {wordTextComponent != null}, 音标: {phoneticTextComponent != null}, 选项数: {optionTexts.Count}");
        }

        // 功能2：统一文本随机化函数
        public static void RandomizeAllTextContents()
        {
            try
            {
                // 确保UI组件引用有效
                if (wordTextComponent == null || phoneticTextComponent == null || optionTexts.Count == 0)
                {
                    RebindingUIElements();
                    if (wordTextComponent == null || phoneticTextComponent == null || optionTexts.Count == 0)
                    {
                        Main.SharedLog.LogError($"UIfail{wordTextComponent},{phoneticTextComponent},{optionTexts.Count}");
                       // return;
                    }
                }



                if (vocabularyList.Count == 0) LoadVocabularyData();

                // 随机选择当前单词
                currentWord = vocabularyList[UnityEngine.Random.Range(0, vocabularyList.Count)];

                // 更新主显示
                wordTextComponent.text = currentWord.Word;
                phoneticTextComponent.text = currentWord.Phonetic;

                // 生成选项
                var wrongDefinitions = vocabularyList
                    .Where(v => v.Definition != currentWord.Definition)
                    .Select(v => v.Definition)
                    .Distinct()
                    .ToList();

                System.Collections.Generic.List<string> options = new System.Collections.Generic.List<string> { currentWord.Definition };
                while (options.Count < 4 && wrongDefinitions.Count > 0)
                {
                    int index = UnityEngine.Random.Range(0, wrongDefinitions.Count);
                    options.Add(wrongDefinitions[index]);
                    wrongDefinitions.RemoveAt(index);
                }

                // 随机打乱选项
                options = options.OrderBy(x => UnityEngine.Random.value).ToList();
                correctButtonIndex = options.IndexOf(currentWord.Definition);

                // 更新按钮文本
                for (int i = 0; i < optionTexts.Count; i++)
                {
                    if (i < options.Count)
                    {
                        optionTexts[i].text = options[i];
                        optionTexts[i].color = UnityEngine.Color.black; // 重置颜色
                    }
                }

            }
            catch (System.Exception ex)
            {
                Main.SharedLog.LogError($"Randomize failed: {ex}");
            }
        }

        private UnityEngine.Vector2 lastScreenSize;
        private void RefreshLayout()
        {
            if (canvasRoot == null) return;

            // 原有网格布局刷新保持不变
            var gridLayouts = canvasRoot.GetComponentsInChildren<GridLayoutGroup>(true);
            foreach (var grid in gridLayouts)
            {
                var parentTransform = grid.transform.parent;
                if (parentTransform == null) continue;

                var newSize = CalculateButtonSize(parentTransform);
                if (newSize != UnityEngine.Vector2.zero)
                {
                    grid.cellSize = newSize;
                    grid.spacing = new UnityEngine.Vector2(
                        Screen.width * SPACING_RATIO,
                        Screen.height * SPACING_RATIO
                    );
                }
            }

            // 修改后的文本尺寸调整逻辑
            var texts = canvasRoot.GetComponentsInChildren<UnityEngine.UI.Text>(true);
            foreach (var text in texts)
            {
                // 通过父对象层级判断主文本框
                var parentName = text.transform.parent?.parent?.parent?.name;

                // 主文本框逻辑
                if (text.transform.parent?.name == "ContentContainer" ||
                    text.name == "WordText" ||
                    text.name == "PhoneticText")
                {
                    // 使用更大的相对尺寸（0.045f），并增加范围限制
                    text.fontSize = Mathf.Clamp((int)(Screen.height * 0.105f), 50, 300);

                    // 针对音标文本的特殊处理
                    if (text.name == "PhoneticText")
                        text.fontSize = (int)(text.fontSize * 0.9f); // 音标比单词小10%
                }
                // 按钮文本保持原尺寸（0.03f）
                else if (text.name == "ButtonText")
                {
                    // 保留原有计算方式，但增加最小尺寸限制
                    text.fontSize = Mathf.Clamp((int)(Screen.height * 0.03f), 18, 160);
                }
            }
        }



    }




    public class AutoSelectButtonRemover : MonoBehaviour
    {

        public static NewOperationController controller = null;

        [HarmonyPatch]
        public static void Setup(Harmony harmony)
        {
            ClassInjector.RegisterTypeInIl2Cpp<AutoSelectButtonRemover>();
            harmony.PatchAll(typeof(AutoSelectButtonRemover));
        }

        [HarmonyPatch(typeof(EventTrigger), "Execute")]
        [HarmonyPrefix]
        private static void EventTriggerMonitor(
                                EventTrigger __instance,
                                EventTriggerType id,
                                BaseEventData eventData)
        {
            try
            {
                int callbackCount = 0;
                foreach (EventTrigger.Entry entry in __instance.triggers)
                {
                    if (entry.eventID == id)
                    {
                        callbackCount += entry.callback.GetPersistentEventCount();
                    }
                }

                string logHeader = "[EventTrigger] " + __instance.gameObject.name + " | ";
                string eventInfo = "Type: " + id.ToString() + " | ";
                string callbackInfo = "Callbacks: " + callbackCount.ToString();

                Main.SharedLog.LogWarning(logHeader + eventInfo + callbackInfo);

                foreach (EventTrigger.Entry entry in __instance.triggers)
                {
                    if (entry.eventID == id)
                    {
                        for (int i = 0; i < entry.callback.GetPersistentEventCount(); i++)
                        {
                            UnityEngine.Object target = entry.callback.GetPersistentTarget(i);
                            string methodName = entry.callback.GetPersistentMethodName(i);

                            Main.SharedLog.LogWarning(
                                "Target: " + target.name +
                                " | Method: " + methodName +
                                " | Index: " + i.ToString()
                            );
                        }
                    }
                }
            }
            catch (System.Exception e)
            {
                Main.SharedLog.LogError("Monitor Error: " + e.ToString());
            }
        }
        [HarmonyPatch(typeof(StageController), "StartRoundAfterAbnormalityChoice_Init")]
        [HarmonyPostfix]
        private static void Postfix_StageController_StartRoundAfterAbnormalityChoice_Init()
        {
            Main.SharedLog.LogWarning("StartRoundAfterAbnormalityChoice_Init");
            ChangeAppearanceTemp.LoadVocabularyData();
            ChangeAppearanceTemp.hasAnswered = false;
            ChangeAppearanceTemp.RandomizeAllTextContents();
            ChangeAppearanceTemp.SetUIVisibility(true);
        }
        [HarmonyPatch(typeof(BattleUnitView), "OnRoundEnd")]
        [HarmonyPostfix]
        private static void OnRoundEnd(BattleUnitView __instance)
        {
            Main.SharedLog.LogWarning("OnRoundEnd");
            ChangeAppearanceTemp.hasAnswered = false;
            ChangeAppearanceTemp.SetUIVisibility(false);
        }





    }
    [BepInPlugin(Guid, Name, Version)]
    public class Main : BasePlugin
    {
        public const string Guid = Author + "." + Name;
        public const string Name = "BanAutoSelectButton";
        public const string Version = "0.0.1";
        public const string Author = "Tintagedfish";
        public static ManualLogSource SharedLog;

        private static int cub = 1;

        public override void Load()
        {
            // 打印日志信息
            SharedLog = Log;
            Harmony harmony = new Harmony("BanAutoSelectButton");

            Harmony.CreateAndPatchAll(typeof(Main));

            AutoSelectButtonRemover.Setup(harmony);
            ChangeAppearanceTemp.Setup(harmony);
            Log.LogInfo("Hello LimbusConpanay!!!This is BanAutoSelectButton MOD!!!");
            Log.LogWarning("This is Warning!!!From BanAutoSelectButton MOD.");
        }
        


    }
}