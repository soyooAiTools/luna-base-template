#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class CreateLunaPool : MonoBehaviour
{
    const string MAT_FOLDER = "Assets/__LunaMaterials";

    [MenuItem("Tools/Create Luna Pool")]
    static void CreateAll()
    {
        // 删除旧的
        GameObject old = GameObject.Find("__LunaPool");
        if (old != null) DestroyImmediate(old);

        // 创建根节点
        GameObject root = new GameObject("__LunaPool");

        int count = 0;

        // 创建基础
        CreateMaterialSource(root);
        CreateGround(root);
        CreateMainLight();
        ConfigureMainCamera();
        // 创建池
        count += CreatePool(root, PrimitiveType.Cube, 5);
        count += CreatePool(root, PrimitiveType.Sphere, 5);
        count += CreatePool(root, PrimitiveType.Cylinder, 3);
        count += CreatePool(root, PrimitiveType.Plane, 3);

        EditorUtility.SetDirty(root);

        Debug.Log("✅ Luna Pool Created: " + count);
    }

    // ===============================
    // 颜色表
    // ===============================

    static (string name, string hex)[] colors =
    {
        ("Red", "#E74C3C"),
        ("Blue", "#3498DB"),
        ("Green", "#2ECC71"),
        ("Yellow", "#F1C40F"),
        ("Orange", "#E67E22"),
        ("Purple", "#9B59B6"),
        ("White", "#FFFFFF"),
        ("Brown", "#8B4513"),
        ("Cyan", "#00BCD4"),
        ("Pink", "#E91E63"),
    };

    // ===============================
    // 创建颜色池
    // ===============================

    static int CreatePool(GameObject root, PrimitiveType type, int perColor)
    {
        int count = 0;

        foreach (var c in colors)
        {
            Color col;
            ColorUtility.TryParseHtmlString(c.hex, out col);

            for (int i = 1; i <= perColor; i++)
            {
                string name =
                    $"__Pool_{type}_{c.name}_{i:00}";

                CreateObj(
                    root,
                    name,
                    type,
                    new Vector3(0, -9999, 0),
                    Vector3.one,
                    c.name,
                    col
                );

                count++;
            }
        }

        return count;
    }

    // ===============================
    // 创建对象
    // ===============================

    static void CreateObj(
        GameObject parent,
        string name,
        PrimitiveType type,
        Vector3 pos,
        Vector3 scale,
        string colorName,
        Color color)
    {
        GameObject obj =
            GameObject.CreatePrimitive(type);

        obj.name = name;
        obj.transform.parent = parent.transform;
        obj.transform.position = pos;
        obj.transform.localScale = scale;

        SetColor(obj, type, colorName, color);

        // Pre-bake ScriptActivator (inactive by default, activated at runtime)
        if (obj.GetComponent<ScriptActivator>() == null)
            obj.AddComponent<ScriptActivator>();
    }

    // ===============================
    // 材质缓存
    // ===============================

    static Dictionary<string, Material> matCache
        = new Dictionary<string, Material>();

    static void SetColor(
        GameObject obj,
        PrimitiveType type,
        string colorName,
        Color color)
    {
        string key = type + "_" + colorName;

        if (!matCache.ContainsKey(key))
        {
            Material mat = GetOrCreateMaterial(
                type,
                colorName,
                color);

            matCache.Add(key, mat);
        }

        obj.GetComponent<Renderer>().sharedMaterial =
            matCache[key];
    }

    // ===============================
    // 创建/加载材质 (.mat)
    // ===============================

    static Material GetOrCreateMaterial(
        PrimitiveType type,
        string colorName,
        Color color)
    {
        if (!AssetDatabase.IsValidFolder(MAT_FOLDER))
        {
            AssetDatabase.CreateFolder(
                "Assets",
                "__LunaMaterials");
        }

        string path =
            $"{MAT_FOLDER}/{type}_{colorName}.mat";

        Material mat =
            AssetDatabase.LoadAssetAtPath<Material>(path);

        if (mat == null)
        {
            mat = new Material(
                Shader.Find(
                    "Universal Render Pipeline/Lit"));

            mat.name = $"{type}_{colorName}";

            mat.SetColor("_BaseColor", color);

            AssetDatabase.CreateAsset(mat, path);
        }

        return mat;
    }

    // ===============================
    // __MaterialSource
    // ===============================

    static void CreateMaterialSource(GameObject root)
    {
        GameObject obj =
            GameObject.CreatePrimitive(
                PrimitiveType.Cube);

        obj.name = "__MaterialSource";

        obj.transform.parent = root.transform;
        obj.transform.position =
            new Vector3(0, -9999, 0);

        Material mat =
            GetOrCreateMaterial(
                PrimitiveType.Cube,
                "Source",
                Color.white);

        obj.GetComponent<Renderer>().sharedMaterial =
            mat;
    }

    // ===============================
    // __Ground
    // ===============================

    static void CreateGround(GameObject root)
    {
        GameObject obj =
            GameObject.CreatePrimitive(
                PrimitiveType.Cube);

        obj.name = "__Ground";

        obj.transform.parent = root.transform;
        obj.transform.position = Vector3.zero;
        obj.transform.localScale =
            new Vector3(50, 0.1f, 50);

        Color col;
        ColorUtility.TryParseHtmlString(
            "#C0C4CC", out col);

        SetColor(
            obj,
            PrimitiveType.Cube,
            "Ground",
            col);
    }

    // ===============================
    // __MainLight
    // ===============================

    static void CreateMainLight()
    {
        GameObject old = GameObject.Find("__MainLight");
        if (old != null) DestroyImmediate(old);
        GameObject lightObj =
            new GameObject("__MainLight");

        Light light =
            lightObj.AddComponent<Light>();

        light.type = LightType.Directional;
        light.intensity = 1f;

        lightObj.transform.rotation =
            Quaternion.Euler(50, -30, 0);
    }
    static void ConfigureMainCamera()
    {
        Camera cam = Camera.main;

        if (cam == null)
        {
            GameObject camObj = new GameObject("Main Camera");
            cam = camObj.AddComponent<Camera>();
            cam.tag = "MainCamera";
        }

        // Position
        cam.transform.position =
            new Vector3(0, 8, -12);

        // Rotation
        cam.transform.rotation =
            Quaternion.Euler(35, 0, 0);

        // Clear Flags
        cam.clearFlags =
            CameraClearFlags.SolidColor;

        // Background color #738B9E
        Color bg;
        ColorUtility.TryParseHtmlString(
            "#738B9E", out bg);

        cam.backgroundColor = bg;
    }
}
#endif