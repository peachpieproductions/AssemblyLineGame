using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {


    public SpriteRenderer spr;
    public Animator animator;

    Rigidbody2D rb;

    private void Awake() {
        rb = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate() {

        rb.velocity = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")) * Time.deltaTime * 200;
        //if (Input.GetAxisRaw("Horizontal") == 0 && Input.GetAxisRaw("Vertical") == 0) rb.velocity *= .85f;
        if (rb.velocity.x != 0) spr.flipX = rb.velocity.x < 0;

        animator.SetBool("Walking", rb.velocity.magnitude > .1f);

        //set z value
        transform.position = new Vector3(transform.position.x, transform.position.y, 0);
        

    }




}
