using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ItemSway : MonoBehaviour
{
    public float swaySize;
    public float swaySmooth;
    private float mouseX, mouseY;

    public void AimInput(InputAction.CallbackContext context)
    {
        var delta = context.ReadValue<Vector2>();
        mouseX = delta.x;
        mouseY = delta.y;
    }

    private void Update()
    {
        Vector2 mouseDelta = -new Vector2(mouseX, mouseY);
        transform.localPosition = Vector3.Lerp(transform.localPosition, Vector3.zero, swaySmooth * Time.deltaTime);
        transform.localPosition += (Vector3)mouseDelta * swaySize;
    }
}
