using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class AssetExposer : MonoBehaviour
{

    public GameObject[] objectsToExpose;

    public float expositionLenght = 20f;
    public float altitude = 0f;
    public float pace = 5f;

    public bool shouldDeletePrevious = true;

    void Start()
    {
        GameObject assets = Asset();

        if (assets == null)
            return;

        //assets.SetActive(false);
    }

    public void ShowObjects()
    {
        float _y = altitude, _x = 0, _z = 0;

        GameObject assets = new GameObject();
        assets.name = "Exposition";

        foreach (GameObject gamo in objectsToExpose)
        {
            Instantiate(gamo, new Vector3(_x, _y, _z), Quaternion.identity, assets.transform);
            _x += pace;

            if (_x >= expositionLenght)
            {
                _x = 0;
                _z += pace;
            }
        }
    }

    public void Delete()
    {
        GameObject assets = Asset();

        if (assets == null)
            return;

        DestroyImmediate(assets);
    }

    private GameObject Asset()
    {
        return GameObject.Find("Exposition");
    }

    public void GenerateColliders()
    {
        GameObject assets = Asset();

        if (assets == null)
            return;

        foreach (MeshRenderer model in assets.GetComponentsInChildren<MeshRenderer>())
        {
            Collider col = model.gameObject.GetComponent<Collider>();
            if (col == null)
                model.gameObject.AddComponent<BoxCollider>();
        }
    }
}
