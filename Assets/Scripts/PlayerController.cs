using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public Rigidbody rigidbody;
    public Transform cameraTransform;
    public float speed = 5;
    public float jumpForce = 6;
    public float turnSpeed = 10;
    public float fallY = -5;

    // آخر مكان آمن يرجع له اللاعب لما يطيح
    Vector3 checkpoint;

    void Start()
    {
        checkpoint = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        var horizontal = Input.GetAxis("Horizontal");
        var vertical = Input.GetAxis("Vertical");
        var jump = Input.GetKeyDown(KeyCode.Space);

        // اتجاه الكاميرا على الأرض (بدون ميلانها لتحت)
        var forward = cameraTransform.forward;
        forward.y = 0;
        forward.Normalize();

        var right = cameraTransform.right;
        right.y = 0;
        right.Normalize();

        // فوق = للأمام على الممر، يمين ويسار حسب الكاميرا
        var direction = forward * vertical + right * horizontal;
        if (direction.magnitude > 1)
        {
            direction.Normalize();
        }

        // واقف إذا ما فيه ضغط، ويمشي إذا فيه
        rigidbody.linearVelocity = new Vector3(
        direction.x * speed,
        rigidbody.linearVelocity.y,
        direction.z * speed);

        // يلف وجهه باتجاه المشي
        if (direction.magnitude > 0.1f)
        {
            var targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
        }

        // هل تحت رجوله أرض؟
        bool isGrounded = Physics.Raycast(transform.position + Vector3.up * 0.1f, Vector3.down, 0.2f, ~0, QueryTriggerInteraction.Ignore);

        // القفز
        if (jump && isGrounded)
        {
            rigidbody.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }

        // طاح من الممر
        if (transform.position.y < fallY)
        {
            Respawn();
        }
    }

    // يرجّع اللاعب لآخر Checkpoint
    public void Respawn()
    {
        rigidbody.linearVelocity = Vector3.zero;
        rigidbody.position = checkpoint;
        transform.position = checkpoint;

        // يبلّغ الشخص الثالث (GameManager) إن اللاعب طاح
        GameObject manager = GameObject.Find("GameManager");
        if (manager != null)
        {
            manager.SendMessage("PlayerFell", SendMessageOptions.DontRequireReceiver);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // وصل منصة آمنة: نحفظ مكانها
        if (other.CompareTag("Checkpoint"))
        {
            checkpoint = other.transform.position + Vector3.up * 0.5f;
        }

        // وصل خط النهاية
        if (other.CompareTag("Finish"))
        {
            GameObject manager = GameObject.Find("GameManager");
            if (manager != null)
            {
                manager.SendMessage("PlayerFinished", SendMessageOptions.DontRequireReceiver);
            }
        }
    }
}