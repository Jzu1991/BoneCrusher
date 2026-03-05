// ============================================================
//  BoneCrusherSetup.cs  v7
//  Bone Crusher: Made of Bones | Unicorn Games
//  MENU: BoneCrusher -> Setup Completo
// ============================================================
using UnityEngine;
using UnityEngine.Events;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Animations;
using UnityEditor.Events;
using System.IO;
using System.Linq;
using System.Collections.Generic;

public class BoneCrusherSetup : EditorWindow
{
    // ── Paleta de nivel ──────────────────────────────────────
    static readonly Color COL_GROUND   = new Color(0.20f, 0.15f, 0.25f);
    static readonly Color COL_PLATFORM = new Color(0.30f, 0.20f, 0.40f);
    static readonly Color COL_WALL     = new Color(0.12f, 0.10f, 0.18f);
    static readonly Color COL_DOOR     = new Color(0.60f, 0.10f, 0.55f);

    [MenuItem("BoneCrusher/Setup Completo")]
    public static void RunFullSetup()
    {
        Debug.Log("=== BONE CRUSHER SETUP v7 ===");
        SetupLayersAndTags();
        AssetDatabase.Refresh();
        CreateAllAnimClips();
        CreateAllControllers();
        CreateAllPrefabs();
        CreateMainMenuScene();
        CreateScene();
        RegisterScenes();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("=== SETUP COMPLETADO ===");
        EditorUtility.DisplayDialog("Bone Crusher listo!",
            "Setup v7 completado.\n\n" +
            "Sprites  -> Assets/_Art/\n" +
            "Clips    -> Assets/_Animations/Clips/\n" +
            "Controllers -> Assets/_Animations/\n" +
            "Prefabs  -> Assets/_Prefabs/\n" +
            "Escena   -> Assets/_Scenes/DemoFloors30to25",
            "OK");
    }

    [MenuItem("BoneCrusher/Solo: Crear Menu Principal")]
    public static void RunMenuScene()
    {
        CreateMainMenuScene();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    [MenuItem("BoneCrusher/Solo: Crear Escena de Juego")]
    public static void RunGameScene()
    {
        SetupLayersAndTags();
        AssetDatabase.Refresh();
        CreateAllAnimClips();
        CreateAllControllers();
        CreateAllPrefabs();
        CreateScene();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    // ══════════════════════════════════════════════════════════
    //  0. BUILD SETTINGS
    // ══════════════════════════════════════════════════════════
    static void RegisterScenes()
    {
        var scenes = new[]
        {
            "Assets/_Scenes/MainMenu.unity",
            "Assets/_Scenes/DemoFloors30to25.unity",
        };

        var list = new System.Collections.Generic.List<EditorBuildSettingsScene>();
        foreach (var path in scenes)
        {
            if (System.IO.File.Exists(path))
                list.Add(new EditorBuildSettingsScene(path, true));
            else
                Debug.LogWarning($"Scene not found for Build Settings: {path}");
        }
        EditorBuildSettings.scenes = list.ToArray();
        Debug.Log($"Build Settings: {list.Count} escenas registradas.");
    }

    // ══════════════════════════════════════════════════════════
    //  1. LAYERS & TAGS
    // ══════════════════════════════════════════════════════════
    static void SetupLayersAndTags()
    {
        var so = new SerializedObject(
            AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);

        var layers = so.FindProperty("layers");
        string[] lnames = { "Ground", "Player", "Enemy", "Projectile" };
        for (int i = 0; i < lnames.Length; i++)
        {
            int idx = 8 + i;
            if (idx < layers.arraySize)
            {
                var s = layers.GetArrayElementAtIndex(idx);
                if (string.IsNullOrEmpty(s.stringValue)) s.stringValue = lnames[i];
            }
        }
        var tags = so.FindProperty("tags");
        foreach (string tag in new[] { "Enemy", "Projectile" })
        {
            bool found = false;
            for (int i = 0; i < tags.arraySize; i++)
                if (tags.GetArrayElementAtIndex(i).stringValue == tag) { found = true; break; }
            if (!found) { tags.arraySize++; tags.GetArrayElementAtIndex(tags.arraySize - 1).stringValue = tag; }
        }
        so.ApplyModifiedProperties();
    }

    // ══════════════════════════════════════════════════════════
    //  2. ANIMATION CLIPS
    //     Sprite folder -> AnimationClip asset
    // ══════════════════════════════════════════════════════════

    // (spriteFolder, clipName, fps, loop)
    static readonly (string folder, string clip, float fps, bool loop)[] ALL_ANIMS =
    {
        // Skarn
        ("Assets/_Art/Skarn/Skarn_Idle",   "Skarn_Idle",   8f,  true),
        ("Assets/_Art/Skarn/Skarn_Walk",   "Skarn_Walk",   10f, true),
        ("Assets/_Art/Skarn/Skarn_Jump",   "Skarn_Jump",   8f,  false),
        ("Assets/_Art/Skarn/Skarn_Attack", "Skarn_Attack", 16f, false),
        ("Assets/_Art/Skarn/Skarn_Damage", "Skarn_Damage", 12f, false),
        ("Assets/_Art/Skarn/Skarn_Die",    "Skarn_Die",    8f,  false),
        // Goblin
        ("Assets/_Art/Goblin/Idle",   "Goblin_Idle",   8f,  true),
        ("Assets/_Art/Goblin/Walk",   "Goblin_Walk",   10f, true),
        ("Assets/_Art/Goblin/Attack", "Goblin_Attack", 14f, false),
        ("Assets/_Art/Goblin/Hurt",   "Goblin_Hurt",   12f, false),
        ("Assets/_Art/Goblin/Die",    "Goblin_Die",    8f,  false),
        // SkeletonArcher
        ("Assets/_Art/SkeletonArcher/Idle",   "Skeleton_Idle",   10f, true),
        ("Assets/_Art/SkeletonArcher/Walk",   "Skeleton_Walk",   10f, true),
        ("Assets/_Art/SkeletonArcher/Attack", "Skeleton_Attack", 18f, false),
        ("Assets/_Art/SkeletonArcher/Hurt",   "Skeleton_Hurt",   12f, false),
        ("Assets/_Art/SkeletonArcher/Die",    "Skeleton_Die",    10f, false),
        // Zombie
        ("Assets/_Art/Zombie/Idle",   "Zombie_Idle",   6f,  true),
        ("Assets/_Art/Zombie/Walk",   "Zombie_Walk",   8f,  true),
        ("Assets/_Art/Zombie/Attack", "Zombie_Attack", 10f, false),
        ("Assets/_Art/Zombie/Hurt",   "Zombie_Hurt",   10f, false),
        ("Assets/_Art/Zombie/Die",    "Zombie_Die",    6f,  false),
        // WarriorAdventurer
        ("Assets/_Art/Warrior/Idle",   "Warrior_Idle",   8f,  true),
        ("Assets/_Art/Warrior/Walk",   "Warrior_Walk",   10f, true),
        ("Assets/_Art/Warrior/Attack", "Warrior_Attack", 14f, false),
        ("Assets/_Art/Warrior/Hurt",   "Warrior_Hurt",   12f, false),
        ("Assets/_Art/Warrior/Die",    "Warrior_Die",    8f,  false),
        // RogueAdventurer
        ("Assets/_Art/RogueAdventurer/Idle",   "Rogue_Idle",   8f,  true),
        ("Assets/_Art/RogueAdventurer/Walk",   "Rogue_Walk",   12f, true),
        ("Assets/_Art/RogueAdventurer/Jump",   "Rogue_Jump",   10f, false),
        ("Assets/_Art/RogueAdventurer/Attack", "Rogue_Attack", 14f, false),
        // Portal (checkpoint)
        ("Assets/_Art/Portal", "Portal_Idle", 10f, true),
        // Projectiles
        ("Assets/_Art/MagicProjectile", "Magic_Fly", 12f, true),
    };

    static void CreateAllAnimClips()
    {
        EnsureFolder("Assets/_Animations");
        EnsureFolder("Assets/_Animations/Clips");

        foreach (var (folder, clipName, fps, loop) in ALL_ANIMS)
            BuildClip(folder, clipName, fps, loop);

        AssetDatabase.SaveAssets();
        Debug.Log($"AnimClips: {ALL_ANIMS.Length} clips creados.");
    }

    static AnimationClip BuildClip(string spriteFolder, string clipName, float fps, bool loop)
    {
        if (!AssetDatabase.IsValidFolder(spriteFolder))
        {
            Debug.LogWarning($"Carpeta no existe: {spriteFolder}");
            return null;
        }

        var sprites = AssetDatabase.FindAssets("t:Texture2D", new[] { spriteFolder })
            .Select(g => AssetDatabase.GUIDToAssetPath(g))
            .OrderBy(p => p)
            .Select(p => AssetDatabase.LoadAssetAtPath<Sprite>(p))
            .Where(s => s != null)
            .ToArray();

        if (sprites.Length == 0) { Debug.LogWarning($"Sin sprites: {spriteFolder}"); return null; }

        string clipPath = $"Assets/_Animations/Clips/{clipName}.anim";
        if (AssetDatabase.LoadAssetAtPath<AnimationClip>(clipPath) != null)
            AssetDatabase.DeleteAsset(clipPath);

        var clip = new AnimationClip { frameRate = fps };

        var keyframes = new ObjectReferenceKeyframe[sprites.Length + 1];
        for (int i = 0; i < sprites.Length; i++)
            keyframes[i] = new ObjectReferenceKeyframe { time = i / fps, value = sprites[i] };
        keyframes[sprites.Length] = new ObjectReferenceKeyframe
            { time = sprites.Length / fps, value = loop ? sprites[0] : sprites[sprites.Length - 1] };

        AnimationUtility.SetObjectReferenceCurve(
            clip,
            EditorCurveBinding.PPtrCurve("", typeof(SpriteRenderer), "m_Sprite"),
            keyframes);

        var settings = AnimationUtility.GetAnimationClipSettings(clip);
        settings.loopTime = loop;
        AnimationUtility.SetAnimationClipSettings(clip, settings);

        AssetDatabase.CreateAsset(clip, clipPath);
        return clip;
    }

    // Helper: load a clip by name
    static AnimationClip C(string name) =>
        AssetDatabase.LoadAssetAtPath<AnimationClip>($"Assets/_Animations/Clips/{name}.anim");

    // ══════════════════════════════════════════════════════════
    //  3. ANIMATOR CONTROLLERS
    //     Each character gets its own controller with correct clips
    // ══════════════════════════════════════════════════════════
    static void CreateAllControllers()
    {
        EnsureFolder("Assets/_Animations");

        // Skarn - full set
        BuildSkarnController();

        // Enemies - generic builder
        BuildEnemyController("Goblin",
            idle:   C("Goblin_Idle"),
            walk:   C("Goblin_Walk"),
            attack: C("Goblin_Attack"),
            hurt:   C("Goblin_Hurt"),
            die:    C("Goblin_Die"),
            jump:   null);

        BuildEnemyController("SkeletonArcher",
            idle:   C("Skeleton_Idle"),
            walk:   C("Skeleton_Walk"),
            attack: C("Skeleton_Attack"),
            hurt:   C("Skeleton_Hurt"),
            die:    C("Skeleton_Die"),
            jump:   null);

        BuildEnemyController("Zombie",
            idle:   C("Zombie_Idle"),
            walk:   C("Zombie_Walk"),
            attack: C("Zombie_Attack"),
            hurt:   C("Zombie_Hurt"),
            die:    C("Zombie_Die"),
            jump:   null);

        BuildEnemyController("WarriorAdventurer",
            idle:   C("Warrior_Idle"),
            walk:   C("Warrior_Walk"),
            attack: C("Warrior_Attack"),
            hurt:   C("Warrior_Hurt"),
            die:    C("Warrior_Die"),
            jump:   null);

        BuildEnemyController("RogueAdventurer",
            idle:   C("Rogue_Idle"),
            walk:   C("Rogue_Walk"),
            attack: C("Rogue_Attack"),
            hurt:   null,
            die:    null,
            jump:   C("Rogue_Jump"));

        // Magic projectile
        BuildPortalController();
        BuildProjectileController();

        AssetDatabase.SaveAssets();
        Debug.Log("AnimatorControllers creados.");
    }

    static void BuildSkarnController()
    {
        string path = "Assets/_Animations/SkarnAnimator.controller";
        if (AssetDatabase.LoadAssetAtPath<AnimatorController>(path) != null)
            AssetDatabase.DeleteAsset(path);

        var ctrl = AnimatorController.CreateAnimatorControllerAtPath(path);
        ctrl.AddParameter("Speed",      AnimatorControllerParameterType.Float);
        ctrl.AddParameter("IsGrounded", AnimatorControllerParameterType.Bool);
        ctrl.AddParameter("Attack",     AnimatorControllerParameterType.Trigger);
        ctrl.AddParameter("Magic",      AnimatorControllerParameterType.Trigger);
        ctrl.AddParameter("Hurt",       AnimatorControllerParameterType.Trigger);
        ctrl.AddParameter("Die",        AnimatorControllerParameterType.Trigger);

        var sm    = ctrl.layers[0].stateMachine;
        var sIdle = St(sm, "Idle",   C("Skarn_Idle"));
        var sWalk = St(sm, "Walk",   C("Skarn_Walk"));
        var sJump = St(sm, "Jump",   C("Skarn_Jump"));
        var sAtk  = St(sm, "Attack", C("Skarn_Attack"));
        var sMag  = St(sm, "Magic",  C("Skarn_Attack"));
        var sHurt = St(sm, "Hurt",   C("Skarn_Damage"));
        var sDie  = St(sm, "Die",    C("Skarn_Die"));
        sm.defaultState = sIdle;

        Tr(sIdle, sWalk).AddCondition(AnimatorConditionMode.Greater, 0.1f, "Speed");
        Tr(sWalk, sIdle).AddCondition(AnimatorConditionMode.Less,    0.1f, "Speed");
        Tr(sIdle, sJump).AddCondition(AnimatorConditionMode.IfNot,   0f,   "IsGrounded");
        Tr(sWalk, sJump).AddCondition(AnimatorConditionMode.IfNot,   0f,   "IsGrounded");
        Tr(sJump, sIdle).AddCondition(AnimatorConditionMode.If,      0f,   "IsGrounded");

        AnyTr(sm, sAtk,  "Attack"); ExitTr(sAtk,  0.9f);
        AnyTr(sm, sMag,  "Magic");  ExitTr(sMag,  0.9f);
        AnyTr(sm, sHurt, "Hurt");   ExitTr(sHurt, 0.8f);
        AnyTr(sm, sDie,  "Die");
    }

    static void BuildEnemyController(string name,
        AnimationClip idle, AnimationClip walk, AnimationClip attack,
        AnimationClip hurt, AnimationClip die, AnimationClip jump)
    {
        string path = $"Assets/_Animations/{name}Animator.controller";
        if (AssetDatabase.LoadAssetAtPath<AnimatorController>(path) != null)
            AssetDatabase.DeleteAsset(path);

        var ctrl = AnimatorController.CreateAnimatorControllerAtPath(path);
        ctrl.AddParameter("Speed",  AnimatorControllerParameterType.Float);
        ctrl.AddParameter("Attack", AnimatorControllerParameterType.Trigger);
        ctrl.AddParameter("Hurt",   AnimatorControllerParameterType.Trigger);
        ctrl.AddParameter("Die",    AnimatorControllerParameterType.Trigger);

        var sm    = ctrl.layers[0].stateMachine;
        var sIdle = St(sm, "Idle",   idle);
        var sWalk = St(sm, "Walk",   walk);
        var sAtk  = St(sm, "Attack", attack);
        var sHurt = St(sm, "Hurt",   hurt);
        var sDie  = St(sm, "Die",    die);
        sm.defaultState = sIdle;

        Tr(sIdle, sWalk).AddCondition(AnimatorConditionMode.Greater, 0.1f, "Speed");
        Tr(sWalk, sIdle).AddCondition(AnimatorConditionMode.Less,    0.1f, "Speed");

        if (jump != null)
        {
            ctrl.AddParameter("IsGrounded", AnimatorControllerParameterType.Bool);
            var sJump = St(sm, "Jump", jump);
            Tr(sIdle, sJump).AddCondition(AnimatorConditionMode.IfNot, 0f, "IsGrounded");
            Tr(sWalk, sJump).AddCondition(AnimatorConditionMode.IfNot, 0f, "IsGrounded");
            Tr(sJump, sIdle).AddCondition(AnimatorConditionMode.If,    0f, "IsGrounded");
        }

        AnyTr(sm, sAtk,  "Attack"); ExitTr(sAtk,  0.9f);
        if (hurt != null) { AnyTr(sm, sHurt, "Hurt"); ExitTr(sHurt, 0.8f); }
        if (die  != null)   AnyTr(sm, sDie,  "Die");
    }

    static void BuildProjectileController()
    {
        EnsureFolder("Assets/_Animations/Projectiles");
        string path = "Assets/_Animations/Projectiles/MagicAnimator.controller";
        if (AssetDatabase.LoadAssetAtPath<AnimatorController>(path) != null)
            AssetDatabase.DeleteAsset(path);

        var ctrl = AnimatorController.CreateAnimatorControllerAtPath(path);
        var sm   = ctrl.layers[0].stateMachine;
        var fly  = St(sm, "Fly", C("Magic_Fly"));
        sm.defaultState = fly;
    }

    static void BuildPortalController()
    {
        EnsureFolder("Assets/_Animations/Misc");
        string path = "Assets/_Animations/Misc/PortalAnimator.controller";
        if (AssetDatabase.LoadAssetAtPath<AnimatorController>(path) != null)
            AssetDatabase.DeleteAsset(path);
        var ctrl = AnimatorController.CreateAnimatorControllerAtPath(path);
        var sm   = ctrl.layers[0].stateMachine;
        var idle = St(sm, "Idle", C("Portal_Idle"));
        sm.defaultState = idle;
    }

    // Animator helpers
    static AnimatorState St(AnimatorStateMachine sm, string name, AnimationClip clip)
    { var s = sm.AddState(name); if (clip != null) s.motion = clip; return s; }

    static AnimatorStateTransition Tr(AnimatorState from, AnimatorState to)
    { var t = from.AddTransition(to); t.hasExitTime = false; t.duration = 0.05f; t.canTransitionToSelf = false; return t; }

    static void AnyTr(AnimatorStateMachine sm, AnimatorState to, string param)
    { var t = sm.AddAnyStateTransition(to); t.hasExitTime = false; t.duration = 0.05f; t.canTransitionToSelf = false; t.AddCondition(AnimatorConditionMode.If, 0f, param); }

    static void ExitTr(AnimatorState s, float exitTime)
    { var t = s.AddExitTransition(); t.hasExitTime = true; t.exitTime = exitTime; t.duration = 0.05f; }

    static RuntimeAnimatorController Ctrl(string name) =>
        AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>($"Assets/_Animations/{name}Animator.controller");

    // ══════════════════════════════════════════════════════════
    //  4. PREFABS
    //     Sprite + Controller + Script fully wired
    // ══════════════════════════════════════════════════════════
    static void CreateAllPrefabs()
    {
        EnsureFolder("Assets/_Prefabs");
        EnsureFolder("Assets/_Prefabs/Player");
        EnsureFolder("Assets/_Prefabs/Enemies");

        MakeSkarn();
        MakeMagicProjectile();
        MakeArrow();
        MakeGoblin();
        MakeSkeletonArcher();
        MakeZombie();
        MakeWarrior();
        MakeRogue();

        Debug.Log("Prefabs OK");
    }

    // ── Skarn ─────────────────────────────────────────────────
    static void MakeSkarn()
    {
        Del("Assets/_Prefabs/Player/Skarn.prefab");
        var go = new GameObject("Skarn"); go.layer = 9; go.tag = "Player";

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sortingOrder = 2;
        sr.sprite = Spr("Assets/_Art/Skarn/Skarn_Idle/000.png");

        go.AddComponent<BoxCollider2D>().size = new Vector2(0.8f, 1.6f);

        var rb = go.AddComponent<Rigidbody2D>();
        rb.gravityScale = 3f; rb.freezeRotation = true;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        var anim = go.AddComponent<Animator>();
        anim.runtimeAnimatorController = Ctrl("Skarn");

        var atk = Sub(go, "AttackPoint",    new Vector3(0.9f,  0f,    0f));
        var mgc = Sub(go, "MagicSpawnPoint",new Vector3(1.2f,  0.3f,  0f));
        var gnd = Sub(go, "GroundCheck",    new Vector3(0f,   -0.85f, 0f));

        SO(go.AddComponent<SkarnController>(), so => {
            F(so,"moveSpeed",5f); F(so,"jumpForce",10f); F(so,"groundCheckRadius",0.2f);
            F(so,"attackRange",0.8f); I(so,"swordDamage",1); F(so,"attackCooldown",0.4f);
            F(so,"magicCooldown",1f); I(so,"magicDamage",2);
            M(so,"groundLayer",1<<8); M(so,"enemyLayer",1<<10);
            R(so,"attackPoint",    atk.transform);
            R(so,"magicSpawnPoint",mgc.transform);
            R(so,"groundCheck",    gnd.transform);
        });
        SO(go.AddComponent<SkarnHealth>(), so => F(so,"invincibilityDuration",1f));

        Save(go, "Assets/_Prefabs/Player/Skarn.prefab");
    }

    // ── MagicProjectile ───────────────────────────────────────
    static void MakeMagicProjectile()
    {
        Del("Assets/_Prefabs/Player/MagicProjectile.prefab");
        var go = new GameObject("MagicProjectile"); go.layer = 11; go.tag = "Projectile";

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sortingOrder = 3;
        sr.sprite = Spr("Assets/_Art/MagicProjectile/000.png");

        var anim = go.AddComponent<Animator>();
        anim.runtimeAnimatorController =
            AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(
                "Assets/_Animations/Projectiles/MagicAnimator.controller");

        var cc = go.AddComponent<CircleCollider2D>(); cc.radius = 0.2f; cc.isTrigger = true;
        var rb = go.AddComponent<Rigidbody2D>(); rb.gravityScale = 0f; rb.freezeRotation = true;

        SO(go.AddComponent<MagicProjectile>(), so => {
            F(so,"speed",8f); F(so,"lifetime",3f); M(so,"enemyLayer",1<<10);
        });
        Save(go, "Assets/_Prefabs/Player/MagicProjectile.prefab");
    }

    // ── Arrow ─────────────────────────────────────────────────
    static void MakeArrow()
    {
        Del("Assets/_Prefabs/Enemies/Arrow.prefab");
        var go = new GameObject("Arrow"); go.layer = 11;

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sortingOrder = 3;
        sr.sprite = Spr("Assets/_Art/Arrow/000.png");

        go.AddComponent<BoxCollider2D>().size = new Vector2(1f, 0.3f);
        var bc = go.GetComponent<BoxCollider2D>(); bc.isTrigger = true;
        var rb = go.AddComponent<Rigidbody2D>(); rb.gravityScale = 0.3f;

        go.AddComponent<ArrowProjectile>();
        Save(go, "Assets/_Prefabs/Enemies/Arrow.prefab");
    }

    // ── Enemy base helper ─────────────────────────────────────
    static GameObject EnemyGO(string name, Vector2 colSize, int hp, int type,
        string spritePath, string ctrlName)
    {
        var go = new GameObject(name); go.layer = 10; go.tag = "Enemy";

        var sr = go.AddComponent<SpriteRenderer>(); sr.sortingOrder = 2;
        var sp = Spr(spritePath);
        if (sp != null) sr.sprite = sp;

        go.AddComponent<BoxCollider2D>().size = colSize;

        var rb = go.AddComponent<Rigidbody2D>();
        rb.gravityScale = 3f; rb.freezeRotation = true;

        var anim = go.AddComponent<Animator>();
        var ctrl = Ctrl(ctrlName);
        if (ctrl != null) anim.runtimeAnimatorController = ctrl;
        else Debug.LogWarning($"Controller not found: {ctrlName}Animator.controller");

        Sub(go, "AttackPoint", new Vector3(0.9f, 0f, 0f));

        var eh = go.AddComponent<EnemyHealth>();
        SO(eh, so => { I(so,"maxHealth",hp); I(so,"enemyType",type); B(so,"isAlly",false); });
        return go;
    }

    // ── Goblin ────────────────────────────────────────────────
    static void MakeGoblin()
    {
        Del("Assets/_Prefabs/Enemies/Goblin.prefab");
        var go = EnemyGO("Goblin", new Vector2(0.7f,1.2f), 5, 0,
            "Assets/_Art/Goblin/Idle/000.png", "Goblin");
        var atk = go.transform.Find("AttackPoint");
        SO(go.AddComponent<GoblinAI>(), so => {
            F(so,"detectionRange",6f); F(so,"attackRange",1f); F(so,"moveSpeed",3.5f);
            F(so,"patrolDistance",3f); I(so,"attackDamage",1); F(so,"attackCooldown",1.2f);
            M(so,"playerLayer",1<<9); M(so,"enemyLayer",1<<10); R(so,"attackPoint",atk);
            F(so,"dashSpeed",6f); F(so,"dashChance",0.3f);
        });
        Save(go, "Assets/_Prefabs/Enemies/Goblin.prefab");
    }

    // ── Skeleton Archer ───────────────────────────────────────
    static void MakeSkeletonArcher()
    {
        Del("Assets/_Prefabs/Enemies/SkeletonArcher.prefab");
        var go = EnemyGO("SkeletonArcher", new Vector2(0.6f,1.4f), 5, 0,
            "Assets/_Art/SkeletonArcher/Idle/000.png", "SkeletonArcher");
        var atk = go.transform.Find("AttackPoint");
        var bow = Sub(go, "BowPoint", new Vector3(0.8f,0.3f,0f));
        var arrowPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
            "Assets/_Prefabs/Enemies/Arrow.prefab");
        SO(go.AddComponent<SkeletonArcherAI>(), so => {
            F(so,"detectionRange",7f); F(so,"attackRange",5f); F(so,"moveSpeed",2f);
            F(so,"patrolDistance",3f); I(so,"attackDamage",1); F(so,"attackCooldown",2f);
            M(so,"playerLayer",1<<9); M(so,"enemyLayer",1<<10); R(so,"attackPoint",atk);
            F(so,"preferredRange",4f); F(so,"fleeRange",2f);
            R(so,"arrowPrefab",arrowPrefab); R(so,"bowPoint",bow.transform);
        });
        Save(go, "Assets/_Prefabs/Enemies/SkeletonArcher.prefab");
    }

    // ── Zombie ────────────────────────────────────────────────
    static void MakeZombie()
    {
        Del("Assets/_Prefabs/Enemies/Zombie.prefab");
        var go = EnemyGO("Zombie", new Vector2(0.8f,1.6f), 7, 0,
            "Assets/_Art/Zombie/Idle/000.png", "Zombie");
        var atk = go.transform.Find("AttackPoint");
        SO(go.AddComponent<ZombieAI>(), so => {
            F(so,"detectionRange",5f); F(so,"attackRange",1f); F(so,"moveSpeed",1.2f);
            F(so,"patrolDistance",2f); I(so,"attackDamage",1); F(so,"attackCooldown",2f);
            M(so,"playerLayer",1<<9); M(so,"enemyLayer",1<<10); R(so,"attackPoint",atk);
        });
        Save(go, "Assets/_Prefabs/Enemies/Zombie.prefab");
    }

    // ── Warrior Adventurer ────────────────────────────────────
    static void MakeWarrior()
    {
        Del("Assets/_Prefabs/Enemies/WarriorAdventurer.prefab");
        var go = EnemyGO("WarriorAdventurer", new Vector2(0.9f,1.8f), 10, 1,
            "Assets/_Art/Warrior/Idle/000.png", "WarriorAdventurer");
        var atk = go.transform.Find("AttackPoint");
        SO(go.AddComponent<WarriorAdventurerAI>(), so => {
            F(so,"detectionRange",8f); F(so,"attackRange",1.2f); F(so,"moveSpeed",2.5f);
            F(so,"patrolDistance",3f); I(so,"attackDamage",1); F(so,"attackCooldown",1.8f);
            M(so,"playerLayer",1<<9); M(so,"enemyLayer",1<<10); R(so,"attackPoint",atk);
            F(so,"chargeSpeed",7f); F(so,"chargeThreshold",0.4f);
        });
        Save(go, "Assets/_Prefabs/Enemies/WarriorAdventurer.prefab");
    }

    // ── Rogue Adventurer ──────────────────────────────────────
    static void MakeRogue()
    {
        Del("Assets/_Prefabs/Enemies/RogueAdventurer.prefab");
        var go = EnemyGO("RogueAdventurer", new Vector2(0.7f,1.5f), 15, 1,
            "Assets/_Art/RogueAdventurer/Idle/000.png", "RogueAdventurer");
        var atk = go.transform.Find("AttackPoint");
        SO(go.AddComponent<RogueAdventurerAI>(), so => {
            F(so,"detectionRange",8f); F(so,"attackRange",1f); F(so,"moveSpeed",4f);
            F(so,"patrolDistance",3f); I(so,"attackDamage",1); F(so,"attackCooldown",1f);
            M(so,"playerLayer",1<<9); M(so,"enemyLayer",1<<10); R(so,"attackPoint",atk);
            F(so,"backstepDistance",2f); F(so,"backstepSpeed",5f);
        });
        Save(go, "Assets/_Prefabs/Enemies/RogueAdventurer.prefab");
    }

    // ══════════════════════════════════════════════════════════
    //  5. SCENE
    // ══════════════════════════════════════════════════════════
    // ══════════════════════════════════════════════════════════
    //  MAIN MENU SCENE
    // ══════════════════════════════════════════════════════════
    static void CreateMainMenuScene()
    {
        EnsureFolder("Assets/_Scenes");
        const string path = "Assets/_Scenes/MainMenu.unity";
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // Fondo negro
        var camGO = new GameObject("Main Camera"); camGO.tag = "MainCamera";
        camGO.transform.position = new Vector3(0,0,-10);
        var cam = camGO.AddComponent<Camera>();
        cam.orthographic = true; cam.orthographicSize = 5f;
        cam.backgroundColor = new Color(0.04f,0.03f,0.08f);
        cam.clearFlags = CameraClearFlags.SolidColor;
        camGO.AddComponent<AudioListener>();

        // Canvas
        var canvasGO = new GameObject("MenuCanvas");
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10;
        canvasGO.AddComponent<UnityEngine.UI.CanvasScaler>().uiScaleMode =
            UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasGO.AddComponent<UnityEngine.UI.GraphicRaycaster>();
        var mmm = canvasGO.AddComponent<MainMenuManager>();

        // Fondo panel oscuro
        var bg = UIPanel(canvasGO, "Background", new Color(0.04f,0.03f,0.10f,0.95f),
            Vector2.zero, new Vector2(1920,1080));

        // Título
        var titleGO = UIText(canvasGO, "Title", "BONE CRUSHER\nMade of Bones",
            new Vector2(0, 140), new Vector2(700, 200), 72, new Color(0.85f,0.15f,0.15f));
        var titleTMP = titleGO.GetComponent<TMPro.TextMeshProUGUI>();
        titleTMP.fontStyle = TMPro.FontStyles.Bold;
        titleTMP.alignment = TMPro.TextAlignmentOptions.Center;

        // Subtítulo
        var subGO = UIText(canvasGO, "Subtitle", "Demo: Pisos 30 - 25",
            new Vector2(0, 60), new Vector2(500, 60), 28, new Color(0.6f,0.5f,0.7f));
        subGO.GetComponent<TMPro.TextMeshProUGUI>().alignment = TMPro.TextAlignmentOptions.Center;

        // Botón Empezar
        var btnStart = UIButton(canvasGO, "BtnStart", "EMPEZAR",
            new Vector2(0, -60), new Vector2(280, 65),
            new Color(0.55f,0.10f,0.55f), new Color(0.80f,0.80f,0.80f));
        WireButton(btnStart.GetComponent<UnityEngine.UI.Button>(), mmm, "StartGame");

        // Boton Cerrar
        var btnQuit = UIButton(canvasGO, "BtnQuit", "CERRAR",
            new Vector2(0, -145), new Vector2(280, 55),
            new Color(0.25f,0.25f,0.35f), new Color(0.65f,0.65f,0.75f));
        WireButton(btnQuit.GetComponent<UnityEngine.UI.Button>(), mmm, "QuitGame");

        // Créditos pequeños
        UIText(canvasGO, "Credits", "(c) Unicorn Games",
            new Vector2(0, -270), new Vector2(400, 40), 18, new Color(0.35f,0.30f,0.45f))
            .GetComponent<TMPro.TextMeshProUGUI>().alignment = TMPro.TextAlignmentOptions.Center;

        // EventSystem - IMPRESCINDIBLE para que los clicks funcionen
        var esGO = new GameObject("EventSystem");
        esGO.AddComponent<UnityEngine.EventSystems.EventSystem>();
        esGO.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();

        EditorSceneManager.SaveScene(scene, path);
        Debug.Log("Escena MainMenu guardada.");
    }

    static void CreateScene()
    {
        EnsureFolder("Assets/_Scenes");
        const string path = "Assets/_Scenes/DemoFloors30to25.unity";
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // Camera
        var camGO = new GameObject("Main Camera"); camGO.tag = "MainCamera";
        camGO.transform.position = new Vector3(0, 0, -10);
        var cam = camGO.AddComponent<Camera>();
        cam.orthographic = true; cam.orthographicSize = 7f;
        cam.backgroundColor = new Color(0.04f,0.03f,0.08f);
        cam.clearFlags = CameraClearFlags.SolidColor;
        camGO.AddComponent<AudioListener>();
        var cf = camGO.AddComponent<CameraFollow>();
        SO(cf, so => { F(so,"smoothSpeed",5f); V3(so,"offset",new Vector3(0f,1f,-10f)); });

        // Managers
        var mgr = new GameObject("_Managers");

        // ── Game Over Panel ──────────────────────────────────────
        var goCanvas = new GameObject("GameOverCanvas");
        var goCV = goCanvas.AddComponent<Canvas>();
        goCV.renderMode = RenderMode.ScreenSpaceOverlay; goCV.sortingOrder = 20;
        goCanvas.AddComponent<UnityEngine.UI.CanvasScaler>().uiScaleMode =
            UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
        goCanvas.AddComponent<UnityEngine.UI.GraphicRaycaster>();
        goCanvas.AddComponent<GameOverManager>();

        var goBg   = UIPanel(goCanvas,"GOBg",  new Color(0,0,0,0.88f), Vector2.zero, new Vector2(1920,1080));
        var goTitle= UIText(goCanvas, "GOTitle","GAME OVER",
            new Vector2(0,100),new Vector2(600,120),64,new Color(0.9f,0.2f,0.2f));
        goTitle.GetComponent<TMPro.TextMeshProUGUI>().alignment = TMPro.TextAlignmentOptions.Center;
        goTitle.GetComponent<TMPro.TextMeshProUGUI>().fontStyle = TMPro.FontStyles.Bold;

        UIText(goCanvas,"GOSub","Sin mas creditos...",
            new Vector2(0,30),new Vector2(500,60),28,new Color(0.6f,0.5f,0.7f))
            .GetComponent<TMPro.TextMeshProUGUI>().alignment = TMPro.TextAlignmentOptions.Center;

        var goBtnR = UIButton(goCanvas,"BtnRestart","REINTENTAR",
            new Vector2(0,-60),new Vector2(260,60),new Color(0.55f,0.10f,0.55f),Color.white);
        var goMgr = goCanvas.GetComponent<GameOverManager>();
        WireButton(goBtnR.GetComponent<UnityEngine.UI.Button>(), goMgr, "Restart");
        var goBtnQ = UIButton(goCanvas,"BtnQuit","CERRAR",
            new Vector2(0,-140),new Vector2(260,55),new Color(0.25f,0.25f,0.35f),new Color(0.7f,0.7f,0.8f));
        WireButton(goBtnQ.GetComponent<UnityEngine.UI.Button>(), goMgr, "Quit");
        goCanvas.SetActive(false);

        // ── Victory Panel ────────────────────────────────────────
        var vicCanvas = new GameObject("VictoryCanvas");
        var vicCV = vicCanvas.AddComponent<Canvas>();
        vicCV.renderMode = RenderMode.ScreenSpaceOverlay; vicCV.sortingOrder = 20;
        vicCanvas.AddComponent<UnityEngine.UI.CanvasScaler>().uiScaleMode =
            UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
        vicCanvas.AddComponent<UnityEngine.UI.GraphicRaycaster>();
        vicCanvas.AddComponent<GameOverManager>();

        var vicBg = UIPanel(vicCanvas,"VicBg",new Color(0,0,0,0.88f), Vector2.zero, new Vector2(1920,1080));
        var vicTitle = UIText(vicCanvas,"VicTitle","VICTORIA!",
            new Vector2(0,100),new Vector2(600,120),64,new Color(0.2f,0.85f,0.4f));
        vicTitle.GetComponent<TMPro.TextMeshProUGUI>().alignment = TMPro.TextAlignmentOptions.Center;
        vicTitle.GetComponent<TMPro.TextMeshProUGUI>().fontStyle = TMPro.FontStyles.Bold;
        UIText(vicCanvas,"VicSub","Pisos 30-25 superados!",
            new Vector2(0,30),new Vector2(600,60),28,new Color(0.6f,0.8f,0.6f))
            .GetComponent<TMPro.TextMeshProUGUI>().alignment = TMPro.TextAlignmentOptions.Center;
        var vicBtnR = UIButton(vicCanvas,"BtnRestart","JUGAR DE NUEVO",
            new Vector2(0,-60),new Vector2(280,60),new Color(0.15f,0.55f,0.25f),Color.white);
        var vicMgr = vicCanvas.GetComponent<GameOverManager>();
        WireButton(vicBtnR.GetComponent<UnityEngine.UI.Button>(), vicMgr, "Restart");
        var vicBtnQ = UIButton(vicCanvas,"BtnQuit","CERRAR",
            new Vector2(0,-140),new Vector2(260,55),new Color(0.25f,0.25f,0.35f),new Color(0.7f,0.7f,0.8f));
        WireButton(vicBtnQ.GetComponent<UnityEngine.UI.Button>(), vicMgr, "Quit");
        vicCanvas.SetActive(false);

        var gm = mgr.AddComponent<GameManager>();
        SO(gm, so => {
            I(so,"currentFloor",30); I(so,"targetFloor",25); F(so,"reviveDelay",2f);
            R(so,"gameOverPanel", goCanvas);
            R(so,"victoryPanel",  vicCanvas);
        });

        // Skarn
        var skarnPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
            "Assets/_Prefabs/Player/Skarn.prefab");
        if (skarnPrefab != null)
        {
            var skarn = PrefabUtility.InstantiatePrefab(skarnPrefab) as GameObject;
            skarn.transform.position = new Vector3(-7f, 1f, 0f);
            SO(cf, so => R(so,"target",skarn.transform));
        }

        // Floors
        Floor(30, Vector3.zero,         "WarriorAdventurer","Guerrero Aventurero","Detente! Hoy es tu fin.");
        Floor(29, new Vector3(0,12,0),  "Goblin","","");
        Floor(28, new Vector3(0,24,0),  "SkeletonArcher","","");
        Floor(27, new Vector3(0,36,0),  "Zombie","","");
        PuzzleFloor(new Vector3(0,48,0));
        Floor(25, new Vector3(0,60,0),  "RogueAdventurer","Picaro Aventurero","Nadie me ha seguido el ritmo... y tu tampoco podras!");

        // EventSystem - necesario para Canvas de GameOver/Victoria
        var esGO2 = new GameObject("EventSystem");
        esGO2.AddComponent<UnityEngine.EventSystems.EventSystem>();
        esGO2.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();

        EditorSceneManager.SaveScene(scene, path);
        Debug.Log("Escena guardada.");
    }

    static void Floor(int num, Vector3 origin, string enemy, string advName, string advLine)
    {
        var root = new GameObject("Floor_" + num); root.transform.position = origin;
        Ground(root, new Vector3(0,-1,0), new Vector3(22,1,1), COL_GROUND);
        GndEdge(root);
        Wall(root, new Vector3(-10.5f,4,0)); Wall(root, new Vector3(10.5f,4,0));
        WallEdge(root, new Vector3(-9.52f,4,0)); WallEdge(root, new Vector3(9.52f,4,0));
        if (num==29||num==28) { Plat(root,new Vector3(-3,1.5f,0)); Plat(root,new Vector3(4,2.5f,0)); }
        Checkpoint(root, new Vector3(-8,0,0));
        var ep = AssetDatabase.LoadAssetAtPath<GameObject>($"Assets/_Prefabs/Enemies/{enemy}.prefab");
        if (ep != null)
        {
            var e = PrefabUtility.InstantiatePrefab(ep) as GameObject;
            e.transform.SetParent(root.transform); e.transform.localPosition = new Vector3(5,0.5f,0);
            if (!string.IsNullOrEmpty(advName)) DlgTrigger(root, new Vector3(3,0,0), advName, advLine);
        }
        PortalExit(root, new Vector3(9f,0.5f,0));
    }

    static void PuzzleFloor(Vector3 origin)
    {
        var root = new GameObject("Floor_26_Puzzle"); root.transform.position = origin;
        Ground(root, new Vector3(0,-1,0), new Vector3(22,1,1), COL_GROUND);
        GndEdge(root);
        Wall(root, new Vector3(-10.5f,4,0)); Wall(root, new Vector3(10.5f,4,0));
        WallEdge(root, new Vector3(-9.52f,4,0)); WallEdge(root, new Vector3(9.52f,4,0));
        Checkpoint(root, new Vector3(-8,0,0));

        // Portal bloqueado hasta resolver el puzzle
        var portalExit = PortalExit(root, new Vector3(9f,0.5f,0), locked: true);

        // PuzzleManager desbloquea el portal al resolver
        var pm = root.AddComponent<PuzzleManager>();
        SO(pm, so => { R(so,"exitDoor",portalExit); F(so,"displayDelay",0.6f); });

        // Texto indicador en el mundo
        var hintGO = new GameObject("HintText"); hintGO.transform.SetParent(root.transform);
        hintGO.transform.localPosition = new Vector3(-2f, 3f, 0f);
        hintGO.transform.localScale    = new Vector3(0.5f, 0.5f, 1f);
        var tmp = hintGO.AddComponent<TMPro.TextMeshPro>();
        tmp.text      = "Pisa las runas en orden: 1 - 2 - 3";
        tmp.fontSize  = 6;
        tmp.color     = new Color(0.8f, 0.7f, 0.2f);
        tmp.alignment = TMPro.TextAlignmentOptions.Center;
    }

    // ── Geometry helpers ──────────────────────────────────────
    static void Ground(GameObject p, Vector3 lp, Vector3 s, Color c)
    { var g=GO("Ground",p,8); g.transform.localPosition=lp; g.transform.localScale=s; var sr=g.AddComponent<SpriteRenderer>(); sr.sprite=WhitePx(); sr.color=c; g.AddComponent<BoxCollider2D>().size=Vector2.one; }

    static void GndEdge(GameObject p)
    { var g=GO("GroundEdge",p,-1); g.transform.localPosition=new Vector3(0,-0.45f,0); g.transform.localScale=new Vector3(22,0.1f,1); var sr=g.AddComponent<SpriteRenderer>(); sr.sprite=WhitePx(); sr.color=new Color(0.55f,0.30f,0.65f); sr.sortingOrder=1; }

    static void Wall(GameObject p, Vector3 lp)
    { var g=GO("Wall",p,8); g.transform.localPosition=lp; g.transform.localScale=new Vector3(1,12,1); var sr=g.AddComponent<SpriteRenderer>(); sr.sprite=WhitePx(); sr.color=COL_WALL; g.AddComponent<BoxCollider2D>().size=Vector2.one; }

    static void WallEdge(GameObject p, Vector3 lp)
    { var g=GO("WallEdge",p,-1); g.transform.localPosition=lp; g.transform.localScale=new Vector3(0.06f,12,1); var sr=g.AddComponent<SpriteRenderer>(); sr.sprite=WhitePx(); sr.color=new Color(0.45f,0.25f,0.55f); sr.sortingOrder=1; }

    static void Plat(GameObject p, Vector3 lp)
    { var g=GO("Platform",p,8); g.transform.localPosition=lp; g.transform.localScale=new Vector3(4,0.5f,1); var sr=g.AddComponent<SpriteRenderer>(); sr.sprite=WhitePx(); sr.color=COL_PLATFORM; sr.sortingOrder=1; g.AddComponent<BoxCollider2D>().size=Vector2.one; }

    static void Checkpoint(GameObject p, Vector3 lp)
    {
        var g = GO("Checkpoint", p, -1);
        g.transform.localPosition = lp;
        var c = g.AddComponent<BoxCollider2D>();
        c.isTrigger = true; c.size = new Vector2(1, 3);
        var vis = new GameObject("Visual"); vis.transform.SetParent(g.transform);
        vis.transform.localScale = new Vector3(0.2f, 0.8f, 1);
        var vsr = vis.AddComponent<SpriteRenderer>();
        vsr.sprite = WhitePx(); vsr.color = new Color(0.9f, 0.8f, 0.1f); vsr.sortingOrder = 2;
        g.AddComponent<CheckpointTrigger>();
    }

    static void DlgTrigger(GameObject p, Vector3 lp, string nm, string line)
    { var g=GO("DialogueTrigger",p,-1); g.transform.localPosition=lp; var c=g.AddComponent<BoxCollider2D>(); c.isTrigger=true; c.size=new Vector2(2,3);
      SO(g.AddComponent<AdventurerDialogueTrigger>(), so=>{S(so,"adventurerName",nm);S(so,"dialogue",line);F(so,"displayDuration",3f);}); }

    static GameObject Door(GameObject p, Vector3 lp)
    { var g=GO("Door",p,8); g.transform.localPosition=lp; g.transform.localScale=new Vector3(1,3,1); var sr=g.AddComponent<SpriteRenderer>(); sr.sprite=WhitePx(); sr.color=COL_DOOR; sr.sortingOrder=1; g.AddComponent<BoxCollider2D>().size=Vector2.one; return g; }

    static void FloorExit(GameObject p, Vector3 lp)
    { var g=GO("FloorExit",p,-1); g.transform.localPosition=lp; var c=g.AddComponent<BoxCollider2D>(); c.isTrigger=true; c.size=new Vector2(1,3);
      SO(g.AddComponent<FloorExitTrigger>(), so=>B(so,"requiresAllEnemiesDefeated",true)); }

    // Portal animado que reemplaza la puerta de salida
    static GameObject PortalExit(GameObject p, Vector3 lp, bool locked = false)
    {
        var g = GO("PortalExit", p, -1);
        g.transform.localPosition = lp;
        g.transform.localScale    = new Vector3(1.5f, 2.5f, 1f);

        var sr = g.AddComponent<SpriteRenderer>();
        sr.sortingOrder = 2;
        var firstFrame = Spr("Assets/_Art/Portal/000.png");
        if (firstFrame != null) sr.sprite = firstFrame;
        // Si bloqueado empieza oscuro/rojizo
        sr.color = locked ? new Color(0.6f, 0.1f, 0.1f, 1f) : Color.white;

        var anim = g.AddComponent<Animator>();
        var ctrl = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(
            "Assets/_Animations/Misc/PortalAnimator.controller");
        if (ctrl != null) anim.runtimeAnimatorController = ctrl;

        // Trigger de salida (solo activo si no está bloqueado)
        var col = g.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size      = new Vector2(0.8f, 1.8f);
        col.enabled   = !locked;

        if (!locked)
            SO(g.AddComponent<FloorExitTrigger>(),
               so => B(so, "requiresAllEnemiesDefeated", true));

        return g;
    }

    static GameObject GO(string name, GameObject parent, int layer)
    { var g=new GameObject(name); if(layer>=0)g.layer=layer; g.transform.SetParent(parent.transform); return g; }

    static GameObject Sub(GameObject parent, string name, Vector3 localPos)
    { var g=new GameObject(name); g.transform.SetParent(parent.transform); g.transform.localPosition=localPos; return g; }

    // ── UI helpers ────────────────────────────────────────────
    static GameObject UIPanel(GameObject parent, string name, Color color,
        Vector2 anchoredPos, Vector2 size)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        var img = go.AddComponent<UnityEngine.UI.Image>();
        img.color = color;
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f,0.5f);
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = size;
        return go;
    }

    static GameObject UIText(GameObject parent, string name, string text,
        Vector2 anchoredPos, Vector2 size, float fontSize, Color color)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        var tmp = go.AddComponent<TMPro.TextMeshProUGUI>();
        tmp.text = text; tmp.fontSize = fontSize; tmp.color = color;
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f,0.5f);
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = size;
        return go;
    }

    static GameObject UIButton(GameObject parent, string name, string label,
        Vector2 anchoredPos, Vector2 size, Color bgColor, Color textColor)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        var img = go.AddComponent<UnityEngine.UI.Image>();
        img.color = bgColor;
        var btn = go.AddComponent<UnityEngine.UI.Button>();
        var colors = btn.colors;
        colors.highlightedColor = new Color(
            Mathf.Min(bgColor.r+0.2f,1f), Mathf.Min(bgColor.g+0.2f,1f),
            Mathf.Min(bgColor.b+0.2f,1f), 1f);
        colors.pressedColor = new Color(bgColor.r*0.7f,bgColor.g*0.7f,bgColor.b*0.7f,1f);
        btn.colors = colors;
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f,0.5f);
        rt.anchoredPosition = anchoredPos; rt.sizeDelta = size;

        var lblGO = new GameObject("Label");
        lblGO.transform.SetParent(go.transform, false);
        var tmp = lblGO.AddComponent<TMPro.TextMeshProUGUI>();
        tmp.text = label; tmp.fontSize = 22; tmp.color = textColor;
        tmp.alignment = TMPro.TextAlignmentOptions.Center;
        tmp.fontStyle = TMPro.FontStyles.Bold;
        var lrt = lblGO.GetComponent<RectTransform>();
        lrt.anchorMin = Vector2.zero; lrt.anchorMax = Vector2.one;
        lrt.offsetMin = lrt.offsetMax = Vector2.zero;
        return go;
    }

    // Wire a button with a persistent (serialized) listener
    static void WireButton(UnityEngine.UI.Button btn, Object target, string methodName)
    {
        if (btn == null || target == null) return;
        btn.onClick.RemoveAllListeners();

        // Use reflection to get the MethodInfo and add as persistent call
        var method = target.GetType().GetMethod(methodName,
            System.Reflection.BindingFlags.Instance |
            System.Reflection.BindingFlags.Public);
        if (method == null) { Debug.LogWarning($"WireButton: method {methodName} not found on {target.GetType()}"); return; }

        var action = (UnityAction)System.Delegate.CreateDelegate(typeof(UnityAction), target, method);
        UnityEventTools.AddPersistentListener(btn.onClick, action);
        EditorUtility.SetDirty(btn);
    }

    // ── SO helpers ────────────────────────────────────────────
    static void SO(Object t, System.Action<SerializedObject> a) { var so=new SerializedObject(t); a(so); so.ApplyModifiedPropertiesWithoutUndo(); }
    static void F (SerializedObject so,string n,float   v){var p=so.FindProperty(n);if(p!=null)p.floatValue=v;           else Debug.LogWarning("Not found: "+n);}
    static void I (SerializedObject so,string n,int     v){var p=so.FindProperty(n);if(p!=null)p.intValue=v;             else Debug.LogWarning("Not found: "+n);}
    static void B (SerializedObject so,string n,bool    v){var p=so.FindProperty(n);if(p!=null)p.boolValue=v;            else Debug.LogWarning("Not found: "+n);}
    static void S (SerializedObject so,string n,string  v){var p=so.FindProperty(n);if(p!=null)p.stringValue=v;          else Debug.LogWarning("Not found: "+n);}
    static void R (SerializedObject so,string n,Object  v){var p=so.FindProperty(n);if(p!=null)p.objectReferenceValue=v; else Debug.LogWarning("Not found: "+n);}
    static void M (SerializedObject so,string n,int     v){var p=so.FindProperty(n);if(p!=null)p.intValue=v;             else Debug.LogWarning("Not found: "+n);}
    static void V3(SerializedObject so,string n,Vector3 v){var p=so.FindProperty(n);if(p!=null)p.vector3Value=v;         else Debug.LogWarning("Not found: "+n);}

    static Sprite _whitePx;
    static Sprite WhitePx()
    {
        if (_whitePx == null)
            _whitePx = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Art/Utils/WhitePixel.png");
        if (_whitePx == null) Debug.LogError("WhitePixel.png no encontrado en Assets/_Art/Utils/");
        return _whitePx;
    }
    static Sprite Spr(string path) => AssetDatabase.LoadAssetAtPath<Sprite>(path);
    static void Save(GameObject go, string path) { PrefabUtility.SaveAsPrefabAsset(go,path); Object.DestroyImmediate(go); Debug.Log(Path.GetFileName(path)+" OK"); }
    static void Del(string path) { if(AssetDatabase.LoadAssetAtPath<Object>(path)!=null) AssetDatabase.DeleteAsset(path); }
    static void EnsureFolder(string path) { if(!AssetDatabase.IsValidFolder(path)){ string par=Path.GetDirectoryName(path)?.Replace('\\','/')??"Assets"; AssetDatabase.CreateFolder(par,Path.GetFileName(path)); } }
}
