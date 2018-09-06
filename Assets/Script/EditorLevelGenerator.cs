using UnityEngine;
using UnityEditor;

namespace LevelGenerator
{
    [CustomEditor(typeof(LevelGenerator))]
    public class EditorLevelDesigner : Editor
    {
        SerializedProperty map;
        SerializedProperty colorMappings;
        SerializedProperty dimension;
        SerializedProperty floorPrefab;
        SerializedProperty objectsToSpawn;

        void OnEnable()
        {
            map = serializedObject.FindProperty("map");
            colorMappings = serializedObject.FindProperty("colorMappings");
            dimension = serializedObject.FindProperty("dimension");
            floorPrefab = serializedObject.FindProperty("floorPrefab");
            objectsToSpawn = serializedObject.FindProperty("objectsToSpawn");
        }


        public override void OnInspectorGUI()
        {
            //DrawDefaultInspector();

            GUILayout.Space(10);
            // Gets reference for my target object
            LevelGenerator myScript = (LevelGenerator)target;

            // Draws default inspector field for dimension
            serializedObject.Update();
            EditorGUILayout.PropertyField(dimension);

            GUILayout.Space(10);

            if (myScript.dimension == Dimension.ThreeD)
            {
                EditorGUILayout.LabelField("Floor generation", EditorStyles.boldLabel);

                EditorGUILayout.PropertyField(floorPrefab);
                myScript.boardSize = EditorGUILayout.Vector2Field("Board Size", myScript.boardSize);
                myScript.floorOffset = EditorGUILayout.Vector3Field("Offset", myScript.floorOffset);
                myScript.multFactor = EditorGUILayout.FloatField("Correction factor", myScript.multFactor);

                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Generate floor"))
                {
                    myScript.GenerateFloor();
                }
                if (GUILayout.Button("Delete floor"))
                {
                    myScript.DeleteFloor();
                }
                GUILayout.EndHorizontal();

                GUILayout.Space(10);
                EditorGUILayout.LabelField("Floor Reference Size", myScript.instantiatedFloor.Count.ToString());
                if (GUILayout.Button("Search for floor"))
                {
                    myScript.SearchForFloor();
                }

                GUILayout.Space(10);
                EditorGUILayout.LabelField("Static generation", EditorStyles.boldLabel);

                EditorGUILayout.PropertyField(objectsToSpawn, true);

                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Find scenario's objects by name"))
                {
                    myScript.LookForGameObjects();
                }
                // mato
                // pedra
                // outros
                GUILayout.EndHorizontal();

                if (GUILayout.Button("Generate static objects"))
                {
                    myScript.GenerateVegetation();
                }
            }

            GUILayout.Space(10);

            EditorGUILayout.LabelField("Level creating", EditorStyles.boldLabel);

            if (myScript.dimension == Dimension.TwoD)
            {
                myScript.correctionFactor = EditorGUILayout.Vector2Field("Correction factor", myScript.correctionFactor);
            }
            if (myScript.dimension == Dimension.ThreeD)
            {
                myScript.correctionFactor = EditorGUILayout.Vector3Field("Correction factor", myScript.correctionFactor);
            }

            myScript.generateAtStart = EditorGUILayout.Toggle("Generate At Start", myScript.generateAtStart);

            // Draws default inspector field for map
            EditorGUILayout.PropertyField(map);

            // Draws default inspector field for colorMappings
            EditorGUILayout.PropertyField(colorMappings, true);

            if (myScript.dimension == Dimension.TwoD)
            {
                GUILayout.BeginHorizontal();

                if (GUILayout.Button("Generate level"))
                {
                    myScript.GenerateLevel();
                }
                if (GUILayout.Button("Delete level"))
                {
                    myScript.DeleteLevel();
                }
                GUILayout.EndHorizontal();
            }

            if (myScript.dimension == Dimension.ThreeD)
            {
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Generate level"))
                {
                    myScript.GenerateLevel();
                }
                if (GUILayout.Button("Delete level"))
                {
                    myScript.DeleteLevel();
                }
                GUILayout.EndHorizontal();
            }

            serializedObject.ApplyModifiedProperties();
        }

    }
}
