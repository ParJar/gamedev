using UnityEngine;
using System.Collections;

public class DamageableObject : MonoBehaviour, IDamageable {

    public float health = 50f;
    public bool isExploding;
    public GameObject deathEffect;

    public void TakeDamage(float amount)
    {
        health -= amount;
        if(health <= 0)
        {
            StartCoroutine(Die());
        }
    }

    private IEnumerator Die()
    {
        if (isExploding)
        {
            Rigidbody rb = gameObject.GetComponent<Rigidbody>();
            if (rb != null)
            {
                Vector3 exploc = rb.position;
                exploc.z = exploc.z + 1;

                rb.AddExplosionForce(20, exploc, 12, 3.0F, ForceMode.Impulse);
                health = 1000;

                GameObject impact = Instantiate(deathEffect, transform.position, transform.rotation);
                Destroy(impact, 2f);

                yield return new WaitForSeconds(3f);
                Destroy(gameObject);
            }
        } else
        {
            Destroy(gameObject);
        }
    }
}
