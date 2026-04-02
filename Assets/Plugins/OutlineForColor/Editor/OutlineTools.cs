using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class OutlineTools : EditorWindow
{
    public GameObject _target;
    public Material _outlineMaterial;
    public bool _useTangent = false;

    [MenuItem("Tools/计算描边数据")]
    public static void ShowWindow()
    {
        var window = GetWindow<OutlineTools>();
    }

    private void OnGUI()
    {
        _target = EditorGUILayout.ObjectField("对象实例", _target, typeof(GameObject), true) as GameObject;
        _outlineMaterial = EditorGUILayout.ObjectField("描边材质", _outlineMaterial, typeof(Material), true) as Material;
        _useTangent = EditorGUILayout.Toggle("使用切线", _useTangent);

        if (GUILayout.Button("保存"))
        {
            if (_target == null)
            {
                EditorUtility.DisplayDialog("错误", "不能为空", "好");
                return;
            }

            MeshFilter[] meshFilters = _target.GetComponentsInChildren<MeshFilter>();
            foreach (var meshFilter in meshFilters)
            {
                meshFilter.sharedMesh = WirteAverageNormalToTangent(meshFilter.sharedMesh);
            }

            MeshRenderer[] meshRenderers = _target.GetComponentsInChildren<MeshRenderer>();

            foreach (MeshRenderer meshRenderer in meshRenderers)
            {
                if (_outlineMaterial != null)
                {
                    bool isHave = false;

                    foreach (Material item in meshRenderer.sharedMaterials)
                    {
                        if (item.shader == _outlineMaterial.shader)
                        {
                            isHave = true;
                            break;
                        }
                    }

                    if (isHave)
                    {
                        continue;
                    }

                    Material[] materials = new Material[meshRenderer.sharedMaterials.Length + 1];

                    for (int i = 0; i < meshRenderer.sharedMaterials.Length; i++)
                    {
                        materials[i] = meshRenderer.sharedMaterials[i];
                    }

                    materials[meshRenderer.sharedMaterials.Length] = _outlineMaterial;
                    meshRenderer.sharedMaterials = materials;
                }
            }

            SkinnedMeshRenderer[] skinMeshRenders = _target.GetComponentsInChildren<SkinnedMeshRenderer>();
            foreach (var skinMeshRender in skinMeshRenders)
            {
                skinMeshRender.sharedMesh = WirteAverageNormalToTangent(skinMeshRender.sharedMesh);

                if (_outlineMaterial != null)
                {
                    bool isHave = false;

                    foreach (Material item in skinMeshRender.sharedMaterials)
                    {
                        if (item.shader == _outlineMaterial.shader)
                        {
                            isHave = true;
                            break;
                        }
                    }

                    if (isHave)
                    {
                        continue;
                    }

                    Material[] materials = new Material[skinMeshRender.sharedMaterials.Length + 1];

                    for (int i = 0; i < skinMeshRender.sharedMaterials.Length; i++)
                    {
                        materials[i] = skinMeshRender.sharedMaterials[i];
                    }

                    materials[skinMeshRender.sharedMaterials.Length] = _outlineMaterial;
                    skinMeshRender.sharedMaterials = materials;
                }
            }
        }

        EditorGUILayout.LabelField("保存目录为：Assets/OutlineMesh", EditorStyles.wordWrappedLabel);
    }

    private Mesh WirteAverageNormalToTangent(Mesh mesh)
    {
        var folderPath = Path.Combine("Assets", "OutlineMesh");

        if (!AssetDatabase.IsValidFolder(folderPath))
        {
            AssetDatabase.CreateFolder("Assets", "OutlineMesh");
        }

        var assetPath = Path.Combine(folderPath, mesh.name + ".asset");

        Mesh assetMesh = AssetDatabase.LoadAssetAtPath<Mesh>(assetPath);

        // 已经存在则返回已经存在的
        if (assetMesh != null)
        {
            return assetMesh;
        }

        var averageNormalHash = new Dictionary<Vector3, Vector3>();
        for (var j = 0; j < mesh.vertexCount; j++)
        {
            if (!averageNormalHash.ContainsKey(mesh.vertices[j]))
            {
                averageNormalHash.Add(mesh.vertices[j], mesh.normals[j]);
            }
            else
            {
                averageNormalHash[mesh.vertices[j]] = (averageNormalHash[mesh.vertices[j]] + mesh.normals[j]).normalized;
            }
        }
        
        var averageNormals = new Vector4[mesh.vertexCount];
        for (var j = 0; j < mesh.vertexCount; j++)
        {
            averageNormals[j] = averageNormalHash[mesh.vertices[j]];
        }

        var vertextColors = new Color[mesh.vertexCount];
        for (var j = 0; j < mesh.vertexCount; j++)
        {
            Vector3 averageNormal = averageNormals[j];
            vertextColors[j] = new Color(averageNormal.x, averageNormal.y, averageNormal.z);
        }

        Mesh instanceMesh = Instantiate(mesh);

        if (_useTangent)
        {
            instanceMesh.tangents = averageNormals;
        }
        else
        {
            instanceMesh.colors = vertextColors;
        }

        AssetDatabase.CreateAsset(instanceMesh, assetPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        return AssetDatabase.LoadAssetAtPath<Mesh>(assetPath);
    }
}
