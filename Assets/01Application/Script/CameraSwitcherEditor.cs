using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(CameraSwitcher))]
public class CameraSwitcherEditor : Editor
{

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        CameraSwitcher cs = (CameraSwitcher)target;

        if (GUILayout.Button("Random"))
        {
            cs.SwitchCamera(Random.Range(0, cs.points.Length));
        }

        int count = 0;
        foreach (Transform point in cs.points)
        {
            var label = "";

            if (count == 0)
                label = count.ToString() + "Default";
            else
                label = count.ToString() + " : " + point.gameObject.name;

            if (GUILayout.Button("Switch " + label))
                cs.SwitchCamera(count);

            count++;
        }

        //Auto Switch ON/OFF Button
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Auto On" ))
            cs.StartAutoChange();
        if (GUILayout.Button("Auto Off"))
            cs.StopAutoChange();
        EditorGUILayout.EndHorizontal();

    }

}
