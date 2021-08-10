using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Pun.UtilityScripts;
using System;
using UnityEngine.UI;

public class FPSPlayerController : MonoBehaviourPunCallbacks, IPunObservable {

    private Animator animator;

    private PhotonView photonView;
    private MPManager manager;

    public AudioSource playerAudioSource;

    private float health;
    private float maxHealth;
    private Transform uiHealth;
    private int lastHitBy;

    private GunController[] holster;
    private int currentGun = 0;
    public int fcurrentGun;

    private Text weaponNameText;
    private Text currentAmmoText;


    void Start() {

        animator = GetComponent<Animator>();
        photonView = gameObject.GetComponent<PhotonView>();

        weaponNameText = GameObject.Find("WeaponName").GetComponent<Text>();
        currentAmmoText = GameObject.Find("AmmoCount").GetComponent<Text>();

        holster = gameObject.GetComponentsInChildren<GunController>(true);
        SelectWeapon();

        maxHealth = 100f;
        health = maxHealth;
        lastHitBy = -1;

        if (photonView.IsMine) {
            uiHealth = GameObject.Find("HUD/Health/Bar").transform;
            manager = GameObject.Find("Manager").GetComponent<MPManager>();
            PhotonNetwork.LocalPlayer.NickName = MainMenuController.playerName; ;
            manager.UpdateScoreBoard();
            currentAmmoText.text = holster[currentGun].currentAmmo.ToString();
        }


    }




    void Update() {

        if (!photonView.IsMine) {
            RefreshMultiplayerState();
            return;
        }

        //manager.UpdateScoreBoard();
        UIRefresh();

        if (health <= 0) {
            Dead();
        }

        if (MPManager.gameOver || MPManager.paused) {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            return;
        }

        if (Input.GetKeyDown(KeyCode.Y) || Input.GetKeyDown(KeyCode.Escape)) {
            manager.TogglePaused();
        }

        if (Input.GetKeyDown(KeyCode.I)) {
            manager.EndGame();
        }


        if (Input.GetKey("w")) {
            AnimatorWalk("MovingForward", true);
        } else {
            AnimatorWalk("MovingForward", false);
        }

        if (Input.GetKey("s")) {
            AnimatorWalk("MovingBackward", true);
        } else {
            AnimatorWalk("MovingBackward", false);
        }

        if (Input.GetKey("a") || Input.GetKey("d")) {
            AnimatorWalk("Strafing", true);
        } else {
            AnimatorWalk("Strafing", false);
        }

        if (Input.GetButton("Fire1")) {
            ProcessShot();
        }

        if (Input.GetKeyDown(KeyCode.K)) {
            photonView.RPC("TakeDamage", RpcTarget.All, 10f);
        }

        int previousSelectedWeapon = currentGun;
        if (Input.GetAxis("Mouse ScrollWheel") > 0f && !holster[currentGun].isReloading) {
            currentGun = (currentGun + 1) % holster.Length;
        } else if (Input.GetAxis("Mouse ScrollWheel") < 0f && !holster[currentGun].isReloading) {
            currentGun = (currentGun + holster.Length + 1) % holster.Length;
        }
        if (previousSelectedWeapon != currentGun) {
            SelectWeapon();
        }
        if (holster[currentGun].isReloading) {
            animator.SetBool("Reloading", true);
        } else {
            animator.SetBool("Reloading", false);

            if (currentAmmoText.text == "0") {
                currentAmmoText.text = holster[currentGun].currentAmmo.ToString();
            }
        }


    }


    void UIRefresh() {
        float healthRatio = health / maxHealth;
        if (Mathf.Sign(healthRatio) == -1) {
            healthRatio = 0;
        }
        uiHealth.localScale = Vector3.Lerp(uiHealth.localScale, new Vector3(healthRatio, 1, 1), Time.deltaTime * 10f);
    }

    void ProcessShot() {
        if (holster[currentGun].Shoot()) {

            currentAmmoText.text = holster[currentGun].currentAmmo.ToString();

            RaycastHit hit;
            if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit)) {

                if (hit.transform.tag == "Player") {
                    hit.transform.GetComponent<PhotonView>().RPC("TakeDamage", RpcTarget.All, holster[currentGun].damage, PhotonNetwork.LocalPlayer.ActorNumber);
                }

                IDamageable target = hit.transform.GetComponent<IDamageable>();
                if (target != null) {
                    target.TakeDamage(holster[currentGun].damage);
                }


                if (hit.rigidbody != null) {
                    hit.rigidbody.AddForce(-hit.normal * holster[currentGun].impactForce, ForceMode.Impulse);
                }

                photonView.RPC("DistributeShotHit", RpcTarget.All, hit.point, hit.normal);
            } else {
                photonView.RPC("DistributeShotMiss", RpcTarget.All);
            }
        }
    }

    void SelectWeapon() {
        int i = 0;
        foreach (GunController weapon in holster) {
            if (i == currentGun) {
                weapon.gameObject.SetActive(true);
            } else {
                weapon.gameObject.SetActive(false);
            }
            i++;
        }

        if (photonView.IsMine) {
            weaponNameText.text = holster[currentGun].transform.name;
            currentAmmoText.text = holster[currentGun].currentAmmo.ToString();
        }

    }

    public float GetHealth() {
        return health;
    }

    public void SetHealth(float health) {
        this.health = health;
    }

    //Network Calls
    [PunRPC]
    void DistributeShotHit(Vector3 point, Vector3 normal) {
        holster[currentGun].muzzleFlash.Play();
        playerAudioSource.PlayOneShot(holster[currentGun].shootSound, 0.1f);
        GameObject impact = Instantiate(holster[currentGun].impactEffect, point, Quaternion.LookRotation(normal));
        Destroy(impact, 2f);
    }

    [PunRPC]
    void DistributeShotMiss() {
        holster[currentGun].muzzleFlash.Play();
        playerAudioSource.PlayOneShot(holster[currentGun].shootSound, 0.1f);
    }

    [PunRPC]
    void TakeDamage(float damage, int playerActorNumber) {
        health -= damage;
        lastHitBy = playerActorNumber;
    }

    public void TakeDamage(float damage) {
        TakeDamage(damage, -1);
        manager.UpdateScoreBoard();
    }

    void Dead() {
        if (lastHitBy != -1) {
            PhotonNetwork.CurrentRoom.GetPlayer(lastHitBy).AddScore(1);
            manager.AddNewKill(PhotonNetwork.LocalPlayer, PhotonNetwork.CurrentRoom.GetPlayer(lastHitBy));
            lastHitBy = -1;
        }

        manager.Spawn();
        if (!MPManager.gameOver) {
            PhotonNetwork.Destroy(gameObject);
        }
    }

    void AnimatorWalk(string s, bool action) {
        animator.SetBool(s, action);
    }

    [PunRPC]
    void FireWeapon() {
        GunController gun = gameObject.GetComponentInChildren<GunController>();
        gun.Shoot();
    }


    //Synchronisation
    public void OnPhotonSerializeView(PhotonStream p_stream, PhotonMessageInfo p_message) {
        if (p_stream.IsWriting) {
            p_stream.SendNext((float)(health));
            p_stream.SendNext((int)(currentGun));
        } else {
            health = (float)p_stream.ReceiveNext();
            currentGun = (int)p_stream.ReceiveNext();
        }
    }

    void RefreshMultiplayerState() {
        SelectWeapon();
    }

    public PlayerRecord ToRecord() {
        return new PlayerRecord(transform.position, transform.rotation, health);
    }
}



[Serializable]
public struct PlayerRecord {
    public Vector3 position;
    public Quaternion rotation;
    public float health;

    public PlayerRecord(Vector3 position, Quaternion rotation, float health) {
        this.position = position;
        this.rotation = rotation;
        this.health = health;
    }
}
