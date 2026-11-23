using UnityEngine;

// Drag to orbit, scroll to zoom, click a shape to poke it. Slowly auto-orbits when idle.
public class OrbitCamera : MonoBehaviour
{
    public Vector3 target = new Vector3(0f, 1f, 0f);
    public float distance = 9f;
    public float minDistance = 4f;
    public float maxDistance = 18f;

    float yaw = 30f;
    float pitch = 20f;
    float idleTime = 10f;
    Vector3 lastMouse;
    bool dragged;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            lastMouse = Input.mousePosition;
            dragged = false;
        }

        if (Input.GetMouseButton(0))
        {
            Vector3 delta = Input.mousePosition - lastMouse;
            lastMouse = Input.mousePosition;
            if (delta.sqrMagnitude > 4f) dragged = true;
            yaw += delta.x * 0.3f;
            pitch = Mathf.Clamp(pitch - delta.y * 0.3f, -5f, 80f);
            idleTime = 0f;
        }

        if (Input.GetMouseButtonUp(0) && !dragged)
        {
            Ray ray = GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                var spinner = hit.collider.GetComponent<Spinner>();
                if (spinner != null) spinner.Poke();
            }
        }

        float scroll = Input.mouseScrollDelta.y;
        if (Mathf.Abs(scroll) > 0.01f)
        {
            distance = Mathf.Clamp(distance - scroll * 0.8f, minDistance, maxDistance);
            idleTime = 0f;
        }

        idleTime += Time.deltaTime;
        if (idleTime > 3f) yaw += 10f * Time.deltaTime;

        Quaternion rot = Quaternion.Euler(pitch, yaw, 0f);
        transform.position = target + rot * new Vector3(0f, 0f, -distance);
        transform.rotation = rot;
    }

    void OnGUI()
    {
        var style = new GUIStyle(GUI.skin.label) { fontSize = 16 };
        style.normal.textColor = new Color(1f, 1f, 1f, 0.85f);
        GUI.Label(new Rect(16, 12, 600, 30), "Drag to orbit  ·  Scroll to zoom  ·  Click a shape", style);
    }
}
