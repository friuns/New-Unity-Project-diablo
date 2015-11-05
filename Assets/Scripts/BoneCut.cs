
using System;
using System.Collections.Generic;
using System.Linq;
//using UnityEditor;
using UnityEngine;
using System.Collections;
using Random = UnityEngine.Random;
[Serializable]
public class BoneSave
{
    public int vertex;
    public int i;
    public int boneid;
}
public class BoneCut : Bs
{
    public SkinnedMeshRenderer model;
    public Transform[] UpperBones;
    public Transform[] lowerBones;
    public List<BoneSave> boneSave = new List<BoneSave>();
    public List<int> triangleSave = new List<int>();
    private Plane plane;
    
    private BoneWeight[] boneWeights;
    private Vector3[] vertices;
    List<int> lbID = new List<int>();
    List<int> ubID = new List<int>();
    public bool drawGizmo;
    public bool Critical;
    public bool waist;
    public List<BoneSave> temp = new List<BoneSave>();
    
    //public CharacterJoint characterJoint;
    public override void OnEditorGui()
    {
        if (GUILayout.Button("Save Split"))
        {
            Save();
        }
        base.OnEditorGui();
    }
    public void InitMe()
    {
        Destroy(collider);
        Destroy(renderer);
        Save();
    }
    public override void Awake()
    {
        if (drawGizmo)
        {
            Destroy(collider);
            Save();            
        }
        base.Awake();
    }
    public void Start()
    {
        if (drawGizmo)
        {
            var r = model;
            Cut2(r, UpperBones[0]);
            var rg = UpperBones[0].GetComponentsInChildren<Rigidbody>();
            foreach (Rigidbody a in rg)
            {
                if (waist)
                    Debug.Log(a.name, a);
                if (a.collider != null)
                    a.collider.enabled = true;
                a.isKinematic = false;
                a.velocity += Vector3.up + Random.insideUnitSphere*2;
            }
        }
    }
    public bool Split=true;
    public bool cutted;
    public void Cut(Vector3 h, float force, bool createBlood = true)
    {
        if (cutted) return;
        cutted = true;
        foreach (var a in temp)
            if (UpperBones[0] == null || UpperBones[0].GetComponentsInChildren<Transform>().Length <= a.i)
                return;

        //if (UpperBones[0] == null) return;
        UpperBones[0].parent = null;
        var upperBone = Split ? (Transform)Instantiate(UpperBones[0], UpperBones[0].position, UpperBones[0].rotation) : UpperBones[0];
        //if (waist) transform.root.GetComponent<Zombie>().waistSplitted = true;
        var r = model;
        Cut2(r, upperBone);


        var bones = new List<Transform>(r.bones);
        var at = upperBone.GetComponentsInChildren<Transform>();
        foreach (var a in temp)
            //if (at.Length > a.i)
            bones[a.boneid] = at[a.i];



        r.bones = bones.ToArray();

        var rg = upperBone.GetComponentsInChildren<Rigidbody>();
        foreach (Rigidbody a in rg)
        {
            if (a.collider != null)
            {
                a.collider.enabled = true;
                a.collider.material = null;
            }
            a.isKinematic = false;
            AddExplosionForce(a, force, h);
        }
        if (createBlood)
        {
            var g = (GameObject)Instantiate(_Database.pouringBlood, upperBone.position, upperBone.rotation);
            g.transform.parent = upperBone;
            var g2 = (GameObject)Instantiate(_Database.pouringBlood, upperBone.position, upperBone.rotation);
            g2.transform.parent = lowerBones[0];
        }
        if (Split)
            Destroy(UpperBones[0].gameObject);
        _Game.StartCoroutine(AddMethod(5, delegate
        {
            foreach (Rigidbody a in upperBone.GetComponentsInChildren<Rigidbody>())
            {
                var joint = a.GetComponent<CharacterJoint>();
                Destroy(joint);
                Destroy(a);
                Destroy(a.collider);
            }
        }));

    }

    private void Cut2(SkinnedMeshRenderer r, Transform upperBone)
    {
        if (Split)
        {
            var triangles = r.sharedMesh.triangles;
            boneWeights = r.sharedMesh.boneWeights;
            foreach (int a in triangleSave)
            {
                triangles[a] = 0;
                triangles[a + 1] = 0;
                triangles[a + 2] = 0;
            }

            foreach (var a in boneSave)
            {
                SetBone(ref boneWeights[a.vertex], a.i, a.boneid);
            }

            r.sharedMesh = (Mesh) Instantiate(r.sharedMesh);
            r.sharedMesh.boneWeights = boneWeights;
            r.sharedMesh.triangles = triangles;
        }
        if (waist)
        {
            var addComponent = upperBone.gameObject.AddComponent<Rigidbody>();
            foreach (Transform a in upperBone)
            {
                var component = a.GetComponent<CharacterJoint>();
                if (component != null)
                    component.connectedBody = addComponent;
            }
        }
        else
            //if (characterJoint != null)
            //Destroy(characterJoint);
            //else
            Destroy(upperBone.GetComponent<CharacterJoint>());

        upperBone.parent = null;
    }

    void Save()
    {
        model = transform.root.GetComponentInChildren<SkinnedMeshRenderer>();

        if (!Split)
            return;
        //foreach (var a in this.transform.root.GetComponentsInChildren<Rigidbody>())
        //    a.isKinematic = true;

        var r = model;
        var lcp = r.transform.localPosition;
        var lcr = r.transform.localRotation;
        var lcp2 = r.transform.root.position;
        var lcr2 = r.transform.root.rotation;
        r.transform.localPosition = Vector3.zero;
        r.transform.localRotation = Quaternion.identity;
        r.transform.root.position = Vector3.zero;
        r.transform.root.rotation = Quaternion.identity;

        

        plane = new Plane(transform.up, transform.position);
        var sharedMesh = new Mesh();
        r.BakeMesh(sharedMesh);
        vertices = sharedMesh.vertices;
        var Points = new Point[vertices.Length];

        var triangles = sharedMesh.triangles;
        boneWeights = r.sharedMesh.boneWeights;
        var bones = new List<Transform>(r.bones);
        foreach(var a in UpperBones)
        {
            var indexOf = bones.IndexOf(a);
            if (indexOf != -1)
                ubID.Add(indexOf);
        }
        if (ubID.Count == 0) throw new Exception("upper bone not found ");
        foreach (var a in lowerBones)
        {
            var indexOf = bones.IndexOf(a);
            if (indexOf != -1)
                lbID.Add(indexOf);
        }
        if (lbID.Count == 0) throw new Exception("lower bone not found ");

        Transform[] bt = UpperBones[0].GetComponentsInChildren<Transform>();
        for (int i = 0; i < bt.Length; i++)
        {
            var indexOf = bones.IndexOf(bt[i]);
            if (indexOf != -1)
                temp.Add(new BoneSave() { boneid = indexOf, i = i });
        }
        
        Dictionary<KeyValuePair<int, int>, int> dict = new Dictionary<KeyValuePair<int, int>, int>();
        for (int i = 0; i < vertices.Length; i++)
        {
            Points[i] = new Point(vertices[i]) { index = i };
            var side = plane.GetSide(vertices[i]);

            var v = vertices[i];
            v += plane.normal * -plane.GetDistanceToPoint(v);
            if (this.collider.bounds.Contains(v))
                for (int j = 0; j < 4; j++)
                {
                    var boneid = GetBone(boneWeights[i], j);
                    if (lbID.Contains(boneid) && side)
                    {
                        SetBone(ref boneWeights[i], j, ubID[0]);
                        dict[new KeyValuePair<int, int>(i, j)] = ubID[0];
                    }

                    if (ubID.Contains(boneid) && !side)
                    {
                        SetBone(ref boneWeights[i], j, lbID[0]);
                        dict[new KeyValuePair<int, int>(i, j)] = lbID[0];
                    }
                }
        }
        boneSave.Clear();
        foreach (var a in dict)
            boneSave.Add(new BoneSave() { vertex = a.Key.Key, i = a.Key.Value, boneid = a.Value });
        triangleSave.Clear();
        for (int i = 0; i < triangles.Length / 3; i++)
        {
            var index0 = i * 3;
            Point p0 = Points[triangles[index0]];
            Point p1 = Points[triangles[index0 + 1]];
            Point p2 = Points[triangles[index0 + 2]];

            var sameSide = SameSide(p0, p1) && SameSide(p0, p2) && SameSide(p1, p2);
            if (drawGizmo)
            {
                Debug.DrawLine(p0.pos, p1.pos, sameSide ? Color.blue : Color.red, 5);
                Debug.DrawLine(p1.pos, p2.pos, sameSide ? Color.blue : Color.red, 5);
                Debug.DrawLine(p2.pos, p0.pos, sameSide ? Color.blue : Color.red, 5);
            }
            if (!sameSide)
            {
                triangleSave.Add(index0);
                triangles[index0] = 0;
                triangles[index0 + 1] = 0;
                triangles[index0 + 2] = 0;
            }
        }
        //r.sharedMesh = (Mesh)Instantiate(r.sharedMesh);
        //r.sharedMesh.boneWeights = boneWeights.ToArray();
        //r.sharedMesh.triangles = triangles.ToArray();
        SetDirty();

        r.transform.localPosition = lcp;
        r.transform.localRotation = lcr;
        r.transform.root.position = lcp2;
        r.transform.root.rotation = lcr2;
    }

    //private List<KeyValuePair<Vector3, Vector3>> WentPoints = new List<KeyValuePair<Vector3, Vector3>>();
    private bool SameSide(Point p, Point p2)
    {
        var sameSide = plane.SameSide(p.pos, p2.pos);
        if (!sameSide)
        {
            var v = (p2.pos + p.pos) / 2;
            v += plane.normal * -plane.GetDistanceToPoint(v);
            if (!this.collider.bounds.Contains(v))
                sameSide = true;
        }
        return sameSide;
    }


    public static int GetBone(BoneWeight bw, int i)
    {
        if (i == 0)
            return bw.boneIndex0;
        else if (i == 1)
            return bw.boneIndex1;
        else if (i == 2)
            return bw.boneIndex2;
        else if (i == 3)
            return bw.boneIndex3;
        throw new Exception("44444");
    }
    public static void SetBone(ref BoneWeight bw, int i, int id)
    {
        if (i == 0)
            bw.boneIndex0 = id;
        else if (i == 1)
            bw.boneIndex1 = id;
        else if (i == 2)
            bw.boneIndex2 = id;
        else if (i == 3)
            bw.boneIndex3 = id;
        else
            throw new Exception("4444");
    }
}
[Serializable]
public class Point
{
    public int index;
    public int triangle;
    public Vector3 pos;
    public bool first;
    public List<Point> links = new List<Point>();
    public Point(Vector3 pos)
    {
        this.pos = pos;
    }
}