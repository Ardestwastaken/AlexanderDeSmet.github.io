using UnityEngine;

// Rotates an object and bobs it up and down. Clicking it gives it a new color and a little bounce.
public class Spinner : MonoBehaviour
{
    public Vector3 spinSpeed = new Vector3(0f, 45f, 0f);
    public float bobHeight = 0.25f;
    public float bobSpeed = 1.5f;

    Vector3 startPos;
    float phase;
    float punch;
    Renderer rend;

    void Start()
    {
        startPos = transform.position;
        phase = Random.value * Mathf.PI * 2f;
        rend = GetComponent<Renderer>();
    }

    void Update()
    {
        transform.Rotate(spinSpeed * Time.deltaTime, Space.Self);
        transform.position = startPos + Vector3.up * Mathf.Sin(Time.time * bobSpeed + phase) * bobHeight;

        punch = Mathf.MoveTowards(punch, 0f, Time.deltaTime * 2f);
        transform.localScale = Vector3.one * (1f + Mathf.Sin(punch * Mathf.PI) * 0.35f);
    }

    public void Poke()
    {
        punch = 1f;
        if (rend != null)
            rend.material.color = Color.HSVToRGB(Random.value, 0.65f, 0.95f);
    }
}
