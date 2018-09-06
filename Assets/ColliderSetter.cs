using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class ColliderSetter : MonoBehaviour
{
    public new string name;

    [ContextMenu("Set colliders to trigger")]
    public void SetCollider(string name)
    {
        Collider[] colliders = GameObject.FindObjectsOfType<Collider>();

        foreach (Collider col in colliders)
        {
            if (col.gameObject.name.ToLower().Contains(name.ToLower()))
                col.isTrigger = true;
        }
    }
}
