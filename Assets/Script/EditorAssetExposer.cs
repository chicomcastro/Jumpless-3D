using UnityEngine;
using UnityEditor;

namespace LevelGenerator
{
    [CustomEditor(typeof(AssetExposer))]
    public class EditorAssetExposer : Editor
    {

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            AssetExposer myScript = (AssetExposer)target;
            GUILayout.Space(10);

            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Expose"))
            {
                myScript.ShowObjects();
            }
            if (GUILayout.Button("Delete last expositure"))
            {
                myScript.Delete();
            }
            GUILayout.EndHorizontal();

			
            if (GUILayout.Button("Generate colliders"))
            {
                myScript.GenerateColliders();
            }
        }

    }
}
