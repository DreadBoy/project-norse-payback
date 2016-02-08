using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(Raycasting))]
public class RaycastingEditor : Editor
{

    Raycasting raycasting;

    public override void OnInspectorGUI()
    {
        raycasting = (Raycasting)target;
        foreach (var ray in raycasting.RaycastHits)
        {
            //Handles.DrawLine(ray.from, ray.to);
        }
    }
}
