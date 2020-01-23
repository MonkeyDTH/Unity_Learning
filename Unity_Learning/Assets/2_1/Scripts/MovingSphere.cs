using UnityEngine;

public class MovingSphere : MonoBehaviour
{
    [SerializeField, Range(0f, 100f)]
    float maxSpeed = 10f;
    [SerializeField, Range(0f, 100f)]
    float maxAcceleration = 10f;
    [SerializeField, Range(0f, 100f)]
    float maxAirAcceleration = 1f;
    [SerializeField, Range(0f, 10f)]
    float jumpHeight = 2f;
    [SerializeField, Range(0, 5)]
    int maxAirJumps = 0;
    [SerializeField, Range(0, 90)]
    float maxGroundAngle = 25f;     // slope angle

    Vector3 velocity;
    Vector3 desiredVelocity;
    Rigidbody body;
    bool desiredJump;   // desired jump height
    bool onGround;      // on ground state
    int jumpPhase;      // jump times
    float minGroundDotProduct;
    Vector3 contactNormal;

    private void Awake()
    {
        transform.localPosition += new Vector3(0f, 0.5f, 0f);
        velocity = new Vector3(0f, 0f, 0f);
        body = GetComponent<Rigidbody>();
        OnValidate();
    }

    void OnValidate()
    {
        minGroundDotProduct = Mathf.Cos(maxGroundAngle * Mathf.Deg2Rad);
    }
    private void Update()
    {
        // get Input
        Vector2 playerInput = new Vector2();
        playerInput.x = Input.GetAxis("Horizontal");
        playerInput.y = Input.GetAxis("Vertical");
        playerInput = Vector2.ClampMagnitude(playerInput, 1f);
        desiredJump |= Input.GetButtonDown("Jump");

        // Move
        desiredVelocity = new Vector3(playerInput.x, 0f, playerInput.y) * maxSpeed;
    }

    private void FixedUpdate()
    {
        UpdateState();
        AdjustVelocity();
        if (desiredJump)
        {
            desiredJump = false;
            Jump();
        }
        body.velocity = velocity;
        onGround = false;
    }
    
    void Jump()
    {
        if (onGround || jumpPhase < maxAirJumps)
        {
            jumpPhase += 1;
            float jumpSpeed = Mathf.Sqrt(-2f * Physics.gravity.y * jumpHeight);
            float alignedSpeed = Vector3.Dot(velocity, contactNormal);
            if (alignedSpeed > 0f)
            {
                jumpSpeed = Mathf.Max(jumpSpeed - alignedSpeed, 0f);
            }
            velocity += contactNormal * jumpSpeed;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        EvaluateCollision(collision);
    }

    private void OnCollisionStay(Collision collision)
    {
        EvaluateCollision(collision);
    }

    private void OnCollisionExit()
    {
        onGround = false;
    }

    // do ground jump
    void EvaluateCollision(Collision collision)
    {
        for (int i = 0; i < collision.contactCount; i++)
        {
            Vector3 normal = collision.GetContact(i).normal;
            if (normal.y >= minGroundDotProduct)
            {
                onGround = true;
                contactNormal = normal;
            }
        }
    }

    // Air jump
    void UpdateState()
    {
        velocity = body.velocity;
        if (onGround)
        {
            jumpPhase = 0;
        }
        else
        {
            contactNormal = Vector3.up;
        }
    }

    // project on contact plane
    Vector3 ProjectOnContactPlane (Vector3 vector)
    {
        return vector - contactNormal * Vector3.Dot(vector, contactNormal);
    }
    
    void AdjustVelocity ()
    {
        Vector3 xAxis = ProjectOnContactPlane(Vector3.right).normalized;
        Vector3 zAxis = ProjectOnContactPlane(Vector3.forward).normalized;

        float currentX = Vector3.Dot(velocity, xAxis);
        float currentZ = Vector3.Dot(velocity, zAxis);

        float acceleration = onGround ? maxAcceleration : maxAirAcceleration;
        float maxSpeedChange = acceleration * Time.deltaTime;
        float newX = Mathf.MoveTowards(currentX, desiredVelocity.x, maxSpeedChange);
        float newZ = Mathf.MoveTowards(currentZ, desiredVelocity.z, maxSpeedChange);

        velocity += xAxis * (newX - currentX) + zAxis * (newZ - currentZ);
    }
}

