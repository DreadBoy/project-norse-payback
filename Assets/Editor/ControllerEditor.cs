using UnityEngine;
using System.Collections;
using UnityEditor;

[ExecuteInEditMode]
[CustomEditor(typeof(Controller))]
public class ControllerEditor : Editor
{


    void OnSceneGUI()
    {
        Controller controller = (Controller)target;

        Handles.color = Color.white;
        if (controller.collider2D == null)
            return;
        var bounds = controller.collider2D.bounds;

        foreach (var ray in controller.RaycastHits)
        {
            Handles.DrawLine(ray.from, ray.to);
        }
    }
}
