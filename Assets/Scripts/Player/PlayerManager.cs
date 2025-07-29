using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager instance;

    [Header("Transform Information")]
    public Transform player;
    public Transform playerOrientation;
    public Transform playerCamera;

    [Header("Player Controller Scripts")]
    public PlayerMovement playerMovement;
    public PlayerInteraction playerInteraction;
    public PlayerInputs playerInputs;

    //[Header("Player Camera Scripts")]
    //public MoveCameraV2 cameraMovement;

    [Header("Inventory Scripts")]
    public ItemManager itemManager;
    public ItemSway itemSway;
    public ItemMotion itemMotion;

    [Header("Other")]
    public TimeManager timeManager;

    private void Awake()
    {
        SetupPlayer();
    }

    public void SetupPlayer()
    {
        if (instance)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;

        DontDestroyOnLoad(gameObject);
    }

    public void SetPlayerPosition(Vector3 position)
    {
        player.position = position;
    }
}
