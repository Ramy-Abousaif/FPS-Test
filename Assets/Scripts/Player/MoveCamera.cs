using UnityEngine;
using UnityEngine.InputSystem;

public class MoveCamera : MonoBehaviour
{
    public bool toggled = true;
    public GameObject player;

    [Range(0, 0.1f)] public float desiredAmplitude = 0.015f;
    [Range(0, 30f)] public float desiredFrequency = 10.0f;

    public Transform vCam;
    public Transform camHolder;
    public Transform head;

    private float toggleSpeed = 1.5f;
    private Vector3 startPos;
    private Rigidbody rb;
    private PlayerMovement pm;
    private PlayerManager manager;

    private void Awake()
    {
        manager = GetComponentInParent<PlayerManager>();
        rb = player.GetComponent<Rigidbody>();
        pm = player.GetComponent<PlayerMovement>();
        startPos = vCam.localPosition;
    }

    private void Update()
    {
        // Lerping caused the camera to be very jittery as it tried to catch up to the player
        camHolder.position = Vector3.Lerp(camHolder.position, new Vector3(player.transform.position.x, head.position.y, player.transform.position.z), Time.deltaTime * 15f);
        //camHolder.position = new Vector3(player.transform.position.x, head.position.y, player.transform.position.z);
        vCam.rotation = Camera.main.transform.rotation;

        if (!toggled)
            return;

        CheckMotion();
    }

    private Vector3 CamMotion()
    {
        Vector3 pos = Vector3.zero;
        float amplitude = desiredAmplitude;
        float frequency = desiredFrequency;
        if (pm.IsCrouching())
        {
            amplitude = desiredAmplitude / 2;
            frequency = desiredFrequency / 2;
        }
        pos = Mathf.Cos(Time.time * frequency / 2) * amplitude * 2 * vCam.transform.right;
        pos.y += Mathf.Sin(Time.time * frequency) * amplitude;

        return pos;
    }

    void PlayMotion(Vector3 motion)
    {
        vCam.localPosition = Vector3.Lerp(vCam.localPosition, startPos + motion, Time.deltaTime * 15f);
    }

    void CheckMotion()
    {

        float speed = new Vector3(rb.velocity.x, 0, rb.velocity.z).magnitude;

        //if (speed < toggleSpeed)
        //    return;
        //if (!pm.isGrounded)
        //    return;
        //
        if (speed < toggleSpeed || !pm.grounded)
            ResetPos();
        else
            PlayMotion(CamMotion());
    }

    void ResetPos()
    {
        vCam.localPosition = Vector3.Lerp(vCam.localPosition, startPos, Time.deltaTime);
    }
}