using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class FPSCameraController : MonoBehaviourPunCallbacks {

    //Uncomment to implement IPunObservable
    //int fxRotation;
    //int fmouseX;

    public float mouseSensitivity = 100f;
    float xRotation = 0f;
    float mouseX;
    float mouseY;

    

    public Transform mainSpine;
    public Transform cameraHook;

    private PhotonView photonView;


    void Start() {

        photonView = gameObject.GetComponent<PhotonView>();

        if (!photonView.IsMine) {
            gameObject.GetComponentInChildren<Camera>().enabled = false;
            gameObject.GetComponentInChildren<AudioListener>().enabled = false;
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update() {

        if (!photonView.IsMine) {
            //RefreshMultiplayerState();
            return;
        }

        if (MPManager.gameOver || MPManager.paused) {
            return;
        }


        mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);    

    }

    void LateUpdate() {

        if (photonView.IsMine) {
            photonView.RPC("CamRot", RpcTarget.All, xRotation, mouseX);            
        }
        photonView.RPC("SpineRot", RpcTarget.All);

        //cameraHook.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        //transform.Rotate(Vector3.up * mouseX);
        //mainSpine.forward = cameraHook.forward;
        //mainSpine.transform.Rotate(0, 60, 0);
    }

    [PunRPC]
    void CamRot(float xRotation, float mouseX) {
        cameraHook.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }

    [PunRPC]
    void SpineRot() {
        mainSpine.forward = cameraHook.forward;
        mainSpine.transform.Rotate(0, 60, 0);
    }



    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    //public void OnPhotonSerializeView(PhotonStream p_stream, PhotonMessageInfo p_message) {
    //    if (p_stream.IsWriting) {
    //        p_stream.SendNext((int)(xRotation) * 100);
    //        p_stream.SendNext((int)(mouseX) * 100);
    //    } else {
    //        fxRotation = (int)p_stream.ReceiveNext() / 100;
    //        fmouseX = (int)p_stream.ReceiveNext() / 100;
    //    }
    //}

    //void RefreshMultiplayerState() {

    //    cameraHook.transform.localRotation = Quaternion.Euler(fxRotation, 0f, 0f);
    //    transform.Rotate(Vector3.up * fmouseX);
    //    mainSpine.forward = cameraHook.forward; // attempt to rotate players upper body to face cameras vertical aim (But it does nothing)
    //    mainSpine.transform.Rotate(0, 60, 0);
    //}

}

