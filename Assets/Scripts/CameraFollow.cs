using UnityEngine;

// الكاميرا تلحق اللاعب بنعومة من وراه ومن فوق
public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0, 3.5f, -5.5f);
    public float smooth = 5;

    // LateUpdate تشتغل بعد ما يتحرك اللاعب، عشان ما تهتز الكاميرا
    void LateUpdate()
    {
        var wantedPosition = target.position + offset;
        transform.position = Vector3.Lerp(transform.position, wantedPosition, smooth * Time.deltaTime);

        // تطالع شوي قدام اللاعب على طول الممر
        transform.LookAt(target.position + Vector3.up * 1.2f + Vector3.forward * 2);
    }
}
