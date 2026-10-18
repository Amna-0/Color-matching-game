using UnityEngine;

// حركة الأرنب بالكود:
// - يمشي بنطّات: يدف، يطير قوس، ينزل وينضغط، وينط مرة ثانية
// - الرجول تنضم وهو طاير، واليدين تتمرجح، والأذنين ترجع لورا
// - وهو واقف: يتنفس ويرمش
// يركّب على نفس الـ Object اللي عليه BunnyBuilder
public class BunnyAnimator : MonoBehaviour
{
    public float hopHeight = 0.22f;     // ارتفاع النطة
    public float hopsPerMeter = 0.55f;  // كم نطة في كل متر
    public float leanAngle = 12f;       // ميلان الجسم مع النطة

    BunnyBuilder bunny;
    Rigidbody rb;

    Vector3 bodyBasePos;
    Vector3 bodyBaseScale;
    Quaternion headBaseRot;
    Quaternion[] earBaseRots;
    Quaternion[] armBaseRots;
    Quaternion[] footBaseRots;
    Vector3[] eyeBaseScales;

    float hopPhase;
    float squash;
    float walkBlend;
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

        earBaseRots = Save(bunny.ears);
        armBaseRots = Save(bunny.arms);
        footBaseRots = Save(bunny.feet);

        eyeBaseScales = new Vector3[bunny.eyes.Count];
        for (int i = 0; i < bunny.eyes.Count; i++) eyeBaseScales[i] = bunny.eyes[i].localScale;
    }

    Quaternion[] Save(System.Collections.Generic.List<Transform> list)
    {
        var r = new Quaternion[list.Count];
        for (int i = 0; i < list.Count; i++) r[i] = list[i].localRotation;
        return r;
    }

    void Update()
    {
        float t = Time.time;
        float dt = Time.deltaTime;
        if (dt <= 0) return;

        // هل يمشي؟ هل هو على الأرض؟
        Vector3 v = rb != null ? rb.linearVelocity : Vector3.zero;
        float speed = new Vector2(v.x, v.z).magnitude;
        bool moving = speed > 0.2f;
        bool grounded = Physics.Raycast(transform.position + Vector3.up * 0.1f, Vector3.down, 0.25f, ~0, QueryTriggerInteraction.Ignore);

        // لحظة النزول من قفزة حقيقية: انضغاط
        if (grounded && !wasGrounded) squash = 1f;
        wasGrounded = grounded;
        squash = Mathf.MoveTowards(squash, 0, dt * 4f);

        // نخلط بين الوقوف والمشي بنعومة
        walkBlend = Mathf.MoveTowards(walkBlend, moving && grounded ? 1 : 0, dt * 6f);

        // ---------- دورة النطة ----------
        // p من 0 لـ 1: أول 70% طاير، آخر 30% على الأرض
        if (moving && grounded) hopPhase += dt * speed * hopsPerMeter;
        else if (walkBlend <= 0) hopPhase = 0;
        float p = hopPhase - Mathf.Floor(hopPhase);

        const float air = 0.7f;
        float arc = 0;        // ارتفاع النطة (0..1)
        float landSquash = 0; // انضغاط النزول (0..1)
        float tuck = 0;       // ضم الرجول (-1..1)
        if (p < air)
        {
            float a = p / air;
            arc = Mathf.Sin(a * Mathf.PI);
            tuck = Mathf.Sin(a * Mathf.PI * 2);   // ترجع لورا ثم لقدام قبل النزول
        }
        else
        {
            landSquash = Mathf.Sin((p - air) / (1 - air) * Mathf.PI);
        }
        arc *= walkBlend;
        landSquash *= walkBlend;
        tuck *= walkBlend;

        // ---------- الجسم ----------
        bunny.body.localPosition = bodyBasePos + Vector3.up * arc * hopHeight;

        // الشكل: تنفس وهو واقف، تمدد في الهوا، انضغاط عند النزول
        Vector3 shape = new Vector3(1, 1 + Mathf.Sin(t * 2f) * 0.02f * (1 - walkBlend), 1);
        shape += new Vector3(-0.04f, 0.08f, -0.04f) * arc;          // ممدود وهو طاير
        shape += new Vector3(0.1f, -0.13f, 0.1f) * landSquash;       // منضغط وهو ينزل
        if (!grounded)
        {
            float stretch = Mathf.Clamp(v.y * 0.03f, -0.08f, 0.1f);
            shape += new Vector3(-stretch * 0.6f, stretch, -stretch * 0.6f);
        }
        shape += new Vector3(0.12f, -0.15f, 0.12f) * squash;
        bunny.body.localScale = Vector3.Lerp(bunny.body.localScale, Vector3.Scale(bodyBaseScale, shape), dt * 18f);

        // ميلان: يرفع راسه وهو يدف، ويميل لقدام وهو نازل
        float pitch = 0;
        if (p < air) pitch = Mathf.Lerp(-leanAngle * 0.6f, leanAngle, p / air);
        else pitch = Mathf.Lerp(leanAngle, -leanAngle * 0.6f, (p - air) / (1 - air));
        pitch = pitch * walkBlend + 4f * walkBlend;
        bunny.body.localRotation = Quaternion.Slerp(bunny.body.localRotation, Quaternion.Euler(pitch, 0, 0), dt * 14f);

        // ---------- الراس ----------
        float headBob = -pitch * 0.4f;
        bunny.head.localRotation = headBaseRot * Quaternion.Euler(headBob, 0, Mathf.Sin(t * 1.3f) * 2.3f * (1 - walkBlend));

        // ---------- الرجول ----------
        float airTuck = grounded ? 0 : 1;
        for (int i = 0; i < bunny.feet.Count; i++)
        {
            float angle = tuck * 35f + airTuck * 30f;
            bunny.feet[i].localRotation = footBaseRots[i] * Quaternion.Euler(angle, 0, 0);
        }

        // ---------- اليدين ----------
        // وحدة لقدام ووحدة لورا مثل الجري، وترتفع لما يقفز
        float run = Mathf.Sin(hopPhase * Mathf.PI * 2) * walkBlend;
        for (int i = 0; i < bunny.arms.Count; i++)
        {
            float side = i == 0 ? 1 : -1;
            float idle = Mathf.Sin(t * 2f + i) * 4f * (1 - walkBlend);
            float swing = run * 40f * side;
            float lift = -airTuck * 45f;
            bunny.arms[i].localRotation = Quaternion.Slerp(bunny.arms[i].localRotation,
                armBaseRots[i] * Quaternion.Euler(swing + lift + idle, 0, 0), dt * 15f);
        }

        // ---------- الأذنين ----------
        for (int i = 0; i < bunny.ears.Count; i++)
        {
            float swing = Mathf.Sin(t * 2.2f + i) * 4.5f * (1 - walkBlend);
            swing -= arc * 22f;                 // ترجع لورا وهو طاير
            swing += landSquash * 12f;          // تنط لقدام لما ينزل
            if (!grounded) swing -= 15f;
            bunny.ears[i].localRotation = Quaternion.Slerp(bunny.ears[i].localRotation,
                earBaseRots[i] * Quaternion.Euler(swing, 0, 0), dt * 15f);
        }

        // ---------- الرمش ----------
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