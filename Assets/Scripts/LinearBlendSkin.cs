using UnityEngine;
using System.Collections;
public class LinearBlendSkin : MonoBehaviour
{
    private MeshFilter filter;
    private Vector3[] originalVertices;
    private Transform[] bones;
    public SkinnedMeshRenderer prefabRenderer;
    void Start()
    {
        bones = new Transform[prefabRenderer.bones.Length];
        Transform[] transforms = transform.root.GetComponentsInChildren<Transform>();
        int boneCount = bones.Length;
        int transCount = transforms.Length;
        for (int bone = 0; bone < boneCount; bone++)
        {
            string boneName = prefabRenderer.bones[bone].name;
            for (int trans = 0; trans < transCount; trans++)
            {
                if (transforms[trans].name == boneName)
                {
                    bones[bone] = transforms[trans];
                    break;
                }
            }
        }
        filter = GetComponent<MeshFilter>();
        originalVertices = filter.sharedMesh.vertices;
        
    }
    void LateUpdate()
    {
        Mesh mesh = filter.mesh;
        Matrix4x4[] binds = mesh.bindposes;
        Vector3[] vertices = mesh.vertices;
        BoneWeight[] weights = mesh.boneWeights;
        int vertexCount = mesh.vertexCount;
        for (int vert = 0; vert < vertexCount; vert++)
        {
            int index0 = weights[vert].boneIndex0;
            float weight0 = weights[vert].weight0;
            vertices[vert] = bones[index0].localToWorldMatrix.MultiplyPoint3x4(binds[index0].MultiplyPoint3x4(originalVertices[vert])) * weight0;
            int index1 = weights[vert].boneIndex1;
            float weight1 = weights[vert].weight1;
            if (weight1 > 0f)
                vertices[vert] += bones[index1].localToWorldMatrix.MultiplyPoint3x4(binds[index1].MultiplyPoint3x4(originalVertices[vert])) * weight1;
            int index2 = weights[vert].boneIndex2;
            float weight2 = weights[vert].weight2;
            if (weight2 > 0f)
                vertices[vert] += bones[index2].localToWorldMatrix.MultiplyPoint3x4(binds[index2].MultiplyPoint3x4(originalVertices[vert])) * weight2;
            int index3 = weights[vert].boneIndex3;
            float weight3 = weights[vert].weight3;
            if (weight3 > 0f)
                vertices[vert] += bones[index3].localToWorldMatrix.MultiplyPoint3x4(binds[index3].MultiplyPoint3x4(originalVertices[vert])) * weight3;
        }
        mesh.vertices = vertices;
    }
}