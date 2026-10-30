using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
using UnityEngine.InputSystem.UI;
#endif

// «Тиын жинаушы» ойынының сахналарын (MainMenu, Game, Level2, Level3), префабын,
// анимациясын және баптауларын жоба бірінші ашылғанда өздігінен құрады.
// Қайта құру: жоғарғы мәзір  Coin Collector → Build game.
[InitializeOnLoad]
public static class GameBuilder
{
    const string ScenesDir = "Assets/Scenes";
    const string TmpSettingsPath = "Assets/TextMesh Pro/Resources/TMP Settings.asset";

    static TMP_FontAsset font;
    static Sprite sqSprite, circleSprite, coinSprite, playerSprite, spikeSprite;
    static AudioClip music, coinClip;
    static GameObject coinPrefab;
    static PhysicsMaterial2D noFriction;

    static bool working;
    static bool importRequested;
    static double startTime;

    static GameBuilder()
    {
        EditorApplication.delayCall += AutoRun;
    }

    static void AutoRun()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) return;
        bool done = File.Exists(ScenesDir + "/MainMenu.unity") &&
                    File.Exists(ScenesDir + "/Game.unity") &&
                    File.Exists(ScenesDir + "/Level2.unity") &&
                    File.Exists(ScenesDir + "/Level3.unity");
        if (!done) StartBuild();
    }

    [MenuItem("Coin Collector/Build game")]
    static void MenuBuild()
    {
        StartBuild();
    }

    static void StartBuild()
    {
        if (working) return;
        working = true;
        importRequested = false;
        startTime = EditorApplication.timeSinceStartup;
        EditorApplication.update += Tick;
    }

    // TMP Essentials дайын болғанша күтеді, содан кейін барлығын құрады
    static void Tick()
    {
        if (EditorApplication.isCompiling || EditorApplication.isUpdating) return;
        if (EditorApplication.isPlayingOrWillChangePlaymode) return;

        if (!File.Exists(TmpSettingsPath))
        {
            if (!importRequested)
            {
                importRequested = true;
                ImportTmpEssentials();
            }
            if (EditorApplication.timeSinceStartup - startTime > 120.0)
            {
                EditorApplication.update -= Tick;
                working = false;
                Debug.LogError("TMP Essentials табылмады. Window → TextMeshPro → Import TMP Essential Resources, содан кейін Coin Collector → Build game.");
            }
            return;
        }

        EditorApplication.update -= Tick;
        Build();
        working = false;
    }

    static void ImportTmpEssentials()
    {
        var info = UnityEditor.PackageManager.PackageInfo.FindForAssembly(typeof(TMP_Text).Assembly);
        if (info == null) { Debug.LogError("TextMeshPro пакеті табылмады."); return; }
        string pkg = Path.Combine(info.resolvedPath, "Package Resources", "TMP Essential Resources.unitypackage");
        if (!File.Exists(pkg)) { Debug.LogError("TMP Essential Resources табылмады: " + pkg); return; }
        AssetDatabase.ImportPackage(pkg, false);
    }

    // ---------------------------------------------------------------- BUILD

    static void Build()
    {
        try
        {
            EditorSettings.defaultBehaviorMode = EditorBehaviorMode.Mode2D;
            AddTags();
            foreach (string d in new[] { "Scenes", "Prefabs", "Animations", "Materials" })
                Directory.CreateDirectory("Assets/" + d);
            AssetDatabase.Refresh();

            LoadSprites();
            music = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/music.wav");
            coinClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/coin.wav");
            MakeFont();
            MakeMaterial();
            MakeCoinPrefab();

            BuildMenuScene();

            // 1-деңгей (Game): жер, 3 шип, 5 тиын. Шиптердің үстінде тиындар: секіру керек.
            BuildLevel("Game", 1,
                new Color(0.07f, 0.17f, 0.24f), new Color(0.30f, 0.62f, 0.30f),
                new float[][] { },
                new[] { V(-3f, -3f), V(2f, -3f), V(5.5f, -3f) },
                new[] { V(-5.5f, -2.8f), V(-3f, -1.3f), V(-0.5f, -2.8f), V(2f, -1.3f), V(5.5f, -1.3f) },
                true, true);

            // 2-деңгей: ауадағы 5 платформа, 5 шип, 8 тиын.
            BuildLevel("Level2", 2,
                new Color(0.20f, 0.11f, 0.30f), new Color(0.50f, 0.38f, 0.60f),
                new[] { P(-5.8f, -1.8f, 2.5f), P(-2.2f, -0.3f, 2.5f), P(1.4f, 1.2f, 2.5f), P(4.8f, -0.3f, 2.5f), P(7.6f, -1.8f, 2f) },
                new[] { V(-3.5f, -3f), V(0f, -3f), V(3f, -3f), V(6.2f, -3f), V(-1.5f, 0.45f) },
                new[] { V(-5.8f, -0.9f), V(-2.9f, 0.7f), V(0.8f, 2.1f), V(2f, 2.1f),
                        V(4.2f, 0.7f), V(5.4f, 0.7f), V(7.6f, -0.9f), V(1.5f, -2.8f) },
                true, false);

            // 3-деңгей (соңғы): ең биік платформалар, 6 шип, 10 тиын.
            BuildLevel("Level3", 3,
                new Color(0.28f, 0.09f, 0.10f), new Color(0.55f, 0.50f, 0.48f),
                new[] { P(-6f, -1.8f, 2.5f), P(-3f, 0f, 2.5f), P(0.2f, 1.8f, 2.5f), P(3.6f, 0f, 2.5f), P(7f, -1.8f, 2.5f) },
                new[] { V(-5f, -3f), V(-2f, -3f), V(1.5f, -3f), V(4.8f, -3f), V(1.1f, 2.55f), V(4.3f, 0.75f) },
                new[] { V(-6.6f, -0.95f), V(-5.4f, -0.95f), V(-3.7f, 0.85f), V(-2.3f, 0.85f),
                        V(-0.5f, 2.65f), V(0.4f, 2.65f), V(2.9f, 0.85f), V(7f, -0.95f),
                        V(-0.25f, -2.8f), V(3.2f, -2.8f) },
                false, false);

            EditorBuildSettings.scenes = new[] { "MainMenu", "Game", "Level2", "Level3" }
                .Select(n => new EditorBuildSettingsScene(ScenesDir + "/" + n + ".unity", true)).ToArray();

            PlayerSettings.productName = "CoinCollector";
            if (string.IsNullOrEmpty(PlayerSettings.companyName) || PlayerSettings.companyName == "DefaultCompany")
                PlayerSettings.companyName = "Student";
            PlayerSettings.fullScreenMode = FullScreenMode.Windowed;
            PlayerSettings.defaultScreenWidth = 1280;
            PlayerSettings.defaultScreenHeight = 720;

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorSceneManager.OpenScene(ScenesDir + "/Game.unity");

            Debug.Log("Тиын жинаушы: барлығы дайын. Assets/Scenes ішінде MainMenu, Game, Level2, Level3.");
            if (!Application.isBatchMode)
                EditorUtility.DisplayDialog("Тиын жинаушы",
                    "Ойын дайын!\n\nAssets/Scenes папкасында 4 сахна бар: MainMenu, Game, Level2, Level3.\n" +
                    "Қазір Game сахнасы ашық. Ойынды мәзірден бастау үшін MainMenu-ды ашып, Play басыңыз.", "Жақсы");
        }
        catch (Exception e)
        {
            Debug.LogError("Құру кезінде қате: " + e);
        }
    }

    static Vector2 V(float x, float y) { return new Vector2(x, y); }
    static float[] P(float x, float y, float w) { return new[] { x, y, w }; }

    // ---------------------------------------------------------------- ASSETS

    static void AddTags()
    {
        foreach (string t in new[] { "Ground", "Coin", "Enemy" })
            if (Array.IndexOf(UnityEditorInternal.InternalEditorUtility.tags, t) < 0)
                UnityEditorInternal.InternalEditorUtility.AddTag(t);
    }

    static Sprite LoadSprite(string path, float ppu)
    {
        var ti = AssetImporter.GetAtPath(path) as TextureImporter;
        if (ti != null && (ti.textureType != TextureImporterType.Sprite || ti.spritePixelsPerUnit != ppu))
        {
            ti.textureType = TextureImporterType.Sprite;
            ti.spriteImportMode = SpriteImportMode.Single;
            ti.spritePixelsPerUnit = ppu;
            ti.mipmapEnabled = false;
            ti.alphaIsTransparency = true;
            ti.filterMode = FilterMode.Bilinear;
            ti.SaveAndReimport();
        }
        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    static void LoadSprites()
    {
        sqSprite = LoadSprite("Assets/Sprites/square.png", 64f);
        circleSprite = LoadSprite("Assets/Sprites/circle.png", 64f);
        playerSprite = LoadSprite("Assets/Sprites/player.png", 64f);
        spikeSprite = LoadSprite("Assets/Sprites/spike.png", 64f);
        coinSprite = LoadSprite("Assets/Sprites/coin.png", 107f);   // диаметрі 0.6 бірлік
    }

    static void MakeFont()
    {
        font = null;
        try
        {
            var src = AssetDatabase.LoadAssetAtPath<Font>("Assets/Fonts/DejaVuSans.ttf");
            if (src == null) { Debug.LogWarning("Assets/Fonts/DejaVuSans.ttf табылмады."); return; }

            string fp = "Assets/Fonts/KazakhFont SDF.asset";
            font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(fp);
            if (font == null)
            {
                font = TMP_FontAsset.CreateFontAsset(src);
                font.name = "KazakhFont SDF";
                AssetDatabase.CreateAsset(font, fp);
                if (font.material != null)
                {
                    font.material.name = "KazakhFont SDF Material";
                    AssetDatabase.AddObjectToAsset(font.material, font);
                }
                if (font.atlasTextures != null)
                    foreach (var t in font.atlasTextures)
                    {
                        if (t == null) continue;
                        t.name = "KazakhFont Atlas";
                        AssetDatabase.AddObjectToAsset(t, font);
                    }
                EditorUtility.SetDirty(font);
                AssetDatabase.SaveAssets();
            }

            // Project Settings → TextMeshPro → Default Font Asset
            var settings = TMP_Settings.instance;
            if (settings != null)
            {
                var so = new SerializedObject(settings);
                var prop = so.FindProperty("m_defaultFontAsset");
                if (prop != null)
                {
                    prop.objectReferenceValue = font;
                    so.ApplyModifiedProperties();
                    EditorUtility.SetDirty(settings);
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning("Қазақ шрифтін жасау мүмкін болмады: " + e.Message);
        }
    }

    static void MakeMaterial()
    {
        string p = "Assets/Materials/NoFriction.physicsMaterial2D";
        AssetDatabase.DeleteAsset(p);
        noFriction = new PhysicsMaterial2D("NoFriction") { friction = 0f, bounciness = 0f };
        AssetDatabase.CreateAsset(noFriction, p);
    }

    static void MakeCoinPrefab()
    {
        // CoinPulse анимациясы: Scale 1 → 1.3 → 1, Loop Time ✓
        var clip = new AnimationClip { name = "CoinPulse" };
        var pulse = new AnimationCurve(new Keyframe(0f, 1f), new Keyframe(0.5f, 1.3f), new Keyframe(1f, 1f));
        var one = new AnimationCurve(new Keyframe(0f, 1f), new Keyframe(1f, 1f));
        clip.SetCurve("", typeof(Transform), "localScale.x", pulse);
        clip.SetCurve("", typeof(Transform), "localScale.y", pulse);
        clip.SetCurve("", typeof(Transform), "localScale.z", one);
        var st = AnimationUtility.GetAnimationClipSettings(clip);
        st.loopTime = true;
        AnimationUtility.SetAnimationClipSettings(clip, st);

        string clipPath = "Assets/Animations/CoinPulse.anim";
        string ctrlPath = "Assets/Animations/CoinPulse.controller";
        AssetDatabase.DeleteAsset(clipPath);
        AssetDatabase.DeleteAsset(ctrlPath);
        AssetDatabase.CreateAsset(clip, clipPath);

        var ctrl = AnimatorController.CreateAnimatorControllerAtPath(ctrlPath);
        var sm = ctrl.layers[0].stateMachine;
        var state = sm.AddState("CoinPulse");
        state.motion = clip;
        sm.defaultState = state;
        EditorUtility.SetDirty(ctrl);

        var go = new GameObject("Coin");
        go.tag = "Coin";
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = coinSprite;
        sr.color = new Color(1f, 0.84f, 0.1f);
        sr.sortingOrder = 2;
        var col = go.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.35f;
        var an = go.AddComponent<Animator>();
        an.runtimeAnimatorController = ctrl;

        coinPrefab = PrefabUtility.SaveAsPrefabAsset(go, "Assets/Prefabs/Coin.prefab");
        UnityEngine.Object.DestroyImmediate(go);
    }

    // ---------------------------------------------------------------- UI HELPERS

    static RectTransform Rt(GameObject g) { return g.GetComponent<RectTransform>(); }

    static GameObject UI(string name, Transform parent)
    {
        var g = new GameObject(name, typeof(RectTransform));
        g.transform.SetParent(parent, false);
        return g;
    }

    static void Stretch(RectTransform r)
    {
        r.anchorMin = Vector2.zero;
        r.anchorMax = Vector2.one;
        r.offsetMin = Vector2.zero;
        r.offsetMax = Vector2.zero;
    }

    static void Place(RectTransform r, Vector2 anchor, Vector2 pos, Vector2 size)
    {
        r.anchorMin = anchor;
        r.anchorMax = anchor;
        r.pivot = anchor;
        r.anchoredPosition = pos;
        r.sizeDelta = size;
    }

    static TMP_Text MakeText(string name, Transform parent, string text, int size,
        Vector2 anchor, Vector2 pos, Vector2 box, TextAlignmentOptions align)
    {
        var g = UI(name, parent);
        var t = g.AddComponent<TextMeshProUGUI>();
        if (font != null) t.font = font;
        t.text = text;
        t.fontSize = size;
        t.alignment = align;
        t.color = Color.white;
        t.raycastTarget = false;
        Place(Rt(g), anchor, pos, box);
        return t;
    }

    static Button MakeButton(string name, Transform parent, string label, Vector2 pos, Color color)
    {
        var g = UI(name, parent);
        var img = g.AddComponent<Image>();
        img.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
        img.type = Image.Type.Sliced;
        img.color = color;
        var b = g.AddComponent<Button>();
        b.targetGraphic = img;
        Place(Rt(g), new Vector2(0.5f, 0.5f), pos, new Vector2(520, 100));

        var t = MakeText("Text (TMP)", g.transform, label, 40, new Vector2(0.5f, 0.5f), Vector2.zero,
            Vector2.zero, TextAlignmentOptions.Center);
        Stretch(Rt(t.gameObject));
        return b;
    }

    static readonly Color Blue = new Color(0.20f, 0.45f, 0.90f);
    static readonly Color Green = new Color(0.15f, 0.55f, 0.28f);
    static readonly Color Red = new Color(0.75f, 0.22f, 0.22f);

    static GameObject MakePanel(string name, Transform parent)
    {
        var g = UI(name, parent);
        Stretch(Rt(g));
        g.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.80f);
        return g;
    }

    static Slider MakeSlider(Transform parent)
    {
        var root = UI("HealthSlider", parent);
        Place(Rt(root), new Vector2(1f, 1f), new Vector2(-40, -100), new Vector2(400, 30));

        var bg = UI("Background", root.transform);
        Stretch(Rt(bg));
        bg.AddComponent<Image>().color = new Color(0.12f, 0.12f, 0.12f, 0.9f);

        var area = UI("Fill Area", root.transform);
        Stretch(Rt(area));

        var fill = UI("Fill", area.transform);
        Stretch(Rt(fill));
        fill.AddComponent<Image>().color = new Color(0.20f, 0.80f, 0.30f);

        var s = root.AddComponent<Slider>();
        s.fillRect = Rt(fill);
        s.direction = Slider.Direction.LeftToRight;
        s.minValue = 0;
        s.maxValue = 100;
        s.wholeNumbers = true;
        s.value = 100;
        s.interactable = false;
        return s;
    }

    static void Click(Button b, UnityAction a)
    {
        UnityEventTools.AddPersistentListener(b.onClick, a);
    }

    static Transform MakeCanvas()
    {
        var go = new GameObject("Canvas", typeof(RectTransform));
        var cv = go.AddComponent<Canvas>();
        cv.renderMode = RenderMode.ScreenSpaceOverlay;
        var sc = go.AddComponent<CanvasScaler>();
        sc.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        sc.referenceResolution = new Vector2(1920, 1080);
        sc.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        sc.matchWidthOrHeight = 0.5f;
        go.AddComponent<GraphicRaycaster>();

        var es = new GameObject("EventSystem");
        es.AddComponent<EventSystem>();
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
        es.AddComponent<InputSystemUIInputModule>();
#else
        es.AddComponent<StandaloneInputModule>();
#endif
        return go.transform;
    }

    static void MakeCamera(Color bg, bool withMusic)
    {
        var go = new GameObject("Main Camera");
        go.tag = "MainCamera";
        go.transform.position = new Vector3(0, 0, -10);
        var cam = go.AddComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = 5.2f;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = bg;
        go.AddComponent<AudioListener>();
        go.AddComponent<CameraFit>();
        if (withMusic)
        {
            var a = go.AddComponent<AudioSource>();
            a.clip = music;
            a.loop = true;
            a.playOnAwake = true;
            a.volume = 0.3f;
        }
    }

    static Scene NewScene()
    {
        return EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
    }

    // ---------------------------------------------------------------- WORLD HELPERS

    // Collider-сіз әшекей нысан
    static GameObject Deco(string name, Sprite sprite, Vector2 pos, Vector2 scale, Color color, int order, Transform parent)
    {
        var g = new GameObject(name);
        if (parent != null) g.transform.SetParent(parent, false);
        g.transform.position = new Vector3(pos.x, pos.y, 0);
        g.transform.localScale = new Vector3(scale.x, scale.y, 1);
        var sr = g.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.color = color;
        sr.sortingOrder = order;
        return g;
    }

    static GameObject Box(string name, Vector2 pos, Vector2 scale, Color color, string tag, int order)
    {
        var g = Deco(name, sqSprite, pos, scale, color, order, null);
        var bc = g.AddComponent<BoxCollider2D>();
        bc.size = Vector2.one;
        if (tag != null) g.tag = tag;
        return g;
    }

    // Жер мен платформаның үстіне шөп жолағы
    static void TopStrip(GameObject parent, Color c)
    {
        var s = new GameObject("TopStrip");
        s.transform.SetParent(parent.transform, false);
        s.transform.localPosition = new Vector3(0f, 0.4f, 0f);
        s.transform.localScale = new Vector3(1f, 0.2f, 1f);
        var sr = s.AddComponent<SpriteRenderer>();
        sr.sprite = sqSprite;
        sr.color = c;
        sr.sortingOrder = parent.GetComponent<SpriteRenderer>().sortingOrder + 1;
    }

    // Фон: күн/ай, төбелер, жұлдыздар
    static void Backdrop(int levelNo, Color bg, Color groundColor)
    {
        var root = new GameObject("Background").transform;
        Color light = Color.Lerp(bg, Color.white, 0.85f);
        Deco("Moon", circleSprite, V(6.8f, 3.3f), V(2.0f, 2.0f), light, -20, root);

        Deco("Hill A", circleSprite, V(-5.5f, -5.3f), V(10f, 6.5f), Color.Lerp(bg, groundColor, 0.28f), -15, root);
        Deco("Hill B", circleSprite, V(3.5f, -5.6f), V(13f, 6.5f), Color.Lerp(bg, groundColor, 0.18f), -16, root);

        var rnd = new System.Random(levelNo * 31 + 7);
        for (int i = 0; i < 28; i++)
        {
            float x = (float)(rnd.NextDouble() * 17.0 - 8.5);
            float y = (float)(rnd.NextDouble() * 4.3 + 0.8);
            float s = 0.06f + (float)rnd.NextDouble() * 0.08f;
            Deco("Star", circleSprite, V(x, y), V(s, s), new Color(1f, 1f, 1f, 0.55f + (float)rnd.NextDouble() * 0.4f), -18, root);
        }
    }

    // ---------------------------------------------------------------- MAIN MENU

    static void BuildMenuScene()
    {
        var scene = NewScene();
        Color bg = new Color(0.08f, 0.12f, 0.25f);
        Color gc = new Color(0.30f, 0.62f, 0.30f);
        MakeCamera(bg, false);
        Backdrop(0, bg, gc);

        // Мәзір артындағы көрініс: жер, кейіпкер, тиындар, шип
        var ground = Deco("Ground", sqSprite, V(0, -4f), V(20, 1), new Color(0.42f, 0.30f, 0.20f), 0, null);
        TopStrip(ground, gc);
        Deco("Player", playerSprite, V(-6.5f, -3f), V(1, 1), new Color(0.20f, 0.50f, 1f), 5, null);
        Deco("Spike", spikeSprite, V(-3.5f, -3f), V(1, 1), new Color(0.9f, 0.1f, 0.1f), 3, null);
        foreach (var c in new[] { V(-6.5f, -1.6f), V(5f, -2.7f), V(6.2f, -2.7f), V(7.4f, -2.7f) })
        {
            var coin = (GameObject)PrefabUtility.InstantiatePrefab(coinPrefab);
            coin.transform.position = new Vector3(c.x, c.y, 0);
        }

        var canvas = MakeCanvas();
        var mmGo = new GameObject("MenuManager");
        var mm = mmGo.AddComponent<MenuManager>();

        var center = new Vector2(0.5f, 0.5f);
        MakeText("Title", canvas, "Тиын жинаушы", 80, center, new Vector2(0, 280),
            new Vector2(1200, 140), TextAlignmentOptions.Center).color = new Color(1f, 0.85f, 0.2f);

        Click(MakeButton("PlayButton", canvas, "Ойнау", new Vector2(0, 80), Green), mm.PlayGame);
        Click(MakeButton("QuitButton", canvas, "Шығу", new Vector2(0, -50), Red), mm.QuitGame);

        mm.recordText = MakeText("RecordText", canvas, "Рекорд жоқ", 40, center, new Vector2(0, -190),
            new Vector2(900, 70), TextAlignmentOptions.Center);

        MakeText("Info", canvas, "A / D немесе ← → : жүру    Пробел : секіру    Esc : пауза", 28,
            new Vector2(0.5f, 0f), new Vector2(0, 30), new Vector2(1500, 50),
            TextAlignmentOptions.Center).color = new Color(1f, 1f, 1f, 0.6f);

        EditorSceneManager.SaveScene(scene, ScenesDir + "/MainMenu.unity");
    }

    // ---------------------------------------------------------------- LEVELS

    static void BuildLevel(string sceneName, int levelNo, Color bg, Color groundColor,
        float[][] platforms, Vector2[] spikes, Vector2[] coins, bool hasNext, bool showHint)
    {
        var scene = NewScene();
        MakeCamera(bg, true);
        Backdrop(levelNo, bg, groundColor);

        // --- дүние
        var ground = Box("Ground", new Vector2(0, -4f), new Vector2(20, 1), Color.Lerp(groundColor, Color.black, 0.45f), "Ground", 0);
        TopStrip(ground, groundColor);
        foreach (var p in platforms)
        {
            var pl = Box("Platform", new Vector2(p[0], p[1]), new Vector2(p[2], 0.5f),
                Color.Lerp(groundColor, Color.black, 0.35f), "Ground", 1);
            TopStrip(pl, Color.Lerp(groundColor, Color.white, 0.2f));
        }

        // көрінбейтін қабырғалар: Player экраннан шығып кетпейді
        var wl = new GameObject("WallLeft");
        wl.transform.position = new Vector3(-9.4f, 0, 0);
        wl.AddComponent<BoxCollider2D>().size = new Vector2(1, 30);
        var wr = new GameObject("WallRight");
        wr.transform.position = new Vector3(9.4f, 0, 0);
        wr.AddComponent<BoxCollider2D>().size = new Vector2(1, 30);

        foreach (var s in spikes)
        {
            var sp = Deco("Spike", spikeSprite, s, Vector2.one, new Color(0.9f, 0.1f, 0.1f), 3, null);
            sp.AddComponent<BoxCollider2D>().size = Vector2.one;
            sp.tag = "Enemy";
        }

        foreach (var c in coins)
        {
            var coin = (GameObject)PrefabUtility.InstantiatePrefab(coinPrefab);
            coin.transform.position = new Vector3(c.x, c.y, 0);
        }

        // --- Player
        var pg = new GameObject("Player");
        pg.transform.position = new Vector3(-8.2f, -2.5f, 0);
        var psr = pg.AddComponent<SpriteRenderer>();
        psr.sprite = playerSprite;
        psr.color = new Color(0.20f, 0.50f, 1f);
        psr.sortingOrder = 5;
        var pcol = pg.AddComponent<BoxCollider2D>();
        pcol.size = Vector2.one;
        pcol.sharedMaterial = noFriction;
        var prb = pg.AddComponent<Rigidbody2D>();
        prb.constraints = RigidbodyConstraints2D.FreezeRotation;
        prb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        var player = pg.AddComponent<Player>();
        player.totalCoins = coins.Length;
        player.coinSound = coinClip;

        // --- менеджерлер
        var gm = new GameObject("GameManager");
        var mm = gm.AddComponent<MenuManager>();
        var pm = new GameObject("PauseManager");
        var pause = pm.AddComponent<PauseManager>();

        // Қосымша тапсырма (3-модуль): Spawner. Өшірулі тұр; көру үшін сахнадағы тиындарды өшіріп, қосыңыз.
        if (levelNo == 1)
        {
            var spw = new GameObject("Spawner");
            spw.AddComponent<Spawner>().coinPrefab = coinPrefab;
            spw.SetActive(false);
        }

        // --- интерфейс
        var canvas = MakeCanvas();
        var tl = new Vector2(0f, 1f);
        var tr = new Vector2(1f, 1f);
        var center = new Vector2(0.5f, 0.5f);

        player.scoreText = MakeText("ScoreText", canvas, "Ұпай: 0", 40, tl, new Vector2(40, -30),
            new Vector2(600, 60), TextAlignmentOptions.Left);
        player.healthText = MakeText("HealthText", canvas, "Өмір: 100", 40, tr, new Vector2(-40, -30),
            new Vector2(600, 60), TextAlignmentOptions.Right);
        player.healthSlider = MakeSlider(canvas);

        MakeText("LevelText", canvas, levelNo + "-деңгей", 40, new Vector2(0.5f, 1f), new Vector2(0, -30),
            new Vector2(500, 60), TextAlignmentOptions.Center).color = new Color(1f, 1f, 1f, 0.8f);
        if (showHint)
            MakeText("HintText", canvas, "A / D немесе ← → : жүру    Пробел : секіру    Esc : пауза", 28,
                new Vector2(0.5f, 0f), new Vector2(0, 20), new Vector2(1500, 50),
                TextAlignmentOptions.Center).color = new Color(1f, 1f, 1f, 0.7f);

        // Game Over
        var over = MakePanel("GameOverPanel", canvas);
        MakeText("Title", over.transform, "Ойын аяқталды!", 70, center, new Vector2(0, 220),
            new Vector2(1200, 120), TextAlignmentOptions.Center);
        Click(MakeButton("RestartButton", over.transform, "Қайта бастау", new Vector2(0, 40), Green), mm.RestartGame);
        Click(MakeButton("MenuButton", over.transform, "Мәзір", new Vector2(0, -90), Blue), mm.ToMenu);
        over.SetActive(false);
        player.gameOverPanel = over;

        // Жеңіс
        var win = MakePanel("WinPanel", canvas);
        MakeText("Title", win.transform, "Жеңіс!", 70, center, new Vector2(0, 270),
            new Vector2(1200, 120), TextAlignmentOptions.Center).color = new Color(1f, 0.85f, 0.2f);
        player.resultText = MakeText("ResultText", win.transform, "", 36, center, new Vector2(0, 150),
            new Vector2(900, 120), TextAlignmentOptions.Center);
        float y = 20;
        if (hasNext)
        {
            Click(MakeButton("NextButton", win.transform, "Келесі деңгей", new Vector2(0, y), Green), mm.NextLevel);
            y -= 130;
        }
        Click(MakeButton("RestartButton", win.transform, "Қайта бастау", new Vector2(0, y), Blue), mm.RestartGame);
        y -= 130;
        Click(MakeButton("MenuButton", win.transform, "Мәзір", new Vector2(0, y), Red), mm.ToMenu);
        win.SetActive(false);
        player.winPanel = win;

        // Пауза (Esc)
        var pp = MakePanel("PausePanel", canvas);
        MakeText("Title", pp.transform, "Пауза", 70, center, new Vector2(0, 180),
            new Vector2(1200, 120), TextAlignmentOptions.Center);
        Click(MakeButton("ResumeButton", pp.transform, "Жалғастыру", new Vector2(0, 20), Green), pause.Resume);
        Click(MakeButton("MenuButton", pp.transform, "Мәзір", new Vector2(0, -110), Red), mm.ToMenu);
        pp.SetActive(false);
        pause.pausePanel = pp;

        EditorSceneManager.SaveScene(scene, ScenesDir + "/" + sceneName + ".unity");
    }
}
