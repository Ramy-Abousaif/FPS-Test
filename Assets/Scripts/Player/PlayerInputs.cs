using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputs : MonoBehaviour
{
    private PlayerManager manager;
    private Camera mainCam;
    private PlayerMovement pm;
    private PlayerInteraction pi;
    private ItemSway iss;
    private ItemMotion imo;
    private ItemManager ima;
    private TimeManager tm;

    bool secondaryModifier = false;

    private void Start()
    {
        mainCam = Camera.main;
        manager = GetComponentInParent<PlayerManager>();
        pm = manager.playerMovement;
        pi = manager.playerInteraction;
        iss = manager.itemSway;
        ima = manager.itemManager;
        imo = manager.itemMotion;
        tm = manager.timeManager;
    }

    public void MoveInput(InputAction.CallbackContext context)
    {
        pm.MoveInput(context);
    }

    public void JumpInput(InputAction.CallbackContext context)
    {
        pm.JumpInput(context);
    }

    public void UsePowerInput(InputAction.CallbackContext context)
    {
        tm.UsePowerInput(context);
    }

    public void SwayAimInput(InputAction.CallbackContext context)
    {
        iss.AimInput(context);
    }

    public void GunMotionAimInput(InputAction.CallbackContext context)
    {
        imo.AimInput(context);
    }

    public void CrouchInput(InputAction.CallbackContext context)
    {
        pm.CrouchInput(context);
    }

    public void SprintInput(InputAction.CallbackContext context)
    {
        pm.SprintInput(context);
    }

    public void PickupInput(InputAction.CallbackContext context)
    {
        ima.PickupItems(context);
    }

    public void InteractInput(InputAction.CallbackContext context)
    {
        pi.InteractInput(context);
    }

    public void UseMainItem(InputAction.CallbackContext context)
    {
        ima.UseInput(context, ItemManager.Inventory.PRIMARY);
    }

    public void UseSecondaryItem(InputAction.CallbackContext context)

    {
        ima.UseInput(context, ItemManager.Inventory.SECONDARY);
    }

    public void ReloadHeldItem(InputAction.CallbackContext context)
    {
        ima.ReloadInput(context, ItemManager.Inventory.PRIMARY);
    }

    public void DiscardMainItem(InputAction.CallbackContext context)
    {
        ima.DiscardWeapon(context, ItemManager.Inventory.PRIMARY);
    }

    public void DiscardSecondaryItem(InputAction.CallbackContext context)
    {
        ima.DiscardWeapon(context, ItemManager.Inventory.SECONDARY);
    }

    public void SwitchItem(InputAction.CallbackContext context)
    {
        if (secondaryModifier)
            ima.SwitchItem(context, ItemManager.Inventory.SECONDARY);
        else
            ima.SwitchItem(context, ItemManager.Inventory.PRIMARY);
    }

    public void SecondaryModififierInput(InputAction.CallbackContext context)
    {
        if (context.started)
            secondaryModifier = true;

        if (context.canceled)
            secondaryModifier = false;
    }
}
