using System;
using System.Collections.Generic;
#if (UNITY_EDITOR)
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEditorInternal;
#endif
using UnityEngine;
using System.Collections;
using Random = UnityEngine.Random;
public class Bs : Base
{
    public bool Android { get { return _Loader.Android; } }

    public static bool exiting;

    public static Dark m_Dark;
    public static Dark _Dark { get { return m_Dark == null ? (m_Dark = (Dark)FindObjectOfType(typeof(Dark))) : m_Dark; } }

    public static ObsCamera m_ObsCamera;
    public static ObsCamera _ObsCamera { get { return m_ObsCamera == null ? (m_ObsCamera = (ObsCamera)FindObjectOfType(typeof(ObsCamera))) : m_ObsCamera; } }
    public static Hud m_Hud;
    public static Hud _Hud { get { return m_Hud == null ? (m_Hud = (Hud)FindObjectOfType(typeof(Hud))) : m_Hud; } }
    public static Database m_Database;
    public static Database _Database { get { return m_Database == null ? (m_Database = (Database)FindObjectOfType(typeof(Database))) : m_Database; } }
    public static Cursor m_Cursor;
    public static Cursor _Cursor { get { return m_Cursor == null ? (m_Cursor = (Cursor)FindObjectOfType(typeof(Cursor))) : m_Cursor; } }
    public static Loader m_Loader;
    public static Loader _Loader { get { return m_Loader == null ? (m_Loader = (Loader)FindObjectOfType(typeof(Loader))) : m_Loader; } }
    public static Game m_Game;
    public static Game _Game { get { return m_Game == null ? (m_Game = (Game)FindObjectOfType(typeof(Game))) : m_Game; } }
    public static Player m_Player;
    public static Player _Player { get { return m_Player == null ? (m_Player = (Player)FindObjectOfType(typeof(Player))) : m_Player; } }
    public static Sounds m_Sounds;
    public static Sounds _Sounds { get { return m_Sounds == null ? (m_Sounds = (Sounds)FindObjectOfType(typeof(Sounds))) : m_Sounds; } }
    internal new Animation animation;
    public virtual void Awake()
    {
        animation = base.animation;
        if (animation != null && Application.isEditor)
            animation.cullingType = AnimationCullingType.AlwaysAnimate;
    }
    public static void AddExplosionForce(Rigidbody r, float explosionForce, Vector3 explosionPosition, float explosionRadius=100,bool massMultiply =false)
    {

        var v = ZeroY(r.position - explosionPosition);

        float force = explosionForce*(massMultiply ? r.mass : 1);
        Vector3 position = explosionPosition;//+ (v.magnitude > .5f ? v/2f : Vector3.zero);

        r.AddExplosionForce(force, position, explosionRadius);
    }
    public void PlaySound(AudioClip a,float v=1)
    {
        if (!_Loader.enableSounds)
            return;
        if (v != 1)
            audio.PlayOneShot(a, v);
        else
            audio.PlayOneShot(a);
    }
    public static Vector3 ZeroY(Vector3 v, float y = 0)
    {
        v.y = y;
        return v;
    }
    public virtual Vector3 pos { get { return transform.position; } set { transform.position = value; } }
    public virtual Quaternion rot { get { return transform.rotation; } set { transform.rotation = value; } }
    public static IEnumerator AddMethod(float seconds, Action act)
    {
        yield return new WaitForSeconds(seconds);
        act();
    }
    public static IEnumerator AddMethod(Action act)
    {
        yield return new WaitForEndOfFrame();
        act();
    }
    public static IEnumerator AddMethod(YieldInstruction y, Action act)
    {
        yield return y;
        act();
    }
    public static IEnumerator AddMethod(Func<bool> y, Action act)
    {
        while (!y())
            yield return null;
        act();
    }
    public static IEnumerator AddUpdate(Func<bool> y)
    {
        yield return null;
        while (y())
            yield return null;
    }

    public IEnumerator MoveObject(Bs t, Vector3 move, float time)
    {
        float tm = Time.time;
        endpos = t.pos + move;
        from = t.pos;
        while (Time.time - tm < time)
        {
            t.pos = Vector3.Lerp(from, endpos, (Time.time - tm) / time);
            yield return null;
        }
    }

    private Vector3 endpos;
    private Vector3 from;
    public IEnumerator MoveObject(Transform t, Vector3 move, float time)
    {
        float tm = Time.time;
        endpos = t.localPosition + move;
        from = t.localPosition;
        while (Time.time - tm < time)
        {
            t.localPosition = Vector3.Lerp(from, endpos, (Time.time - tm) / time);
            yield return null;
        }
    }

    

    
    public static bool mouse0;
    public static bool mouse1;
    public static bool mouse;
    //public static GameObject selection;
    public static void ClearLog()
    {
//#if (UNITY_EDITOR)
//        Assembly assembly = Assembly.GetAssembly(typeof(Macros));
//        Type type = assembly.GetType("UnityEditorInternal.LogEntries");
//        MethodInfo method = type.GetMethod("Clear");
//        method.Invoke(new object(), null);
//#endif
    }
    public void SetDirty(MonoBehaviour g = null)
    {
#if (UNITY_EDITOR)
        if (g == null) g = this;
        if (!Application.isPlaying)
            EditorUtility.SetDirty(g);
#endif
    }
    public static bool ongui
    {
        get { return UICamera.hoveredObject != null || _ObsCamera.InventoryActive; }
    }
    private Camera m_camera;
    public new Camera camera { get { return m_camera == null ? (m_camera = Camera.mainCamera):m_camera; } }

    
    public static string ArrayRandom(string[] source)
    {
        return source[Random.Range(0, source.Length)];
    }
    public static AudioClip ArrayRandom(AudioClip[] source)
    {
        if (source.Length == 0) return null;
        return source[Random.Range(0, source.Length)];
    }

    Dictionary<string, string> m_dict;
    Dictionary<string, string> dict { get { return m_dict ?? LoadDict(); } }
    public string TR(string t)
    {
        string s;
        if (dict.TryGetValue(t, out s))
            return s;
        if (dict.TryGetValue(t.ToLower(), out s))
        {
            dict[t] = s;
            return s;
        }

        return t;
    }

#if UNITY_FLASH
    public static string sb = "";
#else
    public static StringBuilder sb = new StringBuilder();
#endif

    public void Log(params object[] s)
    {
#if UNITY_FLASH
        foreach (var a in s)
            sb += a + "\t";
        sb += "\r\n";
#else
        foreach (var a in s)
            sb.Append(a).Append("\t");
        sb.AppendLine();
#endif

    }


    Dictionary<string, string> LoadDict()
    {
        if (m_dict != null) return m_dict;
        var dictFile = _Game.dictFile;
        m_dict = new Dictionary<string, string>();
        var ss = dictFile.text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        foreach (var s in ss)
        {
            var a = s.Split('=');
            m_dict.Add(a[0].ToLower(), a[1]);
        }
        return m_dict;
    }

    public Slot FirstOrDefault(IEnumerable<Slot> source, Func<Slot, bool> predicate)
    {
        foreach (Slot local in source)
        {
            if (predicate(local))
            {
                return local;
            }
        }
        return null;
    }
    public static void CopyValues<T, T2>(T frm, T2 to)
    {
#if UNITY_EDITOR
        foreach (FieldInfo f in frm.GetType().GetFields())
        {
            try
            {
                to.GetType().GetField(f.Name).SetValue(to, f.GetValue(frm));
            }
            catch (Exception e)
            {
                Debug.Log(e);
            }
        }
#endif
    }

    public static Transform FirstOrDefault(Transform[] transforms1, Func<Transform, bool> act)
    {
        foreach (Transform a in transforms1)
            if (act(a)) return a;
        return null;
    }
    private object[] old;
    private bool[] showpr;
    public void TraceVars(object o=null)
    {
#if UNITY_EDITOR
        if (o == null) o = this;
        if (old == null)
        {
            old = new object[1000];
            showpr = new bool[1000];
        }
        var bind = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
        StringBuilder sb = new StringBuilder();
        var st = new Type[] { typeof(int), typeof(bool), typeof(float), typeof(Enum) };
        int i = 0;
        foreach (MemberInfo a in o.GetType().GetProperties(bind).Cast<MemberInfo>().Union(o.GetType().GetFields(bind)))
        {
            i++;
            var p = a as PropertyInfo;
            var f = a as FieldInfo;
            var propertyType = p != null ? p.PropertyType : f.FieldType;
            if ((st.Contains(propertyType) || st.Contains(propertyType.BaseType)) && !a.Name.EndsWith("BackingField"))
            {
                var value = p != null ? p.GetValue(o, null) : f.GetValue(o);
                if (value != null && old[i] != null && !value.Equals(old[i]))
                    showpr[i] = true;
                if (showpr[i])
                    sb.AppendFormat("{0}\t{1}\r\n", a.Name, value);
                old[i] = value;
            }
        }

        //foreach (var a in )
        //{
        //    i++;
        //    if (st.Contains(a.FieldType) || st.Contains(a.FieldType.BaseType))
        //        sb.AppendFormat("{0}\t{1}\r\n", a.Name, old[i] = a.GetValue(o));
        //}
        _Loader.varTrace.text = sb.ToString();
#endif
    }
    //public static Dictionary<KeyCode, string> keyTexts = new Dictionary<KeyCode, string>();
    public static bool GetKeyDown(KeyCode k, string s = "", bool showAlways = false)
    {
        if (_Loader.showKeys || showAlways)
            _Loader.Log(s + "\t" + k);
            //keyTexts[k] = s;
        return Input.GetKeyDown(k);
    }
}

