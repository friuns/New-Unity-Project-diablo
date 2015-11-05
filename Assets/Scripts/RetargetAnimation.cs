#if UNITY_EDITOR
using System.Linq;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using System.Collections;

public class RetargetAnimation : MonoBehaviour
{
    private Animator anim;
    private Transform[] transforms;
    private List<MyClass> curves = new List<MyClass>();
    private float t;
    private AnimationClip cl;
    private bool oldb = true;
    public float LenFactor = 1;
    //public Transform takeFrom;
    public void Start()
    {
        cl = new AnimationClip();
        //if (takeFrom != null)
        //transforms = takeFrom.GetComponentsInChildren<Transform>();
        //else
        transforms = GetComponentsInChildren<Transform>();
        for (int i = 0; i < transforms.Length; i++)
        {
            var c = new MyClass();
            curves.Add(c);
        }
        anim = GetComponent<Animator>();
    }

    public void Update()
    {
        if (Time.time < 1)
            anim.ForceStateNormalizedTime(0);
        var info = anim.GetCurrentAnimatorStateInfo(0);
        var b = info.normalizedTime <= LenFactor;
        if (oldb && Time.time > 1)
        { 
            if (Time.time - t > .1f || !b)
            {
                t = Time.time;
                for (int i = 1; i < transforms.Length; i++) 
                {
                    SetKey(i, "localPosition.x", transforms[i].localPosition.x);
                    SetKey(i, "localPosition.y", transforms[i].localPosition.y);
                    SetKey(i, "localPosition.z", transforms[i].localPosition.z);

                    SetKey(i, "localRotation.x", transforms[i].localRotation.x);
                    SetKey(i, "localRotation.y", transforms[i].localRotation.y);
                    SetKey(i, "localRotation.z", transforms[i].localRotation.z);
                    SetKey(i, "localRotation.w", transforms[i].localRotation.w);
                }
            }
        }
        oldb = b;
    }

    private void SetKey(int i, string key, float value) 
    {
        if (!curves[i].curves.ContainsKey(key))
            curves[i].curves[key] = new AnimationCurve();

        if (curves[i].curves[key].keys.Length < 2 || curves[i].oldv[key] != value)
        {
            curves[i].curves[key].AddKey(Time.time - 1, value);
            curves[i].oldv[key] = value;
        }
    }

    public void OnApplicationQuit()
    {
        if (transforms == null) return;
        for (int i = 0; i < transforms.Length; i++)
        {
            var path = AnimationUtility.CalculateTransformPath(transforms[i], transform);
            var c = curves[i];
            foreach (KeyValuePair<string, AnimationCurve> a in c.curves)
                cl.SetCurve(path, typeof(Transform), a.Key, a.Value);
            curves.Add(c);
        }

        AssetDatabase.CreateAsset(cl, Selection.activeObject is AnimationClip ? AssetDatabase.GetAssetPath(Selection.activeObject) : "Assets/" + name + ".anim");
        EditorApplication.delayCall += OnRel;
    }

    private void OnRel()
    {
        var g = (RetargetAnimation) FindObjectsOfType(typeof (RetargetAnimation)).FirstOrDefault(a => a.name == name);
        if (g.animation == null)
            g.gameObject.AddComponent<Animation>();

        g.animation.clip = cl;
        g.animation.enabled = false;
        EditorApplication.delayCall -= OnRel;
    }

    public class MyClass
    {
        public Dictionary<string, AnimationCurve> curves = new Dictionary<string, AnimationCurve>();
        public Dictionary<string, float> oldv = new Dictionary<string, float>();
    }
}

#endif