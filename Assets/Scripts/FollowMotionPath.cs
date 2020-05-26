using UnityEngine;
using System.Collections;

/// <summary>
/// This entire class simply moves a gameobject across a MotionPath, according to if it is 2D or not
/// </summary>
public class FollowMotionPath : MonoBehaviour
{
    public MotionPath motionPath;
    public bool is2d;
    public Transform lookAt;
    public float startPosition;
    public float speed; // Realworld units per second you want your object to travel
    public bool loop;
    public float uv;
    public bool yesBe;

    private void Start()
    {
        uv = startPosition;
        if (motionPath == null)
            enabled = false;
    }


    private void FixedUpdate()
    {
        if (yesBe)
        {
            uv += speed / motionPath.length * Time.fixedDeltaTime;
            //uv += ((speed) * Time.fixedDeltaTime);			// This gets you uv amount per second so speed is in realworld units
            if (loop)
                uv = (uv < 0 ? 1 + uv : uv) % 1;
            else if (uv > 1)
                enabled = false;
            var pos = motionPath.PointOnNormalizedPath(uv);
            var norm = motionPath.NormalOnNormalizedPath(uv);

            transform.position = pos;
            transform.forward = speed > 0 ? norm : -norm;
            if (is2d)
                transform.eulerAngles = new Vector3(0, 0, 0);
            else
                transform.LookAt(lookAt);
        }
    }
}