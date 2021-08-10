using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class FPSMovement : MonoBehaviourPunCallbacks {

    public CharacterController controller;

    public Transform groundcheck;
    public float groundDistance = 0.4f;
    public LayerMask groundMask;
    bool isGrounded;

    public float speed = 12;
    public float gravity = -20f;
    public float jumpHeight = 3;

    public Vector3 velocity;

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {

        if (!photonView.IsMine) {
            return;
        }


        isGrounded = Physics.CheckSphere(groundcheck.position, groundDistance, groundMask);

        if (isGrounded && velocity.y < 0) {
            velocity.y = -2f;
        }

        if (MPManager.gameOver || MPManager.paused) {
            return;
        }


        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");
        Vector3 move = transform.right * x + transform.forward * z;
        controller.Move(move * speed * Time.deltaTime);

        if (Input.GetButtonDown("Jump") && isGrounded) {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }


        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
	}
}
