using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    [Header("Volume Sliders")]
    [Range(0.0f, 10.0f)] public float masterVolume = 1.0f;
    [Range(0.0f, 10.0f)] public float bgVolume = 1.0f;
    [Range(0.0f, 10.0f)] public float sfxVolume = 1.0f;
    private float pitch = 1.0f;

    //FMOD Variables
    FMOD.Studio.Bus masterBus;
    FMOD.Studio.EventInstance bgm;
    FMOD.Studio.EventInstance bga;
    [Header("Events Selector")]
    [Space(20)]
    [FMODUnity.EventRef]
    public string MusicEvent = "";
    [FMODUnity.EventRef]
    public string AmbienceEvent = "";

    //Testing Purposes
    [Header("Testing Variables")]
    [Space(20)]
    [Range(0, 1)] public int level = 0;

    private void Start()
    {
        Instance();
        masterBus = FMODUnity.RuntimeManager.GetBus("Bus:/");
        //StartBGA();
        StartBGM();
    }

    private void Update()
    {
        bgm.setVolume(masterVolume * bgVolume);
        bgm.setParameterByName("Level", level);
        bga.setVolume(masterVolume * bgVolume);
        bga.setPitch(pitch);
        bgm.setPitch(pitch);
        if (TimeManager.instance.timeIsStopped)
        {
            pitch = 0.5f;
        }
        else
        {
            pitch = 1.0f;
        }
    }

    private void Instance()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Debug.Log("Instance already exists, destroying " + this.name);
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
    }

    public void PlayFootstep(GameObject player, LayerMask groundLayer)
    {
        FMODUnity.RuntimeManager.PlayOneShotAttached("event:/SFX/Footsteps", "Ground", groundLayer, player, masterVolume * sfxVolume);
    }

    public void PlayShooting(GameObject gun)
    {
        FMODUnity.RuntimeManager.PlayOneShotAttached("event:/SFX/Shot", gun, masterVolume * sfxVolume, pitch);
    }

    public void PlayTimeStopSound()
    {
        FMODUnity.RuntimeManager.PlayOneShot("event:/SFX/TimeStop", masterVolume * sfxVolume);
    }

    public void PlayTimeResumeSound()
    {
        FMODUnity.RuntimeManager.PlayOneShot("event:/SFX/TimeResume", masterVolume * sfxVolume);
    }

    public void StartBGM()
    {
        bgm = FMODUnity.RuntimeManager.CreateInstance(MusicEvent);
        bgm.start();
    }

    public void ReleaseBGM()
    {
        bgm.release();
    }

    public void StartBGA()
    {
        bga = FMODUnity.RuntimeManager.CreateInstance(AmbienceEvent);
        bga.start();
    }

    public void ReleaseBGA()
    {
        bga.release();
    }

    public void Resume()
    {
        masterBus.setPaused(false);
    }

    public void Pause()
    {
        masterBus.setPaused(true);
    }
}
