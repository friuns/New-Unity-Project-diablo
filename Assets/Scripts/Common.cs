using System;
using System.Collections.Generic;
using UnityEngine;

public class Base : MonoBehaviour
{
    public virtual void Init()
    {
    }
    public virtual void OnEditorGui()
    {
    }
}

public class Common
{
}

//[Serializable]
//public class BoneWeight2
//{
//    public int boneIndex0;
//    public int boneIndex1;
//    public int boneIndex2;
//    public int boneIndex3;
//    public float weight0;
//    public float weight1;
//    public float weight2;
//    public float weight3;
//}

[Serializable]
public class Rigidbody2
{
    public string name;
    public float angularDrag;
    public Vector3 angularVelocity;
    public Vector3 centerOfMass;
    public CollisionDetectionMode collisionDetectionMode;
    public RigidbodyConstraints constraints;
    public bool detectCollisions;
    public float drag;
    public bool freezeRotation;
    public Vector3 inertiaTensor;
    public Quaternion inertiaTensorRotation;
    public RigidbodyInterpolation interpolation;
    public bool isKinematic;
    public float mass;
    public float maxAngularVelocity;
    public Vector3 position;
    public Quaternion rotation;
    public float sleepAngularVelocity;
    public float sleepVelocity;
    public int solverIterationCount;
    public bool useConeFriction;
    public bool useGravity;
    public Vector3 velocity;
}
[Serializable]
public class CharacterJoint2
{
    public SoftJointLimit highTwistLimit;
    public SoftJointLimit lowTwistLimit;
    public JointDrive rotationDrive;
    public SoftJointLimit swing1Limit;
    public SoftJointLimit swing2Limit;
    public Vector3 swingAxis;
    public Vector3 targetAngularVelocity;
    public Quaternion targetRotation;
    public Vector3 anchor;
    public Vector3 axis;
    public string connectedBody;
}

[Serializable]
public class BoxCollider2
{
    public Vector3 center;
    public Vector3 extents;
    public Vector3 size;
}
[Serializable]
public class SphereCollider2
{
    public Vector3 center; 
    public float radius;
}

[Serializable]
public class CRC
{
    public Transform t;
    public BoxCollider2 BoxCollider2;
    public SphereCollider2 SphereCollider2;
    public CapsuleCollider2 CapsuleCollider2;
    public CharacterJoint2 CharacterJoint2;
    public Rigidbody2 Rigidbody2;

}

[Serializable]
public class CapsuleCollider2
{
    public Vector3 center;
    public int direction;
    public float height;
    public float radius;
}

public class Timer
{
    //public int _Ticks = Environment.TickCount;
    private int framesOffset = new System.Random().Next(100);
    public int oldtime;
    //public int oldFrames;
    public int fpstimes;
    public double totalfps;
    public double GetFps()
    {
        if (fpstimes > 0)
        {
            double fps = (totalfps / fpstimes);
            fpstimes = 0;
            totalfps = 0;
            if (fps == double.PositiveInfinity) return 0;
            return fps;
        }
        else return 0;
    }

    public int milisecondsFromStart;
    public void Update()
    {
        milisecondsFromStart = (int)(Time.timeSinceLevelLoad * 1000);
        milisecondsElapsed = milisecondsFromStart - oldtime;
        //framesElapsed = Time.frameCount - oldFrames;
        if (milisecondsElapsed > 0)
        {
            oldtime = milisecondsFromStart;
            //oldFrames = Time.frameCount;
            fpstimes++;
            totalfps += Time.timeScale / Time.deltaTime;
            //UpdateAction2s();
        }
    }
    //private void UpdateAction2s()
    //{
    //    CA select = null;
    //    lock (_List)
    //        foreach (var _CA in _List)
    //        {
    //            _CA._Miliseconds -= milisecondsElapsed;
    //            if (_CA._Miliseconds < 0 && (_CA.func == null || _CA.func()) && (select == null || select._Miliseconds > _CA._Miliseconds))
    //            {
    //                select = _CA;
    //            }
    //        }
    //    if (select != null)
    //    {
    //        _List.Remove(select);
    //        //try
    //        //{
    //        select._Action2();
    //        //}
    //        //catch (Exception e) { Debug.LogError("Timer:" + e.Message + "\r\n\r\n" + select.stacktrace + "\r\n\r\n"); }
    //    }
    //}

    //public int framesElapsed;
    public int milisecondsElapsed;
    public double _SecodsElapsed { get { return milisecondsElapsed / (double)1000; } }
    public int _oldTime { get { return milisecondsFromStart - milisecondsElapsed; } }
    public bool FramesElapsed(int frames, int framesOffset = 0)
    {
        if (framesOffset == 0)
            framesOffset = this.framesOffset;
        //if (framesElapsed > frames || frames == 0) return true;
        return (Time.frameCount + framesOffset) % frames == 0;
    }

    public bool TimeElapsed(int pMiliseconds)
    {
        if (milisecondsElapsed > pMiliseconds || pMiliseconds == 0) return true;
        if (milisecondsFromStart % pMiliseconds < _oldTime % pMiliseconds)
            return true;
        return false;
    } //if seconds elapsed from last Update() call this function will be called
    //public void Clear()
    //{
    //    //if (Loader._Loader.EnableLogging) Debug.Log("Timer Clear");
    //    lock (_List)
    //        _List.Clear();
    //}

    //internal List<CA> _List = new List<CA>();
    //internal class CA
    //{
    //public string stacktrace;
    //public int _Miliseconds;
    //public Func<bool> func;
    //public Action _Action2;
    //}

}