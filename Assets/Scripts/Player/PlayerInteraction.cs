using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteraction : MonoBehaviour
{
    private Camera cam;
    public float interactRange = 5.0f;
    public LayerMask interactable;
    public LayerMask whatIsBattery;
    public LayerMask ammo;
    public GameObject weapon;

    private void Start()
    {
        cam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        //weapon = FindObjectOfType<ShootWeapon>().gameObject;
    }

    public void InteractInput(InputAction.CallbackContext context)
    {
        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;
        
        if (context.started)
        {
            //This can handle any and all interctable objects we may need
            if (Physics.Raycast(ray, out hit, interactRange, interactable))
            {
                Interactable interactable = hit.transform.GetComponent<Interactable>();
                if (!interactable)
                    Debug.LogError("Object " + hit.transform.name + " has interactable layer but no class!");

                interactable.Interact();
            }

            if (Physics.Raycast(ray, out hit, interactRange, whatIsBattery))
            {
                //AudioManager.instance.Pickup(hit.transform.position);
                //batCount++;
                Destroy(hit.transform.gameObject);
            }
            
            if (Physics.Raycast(ray, out hit, interactRange, ammo))
            {
                //AudioManager.instance.Pickup(hit.transform.position);
                //weapon.GetComponent<ShootWeapon>().bulletsLeft++;
                Destroy(hit.transform.gameObject);
            }
        }
    }
}
