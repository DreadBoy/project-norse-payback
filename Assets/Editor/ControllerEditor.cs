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
        for (float i = 0; i <= controller.raycastPrecision; i++)
        {
            Vector2 from = new Vector2(
                from.x = bounds.center.x - bounds.extents.x + i / controller.raycastPrecision * 2 * bounds.extents.x,
                bounds.center.y - bounds.extents.y
            );
            Vector2 to = from + controller.groundRay;
            Handles.DrawLine(from, to);
        }
    }
}
