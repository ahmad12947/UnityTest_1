using UnityEngine;

public class ShipSelection : MonoBehaviour
{
    void Update()
    {
        if (!Input.GetMouseButtonDown(0)) return;
        if (Input.GetKey(KeyCode.LeftShift)) return;

        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out var hit))
        {
            foreach (var s in FindObjectsOfType<ShipController>()) s.SetSelected(false);
            var ship = hit.collider.GetComponentInParent<ShipController>();
            if (ship) ship.SetSelected(true);
            else foreach (var s in FindObjectsOfType<ShipController>()) s.SetSelected(false);
        }
        else
        {
            foreach (var s in FindObjectsOfType<ShipController>()) s.SetSelected(false);
        }
    }
}
