using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Facing = Controller.Facing;

public class Raycasting : MonoBehaviour
{

    //Scene Editor info
    public struct Raycast
    {
        public Vector2 from;
        public Vector2 to;
    };

    public class EnvInfo
    {
        public bool grounded, onslope, inwall, onwall, facingslope;
        public float groundDistance, wallDistance;
        public Vector3 groundNormal;

        public EnvInfo(EnvInfo copy)
        {
            grounded = copy.grounded;
            onslope = copy.onslope;
            inwall = copy.inwall;
            facingslope = copy.facingslope;
            groundDistance = copy.groundDistance;
        }

        public EnvInfo()
        {
            grounded = onslope = inwall = facingslope = false;
            groundDistance = 0;
        }
    }


    [Range(0, 0.1f)]
    public float groundMargin = 0.01f;
    [HideInInspector]
    public float raycastPrecision = 5;
    [HideInInspector]
    public float wallMargin = 0.01f;

    [HideInInspector]
    public List<Raycast> RaycastHits = new List<Raycast>();


    public void raycastGround(Bounds bounds, float margin, ref EnvInfo envInfo)
    {
        if (float.IsNaN(margin) || margin < 0)
            throw new System.ArgumentException("Margin Must be non-negative float");
        if (margin == 0)
            margin = groundMargin;


        envInfo.grounded = envInfo.onslope = false;

        RaycastHit2D rayHit = new RaycastHit2D();

        for (float i = 0; i <= raycastPrecision; i++)
        {
            Vector2 from = new Vector2(
                bounds.min.x + i / raycastPrecision * 2 * bounds.extents.x,
                bounds.max.y
            );
            Vector2 to = from;
            to.y = bounds.min.y - margin;

            //RaycastHits.Add(new Raycast() { from = from, to = to });
            rayHit = Physics2D.Raycast(from, to - from, (to - from).magnitude, 1 << LayerMask.NameToLayer("Environment"));
            Debug.DrawRay(from, to - from, Color.white);
            if (rayHit)
            {
                envInfo.grounded = true;
                var distance = Mathf.Abs(rayHit.distance - bounds.extents.y * 2);
                if (distance > groundMargin)
                    envInfo.groundDistance = distance;
                var angle = Vector2.Angle(rayHit.normal, from - to);
                if (Mathf.Abs(angle) > 5)
                {
                    envInfo.onslope = true;
                }
            }
        }

    }


    public void raycastForward(Bounds bounds, Facing facing, float margin, ref EnvInfo envInfo)
    {
        if (float.IsNaN(margin) || margin < 0)
            throw new System.ArgumentException("Margin Must be non-negative float");
        if (margin == 0)
            margin = wallMargin;

        envInfo.inwall = envInfo.onwall = envInfo.facingslope = false;

        RaycastHit2D rayHit = new RaycastHit2D();

        for (float i = 0; i <= raycastPrecision; i++)
        {
            Vector2 from = new Vector2(
                facing == Facing.right ? bounds.min.x + wallMargin : bounds.max.x - wallMargin,
                bounds.min.y + i / raycastPrecision * 2 * bounds.extents.y
            );
            Vector2 to = from;
            to.x = facing == Facing.right ? bounds.max.x + margin : bounds.min.x - margin;

            RaycastHits.Add(new Raycast() { from = from, to = to });
            rayHit = Physics2D.Raycast(from, to - from, (to - from).magnitude, 1 << LayerMask.NameToLayer("Environment"));
            Debug.DrawRay(from, to - from, Color.white);
            if (rayHit)
            {
                envInfo.inwall = true;
                var distance = Mathf.Abs(rayHit.distance - (bounds.extents.x * 2 + wallMargin));
                if (distance > wallMargin)
                    envInfo.wallDistance = distance;
                break;
            }
        }
    }
}
