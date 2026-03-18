using UnityEngine;
using UnityEngine.UI;

public class GameFlowManagerMain : MonoBehaviour
{
    // ============================================================
    // ENTITY INDEX CONSTANTS
    // ============================================================
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
    const int E_ENEMY_SPAWNER = 11;
    const int E_WOODLOG_SPAWNER = 12;
    const int E_WOODHOUSE = 13;
    const int E_WORKER_SPAWNER = 14;
    const int E_TURRET = 15;
    const int E_BOSS = 16;
    const int E_CASTLEWALL = 17;

    const int ENTITY_COUNT = 18;

    // Object pools
    const int MAX_ARROWS = 20;
    const int MAX_ENEMIES = 10;
    const int MAX_COINS = 10;
    const int MAX_WOODLOGS = 6;
    const int MAX_WORKERS = 4;

    // Rule count
    const int RULE_COUNT = 10;

    // ============================================================
    // PARALLEL ARRAYS - ENTITIES
    // ============================================================
    GameObject[] eGo = new GameObject[ENTITY_COUNT];
    bool[] eActive = new bool[ENTITY_COUNT];
    int[] eState = new int[ENTITY_COUNT];
    float[] eTimer = new float[ENTITY_COUNT];
    float[] eHP = new float[ENTITY_COUNT];

    // ============================================================
    // OBJECT POOLS
    // ============================================================
    GameObject[] arrowGo = new GameObject[MAX_ARROWS];
    bool[] arrowActive = new bool[MAX_ARROWS];
    Vector3[] arrowTarget = new Vector3[MAX_ARROWS];
    float[] arrowLife = new float[MAX_ARROWS];
    float[] arrowDmg = new float[MAX_ARROWS];

    GameObject[] enemyGo = new GameObject[MAX_ENEMIES];
    bool[] enemyActive = new bool[MAX_ENEMIES];
    float[] enemyHP = new float[MAX_ENEMIES];
    int[] enemyState = new int[MAX_ENEMIES];

    GameObject[] coinGo = new GameObject[MAX_COINS];
    bool[] coinActive = new bool[MAX_COINS];
    float[] coinTimer = new float[MAX_COINS];

    GameObject[] woodGo = new GameObject[MAX_WOODLOGS];
    bool[] woodActive = new bool[MAX_WOODLOGS];
    int[] woodState = new int[MAX_WOODLOGS];
    int[] woodCarrier = new int[MAX_WOODLOGS];

    GameObject[] workerGo = new GameObject[MAX_WORKERS];
    bool[] workerActive = new bool[MAX_WORKERS];
    int[] workerState = new int[MAX_WORKERS];
    int[] workerTargetLog = new int[MAX_WORKERS];

    // ============================================================
    // RULE TRACKING
    // ============================================================
    bool[] ruleTriggered = new bool[RULE_COUNT];

    // ============================================================
    // RESOURCES & COUNTERS
    // ============================================================
    int gold = 1;
    int wood = 0;
    int enemyKillCount = 0;
    int baseLevel = 1;

    // ============================================================
    // GAMEPLAY PARAMETERS
    // ============================================================
    float playerSpeed = 5f;
    float enemySpeed = 2f;
    float bossSpeed = 1.5f;
    float workerSpeed = 3f;
    float arrowSpeed = 12f;

    float conveyorBuildTime = 1.5f;
    float woodHouseBuildTime = 2f;
    float turretBuildTime = 2f;

    float crossbowFireRate = 1.5f;
    float turretFireRate = 2f;
    float crossbowRange = 10f;
    float turretRange = 12f;

    float enemySpawnInterval = 3f;
    int enemyMaxAlive = 5;
    float woodLogSpawnInterval = 4f;
    int woodLogMaxAlive = 3;
    float workerSpawnInterval = 5f;
    int workerMaxAlive = 2;

    // ============================================================
    // UI & INPUT
    // ============================================================
    Canvas canvas;
    GFM_Joystick joystick;
    Text guideText;
    Text goldText;
    Text woodText;
    Text hpText;
    GameObject healthBarGo;
    GameObject healthBarBgGo;

    // Blueprint UI
    Text blueprintCostText;
    GameObject blueprintRingGo;
    GameObject blueprintIndicatorGo;

    // Auto-play
    float noInputTimer = 0f;
    bool autoPlay = false;
    float autoPlayDelay = 2f;

    // Camera
    Camera mainCam;
    Vector3 camLookTarget = Vector3.zero;
    float camZoom = 8f;

    // Conveyor blueprint flicker
    float blueprintFlicker = 0f;

    // Base position cache
    Vector3 basePos = new Vector3(0, 1.5f, -3);

    // Click detection
    bool clickedThisFrame = false;
    Vector3 clickWorldPos = Vector3.zero;

    // CTA
    float ctaTimer = 0f;
    bool ctaActive = false;
    bool gameEnded = false;

    // Initialization flag
    bool initialized = false;

    // Decoration refs
    GameObject[] decoGo = new GameObject[30];

    // Guide arrow indicator
    GameObject guideArrowGo;
    Vector3 guideArrowTarget = Vector3.zero;
    bool showGuideArrow = false;

    // Global game timer
    float globalTimer = 0f;

    // Touch/tap tracking
    bool wasTouching = false;
    float tapCooldown = 0f;

    // Blueprint positions
    Vector3 conveyorPos = new Vector3(0, 0.15f, 2);
    Vector3 turretPos = new Vector3(0, 1f, 7);
    Vector3 woodHousePos = new Vector3(5, 1.25f, -2);

    // Track if player has ever given joystick input
    bool playerEverMoved = false;

    // Rule timers
    float[] ruleSinceTimer = new float[RULE_COUNT];

    // ============================================================
    // START
    // ============================================================
    void Start()
    {
        // Scene cleanup
        try
        {
            GameObject[] allRoot = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
            for (int i = 0; i < allRoot.Length; i++)
            {
                string n = allRoot[i].name;
                if (n != "Main Camera" && n != "Directional Light" && n != "EventSystem" && n != "GameManager" && n != "__MaterialSource")
                {
                    Destroy(allRoot[i]);
                }
            }
        }
        catch (System.Exception) { }

        GFM_Create.ResetPool();
        GFM_Create.InitMaterialFromScene();

        // Camera setup
        mainCam = Camera.main;
        if (mainCam != null)
        {
            mainCam.orthographic = true;
            mainCam.orthographicSize = 8f;
            mainCam.clearFlags = CameraClearFlags.SolidColor;
            mainCam.backgroundColor = new Color(0.5f, 0.8f, 1f);
            mainCam.transform.rotation = Quaternion.Euler(45f, 0f, 0f);
            Vector3 lookAt = new Vector3(0f, 0f, 0f);
            float dist = 20f;
            mainCam.transform.position = lookAt + new Vector3(0f, dist * 0.707f, -dist * 0.707f);
        }

        // Initialize all entity states
        for (int i = 0; i < ENTITY_COUNT; i++)
        {
            eActive[i] = false;
            eState[i] = 0;
            eTimer[i] = 0f;
            eHP[i] = 0f;
        }

        for (int i = 0; i < RULE_COUNT; i++)
        {
            ruleSinceTimer[i] = 0f;
        }

        // ============================================================
        // CREATE ALL ENTITIES
        // ============================================================

        // 1. Ground
        eGo[E_GROUND] = GFM_Create.Ground(40f, 40f);
        if (eGo[E_GROUND] != null)
        {
            GFM_Create.SetColor(eGo[E_GROUND], new Color(0.35f, 0.25f, 0.15f));
            eGo[E_GROUND].transform.position = new Vector3(0, -0.5f, 0);
        }

        // 2. Player - original position
        eGo[E_PLAYER] = GFM_Create.Obj(PrimitiveType.Cube, new Vector3(-2, 1f, -4), new Vector3(1, 2, 1), "Player");
        if (eGo[E_PLAYER] != null)
            GFM_Create.SetColor(eGo[E_PLAYER], new Color(0.2f, 0.4f, 0.9f));

        // 3. Base
        eGo[E_BASE] = GFM_Create.Obj(PrimitiveType.Cube, new Vector3(0, 1.5f, -3), new Vector3(3, 3, 3), "Base");
        if (eGo[E_BASE] != null)
            GFM_Create.SetColor(eGo[E_BASE], new Color(0.85f, 0.7f, 0.4f));
        eHP[E_BASE] = 50f;

        // 4. WoodFence_L
        eGo[E_WOODFENCE_L] = GFM_Create.Obj(PrimitiveType.Cube, new Vector3(-4, 0.75f, 0), new Vector3(0.3f, 1.5f, 4), "WoodFence_L");
        if (eGo[E_WOODFENCE_L] != null)
            GFM_Create.SetColor(eGo[E_WOODFENCE_L], new Color(0.6f, 0.35f, 0.1f));

        // 5. WoodFence_R
        eGo[E_WOODFENCE_R] = GFM_Create.Obj(PrimitiveType.Cube, new Vector3(4, 0.75f, 0), new Vector3(0.3f, 1.5f, 4), "WoodFence_R");
        if (eGo[E_WOODFENCE_R] != null)
            GFM_Create.SetColor(eGo[E_WOODFENCE_R], new Color(0.6f, 0.35f, 0.1f));

        // 6. PineTree_L
        eGo[E_PINETREE_L] = GFM_Create.Obj(PrimitiveType.Cylinder, new Vector3(-5, 1.5f, 3), new Vector3(0.5f, 3f, 0.5f), "PineTree_L");
        if (eGo[E_PINETREE_L] != null)
            GFM_Create.SetColor(eGo[E_PINETREE_L], new Color(0.1f, 0.55f, 0.1f));

        // 7. PineTree_R
        eGo[E_PINETREE_R] = GFM_Create.Obj(PrimitiveType.Cylinder, new Vector3(5, 1.5f, 3), new Vector3(0.5f, 3f, 0.5f), "PineTree_R");
        if (eGo[E_PINETREE_R] != null)
            GFM_Create.SetColor(eGo[E_PINETREE_R], new Color(0.1f, 0.55f, 0.1f));

        // 8. Generator
        eGo[E_GENERATOR] = GFM_Create.Obj(PrimitiveType.Cube, new Vector3(-3, 1f, 2), new Vector3(2, 2, 2), "Generator");
        if (eGo[E_GENERATOR] != null)
            GFM_Create.SetColor(eGo[E_GENERATOR], new Color(0.5f, 0.5f, 0.55f));

        // 9. ConveyorBelt (blueprint - visible from start)
        eGo[E_CONVEYOR] = GFM_Create.Obj(PrimitiveType.Cube, conveyorPos, new Vector3(4, 0.3f, 1), "ConveyorBelt");
        if (eGo[E_CONVEYOR] != null)
            GFM_Create.SetColor(eGo[E_CONVEYOR], new Color(0.2f, 1f, 0.2f));

        // Blueprint ring around conveyor
        blueprintRingGo = GFM_Create.Obj(PrimitiveType.Cylinder, new Vector3(conveyorPos.x, 0.02f, conveyorPos.z), new Vector3(5f, 0.02f, 5f), "BlueprintRing");
        if (blueprintRingGo != null)
            GFM_Create.SetColor(blueprintRingGo, new Color(0.1f, 1f, 0.1f));

        // Blueprint cost icon (gold coin above conveyor)
        blueprintIndicatorGo = GFM_Create.Obj(PrimitiveType.Sphere, new Vector3(conveyorPos.x, 2.5f, conveyorPos.z), new Vector3(0.8f, 0.8f, 0.8f), "BlueprintCostIcon");
        if (blueprintIndicatorGo != null)
            GFM_Create.SetColor(blueprintIndicatorGo, new Color(1f, 0.85f, 0f));

        // 10. Crossbow_L (hidden initially)
        eGo[E_CROSSBOW_L] = GFM_Create.Obj(PrimitiveType.Cube, new Vector3(0, -999, 0), new Vector3(1, 1.5f, 1), "Crossbow_L");
        if (eGo[E_CROSSBOW_L] != null)
            GFM_Create.SetColor(eGo[E_CROSSBOW_L], new Color(0.5f, 0.5f, 0.55f));

        // 11. Crossbow_R (hidden initially)
        eGo[E_CROSSBOW_R] = GFM_Create.Obj(PrimitiveType.Cube, new Vector3(0, -999, 0), new Vector3(1, 1.5f, 1), "Crossbow_R");
        if (eGo[E_CROSSBOW_R] != null)
            GFM_Create.SetColor(eGo[E_CROSSBOW_R], new Color(0.5f, 0.5f, 0.55f));

        // 12. Arrow pool
        for (int i = 0; i < MAX_ARROWS; i++)
        {
            arrowGo[i] = GFM_Create.Obj(PrimitiveType.Cylinder, new Vector3(0, -999, 0), new Vector3(0.05f, 0.5f, 0.05f), "Arrow_" + i);
            if (arrowGo[i] != null)
                GFM_Create.SetColor(arrowGo[i], new Color(0.6f, 0.35f, 0.1f));
            arrowActive[i] = false;
            arrowTarget[i] = Vector3.zero;
            arrowLife[i] = 0f;
            arrowDmg[i] = 1f;
        }

        // 13. Enemy pool
        for (int i = 0; i < MAX_ENEMIES; i++)
        {
            enemyGo[i] = GFM_Create.Obj(PrimitiveType.Cube, new Vector3(0, -999, 0), new Vector3(0.8f, 1.6f, 0.8f), "Enemy_" + i);
            if (enemyGo[i] != null)
                GFM_Create.SetColor(enemyGo[i], new Color(0.85f, 0.15f, 0.15f));
            enemyActive[i] = false;
            enemyHP[i] = 0f;
            enemyState[i] = 0;
        }

        // 14. Gold coin pool
        for (int i = 0; i < MAX_COINS; i++)
        {
            coinGo[i] = GFM_Create.Obj(PrimitiveType.Sphere, new Vector3(0, -999, 0), new Vector3(0.3f, 0.3f, 0.3f), "GoldCoin_" + i);
            if (coinGo[i] != null)
                GFM_Create.SetColor(coinGo[i], new Color(1f, 0.85f, 0f));
            coinActive[i] = false;
            coinTimer[i] = 0f;
        }

        // 15. Wood log pool
        for (int i = 0; i < MAX_WOODLOGS; i++)
        {
            woodGo[i] = GFM_Create.Obj(PrimitiveType.Cylinder, new Vector3(0, -999, 0), new Vector3(0.3f, 0.8f, 0.3f), "WoodLog_" + i);
            if (woodGo[i] != null)
                GFM_Create.SetColor(woodGo[i], new Color(0.6f, 0.35f, 0.1f));
            woodActive[i] = false;
            woodState[i] = 0;
            woodCarrier[i] = -1;
        }

        // 16. WoodHouse (hidden)
        eGo[E_WOODHOUSE] = GFM_Create.Obj(PrimitiveType.Cube, new Vector3(0, -999, 0), new Vector3(3, 2.5f, 3), "WoodHouse");
        if (eGo[E_WOODHOUSE] != null)
            GFM_Create.SetColor(eGo[E_WOODHOUSE], new Color(0.85f, 0.7f, 0.4f));

        // 17. Worker pool
        for (int i = 0; i < MAX_WORKERS; i++)
        {
            workerGo[i] = GFM_Create.Obj(PrimitiveType.Cube, new Vector3(0, -999, 0), new Vector3(0.8f, 1.5f, 0.8f), "Worker_" + i);
            if (workerGo[i] != null)
                GFM_Create.SetColor(workerGo[i], new Color(0.9f, 0.6f, 0.2f));
            workerActive[i] = false;
            workerState[i] = 0;
            workerTargetLog[i] = -1;
        }

        // 18. Turret (hidden)
        eGo[E_TURRET] = GFM_Create.Obj(PrimitiveType.Cube, new Vector3(0, -999, 0), new Vector3(1.5f, 2, 1.5f), "Turret");
        if (eGo[E_TURRET] != null)
            GFM_Create.SetColor(eGo[E_TURRET], new Color(0.5f, 0.5f, 0.55f));

        // 19. Boss (hidden)
        eGo[E_BOSS] = GFM_Create.Obj(PrimitiveType.Cube, new Vector3(0, -999, 0), new Vector3(1.5f, 3, 1.5f), "Boss");
        if (eGo[E_BOSS] != null)
            GFM_Create.SetColor(eGo[E_BOSS], new Color(0.7f, 0.1f, 0.1f));
        eHP[E_BOSS] = 20f;

        // 20. CastleWall (hidden)
        eGo[E_CASTLEWALL] = GFM_Create.Obj(PrimitiveType.Cube, new Vector3(0, -999, 0), new Vector3(5, 4, 0.5f), "CastleWall");
        if (eGo[E_CASTLEWALL] != null)
            GFM_Create.SetColor(eGo[E_CASTLEWALL], new Color(0.7f, 0.7f, 0.75f));

        // Guide arrow
        guideArrowGo = GFM_Create.Obj(PrimitiveType.Cube, new Vector3(0, -999, 0), new Vector3(0.8f, 0.8f, 0.8f), "GuideArrow");
        if (guideArrowGo != null)
            GFM_Create.SetColor(guideArrowGo, new Color(1f, 1f, 0f));

        // ============================================================
        // DECORATIONS
        // ============================================================
        CreateDecorations();

        // ============================================================
        // UI SETUP
        // ============================================================
        try
        {
            canvas = GFM_UI.CreateCanvas(960, 540);

            // 21. HealthBar_UI
            healthBarBgGo = GFM_Create.Obj(PrimitiveType.Cube, new Vector3(0, 4.5f, -3), new Vector3(3.2f, 0.3f, 0.12f), "HealthBar_BG");
            if (healthBarBgGo != null)
                GFM_Create.SetColor(healthBarBgGo, new Color(0.3f, 0.0f, 0.0f));

            healthBarGo = GFM_Create.Obj(PrimitiveType.Cube, new Vector3(0, 4.5f, -3), new Vector3(3, 0.2f, 0.1f), "HealthBar_UI");
            if (healthBarGo != null)
                GFM_Create.SetColor(healthBarGo, new Color(0.1f, 0.8f, 0.1f));

            if (canvas != null)
            {
                guideText = GFM_UI.CreateText(canvas, "Move to the green blueprint!", new Vector2(0, 230), 36);
                if (guideText != null)
                {
                    guideText.color = Color.white;
                    guideText.alignment = TextAnchor.MiddleCenter;
                }

                goldText = GFM_UI.CreateText(canvas, "Gold: 1", new Vector2(-350, 250), 26);
                if (goldText != null) goldText.color = new Color(1f, 0.85f, 0f);

                woodText = GFM_UI.CreateText(canvas, "Wood: 0", new Vector2(-350, 220), 26);
                if (woodText != null) woodText.color = new Color(0.6f, 0.35f, 0.1f);

                hpText = GFM_UI.CreateText(canvas, "Base HP: 50", new Vector2(-350, 190), 26);
                if (hpText != null) hpText.color = new Color(0.1f, 0.8f, 0.1f);

                blueprintCostText = GFM_UI.CreateText(canvas, "", new Vector2(0, 190), 30);
                if (blueprintCostText != null)
                {
                    blueprintCostText.color = new Color(1f, 1f, 0.3f);
                    blueprintCostText.alignment = TextAnchor.MiddleCenter;
                }

                joystick = GFM_Joystick.Create(canvas, 200f);
            }
        }
        catch (System.Exception) { }

        camLookTarget = conveyorPos;
        camZoom = 8f;

        initialized = true;
    }

    // ============================================================
    // DECORATIONS
    // ============================================================
    void CreateDecorations()
    {
        int d = 0;

        decoGo[d] = GFM_Create.Obj(PrimitiveType.Cube, new Vector3(-8, 0.01f, -7), new Vector3(6, 0.02f, 6), "GrassPatch1");
        if (decoGo[d] != null) GFM_Create.SetColor(decoGo[d], new Color(0.25f, 0.4f, 0.12f));
        d++;

        decoGo[d] = GFM_Create.Obj(PrimitiveType.Cube, new Vector3(8, 0.01f, -7), new Vector3(6, 0.02f, 6), "GrassPatch2");
        if (decoGo[d] != null) GFM_Create.SetColor(decoGo[d], new Color(0.3f, 0.42f, 0.15f));
        d++;

        decoGo[d] = GFM_Create.Obj(PrimitiveType.Cube, new Vector3(-8, 0.01f, 7), new Vector3(6, 0.02f, 6), "GrassPatch3");
        if (decoGo[d] != null) GFM_Create.SetColor(decoGo[d], new Color(0.28f, 0.38f, 0.1f));
        d++;

        decoGo[d] = GFM_Create.Obj(PrimitiveType.Cube, new Vector3(8, 0.01f, 7), new Vector3(6, 0.02f, 6), "GrassPatch4");
        if (decoGo[d] != null) GFM_Create.SetColor(decoGo[d], new Color(0.32f, 0.45f, 0.18f));
        d++;

        decoGo[d] = GFM_Create.Obj(PrimitiveType.Sphere, new Vector3(-7, 0.4f, -5), new Vector3(1f, 0.7f, 1f), "Rock1");
        if (decoGo[d] != null) GFM_Create.SetColor(decoGo[d], new Color(0.5f, 0.5f, 0.5f));
        d++;

        decoGo[d] = GFM_Create.Obj(PrimitiveType.Sphere, new Vector3(7, 0.5f, -6), new Vector3(1.2f, 0.8f, 1f), "Rock2");
        if (decoGo[d] != null) GFM_Create.SetColor(decoGo[d], new Color(0.55f, 0.5f, 0.45f));
        d++;

        decoGo[d] = GFM_Create.Obj(PrimitiveType.Sphere, new Vector3(-9, 0.35f, 6), new Vector3(0.8f, 0.5f, 0.8f), "Rock3");
        if (decoGo[d] != null) GFM_Create.SetColor(decoGo[d], new Color(0.45f, 0.45f, 0.4f));
        d++;

        decoGo[d] = GFM_Create.Obj(PrimitiveType.Sphere, new Vector3(9, 0.4f, 8), new Vector3(1f, 0.6f, 0.9f), "Rock4");
        if (decoGo[d] != null) GFM_Create.SetColor(decoGo[d], new Color(0.5f, 0.48f, 0.45f));
        d++;

        decoGo[d] = GFM_Create.Obj(PrimitiveType.Sphere, new Vector3(-6, 0.6f, -2), new Vector3(1.5f, 1.2f, 1.5f), "Bush1");
        if (decoGo[d] != null) GFM_Create.SetColor(decoGo[d], new Color(0.15f, 0.5f, 0.15f));
        d++;

        decoGo[d] = GFM_Create.Obj(PrimitiveType.Sphere, new Vector3(7, 0.6f, -3), new Vector3(1.3f, 1f, 1.3f), "Bush2");
        if (decoGo[d] != null) GFM_Create.SetColor(decoGo[d], new Color(0.2f, 0.55f, 0.15f));
        d++;

        decoGo[d] = GFM_Create.Obj(PrimitiveType.Sphere, new Vector3(-7, 0.5f, 5), new Vector3(1.2f, 0.9f, 1.2f), "Bush3");
        if (decoGo[d] != null) GFM_Create.SetColor(decoGo[d], new Color(0.18f, 0.52f, 0.12f));
        d++;

        decoGo[d] = GFM_Create.Obj(PrimitiveType.Sphere, new Vector3(7, 0.5f, 6), new Vector3(1.1f, 0.85f, 1.1f), "Bush4");
        if (decoGo[d] != null) GFM_Create.SetColor(decoGo[d], new Color(0.22f, 0.58f, 0.18f));
        d++;

        decoGo[d] = GFM_Create.Obj(PrimitiveType.Cylinder, new Vector3(-9, 2f, 9), new Vector3(0.6f, 3.5f, 0.6f), "PineTree_3");
        if (decoGo[d] != null) GFM_Create.SetColor(decoGo[d], new Color(0.12f, 0.5f, 0.12f));
        d++;

        decoGo[d] = GFM_Create.Obj(PrimitiveType.Cylinder, new Vector3(9, 2f, 9), new Vector3(0.55f, 3.2f, 0.55f), "PineTree_4");
        if (decoGo[d] != null) GFM_Create.SetColor(decoGo[d], new Color(0.08f, 0.48f, 0.08f));
        d++;

        decoGo[d] = GFM_Create.Obj(PrimitiveType.Cylinder, new Vector3(-9, 1.8f, -8), new Vector3(0.5f, 3f, 0.5f), "PineTree_5");
        if (decoGo[d] != null) GFM_Create.SetColor(decoGo[d], new Color(0.1f, 0.45f, 0.1f));
        d++;

        decoGo[d] = GFM_Create.Obj(PrimitiveType.Cylinder, new Vector3(9, 1.8f, -8), new Vector3(0.5f, 3f, 0.5f), "PineTree_6");
        if (decoGo[d] != null) GFM_Create.SetColor(decoGo[d], new Color(0.14f, 0.52f, 0.14f));
        d++;

        decoGo[d] = GFM_Create.Obj(PrimitiveType.Cube, new Vector3(-1, 0.06f, -1), new Vector3(1f, 0.12f, 1f), "Path1");
        if (decoGo[d] != null) GFM_Create.SetColor(decoGo[d], new Color(0.4f, 0.3f, 0.2f));
        d++;

        decoGo[d] = GFM_Create.Obj(PrimitiveType.Cube, new Vector3(1, 0.06f, 0), new Vector3(1f, 0.12f, 1f), "Path2");
        if (decoGo[d] != null) GFM_Create.SetColor(decoGo[d], new Color(0.42f, 0.32f, 0.22f));
        d++;

        decoGo[d] = GFM_Create.Obj(PrimitiveType.Cube, new Vector3(0, 0.06f, 4), new Vector3(1.2f, 0.12f, 1f), "Path3");
        if (decoGo[d] != null) GFM_Create.SetColor(decoGo[d], new Color(0.38f, 0.28f, 0.18f));
        d++;

        decoGo[d] = GFM_Create.Obj(PrimitiveType.Cylinder, new Vector3(2.5f, 2.5f, -5), new Vector3(0.12f, 4f, 0.12f), "Flagpole");
        if (decoGo[d] != null) GFM_Create.SetColor(decoGo[d], new Color(0.6f, 0.6f, 0.6f));
        d++;

        decoGo[d] = GFM_Create.Obj(PrimitiveType.Cube, new Vector3(3.1f, 4.2f, -5), new Vector3(1.2f, 0.7f, 0.05f), "Flag");
        if (decoGo[d] != null) GFM_Create.SetColor(decoGo[d], new Color(0.9f, 0.2f, 0.2f));
        d++;

        decoGo[d] = GFM_Create.Obj(PrimitiveType.Cylinder, new Vector3(-4, 1.5f, 2.5f), new Vector3(0.15f, 2.2f, 0.15f), "Torch1");
        if (decoGo[d] != null) GFM_Create.SetColor(decoGo[d], new Color(0.55f, 0.35f, 0.15f));
        d++;

        decoGo[d] = GFM_Create.Obj(PrimitiveType.Sphere, new Vector3(-4, 2.8f, 2.5f), new Vector3(0.35f, 0.35f, 0.35f), "TorchFire1");
        if (decoGo[d] != null) GFM_Create.SetColor(decoGo[d], new Color(1f, 0.6f, 0f));
        d++;

        decoGo[d] = GFM_Create.Obj(PrimitiveType.Cylinder, new Vector3(4, 1.5f, 2.5f), new Vector3(0.15f, 2.2f, 0.15f), "Torch2");
        if (decoGo[d] != null) GFM_Create.SetColor(decoGo[d], new Color(0.55f, 0.35f, 0.15f));
        d++;

        decoGo[d] = GFM_Create.Obj(PrimitiveType.Sphere, new Vector3(4, 2.8f, 2.5f), new Vector3(0.35f, 0.35f, 0.35f), "TorchFire2");
        if (decoGo[d] != null) GFM_Create.SetColor(decoGo[d], new Color(1f, 0.65f, 0.05f));
        d++;

        decoGo[d] = GFM_Create.Obj(PrimitiveType.Cylinder, new Vector3(-8, 0.03f, -3), new Vector3(2.5f, 0.03f, 2.5f), "WaterPond");
        if (decoGo[d] != null) GFM_Create.SetColor(decoGo[d], new Color(0.2f, 0.4f, 0.8f));
        d++;

        decoGo[d] = GFM_Create.Obj(PrimitiveType.Cube, new Vector3(-4.5f, 0.5f, 3.5f), new Vector3(1f, 1f, 1f), "Crate1");
        if (decoGo[d] != null) GFM_Create.SetColor(decoGo[d], new Color(0.65f, 0.45f, 0.2f));
        d++;

        decoGo[d] = GFM_Create.Obj(PrimitiveType.Cube, new Vector3(-4.5f, 1.5f, 3.5f), new Vector3(0.8f, 0.8f, 0.8f), "Crate2");
        if (decoGo[d] != null) GFM_Create.SetColor(decoGo[d], new Color(0.6f, 0.4f, 0.18f));
        d++;

        decoGo[d] = GFM_Create.Obj(PrimitiveType.Sphere, new Vector3(3, 0.2f, -7), new Vector3(0.4f, 0.3f, 0.4f), "Flower1");
        if (decoGo[d] != null) GFM_Create.SetColor(decoGo[d], new Color(0.95f, 0.3f, 0.6f));
        d++;
    }

    // ============================================================
    // UPDATE
    // ============================================================
    void Update()
    {
        if (!initialized) return;
        if (gameEnded) return;

        float dt = Time.deltaTime;
        if (dt > 0.1f) dt = 0.1f;

        globalTimer += dt;

        // Update rule since timers
        for (int i = 0; i < RULE_COUNT; i++)
        {
            if (ruleTriggered[i]) ruleSinceTimer[i] += dt;
        }

        // Tap cooldown
        if (tapCooldown > 0f) tapCooldown -= dt;

        // Click/tap detection
        clickedThisFrame = false;
        bool touching = Input.GetMouseButton(0);
        if (Input.GetMouseButtonDown(0))
        {
            clickedThisFrame = true;
        }
        if (wasTouching && !touching && tapCooldown <= 0f)
        {
            clickedThisFrame = true;
            tapCooldown = 0.15f;
        }
        wasTouching = touching;

        if (clickedThisFrame && mainCam != null)
        {
            try
            {
                Ray ray = mainCam.ScreenPointToRay(Input.mousePosition);
                if (ray.direction.y < -0.01f)
                {
                    float t = -ray.origin.y / ray.direction.y;
                    clickWorldPos = ray.origin + ray.direction * t;
                }
            }
            catch (System.Exception) { }
        }

        // Check all event rules
        CheckEventRules();

        // Update player
        UpdatePlayer(dt);

        // Update auto-play
        UpdateAutoPlay(dt);

        // Update conveyor belt
        if (eActive[E_CONVEYOR]) UpdateConveyorBelt(dt);

        // Update crossbows
        if (eActive[E_CROSSBOW_L]) UpdateShooter(E_CROSSBOW_L, dt);
        if (eActive[E_CROSSBOW_R]) UpdateShooter(E_CROSSBOW_R, dt);

        // Update turret
        if (eActive[E_TURRET]) UpdateTurret(dt);

        // Update enemy spawner
        if (eActive[E_ENEMY_SPAWNER]) UpdateEnemySpawner(dt);

        // Update wood log spawner
        if (eActive[E_WOODLOG_SPAWNER]) UpdateWoodLogSpawner(dt);

        // Update worker spawner
        if (eActive[E_WORKER_SPAWNER]) UpdateWorkerSpawner(dt);

        // Update WoodHouse
        if (eActive[E_WOODHOUSE]) UpdateWoodHouse(dt);

        // Update Boss
        if (eActive[E_BOSS] && eState[E_BOSS] == 1) UpdateBoss(dt);

        // Update pools
        UpdateArrows(dt);
        UpdateEnemies(dt);
        UpdateCoins(dt);
        UpdateWoodLogs(dt);
        UpdateWorkers(dt);

        // Update guide arrow
        UpdateGuideArrow(dt);

        // Update blueprint indicators
        UpdateBlueprintIndicators(dt);

        // Update health bar
        UpdateHealthBar();

        // Update UI text
        UpdateUI();

        // Update camera
        UpdateCamera(dt);
    }

    // ============================================================
    // EVENT RULES
    // ============================================================
    void CheckEventRules()
    {
        // Rule 0 (Rule 1): gameStart → activate initial entities
        if (!ruleTriggered[0])
        {
            ruleTriggered[0] = true;
            eActive[E_GROUND] = true;
            eActive[E_PLAYER] = true;
            eActive[E_BASE] = true;
            eState[E_BASE] = 1;
            eActive[E_WOODFENCE_L] = true;
            eActive[E_WOODFENCE_R] = true;
            eActive[E_PINETREE_L] = true;
            eActive[E_PINETREE_R] = true;
            eActive[E_GENERATOR] = true;
            eActive[E_CONVEYOR] = true;
            eState[E_CONVEYOR] = 0;
            ShowGuide("Move to the green blueprint to build!");
            camLookTarget = conveyorPos;
            camZoom = 8f;
            showGuideArrow = true;
            guideArrowTarget = conveyorPos;
            if (blueprintCostText != null)
                blueprintCostText.text = "Cost: 1 Gold";
        }

        // Rule 1 (Rule 2): ConveyorBelt.state == built (2)
        if (!ruleTriggered[1] && eState[E_CONVEYOR] == 2)
        {
            ruleTriggered[1] = true;
            ActivateCrossbows();
            eActive[E_ENEMY_SPAWNER] = true;
            eTimer[E_ENEMY_SPAWNER] = 2f;
            eActive[E_WOODLOG_SPAWNER] = true;
            eTimer[E_WOODLOG_SPAWNER] = 3f;
            ShowGuide("Crossbows fire at enemies! Collect gold!");
            camLookTarget = new Vector3(0, 0, 1f);
            camZoom = 10f;
            showGuideArrow = false;

            HideBlueprintVisuals();
        }

        // Rule 2 (Rule 3): Enemy_A.killed >= 3 → activate WoodHouse
        if (!ruleTriggered[2] && enemyKillCount >= 3)
        {
            ruleTriggered[2] = true;
            eActive[E_WOODHOUSE] = true;
            eState[E_WOODHOUSE] = 0;
            if (eGo[E_WOODHOUSE] != null)
                eGo[E_WOODHOUSE].transform.position = woodHousePos;
            ShowGuide("Collect 3 wood to build Wood House!");
            showGuideArrow = true;
            guideArrowTarget = woodHousePos;
        }

        // Rule 3 (Rule 4): WoodHouse.state == built
        if (!ruleTriggered[3] && eState[E_WOODHOUSE] == 2)
        {
            ruleTriggered[3] = true;
            eActive[E_WORKER_SPAWNER] = true;
            eTimer[E_WORKER_SPAWNER] = 4f;
            ShowGuide("Wood House spawns workers!");
            showGuideArrow = false;
        }

        // Rule 4 (Rule 5): Worker.count >= 1
        if (!ruleTriggered[4] && CountActiveWorkers() >= 1)
        {
            ruleTriggered[4] = true;
            ShowGuide("Workers auto-carry wood to base!");
        }

        // Rule 5 (Rule 6): wood >= 5 → activate Turret
        if (!ruleTriggered[5] && wood >= 5)
        {
            ruleTriggered[5] = true;
            eActive[E_TURRET] = true;
            eState[E_TURRET] = 0;
            if (eGo[E_TURRET] != null)
                eGo[E_TURRET].transform.position = turretPos;
            ShowGuide("Go to turret blueprint to build! (5 wood)");
            showGuideArrow = true;
            guideArrowTarget = turretPos;
        }

        // Rule 6 (Rule 7): Turret.state == built
        if (!ruleTriggered[6] && eState[E_TURRET] == 2)
        {
            ruleTriggered[6] = true;
            ShowGuide("Defend the base!");
            showGuideArrow = false;
            camZoom = 12f;
        }

        // Rule 7 (Rule 8): Enemy_A.killed >= 10 → activate Boss
        if (!ruleTriggered[7] && enemyKillCount >= 10)
        {
            ruleTriggered[7] = true;
            eActive[E_BOSS] = true;
            eState[E_BOSS] = 1;
            eHP[E_BOSS] = 20f;
            if (eGo[E_BOSS] != null)
                eGo[E_BOSS].transform.position = new Vector3(20, 1.5f, 0);
            ShowGuide("Defeat the Boss!");
            camZoom = 14f;
        }

        // Rule 8 (Rule 9): Boss.hp <= 0 → activate CastleWall
        if (!ruleTriggered[8] && ruleTriggered[7] && eHP[E_BOSS] <= 0)
        {
            ruleTriggered[8] = true;
            eState[E_BOSS] = 2;
            if (eGo[E_BOSS] != null)
                eGo[E_BOSS].transform.position = new Vector3(0, -999, 0);
            eActive[E_BOSS] = false;
            eActive[E_CASTLEWALL] = true;
            if (eGo[E_CASTLEWALL] != null)
                eGo[E_CASTLEWALL].transform.position = new Vector3(0, 2, 8);
            ShowGuide("Click base to upgrade!");
            eTimer[E_BASE] = 0f;
            showGuideArrow = true;
            guideArrowTarget = basePos;
        }

        // Rule 9 (Rule 10): Base.level >= 2 → end game
        if (!ruleTriggered[9] && baseLevel >= 2)
        {
            ruleTriggered[9] = true;
            ShowGuide("Amazing! Download for more!");
            showGuideArrow = false;
            ctaTimer = 1.5f;
            ctaActive = true;
        }

        // ============================================================
        // AUTO-PROGRESSION SAFETY NETS
        // These have longer timers to allow normal gameplay first
        // ============================================================

        // Force conveyor build at 8s if player hasn't reached it
        if (ruleTriggered[0] && !ruleTriggered[1] && globalTimer > 8f)
        {
            if (eState[E_CONVEYOR] == 0)
            {
                if (eGo[E_PLAYER] != null)
                    eGo[E_PLAYER].transform.position = new Vector3(conveyorPos.x, 1f, conveyorPos.z);
                if (gold < 1) gold = 1;
                gold -= 1;
                eState[E_CONVEYOR] = 1;
                eTimer[E_CONVEYOR] = 0f;
                ShowGuide("Building conveyor belt...");
                showGuideArrow = false;
            }
        }

        // Speed up conveyor build if it's been building too long
        if (eState[E_CONVEYOR] == 1 && globalTimer > 11f)
        {
            eTimer[E_CONVEYOR] = conveyorBuildTime;
        }

        // After conveyor built, fast-kill enemies for rule 2 after 8s
        if (ruleTriggered[1] && !ruleTriggered[2])
        {
            if (ruleSinceTimer[1] > 6f)
            {
                for (int i = 0; i < MAX_ENEMIES; i++)
                {
                    if (enemyActive[i] && enemyHP[i] > 0)
                        enemyHP[i] = 0;
                }
            }
            if (ruleSinceTimer[1] > 8f && enemyKillCount < 3)
                enemyKillCount = 3;
        }

        // After woodhouse appears, grant wood if needed after 6s
        if (ruleTriggered[2] && !ruleTriggered[3] && eState[E_WOODHOUSE] == 0)
        {
            if (ruleSinceTimer[2] > 6f)
            {
                if (wood < 3) wood = 3;
            }
        }

        // After woodhouse built, spawn worker after 4s
        if (ruleTriggered[3] && !ruleTriggered[4] && ruleSinceTimer[3] > 4f)
        {
            if (CountActiveWorkers() < 1)
                SpawnWorker(new Vector3(5, 0.75f, -1));
        }

        // After worker active, grant wood for turret after 5s
        if (ruleTriggered[4] && !ruleTriggered[5] && ruleSinceTimer[4] > 5f)
        {
            if (wood < 5) wood = 5;
        }

        // If turret blueprint visible but not built after 6s, auto-build
        if (ruleTriggered[5] && !ruleTriggered[6] && eState[E_TURRET] == 0 && ruleSinceTimer[5] > 6f)
        {
            if (wood < 5) wood = 5;
            wood -= 5;
            eState[E_TURRET] = 1;
            eTimer[E_TURRET] = 0f;
            ShowGuide("Building turret...");
            showGuideArrow = false;
        }

        // Speed up turret build after 8s
        if (ruleTriggered[5] && !ruleTriggered[6] && eState[E_TURRET] == 1 && ruleSinceTimer[5] > 8f)
        {
            eTimer[E_TURRET] = turretBuildTime;
        }

        // After turret built, speed up kills for boss spawn after 5s
        if (ruleTriggered[6] && !ruleTriggered[7])
        {
            if (ruleSinceTimer[6] > 4f)
            {
                for (int i = 0; i < MAX_ENEMIES; i++)
                {
                    if (enemyActive[i] && enemyHP[i] > 0)
                        enemyHP[i] = 0;
                }
            }
            if (ruleSinceTimer[6] > 6f && enemyKillCount < 10)
                enemyKillCount = 10;
        }

        // After boss spawned, auto-damage boss after 5s
        if (ruleTriggered[7] && !ruleTriggered[8] && eActive[E_BOSS] && eHP[E_BOSS] > 0)
        {
            if (ruleSinceTimer[7] > 5f)
                eHP[E_BOSS] -= Time.deltaTime * 10f;
        }

        // After castle wall shown, auto-upgrade base after 3s
        if (ruleTriggered[8] && !ruleTriggered[9])
        {
            eTimer[E_BASE] += Time.deltaTime;
            if (eTimer[E_BASE] > 3f)
            {
                baseLevel = 2;
                if (eGo[E_BASE] != null)
                {
                    eGo[E_BASE].transform.localScale = new Vector3(4, 4, 4);
                    GFM_Create.SetColor(eGo[E_BASE], new Color(0.95f, 0.85f, 0.5f));
                }
            }
        }
    }

    void HideBlueprintVisuals()
    {
        if (blueprintRingGo != null) blueprintRingGo.transform.position = new Vector3(0, -999, 0);
        if (blueprintIndicatorGo != null) blueprintIndicatorGo.transform.position = new Vector3(0, -999, 0);
        if (blueprintCostText != null) blueprintCostText.text = "";
    }

    // ============================================================
    // PLAYER UPDATE
    // ============================================================
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

        bool hasInput = Mathf.Abs(h) > 0.05f || Mathf.Abs(v) > 0.05f;

        if (hasInput)
        {
            noInputTimer = 0f;
            autoPlay = false;
            playerEverMoved = true;
            Vector3 move = new Vector3(h, 0, v).normalized * playerSpeed * dt;
            eGo[E_PLAYER].transform.position += move;
        }
        else
        {
            noInputTimer += dt;
            if (noInputTimer > autoPlayDelay)
            {
                autoPlay = true;
            }
        }

        // Keep player on ground
        Vector3 pp = eGo[E_PLAYER].transform.position;
        pp.y = 1f;
        pp.x = Mathf.Clamp(pp.x, -18f, 18f);
        pp.z = Mathf.Clamp(pp.z, -18f, 18f);
        eGo[E_PLAYER].transform.position = pp;
    }

    void UpdateAutoPlay(float dt)
    {
        if (!autoPlay || !eActive[E_PLAYER] || eGo[E_PLAYER] == null) return;

        Vector3 target = DetermineAutoTarget();
        Vector3 playerPos = eGo[E_PLAYER].transform.position;
        Vector3 dir = target - playerPos;
        dir.y = 0;

        if (dir.magnitude > 0.5f)
        {
            dir = dir.normalized * playerSpeed * dt;
            eGo[E_PLAYER].transform.position += dir;
        }
    }

    Vector3 DetermineAutoTarget()
    {
        // Priority 1: build conveyor if not built
        if (eActive[E_CONVEYOR] && eState[E_CONVEYOR] == 0 && eGo[E_CONVEYOR] != null)
            return conveyorPos;

        // Priority 2: collect nearest coin
        float nearestCoinDist = 999f;
        Vector3 nearestCoinPos = basePos;
        bool foundCoin = false;
        if (eGo[E_PLAYER] != null)
        {
            for (int i = 0; i < MAX_COINS; i++)
            {
                if (!coinActive[i] || coinGo[i] == null) continue;
                float d = Vector3.Distance(eGo[E_PLAYER].transform.position, coinGo[i].transform.position);
                if (d < nearestCoinDist)
                {
                    nearestCoinDist = d;
                    nearestCoinPos = coinGo[i].transform.position;
                    foundCoin = true;
                }
            }
        }
        if (foundCoin && nearestCoinDist < 15f) return nearestCoinPos;

        // Priority 3: collect nearest wood log
        float nearestWoodDist = 999f;
        Vector3 nearestWoodPos = basePos;
        bool foundWood = false;
        if (eGo[E_PLAYER] != null)
        {
            for (int i = 0; i < MAX_WOODLOGS; i++)
            {
                if (!woodActive[i] || woodState[i] != 1 || woodGo[i] == null) continue;
                float d2 = Vector3.Distance(eGo[E_PLAYER].transform.position, woodGo[i].transform.position);
                if (d2 < nearestWoodDist)
                {
                    nearestWoodDist = d2;
                    nearestWoodPos = woodGo[i].transform.position;
                    foundWood = true;
                }
            }
        }
        if (foundWood && nearestWoodDist < 15f) return nearestWoodPos;

        // Priority 4: go to turret if buildable
        if (eActive[E_TURRET] && eState[E_TURRET] == 0 && wood >= 5 && eGo[E_TURRET] != null)
            return eGo[E_TURRET].transform.position;

        // Priority 5: go to base (for upgrade)
        if (ruleTriggered[8] && !ruleTriggered[9])
            return basePos;

        // Default: patrol near base
        float angle = Time.time * 0.5f;
        return basePos + new Vector3(Mathf.Sin(angle) * 3f, 0, Mathf.Cos(angle) * 3f);
    }

    // ============================================================
    // GUIDE ARROW UPDATE
    // ============================================================
    void UpdateGuideArrow(float dt)
    {
        if (guideArrowGo == null) return;

        if (!showGuideArrow)
        {
            guideArrowGo.transform.position = new Vector3(0, -999, 0);
            return;
        }

        float bounce = Mathf.Sin(Time.time * 3f) * 0.5f;
        guideArrowGo.transform.position = guideArrowTarget + new Vector3(0, 3.5f + bounce, 0);
        guideArrowGo.transform.Rotate(0, 120f * dt, 0);
    }

    // ============================================================
    // BLUEPRINT INDICATORS UPDATE
    // ============================================================
    void UpdateBlueprintIndicators(float dt)
    {
        if (eActive[E_CONVEYOR] && eState[E_CONVEYOR] == 0)
        {
            float t = Time.time;

            if (blueprintRingGo != null)
            {
                float ringPulse = 4f + 1.5f * Mathf.Sin(t * 2f);
                blueprintRingGo.transform.localScale = new Vector3(ringPulse, 0.02f, ringPulse);
                blueprintRingGo.transform.position = new Vector3(conveyorPos.x, 0.02f, conveyorPos.z);

                float gVal = 0.5f + 0.5f * Mathf.Sin(t * 3f);
                GFM_Create.SetColor(blueprintRingGo, new Color(0.05f, gVal, 0.05f));
            }

            if (blueprintIndicatorGo != null)
            {
                float iconBounce = Mathf.Sin(t * 4f) * 0.3f;
                blueprintIndicatorGo.transform.position = new Vector3(conveyorPos.x, 2.5f + iconBounce, conveyorPos.z);
                blueprintIndicatorGo.transform.Rotate(0, 120f * dt, 0);
            }

            if (blueprintCostText != null)
            {
                float textPulse = 0.7f + 0.3f * Mathf.Sin(t * 4f);
                blueprintCostText.color = new Color(1f, textPulse, 0.2f);
                blueprintCostText.text = "Walk here! Cost: 1 Gold";
            }
        }
        else
        {
            if (eState[E_CONVEYOR] != 0)
            {
                HideBlueprintVisuals();
            }
        }
    }

    // ============================================================
    // CONVEYOR BELT UPDATE
    // ============================================================
    void UpdateConveyorBelt(float dt)
    {
        if (eGo[E_CONVEYOR] == null) return;

        if (eState[E_CONVEYOR] == 0)
        {
            // Blueprint state - strong green flicker
            blueprintFlicker += dt * 5f;
            float alpha = 0.5f + 0.5f * Mathf.Sin(blueprintFlicker);

            float r = 0.05f + 0.15f * alpha;
            float g = 0.6f + 0.4f * alpha;
            float b = 0.05f + 0.1f * alpha;
            GFM_Create.SetColor(eGo[E_CONVEYOR], new Color(r, g, b));

            float pulse = 1f + 0.15f * Mathf.Sin(blueprintFlicker * 1.5f);
            eGo[E_CONVEYOR].transform.localScale = new Vector3(4 * pulse, 0.5f * pulse, 1.2f * pulse);
            eGo[E_CONVEYOR].transform.position = new Vector3(conveyorPos.x, 0.25f + 0.15f * Mathf.Sin(blueprintFlicker * 0.8f), conveyorPos.z);

            // Check proximity - player walks near
            if (eGo[E_PLAYER] != null)
            {
                Vector3 pPos = eGo[E_PLAYER].transform.position;
                Vector3 cPos = eGo[E_CONVEYOR].transform.position;
                float dx = pPos.x - cPos.x;
                float dz = pPos.z - cPos.z;
                float flatDist = Mathf.Sqrt(dx * dx + dz * dz);

                if (flatDist < 2.5f && gold >= 1)
                {
                    gold -= 1;
                    eState[E_CONVEYOR] = 1;
                    eTimer[E_CONVEYOR] = 0f;
                    ShowGuide("Building conveyor belt...");
                    showGuideArrow = false;
                }
            }

            // Also allow click/tap on conveyor area
            if (clickedThisFrame && gold >= 1)
            {
                Vector3 cPos = eGo[E_CONVEYOR].transform.position;
                float dx2 = clickWorldPos.x - cPos.x;
                float dz2 = clickWorldPos.z - cPos.z;
                float clickDist = Mathf.Sqrt(dx2 * dx2 + dz2 * dz2);
                if (clickDist < 5f)
                {
                    gold -= 1;
                    eState[E_CONVEYOR] = 1;
                    eTimer[E_CONVEYOR] = 0f;
                    ShowGuide("Building conveyor belt...");
                    showGuideArrow = false;
                }
            }
        }
        else if (eState[E_CONVEYOR] == 1)
        {
            eTimer[E_CONVEYOR] += dt;
            float progress = eTimer[E_CONVEYOR] / conveyorBuildTime;
            float sc = Mathf.Lerp(0.3f, 1f, Mathf.Clamp01(progress));
            eGo[E_CONVEYOR].transform.localScale = new Vector3(4 * sc, 0.3f, 1 * sc);
            eGo[E_CONVEYOR].transform.position = conveyorPos;

            float cVal = Mathf.Lerp(0.3f, 0.5f, progress);
            GFM_Create.SetColor(eGo[E_CONVEYOR], new Color(cVal, cVal, cVal + 0.05f));

            int pct = Mathf.RoundToInt(Mathf.Clamp01(progress) * 100f);
            ShowGuide("Building conveyor belt... " + pct + "%");

            if (eTimer[E_CONVEYOR] >= conveyorBuildTime)
            {
                eState[E_CONVEYOR] = 2;
                eGo[E_CONVEYOR].transform.localScale = new Vector3(4, 0.3f, 1);
                GFM_Create.SetColor(eGo[E_CONVEYOR], new Color(0.5f, 0.5f, 0.55f));
            }
        }
    }

    // ============================================================
    // CROSSBOW ACTIVATION & UPDATE
    // ============================================================
    void ActivateCrossbows()
    {
        eActive[E_CROSSBOW_L] = true;
        eState[E_CROSSBOW_L] = 1;
        eTimer[E_CROSSBOW_L] = 0f;
        if (eGo[E_CROSSBOW_L] != null)
            eGo[E_CROSSBOW_L].transform.position = new Vector3(3, 0.75f, 5);

        eActive[E_CROSSBOW_R] = true;
        eState[E_CROSSBOW_R] = 1;
        eTimer[E_CROSSBOW_R] = 0f;
        if (eGo[E_CROSSBOW_R] != null)
            eGo[E_CROSSBOW_R].transform.position = new Vector3(-3, 0.75f, 5);
    }

    void UpdateShooter(int idx, float dt)
    {
        if (eState[idx] < 1 || eGo[idx] == null) return;
        eTimer[idx] += dt;

        if (eTimer[idx] >= crossbowFireRate)
        {
            Vector3 shooterPos = eGo[idx].transform.position;
            int target = FindNearestEnemy(shooterPos, crossbowRange);

            bool bossInRange = false;
            Vector3 bossPos2 = Vector3.zero;
            if (eActive[E_BOSS] && eState[E_BOSS] == 1 && eGo[E_BOSS] != null)
            {
                bossPos2 = eGo[E_BOSS].transform.position;
                float bossDist = Vector3.Distance(shooterPos, bossPos2);
                if (bossDist < crossbowRange)
                    bossInRange = true;
            }

            if (target >= 0 || bossInRange)
            {
                Vector3 targetPos;
                if (target >= 0)
                    targetPos = enemyGo[target].transform.position;
                else
                    targetPos = bossPos2;

                SpawnArrow(shooterPos, targetPos, 1f);
                eTimer[idx] = 0f;
            }
        }
    }

    // ============================================================
    // TURRET UPDATE
    // ============================================================
    void UpdateTurret(float dt)
    {
        if (eGo[E_TURRET] == null) return;

        if (eState[E_TURRET] == 0)
        {
            // Blueprint flicker
            float flick = 0.5f + 0.5f * Mathf.Sin(Time.time * 4f);
            float r = 0.1f + 0.3f * flick;
            float g = 0.5f + 0.5f * flick;
            float b = 0.1f + 0.2f * flick;
            GFM_Create.SetColor(eGo[E_TURRET], new Color(r, g, b));

            float pulse = 1f + 0.15f * Mathf.Sin(Time.time * 2.5f);
            eGo[E_TURRET].transform.localScale = new Vector3(1.5f * pulse, 2 * pulse, 1.5f * pulse);

            // Check proximity
            if (eGo[E_PLAYER] != null)
            {
                Vector3 pPos = eGo[E_PLAYER].transform.position;
                Vector3 tPos = eGo[E_TURRET].transform.position;
                float dx = pPos.x - tPos.x;
                float dz = pPos.z - tPos.z;
                float flatDist = Mathf.Sqrt(dx * dx + dz * dz);

                if (flatDist < 2.5f && wood >= 5)
                {
                    wood -= 5;
                    eState[E_TURRET] = 1;
                    eTimer[E_TURRET] = 0f;
                    ShowGuide("Building turret...");
                    showGuideArrow = false;
                }
            }

            // Click/tap
            if (clickedThisFrame && wood >= 5)
            {
                Vector3 tPos = eGo[E_TURRET].transform.position;
                float dx2 = clickWorldPos.x - tPos.x;
                float dz2 = clickWorldPos.z - tPos.z;
                float clickDist = Mathf.Sqrt(dx2 * dx2 + dz2 * dz2);
                if (clickDist < 5f)
                {
                    wood -= 5;
                    eState[E_TURRET] = 1;
                    eTimer[E_TURRET] = 0f;
                    ShowGuide("Building turret...");
                    showGuideArrow = false;
                }
            }
        }
        else if (eState[E_TURRET] == 1)
        {
            eTimer[E_TURRET] += dt;
            float progress = eTimer[E_TURRET] / turretBuildTime;
            float sc = Mathf.Lerp(0.3f, 1f, Mathf.Clamp01(progress));
            eGo[E_TURRET].transform.localScale = new Vector3(1.5f * sc, 2 * sc, 1.5f * sc);

            int pct = Mathf.RoundToInt(Mathf.Clamp01(progress) * 100f);
            ShowGuide("Building turret... " + pct + "%");

            if (eTimer[E_TURRET] >= turretBuildTime)
            {
                eState[E_TURRET] = 2;
                eGo[E_TURRET].transform.localScale = new Vector3(1.5f, 2, 1.5f);
                GFM_Create.SetColor(eGo[E_TURRET], new Color(0.5f, 0.5f, 0.55f));
                eTimer[E_TURRET] = 0f;
            }
        }
        else if (eState[E_TURRET] == 2)
        {
            // Auto-shoot
            eTimer[E_TURRET] += dt;
            if (eTimer[E_TURRET] >= turretFireRate)
            {
                Vector3 tPos = eGo[E_TURRET].transform.position;
                int target = FindNearestEnemy(tPos, turretRange);
                bool shot = false;

                if (target >= 0)
                {
                    SpawnArrow(tPos, enemyGo[target].transform.position, 3f);
                    eTimer[E_TURRET] = 0f;
                    shot = true;
                }

                if (!shot && eActive[E_BOSS] && eState[E_BOSS] == 1 && eGo[E_BOSS] != null)
                {
                    float bossDist = Vector3.Distance(tPos, eGo[E_BOSS].transform.position);
                    if (bossDist < turretRange)
                    {
                        SpawnArrow(tPos, eGo[E_BOSS].transform.position, 3f);
                        eTimer[E_TURRET] = 0f;
                    }
                }
            }
        }
    }

    // ============================================================
    // ENEMY SPAWNER
    // ============================================================
    void UpdateEnemySpawner(float dt)
    {
        eTimer[E_ENEMY_SPAWNER] += dt;
        if (eTimer[E_ENEMY_SPAWNER] >= enemySpawnInterval)
        {
            if (CountActiveEnemies() < enemyMaxAlive)
            {
                SpawnEnemy(new Vector3(15, 0.8f, Random.Range(-2f, 5f)));
                eTimer[E_ENEMY_SPAWNER] = 0f;
            }
        }
    }

    // ============================================================
    // WOOD LOG SPAWNER
    // ============================================================
    void UpdateWoodLogSpawner(float dt)
    {
        eTimer[E_WOODLOG_SPAWNER] += dt;
        if (eTimer[E_WOODLOG_SPAWNER] >= woodLogSpawnInterval)
        {
            if (CountActiveWoodLogs() < woodLogMaxAlive)
            {
                SpawnWoodLog(new Vector3(Random.Range(-1f, 1f), 0.4f, 3 + Random.Range(-0.5f, 0.5f)));
                eTimer[E_WOODLOG_SPAWNER] = 0f;
            }
        }
    }

    // ============================================================
    // WORKER SPAWNER
    // ============================================================
    void UpdateWorkerSpawner(float dt)
    {
        eTimer[E_WORKER_SPAWNER] += dt;
        if (eTimer[E_WORKER_SPAWNER] >= workerSpawnInterval)
        {
            if (CountActiveWorkers() < workerMaxAlive)
            {
                SpawnWorker(new Vector3(5, 0.75f, -1));
                eTimer[E_WORKER_SPAWNER] = 0f;
            }
        }
    }

    // ============================================================
    // WOODHOUSE UPDATE
    // ============================================================
    void UpdateWoodHouse(float dt)
    {
        if (eGo[E_WOODHOUSE] == null) return;

        if (eState[E_WOODHOUSE] == 0)
        {
            float flick = 0.5f + 0.5f * Mathf.Sin(Time.time * 3.5f);
            GFM_Create.SetColor(eGo[E_WOODHOUSE], new Color(0.2f + 0.3f * flick, 0.5f + 0.4f * flick, 0.1f + 0.2f * flick));

            float pulse = 1f + 0.1f * Mathf.Sin(Time.time * 2.5f);
            eGo[E_WOODHOUSE].transform.localScale = new Vector3(3 * pulse, 2.5f * pulse, 3 * pulse);

            // Auto-build when wood >= 3
            if (wood >= 3)
            {
                wood -= 3;
                eState[E_WOODHOUSE] = 1;
                eTimer[E_WOODHOUSE] = 0f;
                ShowGuide("Building Wood House...");
                showGuideArrow = false;
            }
        }
        else if (eState[E_WOODHOUSE] == 1)
        {
            eTimer[E_WOODHOUSE] += dt;
            float progress = eTimer[E_WOODHOUSE] / woodHouseBuildTime;
            float sc = Mathf.Lerp(0.3f, 1f, Mathf.Clamp01(progress));
            eGo[E_WOODHOUSE].transform.localScale = new Vector3(3 * sc, 2.5f * sc, 3 * sc);

            int pct = Mathf.RoundToInt(Mathf.Clamp01(progress) * 100f);
            ShowGuide("Building Wood House... " + pct + "%");

            if (eTimer[E_WOODHOUSE] >= woodHouseBuildTime)
            {
                eState[E_WOODHOUSE] = 2;
                eGo[E_WOODHOUSE].transform.localScale = new Vector3(3, 2.5f, 3);
                GFM_Create.SetColor(eGo[E_WOODHOUSE], new Color(0.85f, 0.7f, 0.4f));
            }
        }
    }

    // ============================================================
    // BOSS UPDATE
    // ============================================================
    void UpdateBoss(float dt)
    {
        if (eGo[E_BOSS] == null || eHP[E_BOSS] <= 0) return;

        Vector3 bPos = eGo[E_BOSS].transform.position;
        Vector3 dir = (basePos - bPos);
        dir.y = 0;
        dir = dir.normalized * bossSpeed * dt;
        eGo[E_BOSS].transform.position += dir;

        // Check if reached base
        if (Vector3.Distance(eGo[E_BOSS].transform.position, basePos) < 2f)
        {
            eHP[E_BASE] -= 5f * dt;
        }

        // Keep y
        Vector3 bp = eGo[E_BOSS].transform.position;
        bp.y = 1.5f;
        eGo[E_BOSS].transform.position = bp;

        // Flash red
        float flashVal = 0.7f + 0.3f * Mathf.Sin(Time.time * 8f);
        GFM_Create.SetColor(eGo[E_BOSS], new Color(flashVal, 0.1f, 0.1f));
    }

    // ============================================================
    // ARROW POOL
    // ============================================================
    void SpawnArrow(Vector3 from, Vector3 to, float damage)
    {
        for (int i = 0; i < MAX_ARROWS; i++)
        {
            if (!arrowActive[i] && arrowGo[i] != null)
            {
                arrowGo[i].transform.position = from;
                arrowTarget[i] = to;
                arrowActive[i] = true;
                arrowLife[i] = 3f;
                arrowDmg[i] = damage;
                Vector3 d = (to - from);
                if (d.magnitude > 0.01f)
                    arrowGo[i].transform.rotation = Quaternion.LookRotation(d.normalized);
                return;
            }
        }
    }

    void UpdateArrows(float dt)
    {
        for (int i = 0; i < MAX_ARROWS; i++)
        {
            if (!arrowActive[i] || arrowGo[i] == null) continue;

            arrowLife[i] -= dt;
            if (arrowLife[i] <= 0)
            {
                arrowGo[i].transform.position = new Vector3(0, -999, 0);
                arrowActive[i] = false;
                continue;
            }

            Vector3 dir = (arrowTarget[i] - arrowGo[i].transform.position);
            float flatMag = Mathf.Sqrt(dir.x * dir.x + dir.z * dir.z);
            if (flatMag < 0.8f)
            {
                DamageAtPosition(arrowTarget[i], arrowDmg[i]);
                arrowGo[i].transform.position = new Vector3(0, -999, 0);
                arrowActive[i] = false;
                continue;
            }

            dir = dir.normalized * arrowSpeed * dt;
            arrowGo[i].transform.position += dir;
        }
    }

    void DamageAtPosition(Vector3 pos, float damage)
    {
        float bestDist = 2.5f;
        int bestIdx = -1;
        for (int i = 0; i < MAX_ENEMIES; i++)
        {
            if (!enemyActive[i] || enemyGo[i] == null) continue;
            float d = Vector3.Distance(pos, enemyGo[i].transform.position);
            if (d < bestDist)
            {
                bestDist = d;
                bestIdx = i;
            }
        }
        if (bestIdx >= 0)
        {
            enemyHP[bestIdx] -= damage;
            if (enemyGo[bestIdx] != null)
                GFM_Create.SetColor(enemyGo[bestIdx], new Color(1f, 0.5f, 0.5f));
            return;
        }

        // Check boss
        if (eActive[E_BOSS] && eState[E_BOSS] == 1 && eGo[E_BOSS] != null)
        {
            float d2 = Vector3.Distance(pos, eGo[E_BOSS].transform.position);
            if (d2 < 3f)
            {
                eHP[E_BOSS] -= damage;
            }
        }
    }

    // ============================================================
    // ENEMY POOL
    // ============================================================
    void SpawnEnemy(Vector3 pos)
    {
        for (int i = 0; i < MAX_ENEMIES; i++)
        {
            if (!enemyActive[i] && enemyGo[i] != null)
            {
                enemyGo[i].transform.position = pos;
                enemyActive[i] = true;
                enemyHP[i] = 3f;
                enemyState[i] = 1;
                GFM_Create.SetColor(enemyGo[i], new Color(0.85f, 0.15f, 0.15f));
                return;
            }
        }
    }

    void UpdateEnemies(float dt)
    {
        for (int i = 0; i < MAX_ENEMIES; i++)
        {
            if (!enemyActive[i] || enemyGo[i] == null) continue;

            if (enemyHP[i] <= 0)
            {
                Vector3 deathPos = enemyGo[i].transform.position;
                enemyGo[i].transform.position = new Vector3(0, -999, 0);
                enemyActive[i] = false;
                enemyState[i] = 0;
                enemyKillCount++;
                SpawnCoin(deathPos);
                continue;
            }

            // Move toward base
            Vector3 ePos = enemyGo[i].transform.position;
            Vector3 dir = (basePos - ePos);
            dir.y = 0;
            dir = dir.normalized * enemySpeed * dt;
            enemyGo[i].transform.position += dir;

            // Keep y
            Vector3 ep = enemyGo[i].transform.position;
            ep.y = 0.8f;
            enemyGo[i].transform.position = ep;

            // Restore color
            GFM_Create.SetColor(enemyGo[i], new Color(0.85f, 0.15f, 0.15f));

            // Check if reached base
            if (Vector3.Distance(enemyGo[i].transform.position, basePos) < 2f)
            {
                eHP[E_BASE] -= 1f;
                enemyGo[i].transform.position = new Vector3(0, -999, 0);
                enemyActive[i] = false;
                enemyState[i] = 0;
            }
        }
    }

    int FindNearestEnemy(Vector3 from, float range)
    {
        float bestDist = range;
        int best = -1;
        for (int i = 0; i < MAX_ENEMIES; i++)
        {
            if (!enemyActive[i] || enemyGo[i] == null) continue;
            float d = Vector3.Distance(from, enemyGo[i].transform.position);
            if (d < bestDist)
            {
                bestDist = d;
                best = i;
            }
        }
        return best;
    }

    int CountActiveEnemies()
    {
        int c = 0;
        for (int i = 0; i < MAX_ENEMIES; i++)
            if (enemyActive[i]) c++;
        return c;
    }

    // ============================================================
    // COIN POOL
    // ============================================================
    void SpawnCoin(Vector3 pos)
    {
        for (int i = 0; i < MAX_COINS; i++)
        {
            if (!coinActive[i] && coinGo[i] != null)
            {
                coinGo[i].transform.position = new Vector3(pos.x, 0.5f, pos.z);
                coinActive[i] = true;
                coinTimer[i] = 0f;
                return;
            }
        }
    }

    void UpdateCoins(float dt)
    {
        for (int i = 0; i < MAX_COINS; i++)
        {
            if (!coinActive[i] || coinGo[i] == null) continue;

            coinTimer[i] += dt;

            // Pop-up animation
            if (coinTimer[i] < 0.3f)
            {
                Vector3 cp = coinGo[i].transform.position;
                cp.y = 0.5f + Mathf.Sin(coinTimer[i] / 0.3f * Mathf.PI) * 1f;
                coinGo[i].transform.position = cp;
            }

            coinGo[i].transform.Rotate(0, 180 * dt, 0);

            // Player proximity pickup
            if (eGo[E_PLAYER] != null)
            {
                Vector3 pPos = eGo[E_PLAYER].transform.position;
                Vector3 cPos = coinGo[i].transform.position;
                float dx = pPos.x - cPos.x;
                float dz = pPos.z - cPos.z;
                float flatDist = Mathf.Sqrt(dx * dx + dz * dz);

                if (flatDist < 2f)
                {
                    gold += 1;
                    coinGo[i].transform.position = new Vector3(0, -999, 0);
                    coinActive[i] = false;
                    continue;
                }
            }

            // Click/tap to collect
            if (clickedThisFrame)
            {
                Vector3 cPos = coinGo[i].transform.position;
                float dx2 = clickWorldPos.x - cPos.x;
                float dz2 = clickWorldPos.z - cPos.z;
                float clickDist = Mathf.Sqrt(dx2 * dx2 + dz2 * dz2);
                if (clickDist < 3f)
                {
                    gold += 1;
                    coinGo[i].transform.position = new Vector3(0, -999, 0);
                    coinActive[i] = false;
                    continue;
                }
            }

            // Auto-collect after 5 seconds
            if (coinTimer[i] > 5f)
            {
                gold += 1;
                coinGo[i].transform.position = new Vector3(0, -999, 0);
                coinActive[i] = false;
            }
        }
    }

    // ============================================================
    // WOOD LOG POOL
    // ============================================================
    void SpawnWoodLog(Vector3 pos)
    {
        for (int i = 0; i < MAX_WOODLOGS; i++)
        {
            if (!woodActive[i] && woodGo[i] != null)
            {
                woodGo[i].transform.position = pos;
                woodActive[i] = true;
                woodState[i] = 1;
                woodCarrier[i] = -1;
                return;
            }
        }
    }

    void UpdateWoodLogs(float dt)
    {
        for (int i = 0; i < MAX_WOODLOGS; i++)
        {
            if (!woodActive[i] || woodGo[i] == null) continue;

            if (woodState[i] == 1)
            {
                // Player proximity pickup
                if (eGo[E_PLAYER] != null)
                {
                    Vector3 pPos = eGo[E_PLAYER].transform.position;
                    Vector3 wPos = woodGo[i].transform.position;
                    float dx = pPos.x - wPos.x;
                    float dz = pPos.z - wPos.z;
                    float flatDist = Mathf.Sqrt(dx * dx + dz * dz);

                    if (flatDist < 2f)
                    {
                        wood += 1;
                        woodGo[i].transform.position = new Vector3(0, -999, 0);
                        woodActive[i] = false;
                        woodState[i] = 0;
                        continue;
                    }
                }

                // Click/tap to collect
                if (clickedThisFrame)
                {
                    Vector3 wPos = woodGo[i].transform.position;
                    float dx2 = clickWorldPos.x - wPos.x;
                    float dz2 = clickWorldPos.z - wPos.z;
                    float clickDist = Mathf.Sqrt(dx2 * dx2 + dz2 * dz2);
                    if (clickDist < 3f)
                    {
                        wood += 1;
                        woodGo[i].transform.position = new Vector3(0, -999, 0);
                        woodActive[i] = false;
                        woodState[i] = 0;
                        continue;
                    }
                }

                // Gentle bob
                Vector3 wp = woodGo[i].transform.position;
                wp.y = 0.4f + Mathf.Sin(Time.time * 2f + i) * 0.15f;
                woodGo[i].transform.position = wp;
            }
        }
    }

    int CountActiveWoodLogs()
    {
        int c = 0;
        for (int i = 0; i < MAX_WOODLOGS; i++)
            if (woodActive[i] && woodState[i] == 1) c++;
        return c;
    }

    // ============================================================
    // WORKER POOL
    // ============================================================
    void SpawnWorker(Vector3 pos)
    {
        for (int i = 0; i < MAX_WORKERS; i++)
        {
            if (!workerActive[i] && workerGo[i] != null)
            {
                workerGo[i].transform.position = pos;
                workerActive[i] = true;
                workerState[i] = 1;
                workerTargetLog[i] = -1;
                return;
            }
        }
    }

    void UpdateWorkers(float dt)
    {
        for (int i = 0; i < MAX_WORKERS; i++)
        {
            if (!workerActive[i] || workerGo[i] == null) continue;

            Vector3 wPos = workerGo[i].transform.position;

            if (workerState[i] == 1)
            {
                // Find nearest wood log
                if (workerTargetLog[i] < 0 || !woodActive[workerTargetLog[i]] || woodState[workerTargetLog[i]] != 1)
                {
                    workerTargetLog[i] = FindNearestWoodLog(wPos);
                }

                if (workerTargetLog[i] >= 0 && woodGo[workerTargetLog[i]] != null)
                {
                    Vector3 logPos = woodGo[workerTargetLog[i]].transform.position;
                    Vector3 dir = (logPos - wPos);
                    dir.y = 0;
                    if (dir.magnitude < 1f)
                    {
                        woodState[workerTargetLog[i]] = 2;
                        woodCarrier[workerTargetLog[i]] = i;
                        workerState[i] = 2;
                    }
                    else
                    {
                        dir = dir.normalized * workerSpeed * dt;
                        workerGo[i].transform.position += dir;
                    }
                }
                else
                {
                    workerState[i] = 3;
                }
            }
            else if (workerState[i] == 2)
            {
                // Carrying to base
                Vector3 dir = (basePos - wPos);
                dir.y = 0;

                if (workerTargetLog[i] >= 0 && workerTargetLog[i] < MAX_WOODLOGS && woodActive[workerTargetLog[i]] && woodGo[workerTargetLog[i]] != null)
                {
                    woodGo[workerTargetLog[i]].transform.position = workerGo[i].transform.position + new Vector3(0, 1.2f, 0);
                }

                if (dir.magnitude < 2f)
                {
                    wood += 1;
                    if (workerTargetLog[i] >= 0 && workerTargetLog[i] < MAX_WOODLOGS && woodActive[workerTargetLog[i]] && woodGo[workerTargetLog[i]] != null)
                    {
                        woodGo[workerTargetLog[i]].transform.position = new Vector3(0, -999, 0);
                        woodActive[workerTargetLog[i]] = false;
                        woodState[workerTargetLog[i]] = 0;
                    }
                    workerTargetLog[i] = -1;
                    workerState[i] = 1;
                }
                else
                {
                    dir = dir.normalized * workerSpeed * dt;
                    workerGo[i].transform.position += dir;
                }
            }
            else if (workerState[i] == 3)
            {
                // Idle - check for new logs
                int newLog = FindNearestWoodLog(wPos);
                if (newLog >= 0)
                {
                    workerTargetLog[i] = newLog;
                    workerState[i] = 1;
                }
            }

            // Keep y
            Vector3 wp2 = workerGo[i].transform.position;
            wp2.y = 0.75f;
            workerGo[i].transform.position = wp2;
        }
    }

    int FindNearestWoodLog(Vector3 from)
    {
        float bestDist = 999f;
        int best = -1;
        for (int j = 0; j < MAX_WOODLOGS; j++)
        {
            if (!woodActive[j] || woodState[j] != 1 || woodGo[j] == null) continue;
            bool taken = false;
            for (int w = 0; w < MAX_WORKERS; w++)
            {
                if (workerActive[w] && workerTargetLog[w] == j && workerState[w] <= 2)
                {
                    taken = true;
                    break;
                }
            }
            if (taken) continue;

            float d = Vector3.Distance(from, woodGo[j].transform.position);
            if (d < bestDist)
            {
                bestDist = d;
                best = j;
            }
        }
        return best;
    }

    int CountActiveWorkers()
    {
        int c = 0;
        for (int i = 0; i < MAX_WORKERS; i++)
            if (workerActive[i]) c++;
        return c;
    }

    // ============================================================
    // HEALTH BAR UPDATE
    // ============================================================
    void UpdateHealthBar()
    {
        if (healthBarGo == null) return;
        float hpRatio = Mathf.Clamp01(eHP[E_BASE] / 50f);
        healthBarGo.transform.localScale = new Vector3(3 * hpRatio, 0.2f, 0.1f);
        healthBarGo.transform.position = new Vector3(0, 4.5f, -3);

        Color hpColor = Color.Lerp(new Color(0.8f, 0.1f, 0.1f), new Color(0.1f, 0.8f, 0.1f), hpRatio);
        GFM_Create.SetColor(healthBarGo, hpColor);

        if (healthBarBgGo != null)
            healthBarBgGo.transform.position = new Vector3(0, 4.5f, -3);
    }

    // ============================================================
    // UI UPDATE
    // ============================================================
    void UpdateUI()
    {
        if (goldText != null) goldText.text = "Gold: " + gold;
        if (woodText != null) woodText.text = "Wood: " + wood;
        if (hpText != null) hpText.text = "Base HP: " + Mathf.CeilToInt(eHP[E_BASE]);

        // CTA timer
        if (ctaActive)
        {
            ctaTimer -= Time.deltaTime;
            if (ctaTimer <= 0 && !gameEnded)
            {
                gameEnded = true;
                try { Luna.Unity.LifeCycle.GameEnded(); } catch (System.Exception) { }
                try { Luna.Unity.Playable.InstallFullGame(); } catch (System.Exception) { }
            }
        }

        // Base click to upgrade (Rule 9 condition)
        if (ruleTriggered[8] && !ruleTriggered[9])
        {
            if (clickedThisFrame)
            {
                baseLevel = 2;
                if (eGo[E_BASE] != null)
                {
                    eGo[E_BASE].transform.localScale = new Vector3(4, 4, 4);
                    GFM_Create.SetColor(eGo[E_BASE], new Color(0.95f, 0.85f, 0.5f));
                }
            }
        }

        // Game over if base HP <= 0
        if (eHP[E_BASE] <= 0 && !gameEnded)
        {
            gameEnded = true;
            ShowGuide("Base destroyed! Download for more!");
            try { Luna.Unity.LifeCycle.GameEnded(); } catch (System.Exception) { }
            try { Luna.Unity.Playable.InstallFullGame(); } catch (System.Exception) { }
        }
    }

    // ============================================================
    // CAMERA UPDATE
    // ============================================================
    void UpdateCamera(float dt)
    {
        if (mainCam == null) return;

        mainCam.orthographicSize = Mathf.Lerp(mainCam.orthographicSize, camZoom, dt * 2f);

        Vector3 playerPos = Vector3.zero;
        if (eGo[E_PLAYER] != null)
            playerPos = eGo[E_PLAYER].transform.position;

        Vector3 lookTarget = Vector3.Lerp(playerPos, camLookTarget, 0.3f);

        float camHeight = 15f;
        float camBack = 15f;
        Vector3 desiredPos = lookTarget + new Vector3(0, camHeight, -camBack);
        mainCam.transform.position = Vector3.Lerp(mainCam.transform.position, desiredPos, dt * 3f);
        mainCam.transform.rotation = Quaternion.Euler(45f, 0f, 0f);
    }

    // ============================================================
    // GUIDE TEXT
    // ============================================================
    void ShowGuide(string text)
    {
        if (guideText != null)
        {
            guideText.text = text;
        }
    }
}
