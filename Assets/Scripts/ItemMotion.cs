using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ItemMotion : MonoBehaviour
{
    private float mouseX, mouseY;
    public float amount = 0.02f;
    public float maxamount = 0.03f;
    public float smooth = 3;
    private Quaternion def;
    public float posOffset;
    public float smoothTime;
    public float yRot = 0f;
    private Vector3 defaultPos;
    PlayerMovement pm;
    public Transform desiredGunPos;

    void Start()
    {
        def = transform.localRotation;
        defaultPos = transform.localPosition;
        pm = GetComponentInParent<PlayerManager>().playerMovement;

    }

    public void AimInput(InputAction.CallbackContext context)
    {
        var delta = context.ReadValue<Vector2>();
        mouseX = delta.x;
        mouseY = delta.y;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        Vector3 _defaultPos = defaultPos;
        Vector3 offset = pm.FindVelRelativeToLook() * posOffset;
        float fallspeed = -pm.rb.velocity.y * posOffset;
        Vector3 desiredPos = _defaultPos - new Vector3(offset.x, fallspeed, offset.y);
        desiredGunPos.localPosition = desiredPos;
        transform.position = Vector3.Lerp(transform.position, desiredGunPos.position, Time.deltaTime * smoothTime);
    }

    void Update()
    {
        float factorZ = -(mouseX) * amount;
        float factorX = -(mouseY) * amount;


        if (factorZ > maxamount)
            factorZ = maxamount;

        if (factorZ < -maxamount)
            factorZ = -maxamount;

        if (factorX > maxamount)
            factorX = maxamount;

        if (factorX < -maxamount)
            factorX = -maxamount;

        Quaternion Final = Quaternion.Euler(def.x + factorX, yRot, def.z + factorZ);
        transform.localRotation = Quaternion.Lerp(transform.localRotation, Final, (Time.deltaTime * amount) * smooth);
    }
}
