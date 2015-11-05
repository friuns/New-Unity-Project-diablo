using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using System.Collections;
public enum OptState { rig, saverig, disableRig, hardRig }
public class Loader : Bs
{
    public UIAtlas atlas;
    public Tooltip toolTip;
    //public bool optimizeMats;
    public bool enableShadows;
    public bool enableGraves;
    public bool showKeys;
    public bool PreAwakeObjects;
    
    public bool FrameOptimization = false;
    public int frameFactor = 1;
    public OptState rigOpt;
    //internal bool disableRigidBodies { get { return rigOpt == OptState.disableRig; } }
    public override void Awake()
    {
        if (FindObjectsOfType(typeof(Loader)).Length > 1)
        {
            DestroyImmediate(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);
        if (Application.platform == RuntimePlatform.Android)
            Android = true;
        //if (enableShadows == null)
        //    enableShadows = _Loader.m_enableShadows;
        base.Awake();
    }

    public bool m_10Fps;
    public new bool Android;
    public bool Immortal;
    public bool EnableDeadColliders;
    public void Start()
    {        
        print("Loader Start");
        toolTip.gameObject.SetActive(true);
    }
    public void OnApplicationQuit()
    {
        Shader.globalMaximumLOD = 600;
        exiting = true;
    }
    public bool InstanciateZombies;
    public GUIText text;
    public GUIText varTrace;
    public string exitError;
    public void Update()
    {

#if UNITY_FLASH
        //if(showKeys)
        //foreach (var a in keyTexts)
        //    sb += a.Value + "\t" + a.Key + "\r\n";
        text.text = sb;
        sb = "";
#else
        //if (showKeys)
        //    foreach (var a in keyTexts)
        //        sb.AppendFormat("{0}\t{1}\r\n", a.Value, a.Key);
        text.text = sb.ToString();
        sb = new StringBuilder();
#endif

    }

    public bool enableWakeup;
    public bool hideEffects;

    
    public bool m_enableSounds = true;
    public bool enableSounds { get { return m_enableSounds; } }
    //public bool m_enableOutline;
    //public bool enableOutline { get { return m_enableOutline; } }
}
