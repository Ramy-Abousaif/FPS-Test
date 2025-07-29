using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Gun : Item
{
    [Header("Gun Class Variables")]
    public GameObject bullet;
    public Camera fpsCam;
    public Rigidbody playerRb;
    public Transform muzzle;
    public GameObject muzzleFlash;
    public float shootforce, upwardForce, kickBackForce, kickBackSmooth;

    public float timeBetweenShooting, spread, reloadTime;
    private float timeBetweenShots;
    public int magazineSize;
    [Range(1, 100)]
    public int bulletsPerTap;
    public float playerKnockback;
    public bool autoFiring;

    [Header("Charge Gun Variables")]
    public bool chargeGun;
    public ParticleSystem chargeEffect;
    private ParticleSystem.EmissionModule chargeEmission;
    private Material chargeMat;
    private float ccCount = 1;
    public float chargeSizeRate = 0.025f;
    public float chargeCollisionMultiplier = 2f;
    public float chargeSizeCapacity = 0.15f;
    private bool charging, charged = false;
    private GameObject currentChargeBullet = null;
    private float chargeTime = 0.0f;
    private float _time;
    private float animTime;
    private int bulletsLeft, bulletsShot;

    bool shooting, readyToShoot, reloading;

    //public TextMeshProUGUI ammunitionDisplay;

    private Vector3 _startPosition;
    private Quaternion _startRotation;
    private float _rotationTime;

    private bool allowInvoke = true;

    bool canUse = false;

    protected override void Start()
    {
        base.Start();

        if (GameObject.FindGameObjectWithTag("Player") != null)
            playerRb = GameObject.FindGameObjectWithTag("Player").GetComponent<Rigidbody>();

        if (chargeEffect != null)
        {
            chargeEmission = chargeEffect.emission;
            chargeMat = chargeEffect.GetComponent<Renderer>().material;
        }
        fpsCam = Camera.main;
    }

    protected override void OnPickup()
    {
        base.OnPickup();

        _startPosition = transform.localPosition;
        _startRotation = transform.localRotation;
        bulletsLeft = magazineSize;
        readyToShoot = true;
        canUse = true;
        fpsCam = Camera.main;
    }

    protected override void OnDrop()
    {
        base.OnDrop();
        canUse = false;
    }

    protected override void Update()
    {
        base.Update();

        if (!held)
            return;

        if (readyToShoot && shooting && !reloading && bulletsLeft <= 0) Reload();

        if (chargeGun)
        {
            bulletsShot = 0;
            Charge();
        }
        else
        {
            if (readyToShoot && shooting && !reloading && bulletsLeft > 0)
            {
                bulletsShot = 0;
                Shoot();
            }
        }

        //Debug.Log(readyToShoot + " " + shooting + " " + !reloading + " " + (bulletsLeft > 0));

        Anims();

        //if (ammunitionDisplay != null)
        //    ammunitionDisplay.SetText(bulletsLeft / bulletsPerTap + " / " + magazineSize / bulletsPerTap);
    }

    private void Anims()
    {
        if (_time < animTime)
        {
            _time += Time.deltaTime;
            _time = Mathf.Clamp(_time, 0f, animTime);
            float delta = -(Mathf.Cos(Mathf.PI * (_time / animTime)) - 1f) / 2f;
            transform.localPosition = Vector3.Lerp(_startPosition, Vector3.zero, delta);
            transform.localRotation = Quaternion.Lerp(_startRotation, Quaternion.identity, delta);
        }
        else
        {
            transform.localRotation = Quaternion.identity;
            transform.localPosition = Vector3.Lerp(transform.localPosition, Vector3.zero, kickBackSmooth * Time.deltaTime);
        }

        if (reloading)
        {
            _rotationTime += Time.deltaTime;
            float spinDelta = -(Mathf.Cos(Mathf.PI * (_rotationTime / reloadTime)) - 1f) / 2f;
            transform.localRotation = Quaternion.Euler(new Vector3(spinDelta * 360f, 0, 0));
        }
    }

    public override void ProcessUseInput(InputAction.CallbackContext context)
    {
        base.ProcessUseInput(context);

        if (autoFiring || chargeGun)
        {
            if (context.started)
            {
                charging = true;
                shooting = true;
            }
            if (context.canceled)
            {
                charging = false;
                chargeTime = 0.0f;
                shooting = false;
            }
        }
        else
            shooting = context.performed;
    }

    public override void ProcessRInput(InputAction.CallbackContext context)
    {
        base.ProcessRInput(context);

        if ((context.started) && bulletsLeft < magazineSize && !reloading)
            Reload();
    }

    private void Shoot(GameObject currentBullet = null)
    {
        readyToShoot = false;

        //Shoot Anim is bad atm
        //playerAnims.TriggerUseAnimation();

        Ray ray = fpsCam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;

        Vector3 targetPoint;
        if (Physics.Raycast(ray, out hit))
            targetPoint = hit.point;
        else
            targetPoint = ray.GetPoint(75);

        Vector3 directionWithoutSpread = targetPoint - muzzle.position;

        float x = Random.Range(-spread, spread);
        float y = Random.Range(-spread, spread);

        Vector3 directionWithSpread = directionWithoutSpread + new Vector3(x, y, 0);

        if (!currentBullet)
            currentBullet = Instantiate(bullet, muzzle.position, Quaternion.identity);

        if (Physics.Raycast(ray, out hit, 3f))
        {
            if (!(hit.transform.gameObject.layer == LayerMask.NameToLayer("HeldItem") ||
                hit.transform.CompareTag("Bullet") || hit.transform.CompareTag("Player"))) return;
            Destroy(currentBullet);
            currentBullet.GetComponent<BulletBehaviour>().HitEffect(hit);
        }

        ShootBullet(currentBullet, directionWithSpread);
    }

    private void ShootInvoke()
    {
        Shoot();
    }

    private void Charge(GameObject currentBullet = null)
    {
        Material chargeMat = chargeEffect.GetComponent<Renderer>().material;
        if (charging && readyToShoot)
        {
            if (chargeTime >= 0.0f)
            {
                chargeTime += Time.deltaTime;
                chargeEffect.gameObject.SetActive(true);
                chargeMat.SetFloat("_Alpha", 1.0f);
                chargeEmission.rateOverTime = 500.0f;
            }
            if (chargeTime >= 3.0f)
            {
                if (!charged)
                {
                    currentBullet = Instantiate(bullet, muzzle.position, Quaternion.identity);
                    currentBullet.transform.SetParent(muzzle);
                    currentChargeBullet = currentBullet;
                    currentChargeBullet.transform.localScale = Vector3.zero;
                    currentChargeBullet.gameObject.layer = LayerMask.NameToLayer("ChargeBullet");
                }

                BulletBehaviour ccb = currentChargeBullet.GetComponent<BulletBehaviour>();
                ccb.enableCollision = false;
                Vector3 ccbScale = currentChargeBullet.transform.localScale;
                if (ccbScale.magnitude < chargeSizeCapacity)
                {
                    ccbScale = new Vector3(ccbScale.x + (Time.deltaTime * chargeSizeRate),
                        ccbScale.y + (Time.deltaTime * chargeSizeRate), ccbScale.z + (Time.deltaTime * chargeSizeRate));
                    currentChargeBullet.transform.localScale = ccbScale;
                    ccCount += Time.deltaTime * chargeCollisionMultiplier;
                    ccb.maxCollisions = (int)ccCount;
                }

                charged = true;
            }
        }
        else
        {
            chargeEmission.rateOverTime = 0.0f;
            chargeMat.SetFloat("_Alpha",
                Mathf.Lerp(chargeMat.GetFloat("_Alpha"), 0.0f, Time.deltaTime * 6.0f));
            if (chargeMat.GetFloat("_Alpha") <= 0)
                chargeEffect.gameObject.SetActive(false);

            if (charged && readyToShoot && !reloading && bulletsLeft > 0)
            {
                chargeTime = 0.0f;
                charging = false;
                bulletsShot = 0;
                currentChargeBullet.gameObject.layer = LayerMask.NameToLayer("Bullet");
                Shoot(currentChargeBullet);
            }

            charged = false;
        }
    }

    private void ShootBullet(GameObject currentBullet, Vector3 direction)
    {
        currentBullet.transform.SetParent(null);
        Rigidbody currentRB = currentBullet.GetComponent<Rigidbody>();
        transform.localPosition -= new Vector3(0, 0, kickBackForce);
        currentBullet.GetComponent<BulletBehaviour>().enableCollision = true;
        currentBullet.transform.forward = direction.normalized;
        currentRB.AddForce(direction.normalized * shootforce, ForceMode.Impulse);
        currentRB.AddForce(fpsCam.transform.up * upwardForce, ForceMode.Impulse);
        if (muzzleFlash != null)
            Instantiate(muzzleFlash, muzzle.position, Quaternion.identity, muzzle.transform);

        bulletsLeft--;
        bulletsShot++;
        ccCount = 1;

        if (allowInvoke)
        {
            Invoke("ResetShot", timeBetweenShooting);
            allowInvoke = false;

            if (!autoFiring)
                shooting = false;

            if (playerRb != null)
                playerRb.AddForce(-direction.normalized * playerKnockback, ForceMode.Impulse);
        }

        if (bulletsShot < bulletsPerTap && bulletsLeft > 0)
            Invoke("ShootInvoke", timeBetweenShots);
    }

    private void Reload()
    {
        reloading = true;
        Invoke("ReloadFinished", reloadTime);
    }

    private void ResetShot()
    {
        readyToShoot = true;
        allowInvoke = true;
    }

    private void ReloadFinished()
    {
        bulletsLeft = magazineSize;
        _rotationTime = 0.0f;
        reloading = false;
    }
}
