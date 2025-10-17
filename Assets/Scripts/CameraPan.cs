using UnityEngine;

public class CameraPan : MonoBehaviour
{
    public float planeY = 0f;
    public float dragMultiplier = 1f;
    public float smooth = 15f;

    Camera cam;
    bool dragging;
    Vector3 anchorWorld;
    Vector3 camStart;
    Vector3 target;

    void Awake()
    {
        cam = GetComponent<Camera>();
        if (!cam) cam = Camera.main;
        target = transform.position;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(1) && HitPlane(out anchorWorld))
        {
            dragging = true;
            camStart = transform.position;
        }

        if (dragging)
        {
            if (HitPlane(out var p))
            {
                var d = (anchorWorld - p) * dragMultiplier;
                target = new Vector3(camStart.x + d.x, camStart.y, camStart.z + d.z);
            }
            if (Input.GetMouseButtonUp(1)) dragging = false;
        }

        var pos = transform.position;
        var y = pos.y;
        pos = Vector3.Lerp(pos, target, 1f - Mathf.Exp(-smooth * Time.unscaledDeltaTime));
        pos.y = y;
        transform.position = pos;
    }

    bool HitPlane(out Vector3 p)
    {
        var ray = cam.ScreenPointToRay(Input.mousePosition);
        var dy = ray.direction.y;
        if (Mathf.Abs(dy) < 1e-6f) { p = default; return false; }
        var t = (planeY - ray.origin.y) / dy;
        if (t < 0f) { p = default; return false; }
        p = ray.origin + ray.direction * t;
        return true;
    }
}
