using UnityEngine;
using UnityEngine.AI;

public class ForceReceiver : MonoBehaviour
{
    [SerializeField] private CharacterController controller;
    [SerializeField] private float drag = 0.3f;
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private float fallMultiplyer = 1.0f;
    [SerializeField] private float riseMultiplier = 1.0f;   // extra gravity on the way up = snappier jump

    private float verticalVelocity;

    private Vector3 impact;
    private Vector3 dampingVelocity;

    public Vector3 Movement => impact + Vector3.up * verticalVelocity;

    private void Update()
    {
        if (verticalVelocity < 0f && controller.isGrounded)
        {
            verticalVelocity = Physics.gravity.y * Time.deltaTime; //keeps grounded
        }
        else if (verticalVelocity < 0f && !controller.isGrounded)
        {
            verticalVelocity += (Physics.gravity.y * fallMultiplyer) * Time.deltaTime;    
        }
        else
        {
            verticalVelocity += Physics.gravity.y * riseMultiplier * Time.deltaTime;
        }

        impact = Vector3.SmoothDamp(impact, Vector3.zero, ref dampingVelocity, drag); // stops player moving forward when attacking

        if (agent != null)
        {
            if (impact.sqrMagnitude < 0.2f * 0.2f)
            {
                impact = Vector3.zero;
                agent.enabled = true;
            }
        }


    }

    public void AddForce(Vector3 force)
    {
        impact += force;
        if (agent != null)
        {
            agent.enabled = false;
        }
    }

    public void Jump(float jumpForce)
    {
        // set, not add: every jump (including the double jump) gets the same boost, whether you're rising or falling
        verticalVelocity = jumpForce;
    }

    // jump attack wind-up: hang in the air (call every frame to cancel gravity)
    public void Hover()
    {
        verticalVelocity = 0f;
    }

    // jump attack: drive straight down at this speed
    public void Plunge(float speed)
    {
        verticalVelocity = -Mathf.Abs(speed);
    }

    public void DoubleJump(float jumpForce)
    {
        //double jump should be half as high
        float doubleJump = jumpForce / 1.5f ;
        verticalVelocity = doubleJump;
    }

}
