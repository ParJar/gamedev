using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class GunController : MonoBehaviour {

    public Camera fpsCam;

    public ParticleSystem muzzleFlash;
    public GameObject impactEffect;

    public float impactForce = 10f;
    public float damage = 10f;
    public float fireRate = 4f;
    public int maxAmmo = 30;
    public float reloadTime = 1f;

    public AudioClip shootSound;

    public int currentAmmo;
    public bool isReloading;
    private float nextTimeToFire;

    public Text weaponNameText;
    public Text currentAmmoText;


    void Start() {
        currentAmmo = maxAmmo;
        isReloading = false;
        nextTimeToFire = 0f;
    }


    void OnEnable() {

        isReloading = false;
    }

    void Update() {

    }

    public bool Shoot() {

        //check if reloading
        if (isReloading) {
            return false;
        }
        //check if out of ammo
        if (currentAmmo <= 0) {
            StartCoroutine(Reload());
            return false;
        }
        //Check if firing
        if (Time.time >= nextTimeToFire) {
            nextTimeToFire = Time.time + 1f / fireRate;
            currentAmmo--;
            return true;
        } else {
            return false;
        }
    }


    IEnumerator Reload() {
        isReloading = true;

        yield return new WaitForSeconds(reloadTime);

        currentAmmo = maxAmmo;
        isReloading = false;
    }
}
