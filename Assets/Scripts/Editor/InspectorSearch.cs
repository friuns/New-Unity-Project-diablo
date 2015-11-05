using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEditor;
using System;
using UnityEngine;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;
using GUI = UnityEngine.GUILayout;
using gui = UnityEditor.EditorGUILayout;
using Object = UnityEngine.Object;
using System.IO;
using Random = UnityEngine.Random;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using System.Runtime.Serialization;
using System.Collections;
using print = UnityEngine.Debug;

public class InspectorSearch : EditorWindow
{

    protected virtual void OnGUI()
    {
        Shader.globalMaximumLOD = (int)EditorGUILayout.IntSlider("LOD", Shader.globalMaximumLOD, 100, 999);
        if (GUI.Button("Init"))
            foreach (Base a in FindObjectsOfTypeIncludingAssets(typeof(Base)))
                a.Init();
        OnGUIMono();
        DrawSearch();
    }

    private static Vector3 scroll;

    private static void OnGUIMono()
    {
        //scroll = EditorGUILayout.BeginScrollView(scroll);
        
        foreach (var a in GameObject.FindGameObjectsWithTag("EditorGUI"))
            a.GetComponent<Base>().OnEditorGui();

        if (Selection.activeGameObject != null)
        {
            if (Selection.activeGameObject.GetComponent<Base>() != null)
                lastObj = Selection.activeGameObject.GetComponent<Base>();

            if (Selection.activeGameObject.tag != "EditorGUI" && lastObj != null)
                lastObj.OnEditorGui();
        }
        //EditorGUILayout.EndScrollView();
    }

    public static Base lastObj;

    private bool IncludeValues;
    private bool IncludeTypes;
    private bool AllObjects;
    string search = "";
    Type[] types = new[] { typeof(GameObject), typeof(Material) };
    private void DrawSearch()
    {
        IncludeValues = EditorGUILayout.Toggle("Include Values", IncludeValues);
        IncludeTypes = EditorGUILayout.Toggle("Include Types", IncludeTypes);
        AllObjects = EditorGUILayout.Toggle("All Objects", AllObjects);
        search = EditorGUILayout.TextField("search", search);
        EditorGUIUtility.LookLikeInspector();
        var ago = Selection.activeGameObject;
        var ao = Selection.activeObject;
        if (search.Length > 0 && types.Contains(ao.GetType()) && !(ago != null && ago.camera != null))
        {

            IEnumerable<Object> array = new Object[] { ao };
            if (ago != null)
            {
                array = array.Union(ago.GetComponents<Component>());
                if (ago.renderer != null)
                    array = array.Union(new[] { ago.renderer.sharedMaterial });
            }
            foreach (Object m in array)
            {
                ForeachComponents(m);
            }
        }
    }

    private void ForeachComponents(Object m)
    {
        SerializedObject so = new SerializedObject(m);
        SerializedProperty pr = so.GetIterator();

        pr.NextVisible(true);
        do
        {
            if (pr.propertyPath.ToLower().Contains(search.ToLower()) && !IncludeTypes || IncludeTypes && pr.type.ToLower().Contains(search.ToLower()) ||
                IncludeValues && pr.propertyType == SerializedPropertyType.String && pr.stringValue.ToLower().Contains(search.ToLower()) ||
                IncludeValues && pr.propertyType == SerializedPropertyType.Enum && pr.enumNames.Length >= 0 && pr.enumNames[pr.enumValueIndex].ToLower().Contains(search.ToLower()) && pr.editable)
                EditorGUILayout.PropertyField(pr);


            //if (AllObjects)
            //    {
            //        foreach (var a in FindObjectsOfType(m.GetType()))
            //            if (a.name == m.name)
            //            {
            //                SerializedObject so2 = new SerializedObject(a);
            //                //if (so2.ApplyModifiedProperties())
            //                //{
            //                    SetMultiSelect(a, so2.FindProperty(pr.propertyPath));
            //                //}
            //            }
            //    }
            //else
            if (so.ApplyModifiedProperties())
            {
                //Debug.Log(pr.name);                                
                SetMultiSelect(m, pr);
            }
        } while (pr.NextVisible(true));
    }

    private void SetMultiSelect(Object m, SerializedProperty pr)
    {
        switch (pr.propertyType)
        {
            case SerializedPropertyType.Float:
                MySetValue(m, pr.floatValue, pr.propertyPath, pr.propertyType);
                break;
            case SerializedPropertyType.Boolean:
                MySetValue(m, pr.boolValue, pr.propertyPath, pr.propertyType);
                break;
            case SerializedPropertyType.Integer:
                MySetValue(m, pr.intValue, pr.propertyPath, pr.propertyType);
                break;
            case SerializedPropertyType.String:
                MySetValue(m, pr.stringValue, pr.propertyPath, pr.propertyType);
                break;
            case SerializedPropertyType.Color:
                MySetValue(m, pr.colorValue, pr.propertyPath, pr.propertyType);
                break;
            case SerializedPropertyType.Enum:
                MySetValue(m, pr.enumValueIndex, pr.propertyPath, pr.propertyType);
                break;
        }
    }
    void MySetValue(Object c, object value, string prName, SerializedPropertyType type)
    {
        IEnumerable<Object> array;

        if (AllObjects)
            array = FindObjectsOfTypeIncludingAssets(c.GetType()).Where(a => a.name == c.name);
        else
        {
            array = Selection.gameObjects.Select(a => a.GetComponent(c.GetType())).Cast<Object>().Union(Selection.objects.Where(a => !(a is GameObject)));

            if (c is Material)
            {
                var d = Selection.gameObjects.Select(a => a.renderer).SelectMany(a => a.sharedMaterials).Distinct();
                array = array.Union(d.Cast<Object>());
            }
        }
        foreach (var nc in array) //êîìïîíåíòû gameobjectîâ è âûáðàíûå Objectû
        {
            if (nc != null && nc != c)
            {
                SerializedObject so = new SerializedObject(nc);
                var pr = so.FindProperty(prName);
                switch (type)
                {
                    case SerializedPropertyType.Float:
                        pr.floatValue = (float)value;
                        break;
                    case SerializedPropertyType.Boolean:
                        pr.boolValue = (bool)value;
                        break;
                    case SerializedPropertyType.String:
                        pr.stringValue = (string)value;
                        break;
                    case SerializedPropertyType.Integer:
                        pr.intValue = (int)value;
                        break;
                    case SerializedPropertyType.Color:
                        pr.colorValue = (Color)value;
                        break;
                    case SerializedPropertyType.Enum:
                        pr.enumValueIndex = (int)value;
                        break;
                }

                so.ApplyModifiedProperties();
            }
        }
    }
}

