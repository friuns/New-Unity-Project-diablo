using gui = UnityEngine.GUILayout;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using System.Collections;
using Random = UnityEngine.Random;
public class Game : Bs
{

    public AnimationClip test;
    public static int switchChar;
    public TextMesh scoreText;
    public GameObject blood;
    public GameObject bloodSmall;
    public GameObject bloodSplatter;
    public Material[] bloodTextures;
    public Material bloodMaterial;
    public TextAsset dictFile;
    public Transform PhysxEffect;
    public Animation PhysxEffectAnim;
    public AnimationCurve rotationDistanceCurve;
    public AnimationCurve moveToPointCurve;
    public bool ZombiesFromAllDirs;
    public ParticleSystem dustParticles;
    public WeaponSettings[] wepSettings;
    public GameObject coin;
    public GameObject firePrefab;
    public GameObject hammerFire;
    internal List<Slot> inventorySlots = new List<Slot>();
    internal List<Slot> plSlots = new List<Slot>();
    public Transform firstSlot;
    public Transform zombieSPawn;
    public Transform LaserStream;
    public Transform LaserStreamEnd;
    public List<Monkey> monkeys = new List<Monkey>();
    private double fps;
    Timer timer = new Timer();
    internal List<Zombie> zombies = new List<Zombie>();
    internal List<Destroyable> destroyables = new List<Destroyable>();
    internal List<Shared> aliveZombies = new List<Shared>();
    internal List<Shared> visibleZombies = new List<Shared>();
    internal List<Zombie> canStandup = new List<Zombie>();
    public GameObject[] zombiePrefabs;
    public GameObject[] zombiePrefabs1;
    public GameObject[] zombiePrefabs2;
    public Shader[] replaceShadersFrom;
    public Shader[] replaceShadersTo;
    public List<Shader> replaceOnFlash;
    public AnimationCurve aniDmgCurve;
    public AnimationCurve zombJumpCurve;
    public bool alwaysTargetPlayer;
    
    public void Start()
    {
        wepSettings = (WeaponSettings[])FindObjectsOfType(typeof(WeaponSettings));
        if (_Loader.InstanciateZombies)
            CreateZombies(_Game.zombiePrefabs);
        if (Application.isEditor)
            GameObject.Find("music").SetActive(false);
        List<Item> nwItems = new List<Item>();
        for (int i = 0; i < 15; i++)
        {
            var c = (Item)Instantiate(_Database.items[i % _Database.items.Count]);
            c.Init();
            c.transform.parent = _Database.transform;
            nwItems.Add(c);
        }
        foreach (var item in _Database.items)
            item.Init();
        _Database.items = nwItems;
        var e = _Database.items.GetEnumerator();
        for (int j = 0; j < 3; j++)
            for (int i = 0; i < 5; i++)
            {
                Transform g = (Transform)Instantiate(firstSlot);
                g.transform.parent = firstSlot.transform.parent;
                var slot = g.GetComponent<Slot>();
                if (e.MoveNext() && e.Current.playerHave)
                    slot.item = e.Current;
                inventorySlots.Add(slot);
                g.transform.localPosition = new Vector3(i, -j, 0) * 1.02f;
                g.transform.localScale = Vector3.one;
            }
        firstSlot.gameObject.SetActive(false);

        if (_Loader.PreAwakeObjects)    
            for (int i = 0; i < zombiePrefabs.Length; i++)
            {
                var zombiePrefab = zombiePrefabs[i];
                zombiePrefabs[i] = (GameObject)Instantiate(zombiePrefab);
                zombiePrefabs[i].name = zombiePrefab.name;
                zombiePrefabs[i].gameObject.SetActive(false);
            }


        
    }

    public GameObject shadows;
    public RenderTexture shadowsRendererTexture;
    internal float harlemShake = float.MinValue;
    public void Update()
    {
        aliveZombies.Clear();
        visibleZombies.Clear();
        canStandup.Clear();
        foreach (var a in zombies)
        {
            a.visible = Rect.MinMaxRect(0, 0, 1, 1).Contains(camera.WorldToViewportPoint(a.pos));
            if (a.life < 0 && a.standUp != null && a.visible)
                canStandup.Add(a);
            if (a.alive)
            {
                aliveZombies.Add(a);
                if (a.visible)
                    visibleZombies.Add(a);
            }
        }
        
        if (Input.GetKeyDown(KeyCode.M))
        {
            harlemShake = Time.time;
            PlaySound(_Sounds.harlemShake, .04f);
            StartCoroutine(AddMethod(15.5f, delegate
            {
                foreach (Zombie a in zombies)
                    a.dancet = Time.time;
            }));
        }
        DebugKeys();
        _Loader.Log("Zombies: " + zombies.Count);
        _Loader.Log("VisibleZombies: " + visibleZombies.Count);        
        if (timer.TimeElapsed(1000))
            fps = (int)timer.GetFps();
        _Loader.Log("Fps: " + fps);
        //clear(zombies);
        //clear(destroyables);
        timer.Update();
    }

    private void clear(IList shareds)
    {
        for (int i = shareds.Count - 1; i >= 0; i--)
        {
            if (!((MonoBehaviour)shareds[i]).enabled)
                shareds.Remove(shareds[i]);
        }
    }

    private void DebugKeys()
    {
        if (GetKeyDown(KeyCode.T, "Enable burn out"))
            _Loader.enableWakeup = !_Loader.enableWakeup;
        if (GetKeyDown(KeyCode.Y, "Change Rig Mode: "+_Loader.rigOpt))
        {
            _Loader.rigOpt = _Loader.rigOpt == OptState.saverig ? OptState.rig : OptState.saverig;
            LoadLevel();
        }
        if (GetKeyDown(KeyCode.Alpha4, "Enable Shadows " + _Loader.enableShadows))
        {
            _Loader.enableShadows = !_Loader.enableShadows;
            shadows.SetActive(_Loader.enableShadows);
        }
        if(GetKeyDown(KeyCode.Alpha5))
            _Hud.ngui.SetActive(!_Hud.ngui.activeSelf);
        if (GetKeyDown(KeyCode.X, "Immortality: " + _Loader.Immortal))
            _Loader.Immortal = !_Loader.Immortal;
        if (GetKeyDown(KeyCode.B, "AwakeZombies"))
        {
            foreach (Zombie a in zombies)
                if (!a.alive)
                    a.standUp2();            
        }
        if (GetKeyDown(KeyCode.K, "kill zombies"))
            foreach (Zombie a in zombies)
                if (a.alive && !a.healer)
                    a.SetLife(1000, Vector3.zero, 100);

        if (GetKeyDown(KeyCode.Alpha1, "Create zombies"))
            CreateZombies(_Game.zombiePrefabs);
        if (GetKeyDown(KeyCode.F2))
        {
            _Loader.PreAwakeObjects = !_Loader.PreAwakeObjects;
            LoadLevel();
        }
        if (GetKeyDown(KeyCode.J, "Conf Stats: " + _Hud.stats.SlowDown, true))
            _Hud.stats.SlowDown = 100;

        if (GetKeyDown(KeyCode.Alpha2, "Create zombiesHigh"))
            CreateZombies(_Game.zombiePrefabs1);
        if (GetKeyDown(KeyCode.Alpha3, "Create zombiesLow"))
            CreateZombies(_Game.zombiePrefabs2, 1);
        if (GetKeyDown(KeyCode.P, "quit"))
        {
            print("Application.Quit");
            Application.Quit();
        }
        if (GetKeyDown(KeyCode.R, "Restart"))
        {
            ClearLog();
            LoadLevel();
        }

        if (GetKeyDown(KeyCode.U, "Frame Optimization " + _Loader.FrameOptimization))
            _Loader.FrameOptimization = !_Loader.FrameOptimization;

        if (GetKeyDown(KeyCode.N, "10 Fps"))
        {
            if (Application.targetFrameRate != 10)
            {
                QualitySettings.vSyncCount = 0;
                Application.targetFrameRate = 10;
            }
            else
            {
                QualitySettings.vSyncCount = 0;
                Application.targetFrameRate = 0;
            }
        }
        if (Input.GetKeyDown(KeyCode.H))
        {
            _Loader.showKeys = !_Loader.showKeys;
        }
        if (GetKeyDown(KeyCode.LeftControl, "Pause"))
        {
            if (Application.isEditor)
                Debug.Break();
            else
                Time.timeScale = Time.timeScale == 0 ? 1 : 0;
        }
        if (GetKeyDown(KeyCode.Q, "Change Quality " + camera.actualRenderingPath))
        {
            bool hq = camera.actualRenderingPath != RenderingPath.VertexLit;
            _ObsCamera.SetQuality(!hq);
        }
    }
    private static void LoadLevel()
    {
        Application.LoadLevel(Application.loadedLevel);
    }
    private void CreateZombies(GameObject[] gameObjects,int cnt=20)
    {
        for (int i = 0; i < cnt; i++)
        {
            var id = Random.Range(0, gameObjects.Length);
            var z = (GameObject)Instantiate(gameObjects[id]);
            if (_Loader.PreAwakeObjects)
                z.SetActive(true);
            if (!ZombiesFromAllDirs)
                z.transform.position = zombieSPawn.transform.position;
            else
            {
                z.transform.position = ZeroY(_Player.pos + Random.insideUnitSphere * Zombie.AvatarRange);
            }
        }
    }
//    public void OnApplicationQuit()
//    {
//#if UNITY_EDITOR
//        EditorApplication.delayCall = delegate
//        {
//            EditorApplication.delayCall = null;
//            var game = (Game)FindObjectOfType(typeof(Game));
//            Undo.RegisterUndo(game, "rtools");
//            game.moveToPointCurve = moveToPointCurve;
//            EditorUtility.SetDirty(game);
//        };
//#endif
//    }
    public void CreateBlood(Vector3 position, Vector3 lookRot, bool big, GameObject customBlood = null)
    {
        if (customBlood == null)
            customBlood = blood;
        var instantiate = (GameObject)Instantiate(customBlood, position, Quaternion.LookRotation(lookRot));
        if (_Loader.hideEffects)
            instantiate.hideFlags = HideFlags.HideInHierarchy;
        Destroy(instantiate, 1);
    }
    public void Text(string damage, Vector3 upPos, Color c, float scale = 1)
    {
        var instantiate = (TextMesh)Instantiate(_Game.scoreText, upPos, Quaternion.identity);
        instantiate.text = damage;
        if (c != Color.white)
            instantiate.renderer.material.color = c;
        if (scale != 1)
            instantiate.transform.localScale *= scale;
        Destroy(instantiate.gameObject, 2);
    }

    public void Explode(Vector3 pos, float bangRadius, float damage, bool massMultiply = false)
    {
        //foreach (Rigidbody a in FindObjectsOfType(typeof (Rigidbody)))
        //    if(!a.isKinematic)
        //{
        //    a.velocity /=6;
        //    AddExplosionForce(a, 500, pos, 100,massMultiply);
        //}
        foreach (var z in _Game.destroyables)
            if (z.alive)
            {
                var m = ZeroY(pos - z.pos).magnitude;
                if (m < bangRadius)
                {
                    var f = (bangRadius - m + 1) / bangRadius * damage;
                    z.SetLife(f, pos - transform.forward, 700);
                }
            }
    }

    public static void Destruct(Transform tr, Vector3 explosionPosition, float force = 500)
    {
        var parent = tr.root ?? tr;
        if (parent.tag == "Untagged") return;
        var r = Random.Range(parent.childCount / 2, parent.childCount - 1);
        parent.tag = "Untagged";
        int i = 0;
        List<Transform> ts = new List<Transform>();
        foreach (Transform t in parent)
            ts.Add(t);
        ts.Sort(new CompareTs());
        foreach (Transform t in ts)
        {
            var g = t.gameObject;
            g.SetActive(!g.activeSelf);
            g.layer = (int)Layer.Destroyed;
            if (t.rigidbody != null)
                if (i < r)
                {
                    if (force > 0)
                        AddExplosionForce(t.rigidbody, force, explosionPosition, 500);
                }
                else
                    t.rigidbody.isKinematic = true;
            i++;
        }
        #if !UNITY_FLASH || UNITY_EDITOR
        _Game.StartCoroutine(AddMethod(3, delegate { foreach (Transform a in parent) { Destroy(a.rigidbody); Destroy(a.collider); StaticBatchingUtility.Combine(parent.gameObject); } }));
        #endif

    }
}
public class CompareTs : IComparer<Transform>
{
    public int Compare(Transform x, Transform y)
    {
        return -x.position.y.CompareTo(y.position.y);
    }
}
