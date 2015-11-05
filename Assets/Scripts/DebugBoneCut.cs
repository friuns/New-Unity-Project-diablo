using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public class DebugBoneCut : MonoBehaviour {

	void Start ()
	{
        var r = transform.root.GetComponentInChildren<SkinnedMeshRenderer>();

        r.transform.localPosition = Vector3.zero;
        r.transform.localRotation = Quaternion.identity;
        r.transform.root.position = Vector3.zero;
        r.transform.root.rotation = Quaternion.identity;
        plane = new Plane(transform.up, transform.position);
        var sharedMesh = new Mesh();
        r.BakeMesh(sharedMesh);

	    var triangles = sharedMesh.triangles;
	    var Points = sharedMesh.vertices;
	    for (int i = 0; i < triangles.Length/3; i++)
	    {
	        var index0 = i*3;
            var p0 = Points[triangles[index0]];
            var p1 = Points[triangles[index0 + 1]];
            var p2 = Points[triangles[index0 + 2]];

            var sameSide = SameSide(p0, p1) && SameSide(p0, p2) && SameSide(p1, p2);
            Debug.DrawLine(p0, p1, sameSide ? Color.blue : Color.red, 5);
            Debug.DrawLine(p1, p2, sameSide ? Color.blue : Color.red, 5);
            Debug.DrawLine(p2, p0, sameSide ? Color.blue : Color.red, 5);
            if (!sameSide)
            {
                //Debug.Log(r.sharedMesh.boneWeights[triangles[index0]].boneIndex0);
                var boneIndex0 = r.sharedMesh.boneWeights[triangles[index0]].boneIndex0;
                var bone = r.bones[boneIndex0];
                Debug.Log(bone.name+"    "+boneIndex0+"/"+r.bones.Length, bone);
	        }
	    }

	}
    private Plane plane;
    private bool SameSide(Vector3  p, Vector3 p2)
    {
        var sameSide = plane.SameSide(p, p2);
        //if (!sameSide)
        //{
        //    var v = (p2+ p) / 2;
        //    v += plane.normal * -plane.GetDistanceToPoint(v);
        //    if (!this.collider.bounds.Contains(v))
        //        sameSide = true;
        //}
        return sameSide;
    }

}
