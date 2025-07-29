using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Item : MonoBehaviour
{
    [Header("Item Class Variables")]
    float time = 0.2f;
    public bool pickingUp = false;
    public float forwardThrowForce;
    public float upThrowForce;
    public float rotationForce;
    public ItemManager.Inventory inventory;

    protected bool held;
    protected Rigidbody rb;

    protected virtual void Awake() { }

    protected virtual void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (!rb)
            rb = gameObject.AddComponent<Rigidbody>();

        rb.mass = 0.1f;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

        if (!CompareTag("Item"))
            tag = "Item";
    }

    protected virtual void FixedUpdate() { }

    protected virtual void Update() { }

    protected virtual void LateUpdate() { }

    public virtual void ProcessUseInput(InputAction.CallbackContext context) { }

    public virtual void ProcessRInput(InputAction.CallbackContext context) { }

    protected virtual void OnPickup() { }

    protected virtual void OnDrop() { }

    // Non-Virtual Functions that handle item pickups

    public void PickUp(Transform weaponHolder)
    {
        if (held) return;
        Destroy(rb);
        StartCoroutine(PickingUp(weaponHolder));
        held = true;

        OnPickup();
    }

    public void Drop(Transform playerCamera)
    {
        if (!pickingUp)
        {
            if (!held) return;
            OnDrop();
            rb = gameObject.AddComponent<Rigidbody>();
            rb.mass = 0.1f;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
            var forward = playerCamera.forward;
            forward.y = 0;
            rb.velocity = forward * forwardThrowForce;
            rb.velocity += Vector3.up * upThrowForce;
            rb.angularVelocity = Random.onUnitSphere * rotationForce;
            transform.parent = null;
            held = false;

            //Remove from DoNotDestroy
            //SceneManager.MoveGameObjectToScene(gameObject, SceneManager.GetActiveScene());
        }
    }

    public void Remove()
    {
        if (!pickingUp)
        {
            if (!held) return;
            //transform.parent = null;
            held = false;
            transform.gameObject.SetActive(false);
        }
    }

    public void SpawnHeld(Transform weaponHolder)
    {
        if (rb != null)
            Destroy(rb);
        //transform.parent = weaponHolder;
        //transform.localPosition = Vector3.zero;
        //transform.localRotation = Quaternion.Euler(0, 0, 0);
        transform.gameObject.SetActive(true);
        pickingUp = true;
        pickingUp = false;
        held = true;
    }

    protected IEnumerator PickingUp(Transform weaponHolder)
    {
        float elapsedTime = 0.0f;
        transform.parent = weaponHolder;
        Vector3 startingPosition = transform.localPosition;
        Quaternion startingRotation = transform.localRotation;
        Quaternion targetRotation = Quaternion.Euler(0, 0, 0);
        while (elapsedTime < time)
        {
            pickingUp = true;
            elapsedTime += Time.deltaTime;
            transform.localPosition = Vector3.Lerp(startingPosition, Vector3.zero, (elapsedTime / time));
            // Rotations
            transform.localRotation = Quaternion.Slerp(startingRotation, targetRotation, (elapsedTime / time));
            yield return new WaitForEndOfFrame();
        }
        pickingUp = false;
    }
}
