using UnityEngine;
using UnityEngine.UI;

public class GameFlowManagerMain : MonoBehaviour
{
    // === Object References ===
    GameObject Ground;
    GameObject Player;
    GameObject Base;
    GameObject WoodFence_L;
    GameObject WoodFence_R;
    GameObject PineTree_L;
    GameObject PineTree_R;
    GameObject Generator;
    GameObject ConveyorBelt;
    GameObject Crossbow_L;
    GameObject Crossbow_R;
    GameObject Arrow;
    GameObject EnemySpawner;
    GameObject Enemy_A;
    GameObject GoldCoin;
    GameObject WoodLog_Spawner;
    GameObject WoodLog;
    GameObject WoodHouse;
    GameObject Worker_Spawner;
    GameObject Worker;
    GameObject Turret;
    GameObject Boss;
    GameObject CastleWall;
    GameObject HealthBar_UI;

    // === Entity System ===
    const int MAX_ENTITIES = 60;
    const int RULE_COUNT = 12;

    // Entity indices
    const int E_GROUND = 0;
    const int E_PLAYER = 1;
    const int E_BASE = 2;
    const int E_WOODFENCE_L = 3;
    const int E_WOODFENCE_R = 4;
    const int E_PINETREE_L = 5;
    const int E_PINETREE_R = 6;
    const int E_GENERATOR = 7;
    const int E_CONVEYOR = 8;
    const int E_CROSSBOW_L = 9;
    const int E_CROSSBOW_R = 10;
    const int E_WOODHOUSE = 11;
    const int E_TURRET = 12;
    const int E_BOSS = 13;
    const int E_CASTLEWALL = 14;
    const int E_HEALTHBAR = 15;

    // Enemy pool
    const int ENEMY_START = 16;
    const int ENEMY_COUNT = 15;

    // Coin pool
    const int COIN_START = 31;
    const int COIN_COUNT = 10;

    // Arrow pool
    const int ARROW_START = 41;
    const int ARROW_COUNT = 10;

    // Wood pool
    const int WOOD_START = 51;
    const int WOOD_COUNT = 6;

    // Worker pool
    const int WORKER_START = 57;
    const int WORKER_COUNT = 3;

    GameObject[] eGo = new GameObject[MAX_ENTITIES];
    bool[] eActive = new bool[MAX_ENTITIES];
    int[] eState = new int[MAX_ENTITIES];
    float[] eTimer = new float[MAX_ENTITIES];
    float[] eHP = new float[MAX_ENTITIES];

    // Arrow tracking
    Vector3[] arrowTarget = new Vector3[ARROW_COUNT];
    float[] arrowDmg = new float[ARROW_COUNT];
    float arrowSpeed = 25f;

    // Game state
    bool[] ruleTriggered;
    float gameTimer;
    int gold = 1;
    int wood = 0;
    int enemyKillCount = 0;
    int workerCount = 0;
    int baseLevel = 1;
    float baseHP = 50f;
    float baseMaxHP = 50f;
    float bossHP = 20f;
    bool bossDead = false;

    // Spawner timers
    float enemySpawnTimer = 0f;
    float enemySpawnInterval = 2.5f;
    bool enemySpawnerActive = false;
    float woodSpawnTimer = 0f;
    float woodSpawnInterval = 1.5f;
    bool woodSpawnerActive = false;

    // Joystick
    GFM_Joystick joystick;

    // UI
    Text guideText;
    Text goldText;
    Text woodText;
    bool ctaShown = false;
    bool gameEnded = false;

    // Player
    float playerMoveSpeed = 6f;

    // Crossbow
    float crossbowFireRate = 1.2f;
    float crossbowRange = 12f;
    float crossbowDamage = 1f;
    float crossbowTimerL = 0f;
    float crossbowTimerR = 0f;
    bool crossbowsActive = false;

    // Turret
    float turretFireRate = 1.5f;
    float turretRange = 14f;
    float turretDamage = 3f;
    float turretTimer = 0f;
    bool turretActive = false;

    // Conveyor build
    float conveyorBuildTime = 1.0f;

    // Worker behavior
    int[] workerState_arr = new int[3];
    int[] workerTargetWood = new int[3];
    float workerSpeed = 4f;

    // Damage flash timers
    float[] enemyFlashTimer = new float[15];
    float bossFlashTimer = 0f;

    // Auto-advance timer for stuck prevention
    float stuckTimer = 0f;
    int lastRuleIndex = 0;

    // Auto-move player toward objective
    bool autoMovePlayer = false;
    Vector3 autoMoveTarget = Vector3.zero;
    float autoMoveDelay = 0f;

    // Camera ref
    Camera mainCam;

    // Canvas
    Canvas canvas;

    // Guide flash
    float guideFlashTimer = 0f;

    // CTA pulse
    float ctaPulseTimer = 0f;
    GameObject ctaButtonObj;

    void Start()
    {
        // 1. Find all objects
        Ground = GameObject.Find("Ground_1");
        Player = GameObject.Find("Player");
        Base = GameObject.Find("Building_1");
        WoodFence_L = GameObject.Find("Building_2");
        WoodFence_R = GameObject.Find("Building_3");
        PineTree_L = GameObject.Find("Tree_1");
        PineTree_R = GameObject.Find("Tree_2");
        Generator = GameObject.Find("Building_4");
        ConveyorBelt = GameObject.Find("Building_5");
        Crossbow_L = GameObject.Find("Turret_1");
        Crossbow_R = GameObject.Find("Turret_2");
        Arrow = GameObject.Find("Arrow_1");
        EnemySpawner = GameObject.Find("Enemy_1");
        Enemy_A = GameObject.Find("Enemy_2");
        GoldCoin = GameObject.Find("Coin_1");
        WoodLog_Spawner = GameObject.Find("Building_6");
        WoodLog = GameObject.Find("Building_7");
        WoodHouse = GameObject.Find("Building_8");
        Worker_Spawner = GameObject.Find("Worker_1");
        Worker = GameObject.Find("Worker_2");
        Turret = GameObject.Find("Turret_3");
        Boss = GameObject.Find("Boss_1");
        CastleWall = GameObject.Find("Wall_1");
        HealthBar_UI = GameObject.Find("Building_9");

        // 2. Init rule tracking
        ruleTriggered = new bool[RULE_COUNT];

        // 3. Setup entity arrays
        for (int i = 0; i < MAX_ENTITIES; i++)
        {
            eGo[i] = null;
            eActive[i] = false;
            eState[i] = 0;
            eTimer[i] = 0f;
            eHP[i] = 0f;
        }

        // Assign main entities
        eGo[E_GROUND] = Ground;
        eGo[E_PLAYER] = Player;
        eGo[E_BASE] = Base;
        eGo[E_WOODFENCE_L] = WoodFence_L;
        eGo[E_WOODFENCE_R] = WoodFence_R;
        eGo[E_PINETREE_L] = PineTree_L;
        eGo[E_PINETREE_R] = PineTree_R;
        eGo[E_GENERATOR] = Generator;
        eGo[E_CONVEYOR] = ConveyorBelt;
        eGo[E_CROSSBOW_L] = Crossbow_L;
        eGo[E_CROSSBOW_R] = Crossbow_R;
        eGo[E_WOODHOUSE] = WoodHouse;
        eGo[E_TURRET] = Turret;
        eGo[E_BOSS] = Boss;
        eGo[E_CASTLEWALL] = CastleWall;
        eGo[E_HEALTHBAR] = HealthBar_UI;

        // Create enemy pool
        if (Enemy_A != null)
        {
            eGo[ENEMY_START] = Enemy_A;
            for (int i = 1; i < ENEMY_COUNT; i++)
            {
                eGo[ENEMY_START + i] = Instantiate(Enemy_A);
            }
        }

        // Create coin pool
        if (GoldCoin != null)
        {
            eGo[COIN_START] = GoldCoin;
            for (int i = 1; i < COIN_COUNT; i++)
            {
                eGo[COIN_START + i] = Instantiate(GoldCoin);
            }
        }

        // Create arrow pool
        if (Arrow != null)
        {
            eGo[ARROW_START] = Arrow;
            for (int i = 1; i < ARROW_COUNT; i++)
            {
                eGo[ARROW_START + i] = Instantiate(Arrow);
            }
        }
        for (int i = 0; i < ARROW_COUNT; i++)
        {
            arrowTarget[i] = Vector3.zero;
            arrowDmg[i] = 1f;
        }

        // Create wood pool
        if (WoodLog != null)
        {
            eGo[WOOD_START] = WoodLog;
            for (int i = 1; i < WOOD_COUNT; i++)
            {
                eGo[WOOD_START + i] = Instantiate(WoodLog);
            }
        }

        // Create worker pool
        if (Worker != null)
        {
            eGo[WORKER_START] = Worker;
            for (int i = 1; i < WORKER_COUNT; i++)
            {
                eGo[WORKER_START + i] = Instantiate(Worker);
            }
        }
        for (int i = 0; i < WORKER_COUNT; i++)
        {
            workerState_arr[i] = 0;
            workerTargetWood[i] = -1;
        }
        for (int i = 0; i < ENEMY_COUNT; i++)
        {
            enemyFlashTimer[i] = 0f;
        }

        // 4. Hide ALL objects initially
        for (int i = 0; i < MAX_ENTITIES; i++)
        {
            if (eGo[i] != null)
            {
                eGo[i].transform.position = new Vector3(0, -999, 0);
                eActive[i] = false;
            }
        }

        // Hide spawner objects
        if (EnemySpawner != null) EnemySpawner.transform.position = new Vector3(0, -999, 0);
        if (WoodLog_Spawner != null) WoodLog_Spawner.transform.position = new Vector3(0, -999, 0);
        if (Worker_Spawner != null) Worker_Spawner.transform.position = new Vector3(0, -999, 0);

        // 5. Setup camera
        mainCam = Camera.main;
        if (mainCam != null)
        {
            mainCam.orthographic = true;
            mainCam.orthographicSize = 12f;
            mainCam.transform.position = new Vector3(0, 20, -10);
            mainCam.transform.rotation = Quaternion.Euler(60, 0, 0);
            mainCam.backgroundColor = new Color(0.5f, 0.8f, 1f);
        }

        // 6. Setup UI - find existing Canvas
        GameObject canvasObj = GameObject.Find("Canvas");
        if (canvasObj != null)
        {
            canvas = (Canvas)canvasObj.GetComponent(typeof(Canvas));
        }

        // Create joystick
        if (canvas != null)
        {
            joystick = GFM_Joystick.Create(canvas, 200f);
        }

        // Find existing UI text objects or create them
        SetupUITexts();

        // 7. Set colors
        if (Ground != null) GFM_Create.SetColor(Ground, new Color(0.35f, 0.55f, 0.25f));
        if (Player != null) GFM_Create.SetColor(Player, new Color(0.2f, 0.5f, 0.9f));
        if (Base != null) GFM_Create.SetColor(Base, new Color(0.7f, 0.7f, 0.8f));
        if (WoodFence_L != null) GFM_Create.SetColor(WoodFence_L, new Color(0.6f, 0.4f, 0.2f));
        if (WoodFence_R != null) GFM_Create.SetColor(WoodFence_R, new Color(0.6f, 0.4f, 0.2f));
        if (PineTree_L != null) GFM_Create.SetColor(PineTree_L, new Color(0.1f, 0.5f, 0.15f));
        if (PineTree_R != null) GFM_Create.SetColor(PineTree_R, new Color(0.1f, 0.5f, 0.15f));
        if (Generator != null) GFM_Create.SetColor(Generator, new Color(0.5f, 0.5f, 0.6f));
        if (ConveyorBelt != null) GFM_Create.SetColor(ConveyorBelt, new Color(0.4f, 0.4f, 0.4f));
        if (Crossbow_L != null) GFM_Create.SetColor(Crossbow_L, new Color(0.55f, 0.35f, 0.15f));
        if (Crossbow_R != null) GFM_Create.SetColor(Crossbow_R, new Color(0.55f, 0.35f, 0.15f));
        if (WoodHouse != null) GFM_Create.SetColor(WoodHouse, new Color(0.65f, 0.45f, 0.25f));
        if (Turret != null) GFM_Create.SetColor(Turret, new Color(0.3f, 0.3f, 0.35f));
        if (Boss != null) GFM_Create.SetColor(Boss, new Color(0.8f, 0.1f, 0.1f));
        if (CastleWall != null) GFM_Create.SetColor(CastleWall, new Color(0.6f, 0.6f, 0.65f));

        for (int i = 0; i < ENEMY_COUNT; i++)
        {
            if (eGo[ENEMY_START + i] != null)
                GFM_Create.SetColor(eGo[ENEMY_START + i], new Color(0.8f, 0.2f, 0.2f));
        }
        for (int i = 0; i < COIN_COUNT; i++)
        {
            if (eGo[COIN_START + i] != null)
                GFM_Create.SetColor(eGo[COIN_START + i], new Color(1f, 0.85f, 0f));
        }
        for (int i = 0; i < ARROW_COUNT; i++)
        {
            if (eGo[ARROW_START + i] != null)
                GFM_Create.SetColor(eGo[ARROW_START + i], new Color(0.3f, 0.2f, 0.1f));
        }
        for (int i = 0; i < WOOD_COUNT; i++)
        {
            if (eGo[WOOD_START + i] != null)
                GFM_Create.SetColor(eGo[WOOD_START + i], new Color(0.55f, 0.35f, 0.15f));
        }
        for (int i = 0; i < WORKER_COUNT; i++)
        {
            if (eGo[WORKER_START + i] != null)
                GFM_Create.SetColor(eGo[WORKER_START + i], new Color(0.9f, 0.75f, 0.5f));
        }

        // Set scales
        if (Ground != null) Ground.transform.localScale = new Vector3(40, 1, 40);
        if (Player != null) Player.transform.localScale = new Vector3(1, 2, 1);
        if (Base != null) Base.transform.localScale = new Vector3(3, 3, 3);
        if (WoodFence_L != null) WoodFence_L.transform.localScale = new Vector3(0.3f, 1.5f, 4);
        if (WoodFence_R != null) WoodFence_R.transform.localScale = new Vector3(0.3f, 1.5f, 4);
        if (PineTree_L != null) PineTree_L.transform.localScale = new Vector3(0.5f, 3, 0.5f);
        if (PineTree_R != null) PineTree_R.transform.localScale = new Vector3(0.5f, 3, 0.5f);
        if (Generator != null) Generator.transform.localScale = new Vector3(2, 2, 2);
        if (ConveyorBelt != null) ConveyorBelt.transform.localScale = new Vector3(4, 0.3f, 1);
        if (Crossbow_L != null) Crossbow_L.transform.localScale = new Vector3(1, 1.5f, 1);
        if (Crossbow_R != null) Crossbow_R.transform.localScale = new Vector3(1, 1.5f, 1);
        if (WoodHouse != null) WoodHouse.transform.localScale = new Vector3(3, 2.5f, 3);
        if (Turret != null) Turret.transform.localScale = new Vector3(1.5f, 2, 1.5f);
        if (Boss != null) Boss.transform.localScale = new Vector3(1.5f, 3, 1.5f);
        if (CastleWall != null) CastleWall.transform.localScale = new Vector3(5, 4, 0.5f);

        for (int i = 0; i < ENEMY_COUNT; i++)
        {
            if (eGo[ENEMY_START + i] != null)
                eGo[ENEMY_START + i].transform.localScale = new Vector3(0.8f, 1.6f, 0.8f);
        }
        for (int i = 0; i < COIN_COUNT; i++)
        {
            if (eGo[COIN_START + i] != null)
                eGo[COIN_START + i].transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
        }
        for (int i = 0; i < ARROW_COUNT; i++)
        {
            if (eGo[ARROW_START + i] != null)
                eGo[ARROW_START + i].transform.localScale = new Vector3(0.05f, 0.5f, 0.05f);
        }
        for (int i = 0; i < WOOD_COUNT; i++)
        {
            if (eGo[WOOD_START + i] != null)
                eGo[WOOD_START + i].transform.localScale = new Vector3(0.3f, 0.8f, 0.3f);
        }
        for (int i = 0; i < WORKER_COUNT; i++)
        {
            if (eGo[WORKER_START + i] != null)
                eGo[WORKER_START + i].transform.localScale = new Vector3(0.8f, 1.5f, 0.8f);
        }

        if (HealthBar_UI != null) HealthBar_UI.transform.localScale = new Vector3(3, 0.2f, 0.2f);

        // Start auto-move toward conveyor after short delay
        autoMovePlayer = true;
        autoMoveTarget = new Vector3(0, 1, 2);
        autoMoveDelay = 1.0f;
    }

    void SetupUITexts()
    {
        // Try to find existing text objects first
        GameObject existingGuide = GameObject.Find("GuideText");
        GameObject existingGold = GameObject.Find("GoldText");
        GameObject existingWood = GameObject.Find("WoodText");
        GameObject existingScore = GameObject.Find("ScoreText");

        if (existingGuide != null)
        {
            guideText = (Text)existingGuide.GetComponent(typeof(Text));
        }
        if (existingGold != null)
        {
            goldText = (Text)existingGold.GetComponent(typeof(Text));
        }
        if (existingWood != null)
        {
            woodText = (Text)existingWood.GetComponent(typeof(Text));
        }

        // If ScoreText exists, use it as guide text
        if (guideText == null && existingScore != null)
        {
            guideText = (Text)existingScore.GetComponent(typeof(Text));
        }

        if (canvas == null) return;
        GameObject canvasGO = canvas.gameObject;

        // Create guide text if not found
        if (guideText == null)
        {
            GameObject guideObj = new GameObject("GuideText");
            guideObj.transform.position = Vector3.zero;
            RectTransform gr = (RectTransform)guideObj.AddComponent(typeof(RectTransform));
            guideText = (Text)guideObj.AddComponent(typeof(Text));
            guideText.font = (Font)Resources.GetBuiltinResource(typeof(Font), "Arial.ttf");
            guideText.fontSize = 28;
            guideText.alignment = TextAnchor.UpperCenter;
            guideText.color = Color.white;
            guideText.horizontalOverflow = HorizontalWrapMode.Overflow;
            gr.anchorMin = new Vector2(0.5f, 1f);
            gr.anchorMax = new Vector2(0.5f, 1f);
            gr.anchoredPosition = new Vector2(0, -50);
            gr.sizeDelta = new Vector2(600, 60);
            // Reparent via transform hierarchy
            guideObj.transform.position = Vector3.zero;
            RectTransform canvasRect = (RectTransform)canvasGO.GetComponent(typeof(RectTransform));
            if (canvasRect != null)
            {
                gr.SetParent(canvasRect, false);
            }
        }

        // Create gold text if not found
        if (goldText == null)
        {
            GameObject goldObj = new GameObject("GoldText");
            goldObj.transform.position = Vector3.zero;
            RectTransform goldr = (RectTransform)goldObj.AddComponent(typeof(RectTransform));
            goldText = (Text)goldObj.AddComponent(typeof(Text));
            goldText.font = (Font)Resources.GetBuiltinResource(typeof(Font), "Arial.ttf");
            goldText.fontSize = 24;
            goldText.alignment = TextAnchor.UpperLeft;
            goldText.color = new Color(1f, 0.85f, 0f);
            goldr.anchorMin = new Vector2(0f, 1f);
            goldr.anchorMax = new Vector2(0f, 1f);
            goldr.anchoredPosition = new Vector2(80, -20);
            goldr.sizeDelta = new Vector2(200, 40);
            RectTransform canvasRect = (RectTransform)canvasGO.GetComponent(typeof(RectTransform));
            if (canvasRect != null)
            {
                goldr.SetParent(canvasRect, false);
            }
        }

        // Create wood text if not found
        if (woodText == null)
        {
            GameObject woodObj = new GameObject("WoodText");
            woodObj.transform.position = Vector3.zero;
            RectTransform woodr = (RectTransform)woodObj.AddComponent(typeof(RectTransform));
            woodText = (Text)woodObj.AddComponent(typeof(Text));
            woodText.font = (Font)Resources.GetBuiltinResource(typeof(Font), "Arial.ttf");
            woodText.fontSize = 24;
            woodText.alignment = TextAnchor.UpperLeft;
            woodText.color = new Color(0.6f, 0.4f, 0.2f);
            woodr.anchorMin = new Vector2(0f, 1f);
            woodr.anchorMax = new Vector2(0f, 1f);
            woodr.anchoredPosition = new Vector2(80, -50);
            woodr.sizeDelta = new Vector2(200, 40);
            RectTransform canvasRect = (RectTransform)canvasGO.GetComponent(typeof(RectTransform));
            if (canvasRect != null)
            {
                woodr.SetParent(canvasRect, false);
            }
        }
    }

    void Update()
    {
        if (gameEnded)
        {
            // Pulse CTA button
            if (ctaButtonObj != null)
            {
                ctaPulseTimer += Time.deltaTime;
                float s = 1f + 0.08f * Mathf.Sin(ctaPulseTimer * 4f);
                ctaButtonObj.transform.localScale = new Vector3(s, s, 1f);
            }
            // Allow tap anywhere to install
            if (Input.GetMouseButtonDown(0) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began))
            {
                Luna.Unity.Playable.InstallFullGame();
            }
            return;
        }

        float dt = Time.deltaTime;
        gameTimer += dt;

        // Anti-stuck: auto-advance if stuck too long
        UpdateStuckPrevention(dt);

        CheckEventRules();
        UpdatePlayer(dt);
        UpdateConveyor(dt);
        UpdateEnemySpawner(dt);
        UpdateEnemies(dt);
        UpdateCoins();
        UpdateProjectiles(dt);
        UpdateCrossbows(dt);
        UpdateTurretLogic(dt);
        UpdateWoodSpawner(dt);
        UpdateWoodCollect();
        UpdateWoodHouseBuild(dt);
        UpdateWorkers(dt);
        UpdateBoss(dt);
        UpdateHealthBar();
        UpdateUI();
        UpdateDamageFlash(dt);
        UpdateGuideFlash(dt);
    }

    void UpdateStuckPrevention(float dt)
    {
        int triggered = 0;
        for (int i = 0; i < RULE_COUNT; i++)
        {
            if (ruleTriggered[i]) triggered++;
        }

        if (triggered == lastRuleIndex)
        {
            stuckTimer += dt;
        }
        else
        {
            stuckTimer = 0f;
            lastRuleIndex = triggered;
        }

        // At 2.5s, start auto-moving player toward objective
        if (stuckTimer > 2.5f && stuckTimer < 2.7f)
        {
            StartAutoMove(triggered);
        }

        // If stuck for 5+ seconds, force advance
        if (stuckTimer > 5f)
        {
            stuckTimer = 0f;
            AutoAdvance(triggered);
        }
    }

    void StartAutoMove(int currentTriggered)
    {
        if (!ruleTriggered[1])
        {
            autoMovePlayer = true;
            autoMoveTarget = new Vector3(0, 1, 2);
            autoMoveDelay = 0f;
        }
        else if (ruleTriggered[2] && !ruleTriggered[3])
        {
            autoMovePlayer = true;
            autoMoveTarget = new Vector3(5, 1, -2);
            autoMoveDelay = 0f;
        }
        else if (ruleTriggered[5] && !ruleTriggered[6])
        {
            autoMovePlayer = true;
            autoMoveTarget = new Vector3(0, 1, 7);
            autoMoveDelay = 0f;
        }
        else if (ruleTriggered[8] && !ruleTriggered[9])
        {
            autoMovePlayer = true;
            autoMoveTarget = new Vector3(0, 1, -3);
            autoMoveDelay = 0f;
        }
    }

    void AutoAdvance(int currentTriggered)
    {
        if (!ruleTriggered[0])
        {
            return;
        }
        if (!ruleTriggered[1])
        {
            eState[E_CONVEYOR] = 2;
            if (eGo[E_CONVEYOR] != null)
                GFM_Create.SetColor(eGo[E_CONVEYOR], new Color(0.3f, 0.7f, 0.3f));
        }
        else if (!ruleTriggered[2])
        {
            enemyKillCount = 3;
        }
        else if (!ruleTriggered[3])
        {
            wood = 3;
            eState[E_WOODHOUSE] = 2;
            if (eGo[E_WOODHOUSE] != null)
                GFM_Create.SetColor(eGo[E_WOODHOUSE], new Color(0.5f, 0.7f, 0.3f));
        }
        else if (!ruleTriggered[4])
        {
            workerCount = 1;
        }
        else if (!ruleTriggered[5])
        {
            wood = 5;
        }
        else if (!ruleTriggered[6])
        {
            eState[E_TURRET] = 2;
            turretActive = true;
            if (eGo[E_TURRET] != null)
                GFM_Create.SetColor(eGo[E_TURRET], new Color(0.2f, 0.6f, 0.8f));
        }
        else if (!ruleTriggered[7])
        {
            enemyKillCount = 10;
        }
        else if (!ruleTriggered[8])
        {
            bossHP = 0;
            bossDead = true;
            HideEntity(E_BOSS);
        }
        else if (!ruleTriggered[9])
        {
            baseLevel = 2;
            if (eGo[E_BASE] != null)
            {
                eGo[E_BASE].transform.localScale = new Vector3(4, 4, 4);
                GFM_Create.SetColor(eGo[E_BASE], new Color(0.9f, 0.85f, 0.6f));
            }
        }
    }

    void CheckEventRules()
    {
        // Rule 1: Game Start
        if (!ruleTriggered[0])
        {
            ruleTriggered[0] = true;
            ActivateAt(E_GROUND, new Vector3(0, -0.5f, 0));
            ActivateAt(E_PLAYER, new Vector3(-2, 1, -4));
            ActivateAt(E_BASE, new Vector3(0, 1.5f, -3));
            ActivateAt(E_WOODFENCE_L, new Vector3(-4, 0.75f, 0));
            ActivateAt(E_WOODFENCE_R, new Vector3(4, 0.75f, 0));
            ActivateAt(E_PINETREE_L, new Vector3(-5, 1.5f, 3));
            ActivateAt(E_PINETREE_R, new Vector3(5, 1.5f, 3));
            ActivateAt(E_GENERATOR, new Vector3(-3, 1, 2));
            ActivateAt(E_CONVEYOR, new Vector3(0, 0.15f, 2));
            ActivateAt(E_HEALTHBAR, new Vector3(0, 3.5f, -3));
            eState[E_CONVEYOR] = 0;
            baseHP = baseMaxHP;
            ShowGuide("Move to conveyor to build! (1 Gold)");
        }

        // Rule 2: Conveyor built
        if (!ruleTriggered[1] && eState[E_CONVEYOR] == 2)
        {
            ruleTriggered[1] = true;
            ActivateAt(E_CROSSBOW_L, new Vector3(-3, 0.75f, 5));
            ActivateAt(E_CROSSBOW_R, new Vector3(3, 0.75f, 5));
            crossbowsActive = true;
            enemySpawnerActive = true;
            enemySpawnTimer = 0f;
            ShowGuide("Crossbows active! Defeat enemies!");
        }

        // Rule 3: 3 enemies killed
        if (!ruleTriggered[2] && enemyKillCount >= 3)
        {
            ruleTriggered[2] = true;
            ActivateAt(E_WOODHOUSE, new Vector3(5, 1.25f, -2));
            eState[E_WOODHOUSE] = 0;
            woodSpawnerActive = true;
            woodSpawnTimer = 0f;
            for (int w = 0; w < 3; w++)
            {
                if (w < WOOD_COUNT)
                {
                    int idx = WOOD_START + w;
                    if (eGo[idx] != null)
                    {
                        float rx = Random.Range(-4f, 4f);
                        float rz = Random.Range(1f, 6f);
                        eGo[idx].transform.position = new Vector3(rx, 0.4f, rz);
                        eActive[idx] = true;
                        eState[idx] = 0;
                    }
                }
            }
            ShowGuide("Collect wood & bring to Wood House!");
        }

        // Rule 4: Wood house built
        if (!ruleTriggered[3] && eState[E_WOODHOUSE] == 2)
        {
            ruleTriggered[3] = true;
            SpawnWorker();
            ShowGuide("Worker recruited!");
        }

        // Rule 5: Worker count >= 1
        if (!ruleTriggered[4] && workerCount >= 1)
        {
            ruleTriggered[4] = true;
            ShowGuide("Workers auto-carry wood!");
        }

        // Rule 6: wood >= 5
        if (!ruleTriggered[5] && wood >= 5)
        {
            ruleTriggered[5] = true;
            ActivateAt(E_TURRET, new Vector3(0, 1, 7));
            eState[E_TURRET] = 0;
            ShowGuide("Move to turret to build! (5 Wood)");
        }

        // Rule 7: Turret built
        if (!ruleTriggered[6] && eState[E_TURRET] == 2)
        {
            ruleTriggered[6] = true;
            turretActive = true;
            enemySpawnInterval = 1.8f;
            ShowGuide("Defend the base!");
        }

        // Rule 8: 10 enemies killed
        if (!ruleTriggered[7] && enemyKillCount >= 10)
        {
            ruleTriggered[7] = true;
            ActivateAt(E_BOSS, new Vector3(0, 1.5f, 15));
            eHP[E_BOSS] = 20f;
            bossHP = 20f;
            bossDead = false;
            ShowGuide("BOSS incoming! Destroy it!");
        }

        // Rule 9: Boss defeated
        if (!ruleTriggered[8] && ruleTriggered[7] && bossHP <= 0 && bossDead)
        {
            ruleTriggered[8] = true;
            ActivateAt(E_CASTLEWALL, new Vector3(0, 2, 8));
            enemySpawnerActive = false;
            // Clear remaining enemies
            for (int i = 0; i < ENEMY_COUNT; i++)
            {
                HideEntity(ENEMY_START + i);
            }
            ShowGuide("Tap to upgrade Base!");
        }

        // Rule 10: Base upgraded
        if (!ruleTriggered[9] && baseLevel >= 2)
        {
            ruleTriggered[9] = true;
            gameEnded = true;
            ShowGuide("Victory! Download now!");
            ShowCTA();
            Luna.Unity.LifeCycle.GameEnded();
        }
    }

    void ActivateAt(int idx, Vector3 pos)
    {
        if (idx >= 0 && idx < MAX_ENTITIES && eGo[idx] != null)
        {
            eGo[idx].transform.position = pos;
            eActive[idx] = true;
        }
    }

    void HideEntity(int idx)
    {
        if (idx >= 0 && idx < MAX_ENTITIES && eGo[idx] != null)
        {
            eGo[idx].transform.position = new Vector3(0, -999, 0);
            eActive[idx] = false;
        }
    }

    // === PLAYER ===
    void UpdatePlayer(float dt)
    {
        if (!eActive[E_PLAYER] || eGo[E_PLAYER] == null) return;

        float h = 0f;
        float v = 0f;

        if (joystick != null)
        {
            h = joystick.Horizontal;
            v = joystick.Vertical;
        }

        bool hasInput = Mathf.Abs(h) > 0.1f || Mathf.Abs(v) > 0.1f;

        // Auto-move toward objective if no input
        if (!hasInput && autoMovePlayer)
        {
            if (autoMoveDelay > 0f)
            {
                autoMoveDelay -= dt;
            }
            else
            {
                Vector3 playerPos = eGo[E_PLAYER].transform.position;
                Vector3 diff = autoMoveTarget - playerPos;
                diff.y = 0;
                if (diff.magnitude > 0.5f)
                {
                    Vector3 moveDir = diff.normalized * playerMoveSpeed * 0.7f * dt;
                    Vector3 newPos = playerPos + moveDir;
                    newPos.y = 1f;
                    eGo[E_PLAYER].transform.position = newPos;
                }
                else
                {
                    autoMovePlayer = false;
                }
            }
        }
        else if (hasInput)
        {
            autoMovePlayer = false;
            Vector3 move = new Vector3(h, 0, v).normalized * playerMoveSpeed * dt;
            Vector3 newPos = eGo[E_PLAYER].transform.position + move;
            newPos.x = Mathf.Clamp(newPos.x, -18f, 18f);
            newPos.z = Mathf.Clamp(newPos.z, -8f, 16f);
            newPos.y = 1f;
            eGo[E_PLAYER].transform.position = newPos;
        }

        // Check tap on base for upgrade (Rule 9 condition)
        if (ruleTriggered[8] && !ruleTriggered[9])
        {
            bool tapped = false;
            if (Input.GetMouseButtonDown(0)) tapped = true;
            if (Input.touchCount > 0)
            {
                Touch t = Input.GetTouch(0);
                if (t.phase == TouchPhase.Began) tapped = true;
            }

            if (tapped)
            {
                baseLevel = 2;
                if (eGo[E_BASE] != null)
                {
                    eGo[E_BASE].transform.localScale = new Vector3(4, 4, 4);
                    GFM_Create.SetColor(eGo[E_BASE], new Color(0.9f, 0.85f, 0.6f));
                }
            }
        }
    }

    // === CONVEYOR BELT ===
    void UpdateConveyor(float dt)
    {
        if (!eActive[E_CONVEYOR] || eGo[E_CONVEYOR] == null || eGo[E_PLAYER] == null) return;

        if (eState[E_CONVEYOR] == 0)
        {
            float dist = Vector3.Distance(eGo[E_PLAYER].transform.position, eGo[E_CONVEYOR].transform.position);
            if (dist < 3f && gold >= 1)
            {
                gold -= 1;
                eState[E_CONVEYOR] = 1;
                eTimer[E_CONVEYOR] = 0f;
                ShowGuide("Building conveyor...");
            }
        }
        else if (eState[E_CONVEYOR] == 1)
        {
            eTimer[E_CONVEYOR] += dt;
            float t = Mathf.PingPong(eTimer[E_CONVEYOR] * 3f, 1f);
            GFM_Create.SetColor(eGo[E_CONVEYOR], new Color(0.4f + t * 0.3f, 0.4f + t * 0.3f, 0.4f));

            if (eTimer[E_CONVEYOR] >= conveyorBuildTime)
            {
                eState[E_CONVEYOR] = 2;
                GFM_Create.SetColor(eGo[E_CONVEYOR], new Color(0.3f, 0.7f, 0.3f));
            }
        }
    }

    // === ENEMY SPAWNER ===
    void UpdateEnemySpawner(float dt)
    {
        if (!enemySpawnerActive) return;

        enemySpawnTimer += dt;
        if (enemySpawnTimer >= enemySpawnInterval)
        {
            enemySpawnTimer = 0f;

            for (int i = 0; i < ENEMY_COUNT; i++)
            {
                int idx = ENEMY_START + i;
                if (!eActive[idx] && eGo[idx] != null)
                {
                    float rx = Random.Range(-6f, 6f);
                    eGo[idx].transform.position = new Vector3(rx, 0.8f, 14f);
                    eActive[idx] = true;
                    eHP[idx] = 3f;
                    eState[idx] = 1;
                    break;
                }
            }
        }
    }

    // === ENEMIES ===
    void UpdateEnemies(float dt)
    {
        Vector3 basePos = new Vector3(0, 1.5f, -3);
        float enemySpeed = 2.5f;

        for (int i = 0; i < ENEMY_COUNT; i++)
        {
            int idx = ENEMY_START + i;
            if (!eActive[idx] || eGo[idx] == null) continue;

            if (eHP[idx] <= 0)
            {
                SpawnCoin(eGo[idx].transform.position);
                HideEntity(idx);
                enemyKillCount++;
                continue;
            }

            Vector3 dir = (basePos - eGo[idx].transform.position).normalized;
            eGo[idx].transform.position += dir * enemySpeed * dt;

            if (Vector3.Distance(eGo[idx].transform.position, basePos) < 2f)
            {
                baseHP -= 2f;
                HideEntity(idx);
                eHP[idx] = 0;
                enemyKillCount++;
            }
        }
    }

    // === COINS ===
    void SpawnCoin(Vector3 pos)
    {
        for (int i = 0; i < COIN_COUNT; i++)
        {
            int idx = COIN_START + i;
            if (!eActive[idx] && eGo[idx] != null)
            {
                eGo[idx].transform.position = new Vector3(pos.x, 0.3f, pos.z);
                eActive[idx] = true;
                break;
            }
        }
    }

    void UpdateCoins()
    {
        if (!eActive[E_PLAYER] || eGo[E_PLAYER] == null) return;
        Vector3 playerPos = eGo[E_PLAYER].transform.position;

        for (int i = 0; i < COIN_COUNT; i++)
        {
            int idx = COIN_START + i;
            if (!eActive[idx] || eGo[idx] == null) continue;

            float dist = Vector3.Distance(playerPos, eGo[idx].transform.position);
            if (dist < 2.5f)
            {
                gold += 1;
                HideEntity(idx);
            }
            else
            {
                Vector3 p = eGo[idx].transform.position;
                p.y = 0.3f + Mathf.Sin(gameTimer * 3f + i) * 0.15f;
                eGo[idx].transform.position = p;
            }
        }
    }

    // === CROSSBOWS ===
    void UpdateCrossbows(float dt)
    {
        if (!crossbowsActive) return;

        // Left crossbow
        if (eGo[E_CROSSBOW_L] != null && eActive[E_CROSSBOW_L])
        {
            crossbowTimerL += dt;
            if (crossbowTimerL >= crossbowFireRate)
            {
                bool fired = false;
                int target = FindNearestEnemy(eGo[E_CROSSBOW_L].transform.position, crossbowRange);
                if (target >= 0)
                {
                    SpawnArrow(eGo[E_CROSSBOW_L].transform.position, eGo[target].transform.position, crossbowDamage);
                    fired = true;
                }
                else if (eActive[E_BOSS] && bossHP > 0 && eGo[E_BOSS] != null)
                {
                    float bd = Vector3.Distance(eGo[E_CROSSBOW_L].transform.position, eGo[E_BOSS].transform.position);
                    if (bd < crossbowRange)
                    {
                        SpawnArrow(eGo[E_CROSSBOW_L].transform.position, eGo[E_BOSS].transform.position, crossbowDamage);
                        fired = true;
                    }
                }
                if (fired) crossbowTimerL = 0f;
            }
        }

        // Right crossbow
        if (eGo[E_CROSSBOW_R] != null && eActive[E_CROSSBOW_R])
        {
            crossbowTimerR += dt;
            if (crossbowTimerR >= crossbowFireRate)
            {
                bool fired = false;
                int target = FindNearestEnemy(eGo[E_CROSSBOW_R].transform.position, crossbowRange);
                if (target >= 0)
                {
                    SpawnArrow(eGo[E_CROSSBOW_R].transform.position, eGo[target].transform.position, crossbowDamage);
                    fired = true;
                }
                else if (eActive[E_BOSS] && bossHP > 0 && eGo[E_BOSS] != null)
                {
                    float bd = Vector3.Distance(eGo[E_CROSSBOW_R].transform.position, eGo[E_BOSS].transform.position);
                    if (bd < crossbowRange)
                    {
                        SpawnArrow(eGo[E_CROSSBOW_R].transform.position, eGo[E_BOSS].transform.position, crossbowDamage);
                        fired = true;
                    }
                }
                if (fired) crossbowTimerR = 0f;
            }
        }
    }

    // === TURRET ===
    void UpdateTurretLogic(float dt)
    {
        if (!eActive[E_TURRET] || eGo[E_TURRET] == null) return;

        if (eState[E_TURRET] == 0 && eGo[E_PLAYER] != null)
        {
            float dist = Vector3.Distance(eGo[E_PLAYER].transform.position, eGo[E_TURRET].transform.position);
            if (dist < 3f && wood >= 5)
            {
                wood -= 5;
                eState[E_TURRET] = 1;
                eTimer[E_TURRET] = 0f;
                ShowGuide("Building turret...");
            }
        }
        else if (eState[E_TURRET] == 1)
        {
            eTimer[E_TURRET] += dt;
            float t = Mathf.PingPong(eTimer[E_TURRET] * 3f, 1f);
            GFM_Create.SetColor(eGo[E_TURRET], new Color(0.3f + t * 0.3f, 0.3f + t * 0.3f, 0.35f));

            if (eTimer[E_TURRET] >= 1.5f)
            {
                eState[E_TURRET] = 2;
                GFM_Create.SetColor(eGo[E_TURRET], new Color(0.2f, 0.6f, 0.8f));
            }
        }

        // Auto-shoot
        if (!turretActive) return;

        turretTimer += dt;
        if (turretTimer >= turretFireRate)
        {
            bool fired = false;

            if (eActive[E_BOSS] && bossHP > 0 && eGo[E_BOSS] != null)
            {
                float bDist = Vector3.Distance(eGo[E_TURRET].transform.position, eGo[E_BOSS].transform.position);
                if (bDist < turretRange)
                {
                    SpawnArrow(eGo[E_TURRET].transform.position, eGo[E_BOSS].transform.position, turretDamage);
                    fired = true;
                }
            }

            if (!fired)
            {
                int target = FindNearestEnemy(eGo[E_TURRET].transform.position, turretRange);
                if (target >= 0)
                {
                    SpawnArrow(eGo[E_TURRET].transform.position, eGo[target].transform.position, turretDamage);
                    fired = true;
                }
            }

            if (fired) turretTimer = 0f;
        }
    }

    int FindNearestEnemy(Vector3 from, float range)
    {
        float minDist = range;
        int nearest = -1;

        for (int i = 0; i < ENEMY_COUNT; i++)
        {
            int idx = ENEMY_START + i;
            if (!eActive[idx] || eHP[idx] <= 0 || eGo[idx] == null) continue;

            float d = Vector3.Distance(from, eGo[idx].transform.position);
            if (d < minDist)
            {
                minDist = d;
                nearest = idx;
            }
        }

        return nearest;
    }

    // === ARROWS ===
    void SpawnArrow(Vector3 from, Vector3 to, float damage)
    {
        for (int i = 0; i < ARROW_COUNT; i++)
        {
            int idx = ARROW_START + i;
            if (!eActive[idx] && eGo[idx] != null)
            {
                eGo[idx].transform.position = from;
                arrowTarget[i] = to;
                arrowDmg[i] = damage;
                eActive[idx] = true;
                return;
            }
        }
    }

    void UpdateProjectiles(float dt)
    {
        for (int i = 0; i < ARROW_COUNT; i++)
        {
            int idx = ARROW_START + i;
            if (!eActive[idx] || eGo[idx] == null) continue;

            Vector3 dir = (arrowTarget[i] - eGo[idx].transform.position);
            float dist = dir.magnitude;

            if (dist < 0.5f)
            {
                DamageAtPoint(arrowTarget[i], arrowDmg[i]);
                HideEntity(idx);
                continue;
            }

            Vector3 normDir = dir / dist;
            Vector3 newPos = eGo[idx].transform.position + normDir * arrowSpeed * dt;
            eGo[idx].transform.position = newPos;

            if (normDir.sqrMagnitude > 0.001f)
            {
                eGo[idx].transform.forward = normDir;
            }

            if (newPos.magnitude > 60f)
            {
                HideEntity(idx);
            }
        }
    }

    void DamageAtPoint(Vector3 point, float damage)
    {
        float bestDist = 2.5f;
        int bestEnemy = -1;

        for (int i = 0; i < ENEMY_COUNT; i++)
        {
            int idx = ENEMY_START + i;
            if (!eActive[idx] || eHP[idx] <= 0 || eGo[idx] == null) continue;

            float d = Vector3.Distance(point, eGo[idx].transform.position);
            if (d < bestDist)
            {
                bestDist = d;
                bestEnemy = i;
            }
        }

        if (bestEnemy >= 0)
        {
            int idx = ENEMY_START + bestEnemy;
            eHP[idx] -= damage;
            if (eGo[idx] != null)
            {
                GFM_Create.SetColor(eGo[idx], new Color(1f, 0.6f, 0.6f));
            }
            enemyFlashTimer[bestEnemy] = 0.15f;
            return;
        }

        if (eActive[E_BOSS] && bossHP > 0 && eGo[E_BOSS] != null)
        {
            float d = Vector3.Distance(point, eGo[E_BOSS].transform.position);
            if (d < 3f)
            {
                bossHP -= damage;
                GFM_Create.SetColor(eGo[E_BOSS], new Color(1f, 0.5f, 0.5f));
                bossFlashTimer = 0.15f;
            }
        }
    }

    void UpdateDamageFlash(float dt)
    {
        for (int i = 0; i < ENEMY_COUNT; i++)
        {
            if (enemyFlashTimer[i] > 0)
            {
                enemyFlashTimer[i] -= dt;
                if (enemyFlashTimer[i] <= 0)
                {
                    int idx = ENEMY_START + i;
                    if (eActive[idx] && eHP[idx] > 0 && eGo[idx] != null)
                    {
                        GFM_Create.SetColor(eGo[idx], new Color(0.8f, 0.2f, 0.2f));
                    }
                }
            }
        }

        if (bossFlashTimer > 0)
        {
            bossFlashTimer -= dt;
            if (bossFlashTimer <= 0 && eActive[E_BOSS] && bossHP > 0 && eGo[E_BOSS] != null)
            {
                GFM_Create.SetColor(eGo[E_BOSS], new Color(0.8f, 0.1f, 0.1f));
            }
        }
    }

    // === WOOD SPAWNER ===
    void UpdateWoodSpawner(float dt)
    {
        if (!woodSpawnerActive) return;

        woodSpawnTimer += dt;
        if (woodSpawnTimer >= woodSpawnInterval)
        {
            woodSpawnTimer = 0f;

            int activeWood = 0;
            for (int i = 0; i < WOOD_COUNT; i++)
            {
                if (eActive[WOOD_START + i]) activeWood++;
            }
            if (activeWood >= 4) return;

            for (int i = 0; i < WOOD_COUNT; i++)
            {
                int idx = WOOD_START + i;
                if (!eActive[idx] && eGo[idx] != null)
                {
                    float rx = Random.Range(-5f, 5f);
                    float rz = Random.Range(1f, 7f);
                    eGo[idx].transform.position = new Vector3(rx, 0.4f, rz);
                    eActive[idx] = true;
                    eState[idx] = 0;
                    break;
                }
            }
        }
    }

    // === WOOD COLLECT ===
    void UpdateWoodCollect()
    {
        if (!eActive[E_PLAYER] || eGo[E_PLAYER] == null) return;
        Vector3 playerPos = eGo[E_PLAYER].transform.position;

        for (int i = 0; i < WOOD_COUNT; i++)
        {
            int idx = WOOD_START + i;
            if (!eActive[idx] || eState[idx] != 0 || eGo[idx] == null) continue;

            float dist = Vector3.Distance(playerPos, eGo[idx].transform.position);
            if (dist < 2f)
            {
                wood += 1;
                HideEntity(idx);
            }
        }
    }

    // === WOOD HOUSE BUILD ===
    void UpdateWoodHouseBuild(float dt)
    {
        if (!eActive[E_WOODHOUSE] || eGo[E_WOODHOUSE] == null) return;

        if (eState[E_WOODHOUSE] == 0)
        {
            if (wood >= 3 && eGo[E_PLAYER] != null)
            {
                float dist = Vector3.Distance(eGo[E_PLAYER].transform.position, eGo[E_WOODHOUSE].transform.position);
                if (dist < 5f)
                {
                    wood -= 3;
                    eState[E_WOODHOUSE] = 1;
                    eTimer[E_WOODHOUSE] = 0f;
                    ShowGuide("Building Wood House...");
                }
            }
        }
        else if (eState[E_WOODHOUSE] == 1)
        {
            eTimer[E_WOODHOUSE] += dt;
            float t = Mathf.PingPong(eTimer[E_WOODHOUSE] * 3f, 1f);
            GFM_Create.SetColor(eGo[E_WOODHOUSE], new Color(0.65f + t * 0.2f, 0.45f + t * 0.2f, 0.25f));

            if (eTimer[E_WOODHOUSE] >= 1.5f)
            {
                eState[E_WOODHOUSE] = 2;
                GFM_Create.SetColor(eGo[E_WOODHOUSE], new Color(0.5f, 0.7f, 0.3f));
                ShowGuide("Wood House built!");
            }
        }
    }

    // === WORKERS ===
    void SpawnWorker()
    {
        for (int i = 0; i < WORKER_COUNT; i++)
        {
            int idx = WORKER_START + i;
            if (!eActive[idx] && eGo[idx] != null)
            {
                eGo[idx].transform.position = new Vector3(5, 0.75f, -2);
                eActive[idx] = true;
                workerState_arr[i] = 0;
                workerCount++;
                break;
            }
        }
    }

    void UpdateWorkers(float dt)
    {
        for (int i = 0; i < WORKER_COUNT; i++)
        {
            int idx = WORKER_START + i;
            if (!eActive[idx] || eGo[idx] == null) continue;

            Vector3 pos = eGo[idx].transform.position;

            if (workerState_arr[i] == 0)
            {
                int nearestWood = -1;
                float minDist = 999f;
                for (int w = 0; w < WOOD_COUNT; w++)
                {
                    int widx = WOOD_START + w;
                    if (!eActive[widx] || eState[widx] != 0 || eGo[widx] == null) continue;
                    float d = Vector3.Distance(pos, eGo[widx].transform.position);
                    if (d < minDist)
                    {
                        minDist = d;
                        nearestWood = w;
                    }
                }

                if (nearestWood >= 0)
                {
                    workerTargetWood[i] = nearestWood;
                    workerState_arr[i] = 1;
                }
            }
            else if (workerState_arr[i] == 1)
            {
                int w = workerTargetWood[i];
                if (w < 0 || w >= WOOD_COUNT)
                {
                    workerState_arr[i] = 0;
                    continue;
                }
                int widx = WOOD_START + w;

                if (!eActive[widx] || eState[widx] != 0 || eGo[widx] == null)
                {
                    workerState_arr[i] = 0;
                    continue;
                }

                Vector3 target = eGo[widx].transform.position;
                Vector3 dir = (target - pos).normalized;
                eGo[idx].transform.position += dir * workerSpeed * dt;

                if (Vector3.Distance(eGo[idx].transform.position, target) < 1f)
                {
                    HideEntity(widx);
                    workerState_arr[i] = 2;
                }
            }
            else if (workerState_arr[i] == 2)
            {
                Vector3 target = new Vector3(5, 0.75f, -2);
                if (eActive[E_WOODHOUSE] && eGo[E_WOODHOUSE] != null)
                {
                    target = eGo[E_WOODHOUSE].transform.position;
                    target.y = 0.75f;
                }

                Vector3 dir = (target - eGo[idx].transform.position).normalized;
                eGo[idx].transform.position += dir * workerSpeed * dt;

                if (Vector3.Distance(eGo[idx].transform.position, target) < 1.5f)
                {
                    wood += 1;
                    workerState_arr[i] = 0;
                }
            }
        }
    }

    // === BOSS ===
    void UpdateBoss(float dt)
    {
        if (!eActive[E_BOSS] || eGo[E_BOSS] == null) return;

        if (bossHP <= 0 && !bossDead)
        {
            bossDead = true;
            Vector3 bossPos = eGo[E_BOSS].transform.position;
            HideEntity(E_BOSS);
            for (int i = 0; i < 3; i++)
            {
                SpawnCoin(bossPos + new Vector3(Random.Range(-2f, 2f), 0, Random.Range(-1f, 1f)));
            }
            return;
        }

        if (bossDead) return;

        Vector3 basePos = new Vector3(0, 1.5f, -3);
        Vector3 dir = (basePos - eGo[E_BOSS].transform.position).normalized;
        eGo[E_BOSS].transform.position += dir * 1.5f * dt;

        // Boss pulsing scale for visual effect
        float bossPulse = 1f + 0.05f * Mathf.Sin(gameTimer * 4f);
        eGo[E_BOSS].transform.localScale = new Vector3(1.5f * bossPulse, 3f * bossPulse, 1.5f * bossPulse);

        if (Vector3.Distance(eGo[E_BOSS].transform.position, basePos) < 2.5f)
        {
            baseHP -= 5f * dt;
        }
    }

    // === HEALTH BAR ===
    void UpdateHealthBar()
    {
        if (!eActive[E_HEALTHBAR] || !eActive[E_BASE] || eGo[E_HEALTHBAR] == null) return;

        float ratio = Mathf.Clamp01(baseHP / baseMaxHP);
        eGo[E_HEALTHBAR].transform.localScale = new Vector3(3f * ratio, 0.2f, 0.2f);

        Color hpColor = Color.Lerp(new Color(0.8f, 0.1f, 0.1f), new Color(0.1f, 0.8f, 0.1f), ratio);
        GFM_Create.SetColor(eGo[E_HEALTHBAR], hpColor);

        eGo[E_HEALTHBAR].transform.position = new Vector3(0, 3.5f, -3);

        if (baseHP <= 0 && !gameEnded)
        {
            gameEnded = true;
            ShowGuide("Base destroyed! Download to retry!");
            ShowCTA();
            Luna.Unity.LifeCycle.GameEnded();
        }
    }

    // === UI ===
    void UpdateUI()
    {
        if (goldText != null)
            goldText.text = "Gold: " + gold;
        if (woodText != null)
            woodText.text = "Wood: " + wood;
    }

    void ShowGuide(string msg)
    {
        if (guideText != null)
        {
            guideText.text = msg;
            guideFlashTimer = 0f;
        }
    }

    void UpdateGuideFlash(float dt)
    {
        if (guideText == null) return;
        guideFlashTimer += dt;
        float alpha = 0.7f + 0.3f * Mathf.Sin(guideFlashTimer * 3f);
        guideText.color = new Color(1f, 1f, 1f, alpha);
    }

    void ShowCTA()
    {
        if (ctaShown) return;
        ctaShown = true;

        if (canvas == null) return;

        GameObject canvasGO = canvas.gameObject;
        RectTransform canvasRect = (RectTransform)canvasGO.GetComponent(typeof(RectTransform));
        if (canvasRect == null) return;

        ctaButtonObj = new GameObject("CTAButton");
        RectTransform btnRect = (RectTransform)ctaButtonObj.AddComponent(typeof(RectTransform));
        Image btnImg = (Image)ctaButtonObj.AddComponent(typeof(Image));
        btnImg.color = new Color(0.2f, 0.8f, 0.3f);
        Button btn = (Button)ctaButtonObj.AddComponent(typeof(Button));
        btn.onClick.AddListener(OnCTAClick);
        btnRect.SetParent(canvasRect, false);
        btnRect.anchorMin = new Vector2(0.5f, 0.5f);
        btnRect.anchorMax = new Vector2(0.5f, 0.5f);
        btnRect.anchoredPosition = new Vector2(0, -30);
        btnRect.sizeDelta = new Vector2(300, 80);

        GameObject btnTextObj = new GameObject("CTAText");
        RectTransform txtRect = (RectTransform)btnTextObj.AddComponent(typeof(RectTransform));
        Text btnText = (Text)btnTextObj.AddComponent(typeof(Text));
        btnText.font = (Font)Resources.GetBuiltinResource(typeof(Font), "Arial.ttf");
        btnText.fontSize = 32;
        btnText.alignment = TextAnchor.MiddleCenter;
        btnText.color = Color.white;
        btnText.text = "DOWNLOAD NOW!";
        txtRect.SetParent(btnRect, false);
        txtRect.anchorMin = Vector2.zero;
        txtRect.anchorMax = Vector2.one;
        txtRect.offsetMin = Vector2.zero;
        txtRect.offsetMax = Vector2.zero;

        ctaPulseTimer = 0f;
    }

    void OnCTAClick()
    {
        Luna.Unity.Playable.InstallFullGame();
    }
}
