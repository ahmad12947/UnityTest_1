using UnityEngine;
using System.Collections.Generic;

public class ShipController : MonoBehaviour
{
    public float accel = 10f;
    public float maxSpeed = 8f;
    public float rotSpeed = 360f;
    public float planeY = 0f;
    public float arriveRadius = 0.4f;
    public float slowRadius = 2.5f;
    public float alignDelay = 2f;
    public Renderer highlight;
    public WaypointMarker waypointPrefab;
    public LineRenderer path;

    Rigidbody rb;
    bool selected, dragging, aligning;
    float alignYaw;
    WaypointMarker pending;
    Vector3 dragStart;
    List<WaypointMarker> points = new List<WaypointMarker>();
    int cur;
    Collider[] highlightCols;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (highlight) highlight.enabled = false;
        highlightCols = highlight ? highlight.GetComponentsInChildren<Collider>(true) : System.Array.Empty<Collider>();
    }

    void Update()
    {
        if (!selected) return;

        if (Input.GetKey(KeyCode.LeftShift))
        {
            if (Input.GetMouseButtonDown(0) && TryGetPlanePoint(out var p0))
            {
                p0.y = planeY;
                dragStart = p0;
                dragging = true;
            }
            if (Input.GetMouseButtonUp(0) && dragging && TryGetPlanePoint(out var p1))
            {
                dragging = false;
                p1.y = planeY;
                var d = p1 - dragStart; d.y = 0f;
                if (d.sqrMagnitude < 0.01f) d = transform.forward;

                var m = Instantiate(waypointPrefab, dragStart, Quaternion.identity);
                m.transform.position = new Vector3(m.transform.position.x, planeY, m.transform.position.z);
                m.SetRotation(d);
                var wpCol = m.GetComponent<Collider>();
                if (wpCol != null && highlightCols != null)
                    for (int i = 0; i < highlightCols.Length; i++)
                        if (highlightCols[i]) Physics.IgnoreCollision(highlightCols[i], wpCol, true);

                points.Add(m);
                UpdatePath();
            }
        }
    }

    void FixedUpdate()
    {
        var pos = transform.position;
        if (Mathf.Abs(pos.y - planeY) > 1e-4f) { pos.y = planeY; transform.position = pos; }

        if (aligning)
        {
            rb.linearVelocity = Vector3.MoveTowards(rb.linearVelocity, Vector3.zero, accel * 2f * Time.fixedDeltaTime);
            var q = Quaternion.Euler(0f, alignYaw, 0f);
            rb.MoveRotation(Quaternion.RotateTowards(rb.rotation, q, rotSpeed * Time.fixedDeltaTime));
            if (Quaternion.Angle(rb.rotation, q) < 0.75f)
            {
                Debug.Log($"Reached waypoint {cur + 1}");
                if (pending) Destroy(pending.gameObject);
                pending = null;
                aligning = false;
                cur++;
                if (cur >= points.Count) { points.Clear(); cur = 0; }
                UpdatePath();
            }
            return;
        }

        while (cur < points.Count && (points[cur] == null || !points[cur].gameObject.activeInHierarchy)) cur++;
        if (cur >= points.Count) { rb.linearVelocity = Vector3.zero; points.Clear(); cur = 0; UpdatePath(); return; }

        var t = points[cur].transform.position;
        var to = t - transform.position; to.y = 0f;

        var desired = to.sqrMagnitude > 1e-6f ? to.normalized * maxSpeed : Vector3.zero;
        var v = rb.linearVelocity; v.y = 0f;
        var newV = Vector3.MoveTowards(v, desired, accel * Time.fixedDeltaTime);
        rb.linearVelocity = new Vector3(newV.x, 0f, newV.z);

        if (to.sqrMagnitude > 1e-6f)
        {
            var q = Quaternion.LookRotation(to);
            rb.MoveRotation(Quaternion.RotateTowards(rb.rotation, q, rotSpeed * Time.fixedDeltaTime));
        }
    }

    void LateUpdate()
    {
        UpdatePath();
    }

    void OnTriggerEnter(Collider other)
    {
        if (cur >= points.Count) return;
        var w = other.GetComponent<WaypointMarker>();
        if (!w) return;
        if (w != points[cur]) return;
        if (pending) return;

        var c = w.GetComponent<Collider>();
        if (c) c.enabled = false;
        pending = w;
        StartCoroutine(StartAlignAfterDelay());
    }

    System.Collections.IEnumerator StartAlignAfterDelay()
    {
        yield return new WaitForSeconds(alignDelay);
        if (!pending) yield break;
        alignYaw = pending.yaw;
        aligning = true;
        rb.linearVelocity = Vector3.zero;
    }

    void UpdatePath()
    {
        if (!path) return;
        int count = 0;
        for (int i = cur; i < points.Count; i++)
        {
            var w = points[i];
            if (w && w.gameObject.activeInHierarchy) count++;
        }
        if (count < 2) { path.positionCount = 0; return; }
        path.positionCount = count;
        int k = 0;
        for (int i = cur; i < points.Count; i++)
        {
            var w = points[i];
            if (!w || !w.gameObject.activeInHierarchy) continue;
            var p = w.transform.position; p.y = planeY;
            path.SetPosition(k++, p);
        }
    }

    bool TryGetPlanePoint(out Vector3 p)
    {
        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        var dy = ray.direction.y;
        if (Mathf.Abs(dy) < 1e-6f) { p = default; return false; }
        var t = (planeY - ray.origin.y) / dy;
        if (t < 0f) { p = default; return false; }
        p = ray.origin + ray.direction * t;
        return true;
    }

    public void SetSelected(bool s)
    {
        selected = s;
        if (highlight) highlight.enabled = s;
    }
}
