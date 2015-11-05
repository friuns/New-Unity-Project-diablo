using System.Linq;
using System;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;

public class Tools : Editor
{

    

    public static Transform curObj;
    private static Vector3 mytp;
    private static Quaternion mytr;
    private static Vector3 myts;
    [MenuItem("RTools/Initgrave")]
    public static void Rename()
    {
        Undo.RegisterSceneUndo("rtools");
        foreach (var b in Selection.transforms)
        {
            int i = 0;
            foreach (Transform t in b.Cast<Transform>().ToArray())
            {
                PrefabUtility.DisconnectPrefabInstance(t.gameObject);
                t.gameObject.layer = (int)Layer.Level;
                t.gameObject.tag = "Destroyable";
                if (i > 0)
                {
                    if (t.collider != null)
                    {
                        var r = t.gameObject.AddComponent<Rigidbody>();
                        var sz = r.collider.bounds.size;
                        var min = sz.magnitude;
                        Debug.Log(min);
                        if (r.inertiaTensor == Vector3.one || min < .1f)
                            t.transform.parent = null;
                        else
                            t.gameObject.SetActive(false);
                    }
                    else
                        t.transform.parent = null;
                }
                i++;
            }

            PrefabUtility.CreatePrefab("Assets/" + b.name + ".prefab", b.gameObject);
        }

        Selection.activeTransform.gameObject.tag = "Destroyable";
    }
    [MenuItem("RTools/Optimize")]
    public static void Optimize()
    {
        Undo.RegisterSceneUndo("rtools");
        Debug.Log(Selection.activeTransform.root.GetComponentsInChildren<Transform>().Length);
        var all = Selection.activeTransform.root.GetComponentsInChildren<Transform>();
        var sk = Selection.activeTransform.root.GetComponentInChildren<SkinnedMeshRenderer>();

        foreach (Transform a in all)
            if (!sk.bones.Contains(a) && a.childCount == 0 && a.renderer == null && (a.tag == null || a.tag == "Untagged" || a.tag == ""))
                DestroyImmediate(a.gameObject);

        for (int i = 0; i < sk.bones.Length; i++)
        {
            if (sk.bones[i] != null && !sk.sharedMesh.boneWeights.Any(a => a.boneIndex0 == i || a.boneIndex1 == i))
                DestroyImmediate(sk.bones[i].gameObject);
        }
        Debug.Log(Selection.activeTransform.root.GetComponentsInChildren<Transform>().Length);
    }


    [MenuItem("RTools/OptimizeCut")]
    public static void Cut()
    {
        Undo.RegisterSceneUndo("rtools");
        Debug.Log(Selection.activeTransform.root.GetComponentsInChildren<Transform>().Length);
        var sk = Selection.activeTransform.root.GetComponentInChildren<SkinnedMeshRenderer>();
        BoneWeight[] boneWeights = sk.sharedMesh.boneWeights.ToArray();

        int pc = ParentCount(Selection.activeTransform);
        foreach (var t in Selection.activeTransform.root.GetComponentsInChildren<Transform>().Where(t => ParentCount(t) == pc))
        {
            int j;
            for (j = 0; j < sk.bones.Length; j++)
                if (sk.bones[j] == t) break;

            foreach (var b in t.GetComponentsInChildren<Transform>())
            {                    
                for (int i = 0; i < sk.bones.Length; i++)
                    if (sk.bones[i] == b)
                    {
                        for (int k = 0; k < sk.sharedMesh.boneWeights.Length; k++)
                            if (sk.sharedMesh.boneWeights[k].boneIndex0 == i)
                            {
                                boneWeights[k].boneIndex0 = j;
                                boneWeights[k].weight0 = 1;
                                boneWeights[k].boneIndex1 = boneWeights[k].boneIndex2 = boneWeights[k].boneIndex3 = 0;
                                boneWeights[k].weight1 = boneWeights[k].weight2 = boneWeights[k].weight3 = 0;
                            }
                    }
            }
            foreach (Transform a in t.Cast<Transform>().ToArray())
                DestroyImmediate(a.gameObject);
        }
        
        sk.sharedMesh.boneWeights = boneWeights;
        AssetDatabase.CreateAsset(Instantiate(sk.sharedMesh), "Assets/" + Selection.activeTransform.root.name + ".asset");    
        Debug.Log(Selection.activeTransform.root.GetComponentsInChildren<Transform>().Length);
    }
    public static int ParentCount(Transform t)
    {
        int i;
        for (i = 0; t != null; i++)
        {
            t = t.parent;
        }
        return i;
    }
    [MenuItem("RTools/CopyTransform")]
    public static void CopyTransform()
    {
        //Undo.RegisterSceneUndo("rtools");
        curObj = Selection.activeTransform;
        mytp = Selection.activeGameObject.transform.localPosition;
        mytr = Selection.activeGameObject.transform.localRotation;
        myts = Selection.activeGameObject.transform.localScale;
    }

    [MenuItem("RTools/PasteValues")]
    public static void PasteValues()
    {
        var to = Selection.activeGameObject.GetComponent<MonoBehaviour>();
        Bs.CopyValues(curObj.GetComponent<MonoBehaviour>(), to);
        EditorUtility.SetDirty(to);
    }

    [MenuItem("RTools/PasteTransform")]
    public static void PasteTransform()
    {
        Undo.RegisterSceneUndo("rtools");
        var t = Selection.activeGameObject.transform;
        t.localPosition = mytp;
        t.localRotation = mytr;
        t.localScale = myts;
        EditorUtility.SetDirty(t);
    }
    [MenuItem("RTools/Copy Animation or texture")]
    public static void CopyAnim()
    {
        var obj = Selection.activeObject;
        string path = Path.GetDirectoryName(AssetDatabase.GetAssetPath(obj)) + "/" + obj.name + ((obj is AnimationClip) ? ".anim" : ".png");
        AssetDatabase.CreateAsset(Instantiate(obj), path);

    }

    [MenuItem("Window/Rtools", false, 0)]
    static void rtoolsclick()
    {
        EditorWindow.GetWindow<InspectorSearch>();
    }
    // Use this for initialization
    private static PropertyInfo m_systemCopyBufferProperty = null;
    private static PropertyInfo GetSystemCopyBufferProperty()
    {
        if (m_systemCopyBufferProperty == null)
        {
            Type T = typeof(GUIUtility);
            m_systemCopyBufferProperty = T.GetProperty("systemCopyBuffer", BindingFlags.Static | BindingFlags.NonPublic);
            if (m_systemCopyBufferProperty == null)
                throw new Exception("Can't access internal member 'GUIUtility.systemCopyBuffer' it may have been removed / renamed");
        }
        return m_systemCopyBufferProperty;
    }
    public static string clipBoard
    {
        get
        {
            PropertyInfo P = GetSystemCopyBufferProperty();
            return (string)P.GetValue(null, null);
        }
        set
        {
            PropertyInfo P = GetSystemCopyBufferProperty();
            P.SetValue(null, value, null);
        }
    }


}















[CustomPropertyDrawer(typeof(LabelAttribute))]
public class LabelDrawer : PropertyDrawer
{

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        GUI.Box(position, "");
        GUI.Label(position, ((LabelAttribute)attribute).d);
    }
}












































//private void BakeMesh(MeshFilter filter)
//    {
//        var bones = new Transform[skinnedMeshRenderer.bones.Length];
//        int boneCount = bones.Length;
//        int transCount = transforms.Length;
//        for (int bone = 0; bone < boneCount; bone++)
//        {
//            string boneName = skinnedMeshRenderer.bones[bone].name;
//            for (int trans = 0; trans < transCount; trans++)
//            {
//                if (transforms[trans].name == boneName)
//                {
//                    bones[bone] = transforms[trans];
//                    break;
//                }
//            }
//        }


//        var originalVertices = filter.sharedMesh.vertices;
//        Mesh mesh = filter.mesh;
//        Matrix4x4[] binds = mesh.bindposes;
//        Vector3[] vertices = mesh.vertices;
//        BoneWeight[] weights = mesh.boneWeights;
//        int vertexCount = mesh.vertexCount;
//        for (int vert = 0; vert < vertexCount; vert++)
//        {
//            int index0 = weights[vert].boneIndex0;
//            float weight0 = weights[vert].weight0;
//            vertices[vert] = cbone(bones[index0], binds[index0], originalVertices[vert], weight0);
//            int index1 = weights[vert].boneIndex1;
//            float weight1 = weights[vert].weight1;
//            if (weight1 > 0f)
//                vertices[vert] += cbone(bones[index1], binds[index1], originalVertices[vert], weight1);
//            int index2 = weights[vert].boneIndex2;
//            float weight2 = weights[vert].weight2;
//            if (weight2 > 0f)
//                vertices[vert] += cbone(bones[index2], binds[index2], originalVertices[vert], weight2);
//            int index3 = weights[vert].boneIndex3;
//            float weight3 = weights[vert].weight3;
//            if (weight3 > 0f)
//                vertices[vert] += cbone(bones[index3], binds[index3], originalVertices[vert], weight3);
//        }
//        mesh.vertices = vertices;
//    }
//private Vector3 cbone(Transform bone, Matrix4x4 binds, Vector3 originalVertices, float weight0)
//    {
//        return (rootBone.worldToLocalMatrix * bone.localToWorldMatrix).MultiplyPoint3x4(binds.MultiplyPoint3x4(originalVertices)) * weight0;
//    }

