using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Parallax2D))]
public class ParallaxEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        Parallax2D p = (Parallax2D)target;
        Vector3 startPos = p.transform.position;
        if (GUILayout.Button("Test Redraw"))
        {
            p.Redraw();
            Debug.Log("Redraw");
        }
        if (GUILayout.Button("Reset Redraw"))
        {
            p.transform.position = startPos;
            Debug.Log("Reset");
        }
    }
}
