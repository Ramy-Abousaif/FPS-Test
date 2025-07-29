using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private bool dead;
    public Transform playerCam;
    public Transform orientation;
    private float xRotation;
    public Rigidbody rb;

    private float moveSpeed = 4500f;
    private float walkSpeed = 20f;
    private float runSpeed = 10f;
    public bool grounded;
    public LayerMask whatIsGround;
    private bool readyToJump;
    private float jumpCooldown = 0.25f;
    private float jumpForce = 550f;
    private float x;
    private float y;
    private bool jumping;
    private bool sprinting;
    private bool crouching;
    private Vector3 normalVector;
    private Vector3 wallNormalVector;
    private bool wallRunning;
    private Vector3 wallRunPos;

    private Collider playerCollider;
    public bool paused;
    private Vector3 previousLookdir;
    private float offsetMultiplier;
    private float offsetVel;
    private float distance;
    private float slideSlowdown = 0.2f;
    private float actualWallRotation;
    private float wallRotationVel;
    private float desiredX;
    private bool cancelling;
    private bool readyToWallrun = true;
    private float wallRunGravity = 1f;
    private float maxSlopeAngle = 35f;
    private float wallRunRotation;
    private bool onWall;
    private bool onGround;
    private bool surfing;
    private bool cancellingGrounded;
    private bool cancellingWall;
    private bool cancellingSurf;

    public bool objAbove;
    private bool isCrouchPressed = false;

    public static PlayerMovement Instance { get; private set; }

    private void Awake()
    {
        PlayerMovement.Instance = this;
        this.rb = base.GetComponent<Rigidbody>();
    }

    private void Start()
    {
        this.playerCollider = base.GetComponent<Collider>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        this.readyToJump = true;
        this.wallNormalVector = Vector3.up;
    }

    private void LateUpdate()
    {
        if (this.dead || this.paused)
        {
            return;
        }
        this.WallRunning();
    }

    private void FixedUpdate()
    {
        if (this.dead || /*Game.Instance.done || */this.paused)
        {
            return;
        }
        this.Movement();
    }

    private void Update()
    {
        if (this.dead || /*Game.Instance.done || */this.paused)
        {
            return;
        }
        this.x = Input.GetAxisRaw("Horizontal");
        this.y = Input.GetAxisRaw("Vertical");
        this.Look();
    }

    private void Pause()
    {
        if (this.dead)
        {
            return;
        }
        if (this.paused)
        {
            Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            this.paused = false;
            return;
        }
        this.paused = true;
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void StartCrouch()
    {
        float d = 400f;
        base.transform.localScale = new Vector3(1f, 0.5f, 1f);
        base.transform.position = new Vector3(base.transform.position.x, base.transform.position.y - 0.5f, base.transform.position.z);
        if (this.rb.velocity.magnitude > 0.1f && this.grounded)
        {
            this.rb.AddForce(this.orientation.transform.forward * d);
            //AudioManager.Instance.Play("StartSlide");
            //AudioManager.Instance.Play("Slide");
        }
    }

    private void StopCrouch()
    {
        base.transform.localScale = new Vector3(1f, 1f, 1f);
        base.transform.position = new Vector3(base.transform.position.x, base.transform.position.y + 0.5f, base.transform.position.z);
    }

    private void FootSteps()
    {
        if (this.crouching || this.dead)
        {
            return;
        }
        if (this.grounded || this.wallRunning)
        {
            float num = 1.2f;
            float num2 = this.rb.velocity.magnitude;
            if (num2 > 20f)
            {
                num2 = 20f;
            }
            this.distance += num2;
            if (this.distance > 300f / num)
            {
                //AudioManager.Instance.PlayFootStep();
                this.distance = 0f;
            }
        }
    }

    private void Movement()
    {
        if (this.dead)
        {
            return;
        }
        this.rb.AddForce(Vector3.down * Time.deltaTime * 10f);
        Vector2 vector = this.FindVelRelativeToLook();
        float num = vector.x;
        float num2 = vector.y;
        this.FootSteps();
        this.CounterMovement(this.x, this.y, vector);
        if (this.readyToJump && this.jumping)
        {
            this.Jump();
        }
        float num3 = this.walkSpeed;
        if (this.sprinting)
        {
            num3 = this.runSpeed;
        }
        if (this.crouching && this.grounded && this.readyToJump)
        {
            this.rb.AddForce(Vector3.down * Time.deltaTime * 3000f);
            return;
        }
        if (this.x > 0f && num > num3)
        {
            this.x = 0f;
        }
        if (this.x < 0f && num < -num3)
        {
            this.x = 0f;
        }
        if (this.y > 0f && num2 > num3)
        {
            this.y = 0f;
        }
        if (this.y < 0f && num2 < -num3)
        {
            this.y = 0f;
        }
        float d = 1f;
        float d2 = 1f;
        if (!this.grounded)
        {
            d = 0.5f;
            d2 = 0.5f;
        }
        if (this.grounded && this.crouching)
        {
            d2 = 0f;
        }
        if (this.wallRunning)
        {
            d2 = 0.3f;
            d = 0.3f;
        }
        if (this.surfing)
        {
            d = 0.7f;
            d2 = 0.3f;
        }
        this.rb.AddForce(this.orientation.transform.forward * this.y * this.moveSpeed * Time.deltaTime * d * d2);
        this.rb.AddForce(this.orientation.transform.right * this.x * this.moveSpeed * Time.deltaTime * d);
    }

    private void CameraShake()
    {
        float num = this.rb.velocity.magnitude / 9f;
        //CameraShaker.Instance.ShakeOnce(num, 0.1f * num, 0.25f, 0.2f);
        base.Invoke("CameraShake", 0.2f);
    }

    private void ResetJump()
    {
        this.readyToJump = true;
    }

    private void Jump()
    {
        if ((this.grounded || this.wallRunning || this.surfing) && this.readyToJump)
        {
            MonoBehaviour.print("jumping");
            Vector3 velocity = this.rb.velocity;
            this.readyToJump = false;
            this.rb.AddForce(Vector2.up * this.jumpForce * 1.5f);
            this.rb.AddForce(this.normalVector * this.jumpForce * 0.5f);
            if (this.rb.velocity.y < 0.5f)
            {
                this.rb.velocity = new Vector3(velocity.x, 0f, velocity.z);
            }
            else if (this.rb.velocity.y > 0f)
            {
                this.rb.velocity = new Vector3(velocity.x, velocity.y / 2f, velocity.z);
            }
            if (this.wallRunning)
            {
                this.rb.AddForce(this.wallNormalVector * this.jumpForce * 3f);
            }
            base.Invoke("ResetJump", this.jumpCooldown);
            if (this.wallRunning)
            {
                this.wallRunning = false;
            }
            //AudioManager.Instance.PlayJump();
        }
    }

    private void Look()
    {
        Vector3 eulerAngles = this.playerCam.transform.localRotation.eulerAngles;
        this.desiredX = eulerAngles.y;
        this.FindWallRunRotation();
        this.actualWallRotation = Mathf.SmoothDamp(this.actualWallRotation, this.wallRunRotation, ref this.wallRotationVel, 0.2f);
        this.playerCam.transform.localRotation = Quaternion.Euler(this.xRotation, this.desiredX, this.actualWallRotation);
        this.orientation.transform.localRotation = Quaternion.Euler(0f, this.desiredX, 0f);
    }

    private void CounterMovement(float x, float y, Vector2 mag)
    {
        if (!this.grounded || this.jumping)
        {
            return;
        }
        float d = 0.16f;
        float num = 0.01f;
        if (this.crouching)
        {
            this.rb.AddForce(this.moveSpeed * Time.deltaTime * -this.rb.velocity.normalized * this.slideSlowdown);
            return;
        }
        if ((Mathf.Abs(mag.x) > num && Mathf.Abs(x) < 0.05f) || (mag.x < -num && x > 0f) || (mag.x > num && x < 0f))
        {
            this.rb.AddForce(this.moveSpeed * this.orientation.transform.right * Time.deltaTime * -mag.x * d);
        }
        if ((Mathf.Abs(mag.y) > num && Mathf.Abs(y) < 0.05f) || (mag.y < -num && y > 0f) || (mag.y > num && y < 0f))
        {
            this.rb.AddForce(this.moveSpeed * this.orientation.transform.forward * Time.deltaTime * -mag.y * d);
        }
        if (Mathf.Sqrt(Mathf.Pow(this.rb.velocity.x, 2f) + Mathf.Pow(this.rb.velocity.z, 2f)) > this.walkSpeed)
        {
            float num2 = this.rb.velocity.y;
            Vector3 vector = this.rb.velocity.normalized * this.walkSpeed;
            this.rb.velocity = new Vector3(vector.x, num2, vector.z);
        }
    }

    public Vector2 FindVelRelativeToLook()
    {
        float current = this.orientation.transform.eulerAngles.y;
        float target = Mathf.Atan2(this.rb.velocity.x, this.rb.velocity.z) * 57.29578f;
        float num = Mathf.DeltaAngle(current, target);
        float num2 = 90f - num;
        float magnitude = this.rb.velocity.magnitude;
        float num3 = magnitude * Mathf.Cos(num * 0.017453292f);
        return new Vector2(magnitude * Mathf.Cos(num2 * 0.017453292f), num3);
    }

    private void FindWallRunRotation()
    {
        if (!this.wallRunning)
        {
            this.wallRunRotation = 0f;
            return;
        }
        Vector3 normalized = new Vector3(0f, this.playerCam.transform.rotation.y, 0f).normalized;
        new Vector3(0f, 0f, 1f);
        float current = this.playerCam.transform.rotation.eulerAngles.y;
        if (Mathf.Abs(this.wallNormalVector.x - 1f) >= 0.1f)
        {
            if (Mathf.Abs(this.wallNormalVector.x - -1f) >= 0.1f)
            {
                if (Mathf.Abs(this.wallNormalVector.z - 1f) >= 0.1f)
                {
                    if (Mathf.Abs(this.wallNormalVector.z - -1f) < 0.1f)
                    {
                    }
                }
            }
        }
        float target = Vector3.SignedAngle(new Vector3(0f, 0f, 1f), this.wallNormalVector, Vector3.up);
        float num = Mathf.DeltaAngle(current, target);
        this.wallRunRotation = -(num / 90f) * 15f;
        if (!this.readyToWallrun)
        {
            return;
        }
        if ((Mathf.Abs(this.wallRunRotation) >= 4f || this.y <= 0f || Mathf.Abs(this.x) >= 0.1f) &&
            (Mathf.Abs(this.wallRunRotation) <= 22f || this.y >= 0f || Mathf.Abs(this.x) >= 0.1f))
        {
            this.cancelling = false;
            base.CancelInvoke("CancelWallrun");
            return;
        }
        if (this.cancelling)
        {
            return;
        }
        this.cancelling = true;
        base.CancelInvoke("CancelWallrun");
        base.Invoke("CancelWallrun", 0.2f);
    }

    private void CancelWallrun()
    {
        MonoBehaviour.print("cancelled");
        base.Invoke("GetReadyToWallrun", 0.1f);
        this.rb.AddForce(this.wallNormalVector * 600f);
        this.readyToWallrun = false;
        //AudioManager.Instance.PlayLanding();
    }

    private void GetReadyToWallrun()
    {
        this.readyToWallrun = true;
    }

    private void WallRunning()
    {
        if (this.wallRunning)
        {
            this.rb.AddForce(-this.wallNormalVector * Time.deltaTime * this.moveSpeed);
            this.rb.AddForce(Vector3.up * Time.deltaTime * this.rb.mass * 100f * this.wallRunGravity);
        }
    }

    private bool IsFloor(Vector3 v)
    {
        return Vector3.Angle(Vector3.up, v) < this.maxSlopeAngle;
    }

    private bool IsSurf(Vector3 v)
    {
        float num = Vector3.Angle(Vector3.up, v);
        return num < 89f && num > this.maxSlopeAngle;
    }

    private bool IsWall(Vector3 v)
    {
        return Mathf.Abs(90f - Vector3.Angle(Vector3.up, v)) < 0.1f;
    }

    private bool IsRoof(Vector3 v)
    {
        return v.y == -1f;
    }

    private void StartWallRun(Vector3 normal)
    {
        if (this.grounded || !this.readyToWallrun)
        {
            return;
        }
        this.wallNormalVector = normal;
        float d = 20f;
        if (!this.wallRunning)
        {
            this.rb.velocity = new Vector3(this.rb.velocity.x, 0f, this.rb.velocity.z);
            this.rb.AddForce(Vector3.up * d, ForceMode.Impulse);
        }
        this.wallRunning = true;
    }

    private void OnCollisionStay(Collision other)
    {
        int layer = other.gameObject.layer;
        if (this.whatIsGround != (this.whatIsGround | 1 << layer))
        {
            return;
        }
        for (int i = 0; i < other.contactCount; i++)
        {
            Vector3 normal = other.contacts[i].normal;
            if (this.IsFloor(normal))
            {
                if (this.wallRunning)
                {
                    this.wallRunning = false;
                }
                if (!this.grounded && this.crouching)
                {
                    //AudioManager.Instance.Play("StartSlide");
                    //AudioManager.Instance.Play("Slide");
                }
                this.grounded = true;
                this.normalVector = normal;
                this.cancellingGrounded = false;
                base.CancelInvoke("StopGrounded");
            }
            if (this.IsWall(normal) && layer == LayerMask.NameToLayer("Ground"))
            {
                if (!this.onWall)
                {
                    //AudioManager.Instance.Play("StartSlide");
                    //AudioManager.Instance.Play("Slide");
                }
                this.StartWallRun(normal);
                this.onWall = true;
                this.cancellingWall = false;
                base.CancelInvoke("StopWall");
            }
            if (this.IsSurf(normal))
            {
                this.surfing = true;
                this.cancellingSurf = false;
                base.CancelInvoke("StopSurf");
            }
            this.IsRoof(normal);
        }
        float num = 3f;
        if (!this.cancellingGrounded)
        {
            this.cancellingGrounded = true;
            base.Invoke("StopGrounded", Time.deltaTime * num);
        }
        if (!this.cancellingWall)
        {
            this.cancellingWall = true;
            base.Invoke("StopWall", Time.deltaTime * num);
        }
        if (!this.cancellingSurf)
        {
            this.cancellingSurf = true;
            base.Invoke("StopSurf", Time.deltaTime * num);
        }
    }

    private void StopGrounded()
    {
        this.grounded = false;
    }

    private void StopWall()
    {
        this.onWall = false;
        this.wallRunning = false;
    }

    private void StopSurf()
    {
        this.surfing = false;
    }

    public Vector3 GetVelocity()
    {
        return this.rb.velocity;
    }

    public float GetFallSpeed()
    {
        return this.rb.velocity.y;
    }

    public Collider GetPlayerCollider()
    {
        return this.playerCollider;
    }

    public Transform GetPlayerCamTransform()
    {
        return this.playerCam.transform;
    }

    public bool IsCrouching()
    {
        return this.crouching;
    }

    public bool IsDead()
    {
        return this.dead;
    }

    public Rigidbody GetRb()
    {
        return this.rb;
    }

    public void MoveInput(InputAction.CallbackContext context)
    {
        //var delta = context.ReadValue<Vector2>();
        //this.x = delta.x;
        //this.y = delta.y;
    }

    public void SprintInput(InputAction.CallbackContext context)
    {
        bool sprintPressed = false;
        sprintPressed = Mathf.Approximately(context.ReadValue<float>(), 1);

        sprinting = sprintPressed && y > 0;
    }

    public void JumpInput(InputAction.CallbackContext context)
    {
        bool jumpPressed = false;
        jumpPressed = Mathf.Approximately(context.ReadValue<float>(), 1);

        jumping = jumpPressed && grounded;
    }

    public void CrouchInput(InputAction.CallbackContext context)
    {
        if (context.started && !objAbove)
        {
            StartCrouch();
            crouching = true;
        }
        if (context.canceled && !objAbove)
        {
            StopCrouch();
            crouching = false;
        }
        if (context.started)
        {
            isCrouchPressed = true;
        }
        if (context.canceled)
        {
            isCrouchPressed = false;
        }
    }
}