using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    private Rigidbody2D rb;
    private Collider2D coll;
    private Animator anim;

    [SerializeField] private float speed = 8f;
    [SerializeField] private float jumpForce = 4f;
    [SerializeField] private LayerMask ground;
    [SerializeField] private bool isJumping = false;

    [SerializeField] private int hurtForce = 10;

    [SerializeField] private int cherries = 0;
    [SerializeField] private int health = 5;
    [SerializeField] private Text healthTXT;
    [SerializeField] private Text cherriesTXT;

    #region FiniteStateMachine
    private enum State { idle, running, jumping, falling, hurt }
    private State state = State.idle;
    #endregion

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        coll = GetComponent<Collider2D>();
        anim = GetComponent<Animator>();

        healthTXT.text = health.ToString();
    }

    void Update()
    {
        if (Time.deltaTime != 0)
        {
            if (state != State.hurt)
            {
                Movement();
            }
            AnimationState();
            anim.SetInteger("state", (int)state);
        }
    }

    private void Movement()
    {
        float hDirection = Input.GetAxisRaw("Horizontal");
        if (hDirection < 0)
        {
            rb.velocity = new Vector2(-speed, rb.velocity.y);
            transform.localScale = new Vector2(-1, 1);
        }
        else if (hDirection > 0)
        {
            rb.velocity = new Vector2(speed, rb.velocity.y);
            transform.localScale = new Vector2(1, 1);
        }
        else if (Input.GetButtonUp("Horizontal") && isJumping == false)
        {
            rb.velocity = new Vector2(0, rb.velocity.y);
        }
        else if (Input.GetButtonDown("Jump") && coll.IsTouchingLayers(ground))
        {
            Jump();
        }
    }

    private void Jump()
    {
        rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        state = State.jumping;
        isJumping = false;
    }

    private void AnimationState()
    {
        if (state == State.jumping)
        {
            if (rb.velocity.y < 0.3f)
            {
                state = State.falling;
            }
        }
        else if (state == State.falling)
        {
            if (coll.IsTouchingLayers(ground))
            {
                state = State.idle;
                isJumping = false;
            }
        }
        else if (state == State.hurt)
        {
            if (Mathf.Abs(rb.velocity.y) < 0.1f)
            {
                state = State.idle;
            }
        }
        else if (Mathf.Abs(rb.velocity.x) > 2f)
        {
            state = State.running;
        }
        else
        {
            state = State.idle;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Collectable")
        {
            Destroy(collision.gameObject);
            cherries += 1;
            cherriesTXT.text = cherries.ToString();
        }

    }

    private void OnCollisionEnter2D(Collision2D other)
     {
        

        if(other.gameObject.tag == "Enemy")
        {
            Enemy enemy = other.gameObject.GetComponent<Enemy>();
            if (state == State.falling)
            {
                enemy.JumpedOn();
                Jump();
                
            }
            else
            {
                state = State.hurt;
                if (other.gameObject.transform.position.x > transform.position.x)
                {
                    rb.velocity = new Vector2(-hurtForce, rb.velocity.y);
                    health -= 1;
                    healthTXT.text = health.ToString();
                }
                else
                {
                    rb.velocity = new Vector2(hurtForce, rb.velocity.y);
                    health -= 1;
                    healthTXT.text = health.ToString();
                }
                
            }
        }
    
}}
