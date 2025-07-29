using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StopTime : MonoBehaviour
{
    private Rigidbody rb;
    private float originalMass;
    private float timer = 0.0f;
    private float startTimer = 0.0f;
    private Vector3 storedVelocity, storedAngVelo;
    private bool isTimeStopped = false;
    private bool isPrevFrameFrozen = false;

    private void Start()
    {
        timer = 0.0f;
        startTimer = 0.0f;
    }

    void Update()
    {
        isTimeStopped = TimeManager.instance.timeIsStopped;
        if (GetComponent<Rigidbody>() != null)
        {
            startTimer += Time.deltaTime;
            rb = GetComponent<Rigidbody>();
            if(startTimer < 0.05f)
            {
                originalMass = rb.mass;
                storedAngVelo = rb.angularVelocity;
            }

            if (!isTimeStopped)
            {
                if(timer < 0.3f)
                {
                    if(Mathf.Round(storedVelocity.magnitude) > 0)
                        rb.velocity = storedVelocity;
                    if(Mathf.Round(storedAngVelo.magnitude) > 0)
                        rb.angularVelocity = storedAngVelo;
                }
                if (isTimeStopped != isPrevFrameFrozen)
                {
                    timer = 0.0f;
                }
                rb.isKinematic = false;
                rb.mass = originalMass;
                timer += Time.deltaTime;
            }
            else
            {
                if (timer > 0.03f)
                    rb.isKinematic = true;
                else
                {
                    storedVelocity = rb.velocity;
                    storedAngVelo = rb.angularVelocity;
                }

                if (isTimeStopped != isPrevFrameFrozen)
                {
                    timer = 0.0f;
                }

                rb.mass = 10000.0f;
                timer += Time.deltaTime;
            }
        }
        if (GetComponent<ParticleSystem>() != null)
        {
            ParticleSystem.MainModule ps = GetComponent<ParticleSystem>().main;
            if (!isTimeStopped)
            {
                if (timer < 0.3f)
                {
                    ps.simulationSpeed = 1.0f;
                }
                if (isTimeStopped != isPrevFrameFrozen)
                {
                    timer = 0.0f;
                }
                timer += Time.deltaTime;
            }
            else
            {
                if (timer > 0.03f)
                    ps.simulationSpeed = 0.0f;

                if (isTimeStopped != isPrevFrameFrozen)
                {
                    timer = 0.0f;
                }
                timer += Time.deltaTime;
            }
        }
        isPrevFrameFrozen = isTimeStopped;
    }
}
