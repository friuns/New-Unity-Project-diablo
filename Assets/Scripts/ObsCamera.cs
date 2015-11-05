#if UNITY_EDITOR
using UnityEditor;
#endif
using gui = UnityEngine.GUILayout;
using UnityEngine;
using System.Collections;
using System;
using Random = UnityEngine.Random;
public class ObsCamera : Bs
{
    public Transform camT;
    public GameObject[] topDownOnly;
    public Light[] lights;
    public Color ambient;
    void Start()
    {
        ambient = RenderSettings.ambientLight;
        SetPreset();
        SetQuality(RenderingPath.VertexLit != camera.actualRenderingPath);
        if (FindObjectsOfType(typeof(GUILayer)).Length > 1)
            camera.GetComponent<GUILayer>().enabled = false;
        
    }
    public bool InventoryActive { get { return _Hud.Inventory.activeSelf; } }
    private bool oldInv;
    internal float shake;
    private Vector3 vector3;
    public bool highQuality;
    public Vector3 camRot;
    public float clickTime;
    public bool spec { get { return curPreset.spec; } }
    void Update()
    {
        if (curPreset.spec)
        {
            if (Input.GetMouseButtonDown(0) && Time.time - clickTime < .2f)
                Screen.lockCursor = true;
        }
        else
            Screen.lockCursor = false;

        if (Input.GetMouseButtonDown(0))
            clickTime = Time.time;
        if (Input.GetKeyDown(KeyCode.Escape))
            Screen.lockCursor = false;
        vector3 = Vector3.Lerp(vector3, shake * Random.insideUnitSphere, 20 * Time.deltaTime);
        camT.position = (_Player.alive ? _Player.pos : _Player.hipsPos) + vector3 + curPreset.offset;
        if (curPreset.spec)
        {
            camRot.y += Input.GetAxis("Mouse X");
            camRot.x -= Input.GetAxis("Mouse Y");
            camRot.x = Mathf.Clamp(camRot.x, -20, 60);
            camera.transform.localPosition = new Vector3(0, 0, -camera.transform.localPosition.magnitude);
            camera.transform.localRotation = Quaternion.identity;
            camT.rotation = Quaternion.Euler(camRot);
        }
        if (GetKeyDown(KeyCode.Z))
        {
            currentPreset++;
            SetPreset();
        }
        shake = Math.Max(0, shake - Time.deltaTime * 3);
        if (oldInv != InventoryActive)
        {
            if (InventoryActive)
                SetPreset(presets[2]);
            else
                SetPreset();
            Time.timeScale = InventoryActive ? .1f : 1;
            Time.fixedDeltaTime = 0.02f * Time.timeScale;
        }
        oldInv = InventoryActive;
    }

    private void SetPreset(CameraPreset c = null)
    {
        if (c == null)
            c = presets[presetsSwitch[currentPreset % presetsSwitch.Length]];
        curPreset = c;
        camera.transform.localPosition = c.pos;
        camera.transform.localRotation = Quaternion.Euler(c.rot);
        camera.fieldOfView = c.fov;
        camera.farClipPlane = spec ? 30 : 1000;
        camT.rotation = Quaternion.identity;
        RenderSettings.fog = spec && _Dark.night;
        foreach (var a in topDownOnly)
            a.SetActive(!c.spec);
        if (spec)
            RenderSettings.ambientLight = Color.white * .5f;
        else
            RenderSettings.ambientLight = ambient;

        //if (light == 1 && spec)
        //{
        //    light *= .5f;
        //    foreach (var a in lights)
        //        a.intensity *= light;
        //}
        //if (light != 1 && !spec)
        //{
        //    foreach (var a in lights)
        //        a.intensity /= light;
        //    light = 1;
        //}

    }

    //private float light = 1;
    public void SetQuality(bool hq)
    {
        highQuality = hq;
        camera.renderingPath = hq ? RenderingPath.DeferredLighting : RenderingPath.VertexLit;
        Shader.globalMaximumLOD = camera.actualRenderingPath == RenderingPath.VertexLit ? 150 : Android ? 300 : 900;
        Shader.Find("FX/Glass/Stained BumpDistort").maximumLOD = Android ? Shader.globalMaximumLOD / 10 : Shader.globalMaximumLOD;
        if (_Game.shadows != null)
        {
            _Game.shadows.SetActive(_Loader.enableShadows && camera.actualRenderingPath != RenderingPath.DeferredLighting);
        }
    }
    public int currentPreset;
    public int[] presetsSwitch;
    public CameraPreset[] presets;
    public CameraPreset curPreset;
#if UNITY_EDITOR
    public override void OnEditorGui()
    {
        if (gui.Button("SetPreset"))
        {
            Undo.RegisterSceneUndo("rtools");
            var v = presets[currentPreset];
            Camera camera2 = UnityEditor.Selection.activeTransform.GetComponent<Camera>() ? UnityEditor.Selection.activeTransform.GetComponent<Camera>() : camera;
            v.pos = camera2.transform.position;
            v.rot = camera2.transform.rotation.eulerAngles;
            v.fov = camera2.fieldOfView;
            SetDirty();
        }
        if (gui.Button("GetPresset"))
        {
            var c = presets[currentPreset];
            camera.transform.localPosition = c.pos;
            camera.transform.localRotation = Quaternion.Euler(c.rot);
            camera.fieldOfView = c.fov;
        }
        base.OnEditorGui();
    }
#endif
}
[Serializable]
public class CameraPreset
{
    public string Name;
    public bool spec;
    public Vector3 offset;
    public Vector3 pos;
    public Vector3 rot;
    public float fov;
}