using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletBehaviour : MonoBehaviour
{
    public bool enableCollision { get; set; }
    public Rigidbody rb;
    public GameObject impactEffect;
    public LayerMask whatIsEnemies;

    public bool useGravity;

    public int damage;
    public float explosionRange;
    public float explosionForce;
    public float playerKnockBackMulti;
    public float bulletForce = 0.0f;

    public int maxCollisions;
    public float maxLifeTime;
    public bool destroyOnTouch = true;

    int collisions;
    public bool collisionEffect = false;
    public Vector3 prevPos;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        var dir = rb.velocity;
        if (dir != Vector3.zero)
            transform.rotation = Quaternion.LookRotation(dir);

        if (collisions > maxCollisions) Explode();

        if (enableCollision)
        {
            maxLifeTime -= Time.deltaTime;
            if (maxLifeTime <= 0) Explode();
        }
        HitDetection();
    }

    private void Explode()
    {
        Collider[] enemies = Physics.OverlapSphere(transform.position, explosionRange, whatIsEnemies);

        for (int i = 0; i < enemies.Length; i++)
        {
            //Get component of enemy and call take damage

            if (enemies[i].GetComponent<Rigidbody>() != null)
            {
                if (!enemies[i].CompareTag("Player"))
                    enemies[i].GetComponent<Rigidbody>().AddExplosionForce(explosionForce, transform.position, explosionRange);
                else
                    enemies[i].GetComponent<Rigidbody>().AddExplosionForce(explosionForce * playerKnockBackMulti, transform.position, explosionRange);
            }


        }

        Destroy(gameObject);
    }

    private void LateUpdate()
    {
        prevPos = transform.position;
    }

    private void HitDetection()
    {
        if (!enableCollision) return;

        RaycastHit hit;

        if (Physics.Raycast(prevPos, (transform.position - prevPos).normalized, out hit, (transform.position - prevPos).magnitude))
        {
            if (hit.transform.CompareTag("Bullet") || hit.transform.CompareTag("Player") || hit.transform.gameObject.layer == LayerMask.NameToLayer("HeldItem")) return;

            if (hit.rigidbody != null)
                hit.rigidbody.AddForceAtPosition((transform.position - prevPos).normalized * bulletForce, hit.point);

            if (collisions < maxCollisions)
            {
                transform.position = hit.point;
                rb.velocity = Vector3.Reflect(rb.velocity, hit.normal);
                transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y + 0.5f, transform.localPosition.z);
                if (collisionEffect)
                    HitEffect(hit);
            }

            HitEffect(hit);
            collisions++;
            Debug.Log(hit.transform.gameObject.name);
            if (hit.transform.CompareTag("Enemy") && destroyOnTouch) Explode();
        }

        Debug.DrawRay(prevPos, (transform.position - prevPos).normalized);
    }

    public void HitEffect(RaycastHit hit)
    {
        //if (hit.transform.CompareTag("Enemy"))
        //    Enemy.DamageEnemy(hit, 7.5f, Vector3.zero);
        //else if (impactEffect != null)
        //    Instantiate(impactEffect, hit.point, Quaternion.identity);
        if (impactEffect != null)
            Instantiate(impactEffect, hit.point, Quaternion.identity);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRange);
    }
}
