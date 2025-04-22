using UnityEngine;

public class PlayerMovementScript : MonoBehaviour
{
    public CharacterController controller;
    public Transform groundCheck;
    public LayerMask groundMask;
    public float speed = 10f;
    public float sprint = 20f;
    public float gravity = -9.81f;
    public float groudDistance = 0.4f;
    public float jumpHeight = 3f;

    Vector3 velocity;
    bool isGrounded;

    // Update is called once per frame
    void Update(){
        // Checking ground collision
        isGrounded = Physics.CheckSphere(groundCheck.position, groudDistance, groundMask);

        if (isGrounded && velocity.y < 0)
            velocity.y = -2f;

        // Axis
        float x = Input.GetAxis("Horizontal");   
        float z = Input.GetAxis("Vertical");   

        // Moving
        Vector3 move = transform.right * x + transform.forward * z;
        controller.Move(move * speed * Time.deltaTime);

        // Jumping
        if (Input.GetButtonDown("Jump") && isGrounded)
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);

        if (Input.GetKey(KeyCode.LeftShift) && isGrounded)
            speed = sprint;
        else
            speed = 10f;
            
        // Gravity
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
}
