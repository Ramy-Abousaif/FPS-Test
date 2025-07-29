using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotate : MonoBehaviour
{
    public float rotSpeed = 100.0f;
    public float bobSpeed = 1.0f;
    public int frameCount = 360;
    int elapsedFramesEnlarge = 0;
    public LayerMask lm;
    public static Rotate instance;

    void Start()
    {
        transform.localScale = new Vector3(0, 0, 0);
    }

    void Update()
    {
        RaycastHit hit;
        if(Physics.Raycast(new Vector3(transform.position.x, transform.position.y + 1, transform.position.z),
            Vector3.down, out hit, Mathf.Infinity, lm))
        {
            transform.position = new Vector3(transform.position.x, (0.25f * Mathf.Sin((Time.time) * bobSpeed)) + hit.point.y + 1, transform.position.z);
        }
        float interpolationRatio = (float)elapsedFramesEnlarge / frameCount;
        transform.localScale = Vector3.Lerp(transform.localScale, new Vector3(1, 1, 1), interpolationRatio);
        elapsedFramesEnlarge = (elapsedFramesEnlarge + 1) % (frameCount + 1);
        transform.Rotate(0, rotSpeed * Time.deltaTime, 0);
    }
}
