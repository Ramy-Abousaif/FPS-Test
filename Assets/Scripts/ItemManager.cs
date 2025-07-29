using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.InputSystem;

public class ItemManager : MonoBehaviour
{
    //public LayerMask primaryLayer;
    //public LayerMask secondaryLayer;
    public float pickupRange;
    public float pickupRadius;

    public Transform primaryHoldPosition;
    public Transform secondaryHoldPosition;
    //public TMP_Text ammoText;
    public Camera playerCam;
    // public Image crosshairImage;

    public bool _isPrimaryHeld;
    public bool _isSecondaryHeld;
    //public Inventory currentInventory = Inventory.PRIMARY;
    public int currentPrimary = 0;
    public int currentSecondary = 0;
    public Item[] allPrimaries;
    public Item heldPrimary;
    public Item[] allSecondaries;
    public Item heldSecondary;

    Vector3 pos1;
    Vector3 pos2;
    public float handSpeed = 1f;
    public float handExtension = 0.5f;

    private void Start()
    {
        pos1 = primaryHoldPosition.localPosition;
        pos2 = secondaryHoldPosition.localPosition;
    }

    public enum Inventory
    {
        PRIMARY,
        SECONDARY
    }

    public void DiscardWeapon(InputAction.CallbackContext context, Inventory inventory)
    {
        if (context.started)
        {
            switch (inventory)
            {
                case Inventory.PRIMARY:
                    DiscardFunc(ref currentPrimary, ref allPrimaries, ref heldPrimary, ref _isPrimaryHeld);
                    break;
                case Inventory.SECONDARY:
                    DiscardFunc(ref currentSecondary, ref allSecondaries, ref heldSecondary, ref _isSecondaryHeld);
                    break;
            }
        }
    }

    private void DiscardFunc(ref int currentItem, ref Item[] allItems, ref Item heldItem, ref bool _isItemHeld)
    {
        if (_isItemHeld && !heldItem.pickingUp)
        {
            allItems[currentItem] = null;
            SetLayers(heldItem.gameObject, LayerMask.NameToLayer("Item"));
            heldItem.Drop(playerCam.transform);
            heldItem = null;
            _isItemHeld = false;
        }
    }

    public void PickupItems(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            Item itemToGet = CheckForItems();
            if (!itemToGet)
                return;

            switch (itemToGet.inventory)
            {
                case Inventory.PRIMARY:
                    PickupFunc(ref currentPrimary, ref allPrimaries, ref heldPrimary, ref itemToGet, ref _isPrimaryHeld, ref primaryHoldPosition);
                    break;
                case Inventory.SECONDARY:
                    PickupFunc(ref currentSecondary, ref allSecondaries, ref heldSecondary, ref itemToGet, ref _isSecondaryHeld, ref secondaryHoldPosition);
                    break;
            }
        }
    }

    public string LayermaskToName(LayerMask layer)
    {
        return LayerMask.LayerToName((int)Mathf.Log(layer.value, 2));
    }

    private Item CheckForItems()
    {
        var hitList = new RaycastHit[256];
        var hitNumber = Physics.CapsuleCastNonAlloc(playerCam.transform.position,
            playerCam.transform.position + playerCam.transform.forward * pickupRange, pickupRadius, playerCam.transform.forward,
            hitList, 1 << LayerMask.NameToLayer("Item"));

        var realList = new List<RaycastHit>();
        for (var i = 0; i < hitNumber; i++)
        {
            var hit = hitList[i];
            if (hit.transform.gameObject.layer != LayerMask.NameToLayer("Item"))
                continue;
            if (hit.point == Vector3.zero)
                realList.Add(hit);
            else if (Physics.Raycast(playerCam.transform.position, hit.point - playerCam.transform.position, out var hitInfo,
                hit.distance + 0.1f, 1 << LayerMask.NameToLayer("Item")) && hitInfo.transform == hit.transform)
                realList.Add(hit);
        }

        if (realList.Count == 0) return null;

        realList.Sort((hit1, hit2) => {
            var dist1 = GetDistanceTo(hit1);
            var dist2 = GetDistanceTo(hit2);
            return Mathf.Abs(dist1 - dist2) < 0.001f ? 0 : dist1 < dist2 ? -1 : 1;
        });

        Item item = realList[0].transform.GetComponentInParent<Item>();
        if (!item)
        {
            Debug.LogError("Item does not have Item Component");
            return null;
        }

        return item;
    }

    private void PickupFunc(ref int currentItem, ref Item[] allItems, ref Item heldItem, ref Item newItem, ref bool _isItemHeld, ref Transform itemHoldPosition)
    {
        if (!_isItemHeld)
        {
            _isItemHeld = true;

            heldItem = newItem;
            heldItem.PickUp(itemHoldPosition);
            SetLayers(heldItem.gameObject, LayerMask.NameToLayer("HeldItem"));

            for (int a = currentItem; a < allItems.Length - 1; a++)
            {
                allItems[a] = allItems[a + 1];
            }
            System.Array.Resize(ref allItems, allItems.Length);
            allItems[currentItem] = heldItem;
        }
    }

    private void SetLayers(GameObject obj, LayerMask layerMask)
    {
        obj.layer = layerMask;
        Transform[] children = obj.transform.GetComponentsInChildren<Transform>();
        for (int i = 0; i < children.Length; i++)
        {
            children[i].gameObject.layer = layerMask;
        }
    }

    public void SwitchItem(InputAction.CallbackContext context, Inventory inventory)
    {
        float scrollWheel = context.ReadValue<float>();
        switch (inventory)
        {
            case Inventory.PRIMARY:
                ScrollItems(ref scrollWheel, ref currentPrimary, ref allPrimaries, ref heldPrimary, ref _isPrimaryHeld, ref primaryHoldPosition);
                break;
            case Inventory.SECONDARY:
                ScrollItems(ref scrollWheel, ref currentSecondary, ref allSecondaries, ref heldSecondary, ref _isSecondaryHeld, ref secondaryHoldPosition);
                break;
        }
    }

    private void ScrollItems(ref float scrollWheel, ref int currentItem, ref Item[] allItems, ref Item heldItem, ref bool _isItemHeld, ref Transform itemHoldPosition)
    {
        if (scrollWheel > 0)
        {
            if (currentItem >= allItems.Length - 1)
                currentItem = 0;
            else
                currentItem++;
        }
        if (scrollWheel < 0)
        {
            if (currentItem <= 0)
                currentItem = allItems.Length - 1;
            else
                currentItem--;
        }

        if (heldItem != null)
        {
            heldItem.Remove();
            heldItem = null;
            _isItemHeld = false;
        }
        for (int i = 0; i < allItems.Length; i++)
        {
            if (currentItem == i)
            {
                if (allItems[i] != null)
                {
                    heldItem = allItems[i];
                    heldItem.SpawnHeld(itemHoldPosition);
                    _isItemHeld = true;
                }
            }
        }
    }

    private float GetDistanceTo(RaycastHit hit)
    {
        return Vector3.Distance(playerCam.transform.position, hit.point == Vector3.zero ? hit.transform.position : hit.point);
    }

    public virtual void UseInput(InputAction.CallbackContext context, Inventory inventory)
    {
        switch (inventory)
        {
            case Inventory.PRIMARY:
                if (heldPrimary)
                    heldPrimary.ProcessUseInput(context);
                break;
            case Inventory.SECONDARY:
                if (heldSecondary)
                    heldSecondary.ProcessUseInput(context);
                break;
        }
    }

    public virtual void ReloadInput(InputAction.CallbackContext context, Inventory inventory)
    {
        switch (inventory)
        {
            case Inventory.PRIMARY:
                if (heldPrimary)
                    heldPrimary.ProcessRInput(context);
                break;
            case Inventory.SECONDARY:
                if (heldSecondary)
                    heldSecondary.ProcessRInput(context);
                break;
        }
        if (heldPrimary)
            heldPrimary.ProcessRInput(context);
    }

    //public void SwitchInvInput(InputAction.CallbackContext context)
    //{
    //    if(context.started)
    //    {
    //        switch(currentInventory)
    //        {
    //            case Inventory.PRIMARY:
    //                currentInventory = Inventory.SECONDARY;
    //                break;
    //            case Inventory.SECONDARY:
    //                currentInventory = Inventory.PRIMARY;
    //                break;
    //        }
    //    }
    //}
}
