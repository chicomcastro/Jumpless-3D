using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ColliderSetter))]
public class EditorColliderSetter : Editor
{

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        ColliderSetter myScript = (ColliderSetter)target;
        GUILayout.Space(10);

        GUILayout.BeginHorizontal();

        if (GUILayout.Button("Set colliders to trigger"))
        {
            myScript.SetCollider(myScript.name);
        }

        GUILayout.EndHorizontal();
    }

}
