using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace LevelGenerator
{

    public enum Dimension
    {
        TwoD,
        ThreeD,
    };

    [Serializable]
    public class ScenarioStaticObject
    {
        public string name;
        public GameObject[] objects;
        public float probability = 0.2f;

        [Range(0, 1)]
        public float dispersionRadius = 5.0f;
        public float relativaScale = 1.0f;
    }

    [ExecuteInEditMode]
    public class LevelGenerator : MonoBehaviour
    {

        public Dimension dimension = Dimension.ThreeD;

        public Vector3 correctionFactor = Vector3.one;
        public bool generateAtStart = false;

        [Space]

        public Texture2D map;

        public ColorToPrefab[] colorMappings;
        private List<GameObject> instantiatedLevel = new List<GameObject>();
        public List<GameObject> instantiatedFloor = new List<GameObject>();
        public List<GameObject> instantiatedVegetation = new List<GameObject>();

        [SerializeField]
        public ScenarioStaticObject[] objectsToSpawn;

        // 3D setting
        public Vector2 boardSize = new Vector2(10, 10);
        public Vector3 floorOffset = new Vector3(0, 0, 0);
        public GameObject floorPrefab;
        public float multFactor = 1;

        public bool sobrepode = false;

        private void Start()
        {
            if (generateAtStart)
            {
                GenerateLevel();
            }
        }

        public void GenerateLevel()
        {
            print("Generating new level");

            GameObject level = new GameObject();
            level.name = "Level";

            for (int x = 0; x < map.width; x++)
            {
                for (int y = 0; y < map.height; y++)
                {
                    GenerateTile(x, y, level);
                }
            }
        }

        void GenerateTile(int x, int y, GameObject parent)
        {
            Color pixelColor = map.GetPixel(x, y);

            if (pixelColor.a == 0)
                return;

            foreach (ColorToPrefab colorMapping in colorMappings)
            {
                if (colorMapping.color.Equals(pixelColor))
                {
                    Vector3 position = new Vector3(x * correctionFactor.x, 0, y * correctionFactor.y);
                    GameObject gamo = Instantiate(colorMapping.prefab, position, Quaternion.identity, parent.transform);
                    if (gamo.GetComponent<Collider>() == null)
                        gamo.AddComponent<BoxCollider>();
                    instantiatedLevel.Add(gamo);
                }
            }
        }

        public void DeleteLevel()
        {
            foreach (GameObject gamo in instantiatedLevel)
            {
                DestroyImmediate(gamo);
            }

            instantiatedLevel.Clear();

            GameObject level = GameObject.Find("Level");
            if (level == null)
                return;

            SpriteRenderer[] gamos = level.GetComponentsInChildren<SpriteRenderer>(); ;

            foreach (SpriteRenderer gamo in gamos)
            {
                DestroyImmediate(gamo.gameObject);
            }

            DestroyImmediate(level);
        }

        public void GenerateFloor()
        {
            GameObject floor = new GameObject();
            floor.name = "Floor";

            for (int i = 0; i < boardSize.x; i++)
            {
                for (int j = 0; j < boardSize.y; j++)
                {
                    GameObject gamo = Instantiate(floorPrefab, (new Vector3(i, 0, j)) * multFactor + floorOffset, Quaternion.identity, floor.transform);
                    if (gamo.GetComponent<Collider>() == null)
                        gamo.AddComponent<BoxCollider>();
                    instantiatedFloor.Add(gamo);
                }
            }

            SearchForFloor();
        }
        public void DeleteFloor()
        {
            GameObject floor = GameObject.Find("Floor");

            if (floor == null)
                return;

            DestroyImmediate(floor);

            instantiatedFloor.Clear();
        }

        [ContextMenu("Reset")]
        public void ResetGeneration()
        {

            foreach (GameObject gamo in instantiatedLevel)
            {
                DestroyImmediate(gamo);
            }

            instantiatedLevel.Clear();

            foreach (GameObject gamo in instantiatedFloor)
            {
                DestroyImmediate(gamo);
            }

            instantiatedFloor.Clear();

            foreach (GameObject gamo in instantiatedVegetation)
            {
                DestroyImmediate(gamo);
            }

            instantiatedVegetation.Clear();
        }

        [ContextMenu("Delete vegetation")]
        public void DeleteVegetation()
        {
            foreach (GameObject gamo in instantiatedVegetation)
            {
                DestroyImmediate(gamo);
            }

            instantiatedVegetation.Clear();
        }
        public void LookForGameObjects()
        {
            foreach (ScenarioStaticObject _o in objectsToSpawn)
            {
                List<GameObject> found = new List<GameObject>();

                foreach (GameObject gamo in GetComponent<AssetExposer>().objectsToExpose)
                {
                    if (gamo.name.ToLower().Contains(_o.name))
                    {
                        found.Add(gamo);
                    }
                }

                _o.objects = ToArray(found);
            }
        }

        public void SearchForFloor()
        {
            instantiatedFloor.Clear();

            GameObject floor = GameObject.Find("Floor");

            if (floor == null)
                return;

            MeshRenderer[] floors = floor.GetComponentsInChildren<MeshRenderer>();

            foreach (MeshRenderer _m in floors)
            {
                if (_m.gameObject.transform.parent.gameObject == floor)
                    instantiatedFloor.Add(_m.gameObject);
            }
        }

        [ContextMenu("Serch for level objects")]
        public void SearchForLevel()
        {
            instantiatedLevel.Clear();

            GameObject level = GameObject.Find("Level");

            if (level == null)
                return;

            MeshRenderer[] levels = level.GetComponentsInChildren<MeshRenderer>();

            foreach (MeshRenderer _m in levels)
            {
                if (_m.gameObject.transform.parent.gameObject == level)
                    instantiatedLevel.Add(_m.gameObject);
            }
        }

        [ContextMenu("Search for scenario objects")]
        public void SearchForStaticObjects()
        {
            SearchForFloor();
            SearchForLevel();

            instantiatedVegetation.Clear();

            List<GameObject> objectsToVegetate = new List<GameObject>(instantiatedFloor.Count + instantiatedLevel.Count);

            instantiatedFloor.ForEach(p => objectsToVegetate.Add(p));
            instantiatedLevel.ForEach(p => objectsToVegetate.Add(p));

            foreach (GameObject gamo in objectsToVegetate)
            {
                MeshRenderer[] childrens = gamo.GetComponentsInChildren<MeshRenderer>();

                foreach (MeshRenderer _m in childrens)
                {
                    if (_m.gameObject != gamo)
                    {
                        instantiatedVegetation.Add(_m.gameObject);
                    }
                }
            }

            print(instantiatedVegetation.Count);
        }

        [ContextMenu("Test")]
        public void Test()
        {
            print(UnityEngine.Random.insideUnitSphere);
        }

        public void GenerateVegetation()
        {
            SearchForStaticObjects();

            DeleteVegetation();

            SearchForFloor();

            SearchForLevel();

            List<GameObject> objectToVegetate = new List<GameObject>(instantiatedFloor.Count + instantiatedLevel.Count);

            instantiatedFloor.ForEach(p => objectToVegetate.Add(p));
            instantiatedLevel.ForEach(p => objectToVegetate.Add(p));

            foreach (GameObject gamo in objectToVegetate)
            {
                GameObject g = null;

                foreach (ScenarioStaticObject _o in objectsToSpawn)
                {
                    if (UnityEngine.Random.Range(0f, 1f) <= _o.probability)
                    {
                        Vector3 pos = UnityEngine.Random.insideUnitSphere * 5f * _o.dispersionRadius;
                        pos.y = gamo.GetComponent<Collider>().bounds.size.y;
                        pos += gamo.transform.position + new Vector3(2.5f, 0f, 2.5f);

                        Quaternion rot = Quaternion.Euler(0, UnityEngine.Random.Range(0f, 360f), 0);

                        GameObject gamoToInst = _o.objects[UnityEngine.Random.Range(0, _o.objects.Length)];

                        if (gamoToInst != null)
                        {
                            g = Instantiate(gamoToInst, pos, rot, gamo.transform);
                            g.transform.localScale *= _o.relativaScale;

                            if (g != null)
                                if (g.GetComponent<Collider>() == null)
                                    g.AddComponent<BoxCollider>();

                            instantiatedVegetation.Add(g);

                            if (g.name.Contains("grass"))
                            {
                                // See if we're under bounds
                                float boundX_max = g.GetComponent<Collider>().bounds.center.x + g.GetComponent<Collider>().bounds.extents.x;
                                float boundX_min = g.GetComponent<Collider>().bounds.center.x - g.GetComponent<Collider>().bounds.extents.x;
                                float boundZ_max = g.GetComponent<Collider>().bounds.center.z + g.GetComponent<Collider>().bounds.extents.z;
                                float boundZ_min = g.GetComponent<Collider>().bounds.center.z - g.GetComponent<Collider>().bounds.extents.z;

                                if (boundZ_max > gamo.GetComponent<Collider>().bounds.center.z + gamo.GetComponent<Collider>().bounds.extents.z
                                || boundZ_max < gamo.GetComponent<Collider>().bounds.center.z - gamo.GetComponent<Collider>().bounds.extents.z)
                                {
                                    if (g != null)
                                        DestroyImmediate(g);
                                }
                                if (boundZ_min > gamo.GetComponent<Collider>().bounds.center.z + gamo.GetComponent<Collider>().bounds.extents.z
                                || boundZ_min < gamo.GetComponent<Collider>().bounds.center.z - gamo.GetComponent<Collider>().bounds.extents.z)
                                {
                                    if (g != null)
                                        DestroyImmediate(g);
                                }
                                if (boundX_max > gamo.GetComponent<Collider>().bounds.center.x + gamo.GetComponent<Collider>().bounds.extents.x
                                  || boundX_max < gamo.GetComponent<Collider>().bounds.center.x - gamo.GetComponent<Collider>().bounds.extents.x)
                                {
                                    if (g != null)
                                        DestroyImmediate(g);
                                }
                                if (boundX_min > gamo.GetComponent<Collider>().bounds.center.x + gamo.GetComponent<Collider>().bounds.extents.x
                                || boundX_min < gamo.GetComponent<Collider>().bounds.center.x - gamo.GetComponent<Collider>().bounds.extents.x)
                                {
                                    if (g != null)
                                        DestroyImmediate(g);
                                }
                            }

                            if (!sobrepode)
                                break;
                        }
                    }
                }
            }

            gameObject.GetComponent<ColliderSetter>().SetCollider("grass");
        }

        private List<GameObject> ToList(GameObject[] _array)
        {
            List<GameObject> list = new List<GameObject>();

            foreach (GameObject gamo in _array)
            {
                list.Add(gamo);
            }

            return list;
        }
        private GameObject[] ToArray(List<GameObject> _list)
        {
            GameObject[] _array = new GameObject[_list.Count];

            for (int i = 0; i < _list.Count; i++)
            {
                _array[i] = _list[i];
            }

            return _array;
        }
    }

    /* Requisites for sprites
	* 
	* Compression = none
	* Filter mode = point (no filter)
	* Advanced > Can read/write
    * Non power of 2 = None
	*
    * Remember to set alpha to 1 at color mapping's elements
    */

}

/* To do
- Place an object on terrain and normally to it
- Easy editying (click, double, positioning with mouse, undo) */
