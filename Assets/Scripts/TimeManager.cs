using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.InputSystem;

public class TimeManager : MonoBehaviour
{
    public bool timeIsStopped = false;
    private bool buttonPressed = false;
    public static TimeManager instance;
    private VolumeProfile volume;
    private ColorAdjustments thisColor;
    private float elapsedTime, timer;
    public float fxTime, cooldown;
    public Material ripple;
    public float maxRippleTime = 1.0f;

    void Awake()
    {
        timer = cooldown;
        elapsedTime = 0.0f;
        volume = GameObject.FindGameObjectWithTag("PostProcessing").GetComponent<Volume>().profile;
        volume.TryGet(out thisColor);
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void TimeStopFX()
    {
        if (timeIsStopped)
        {
            StartCoroutine(ResumeTimeFX());
        }
        else
        {
            StartCoroutine(StopTimeFX());
        }
    }

    IEnumerator StopTimeFX()
    {
        while (elapsedTime <= fxTime)
        {
            if (elapsedTime < maxRippleTime)
            {
                ripple.SetFloat("_Size", 0.3f);
                ripple.SetFloat("_Timer", elapsedTime);
            }
            else
            {
                ripple.SetFloat("_Size", 0);
            }
            if(elapsedTime > (fxTime - 1))
            {
                thisColor.saturation.Interp(thisColor.saturation.value, -100, elapsedTime * 6 / (180 * fxTime));
            }
            thisColor.hueShift.Interp(thisColor.hueShift.value, 180, elapsedTime / 180);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
    }

    IEnumerator ResumeTimeFX()
    {
        while (elapsedTime <= fxTime)
        {
            if (elapsedTime < maxRippleTime)
            {
                ripple.SetFloat("_Size", -0.2f);
                ripple.SetFloat("_Timer", elapsedTime);
            }
            else
            {
                ripple.SetFloat("_Size", 0);
            }
            thisColor.saturation.Interp(thisColor.saturation.value, 0, elapsedTime * 5 / (180 * fxTime));
            thisColor.hueShift.Interp(thisColor.hueShift.value, 0, elapsedTime * 5/ 180);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
    }

    public void UsePowerInput(InputAction.CallbackContext context)
    {
        buttonPressed = Mathf.Approximately(context.ReadValue<float>(), 1);
    }

    void Update()
    {
        //Debug.Log(thisColor.hueShift.value);
        //Debug.Log(elapsedTime);
        //Debug.Log(thisColor.saturation.value);
        timer += Time.deltaTime;
        if(timer > cooldown)
        {
            if (buttonPressed)
            {
                timer = 0.0f;
                elapsedTime = 0.0f;
                if (timeIsStopped)
                {
                    AudioManager.instance.PlayTimeResumeSound();
                    thisColor.saturation.value = -100.0f;
                    thisColor.hueShift.value = 180.0f;
                    elapsedTime = 0.0f;
                    ripple.SetFloat("_Size", 0);
                }
                else
                {
                    AudioManager.instance.PlayTimeStopSound();
                    thisColor.saturation.value = 0.0f;
                    thisColor.hueShift.value = 0.0f;
                    elapsedTime = 0.0f;
                    ripple.SetFloat("_Size", 0);
                }
                TimeStopFX();
                timeIsStopped = !timeIsStopped;
            }
        }
    }

    private void OnApplicationQuit()
    {
        thisColor.saturation.value = 0.0f;
        thisColor.hueShift.value = 0.0f;
        elapsedTime = 0.0f;
        ripple.SetFloat("_Size", 0);
    }
}
