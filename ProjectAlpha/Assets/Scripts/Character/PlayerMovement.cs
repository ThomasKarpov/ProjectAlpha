﻿/*
 * Project Name: Project Alpha
 *       Author: A variaton of Erk's script, modified to not include networking.

 *  Description: The class that handles the movement logic of the player character.
 */


using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 3f;
    public float jumpHeight = 5f;
    public Vector2 drag = new Vector2(0.25f, 0.4f);
    public float gravity = -20f;


    public float dashDistance = 5f;
    public float dashCooldown = 1f;
    public float dashDuration = 1f;


    public float groundCheckDist = 1f;
    public LayerMask groundMask;

    // Floats

    private float dashCooldownTimer = 0;
    private float dashTimer = 0;

    // Components
    private CharacterController characterController;
    private Vector3 moveDirection;
    private Animator animator;

    // Bools
    private bool isGrounded;
    private bool isFacingRight = true;
    private bool isDashing;


    private void Start()
    {
        characterController = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        FindObjectOfType<GameplayContext>().SetPlayer(this);

    }

    private void Update()
    {
        isGrounded = characterController.isGrounded;
        animator.SetBool("Grounded", isGrounded);

        DashTimers();
    }

    private void FixedUpdate()
    {
        if (isGrounded && moveDirection.y < 0)
        {
            moveDirection.y = 0;
        }
        else // Apply gravity
        {
            moveDirection.y += gravity * Time.deltaTime;
        }

        moveDirection.z = 0;
        MoveCharacter();
        AddDrag();
    }

    /// <summary>
    /// All dash related timers are updated here.
    /// </summary>
    private void DashTimers()
    {
        if (dashTimer > 0)
        {
            dashTimer -= Time.deltaTime;
        }
        else if (isDashing)
        {
            isDashing = false;
        }

        if (dashCooldownTimer > 0)
        {
            dashCooldownTimer -= Time.deltaTime;
        }
    }

    /// <summary>
    /// Adds vertical and horizontal drag to the movement.
    /// This is used to simulate air/ground resistance.
    /// </summary>
    private void AddDrag()
    {
        moveDirection.x /= 1 + drag.x * Time.deltaTime;
        moveDirection.y /= 1 + drag.y * Time.deltaTime;
    }

    /// <summary>
    /// Adds the movement to the playercontroller
    /// Flips the player if necessary
    /// Sets the speed for the animator
    /// </summary>
    private void MoveCharacter()
    {
        characterController.Move(moveDirection * Time.deltaTime);

        if ((moveDirection.x > 0 && !isFacingRight) ||
            (moveDirection.x < 0 && isFacingRight))
        {
            Flip();
        }

        animator.SetFloat("Speed", Mathf.Abs(moveDirection.x) / moveSpeed);
    }

    /// <summary>
    /// Flips the character so it faces the correct direction.
    /// This is based on the players horizontal movement speed.
    /// </summary>
    private void Flip()
    {
        isFacingRight = !isFacingRight;
        transform.rotation = Quaternion.Euler(0, isFacingRight ? 90 : -90, 0);
    }

    /// <summary>
    /// Sets the horizontal speed of the character.
    /// Speed is increased based on input and is capped based on 'moveSpeed'
    /// </summary>
    /// <param name="input"></param>
    public void SetHorizontalSpeed(float input)
    {
        if (isDashing)
            return;
        float newSpeed = moveDirection.x + input;
        moveDirection.x = Mathf.Clamp(newSpeed, -moveSpeed, moveSpeed);
    }

    /// <summary>
    /// Makes the player jump by adding a burst of speed upwards
    /// Only works if the player IS grounded
    /// Also plays the jump animation
    /// </summary>
    public void Jump()
    {
        if (!isGrounded)
            return;

        animator.SetTrigger("Jump");
        moveDirection.y += jumpHeight;
    }

    /// <summary>
    /// Performs a dash by adding a burst of speed in the direciton the player is facing.
    /// Is only performed if previous dash duration is over.
    /// </summary>
    public void Dash()
    {
        if (dashCooldownTimer > 0)
            return;

        moveDirection +=
            Vector3.Scale(transform.forward,
            dashDistance * new Vector3((Mathf.Log(1f / (Time.deltaTime * drag.x + 1)) / -Time.deltaTime),
            0,
            (Mathf.Log(1f / (Time.deltaTime * drag.y + 1)) / -Time.deltaTime)));
        isDashing = true;
        dashTimer = dashDuration;
        dashCooldownTimer = dashCooldown;

    }
}
