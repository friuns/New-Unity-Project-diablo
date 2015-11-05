//using System;
//using System.Collections;
//using System.IO;
//using System.Linq;
//using UnityEngine;
//using System.Collections.Generic;
//using Random = UnityEngine.Random;
//using gui = UnityEngine.GUILayout;
//public class CustomWindow : Bs
//{
//    public override void Awake()
//    {
        
//        enabled = false;
//        base.Awake();
//    }
//    public void OnGUI()
//    {
//        if (Window == null)
//            enabled = false;
//        else
//            DrawWindow(sizex, sizey, windowTitle, DWind, 0, dock, GUI.skin.window);
        
//    }
//    public void DWind(int i)
//    {
//        if (GUILayout.Button("<<Back"))
//            CloseWindow();
//        else
//            Window();
//    }

//    public void CloseWindow()
//    {
//        Window = null;
//        update = null;
//    }
//    public void Update()
//    {
//        if (update != null)
//            update();
//    }
//    public int sizex;
//    public int sizey;
//    public Action Window;
//    public string windowTitle;
//    public Dock dock = Dock.Center;
//    public Queue<GUI.WindowFunction> windows = new Queue<GUI.WindowFunction>();
//    public void ShowWindow(Action func)
//    {
//        ShowWindow(300, 300, "Window", Dock.Center, func);
//    }

//    public Action update;
//    public void ShowWindow(int x, int y, string windowTitle, Dock dock, Action func, Action updateFunc = null)
//    {
//        Screen.lockCursor = false;
//        enabled = true;
//        sizex = x;
//        sizey = y;
//        this.windowTitle = windowTitle;
//        this.dock = dock;
//        Window = func;
//        update = updateFunc;
//    }

//    public enum Dock
//    {
//        Right,
//        Left,
//        Center
//    }

//    public static void DrawWindow(int w, int h, string tt, GUI.WindowFunction mh, int win, Dock dock, GUIStyle style)
//    {
        
//        var c = Vector3.zero;
//        var s = new Vector3(Mathf.Min(Screen.width, w), Mathf.Min(Screen.height, h)) / 2f;
//        if (dock == Dock.Right)
//            c = new Vector3(Screen.width - s.x - 20, Screen.height / 2f);
//        else if (dock == Dock.Left)
//            c = new Vector3(0, Screen.height / 2f);
//        else
//            c = new Vector3(Screen.width, Screen.height) / 2f;
//        var v1 = c - s;
//        var v2 = c + s;
//        GUI.Window((int)win, Rect.MinMaxRect(v1.x, v1.y, v2.x, v2.y), null, tt, style);


//    }

//}
