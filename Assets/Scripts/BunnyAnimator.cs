using UnityEngine;

// حركة الأرنب بالكود: تنفس وهو واقف، تنطيط وهو يمشي، تمدد وانضغاط مع القفز، والأذنين تتمايل، ويرمش
// يركّب على نفس الـ Object اللي عليه BunnyBuilder
public class BunnyAnimator : MonoBehaviour
{
    public float hopHeight = 0.12f;
    public float hopSpeed = 3.5f;
    public float leanAngle = 8f;

    BunnyBuilder bunny;
    Rigidbody rb;

    Vector3 bodyBasePos;
    Vector3 bodyBaseScale;
    Quaternion headBaseRot;
    Quaternion[] earBaseRots;
    Vector3[] eyeBaseScales;

    float hopPhase;
    float squash;
    float nextBlink = 2f;
    float blinkTimer;
    bool wasGrounded = true;

    void Start()
    {
        bunny = GetComponent<BunnyBuilder>();
        rb = GetComponentInParent<Rigidbody>();

        bodyBasePos = bunny.body.localPosition;
        bodyBaseScale = bunny.body.localScale;
        headBaseRot = bunny.head.localRotation;

        earBaseRots = new Quaternion[bunny.ears.Count];
        for (int i = 0; i < bunny.ears.Count; i++) earBaseRots[i] = bunny.ears[i].localRotation;

        eyeBaseScales = new Vector3[bunny.eyes.Count];
        for (int i = 0; i < bunny.eyes.Count; i++) eyeBaseScales[i] = bunny.eyes[i].localScale;
    }

    void Update()
    {
        float t = Time.time;
        float dt = Time.deltaTime;

        // هل يمشي؟ هل هو على الأرض؟
        Vector3 v = rb != null ? rb.linearVelocity : Vector3.zero;
        float speed = new Vector2(v.x, v.z).magnitude;
        bool moving = speed > 0.2f;
        bool grounded = Physics.Raycast(transform.position + Vector3.up * 0.1f, Vector3.down, 0.25f);

        // لحظة النزول على الأرض: انضغاط بسيط
        if (grounded && !wasGrounded) squash = 1f;
        wasGrounded = grounded;
        squash = Mathf.MoveTowards(squash, 0, dt * 4f);

        // التنطيط وهو يمشي
        float bob = 0;
        if (moving && grounded)
        {
            hopPhase += dt * hopSpeed;
            bob = Mathf.Abs(Mathf.Sin(hopPhase * Mathf.PI)) * hopHeight;
        }
        else
        {
            hopPhase = 0;
        }
        bunny.body.localPosition = Vector3.Lerp(bunny.body.localPosition, bodyBasePos + Vector3.up * bob, dt * 15f);

        // الشكل: تنفس وهو واقف، تمدد في الهواء، انضغاط عند النزول
        Vector3 shape = new Vector3(1, 1 + Mathf.Sin(t * 2f) * 0.02f, 1);
        if (!grounded)
        {
            float stretch = Mathf.Clamp(v.y * 0.03f, -0.08f, 0.1f);
            shape = new Vector3(1 - stretch * 0.6f, 1 + stretch, 1 - stretch * 0.6f);
        }
        shape += new Vector3(0.12f, -0.15f, 0.12f) * squash;
        bunny.body.localScale = Vector3.Lerp(bunny.body.localScale, Vector3.Scale(bodyBaseScale, shape), dt * 12f);

        // ميلان بسيط للأمام وهو يمشي
        Quaternion lean = Quaternion.Euler(moving ? leanAngle : 0, 0, 0);
        bunny.body.localRotation = Quaternion.Slerp(bunny.body.localRotation, lean, dt * 8f);

        // ميلان الراس
        bunny.head.localRotation = headBaseRot * Quaternion.Euler(0, 0, Mathf.Sin(t * 1.3f) * 2.3f);

        // الأذنين: تتمايل دايماً، وأكثر وهو يمشي أو في الهواء
        for (int i = 0; i < bunny.ears.Count; i++)
        {
            float swing = Mathf.Sin(t * 2.2f + i) * 4.5f;
            if (moving) swing += Mathf.Sin(hopPhase * Mathf.PI * 2f + i) * 12f;
            if (!grounded) swing -= 15f;
            bunny.ears[i].localRotation = earBaseRots[i] * Quaternion.Euler(swing, 0, 0);
        }

        // الرمش كل كم ثانية
        nextBlink -= dt;
        if (nextBlink <= 0)
        {
            blinkTimer = 0.12f;
            nextBlink = Random.Range(2.5f, 4.5f);
        }
        blinkTimer -= dt;
        float eyeOpen = blinkTimer > 0 ? 0.1f : 1f;
        for (int i = 0; i < bunny.eyes.Count; i++)
        {
            var s = eyeBaseScales[i];
            bunny.eyes[i].localScale = new Vector3(s.x, s.y * eyeOpen, s.z);
        }
    }
}
