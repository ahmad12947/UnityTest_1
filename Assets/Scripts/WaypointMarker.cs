using UnityEngine;

public class WaypointMarker : MonoBehaviour
{
    public float yaw;

    public void SetRotation(Vector3 dir)
    {
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.0001f) dir = Vector3.forward;
        yaw = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(90f, yaw, 0f);
    }
}
