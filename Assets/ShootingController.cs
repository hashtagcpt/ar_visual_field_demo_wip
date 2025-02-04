using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR;

public class ShootingController : MonoBehaviour
{
    public float maxDistance = 10f;
    public LayerMask dotLayer;
    public DotSpawner spawner;
    public ScoreManager scoreManager;

    void Update()
    {
        if (OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.RTouch))
        {
            Ray ray = new Ray(transform.position, transform.forward);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, maxDistance, dotLayer))
            {
                GameObject hitDot = hit.collider.gameObject;
                float accuracy = Vector3.Distance(hit.point, hitDot.transform.position);
                scoreManager.AddScore(CalculateScore(accuracy));
                spawner.GetActiveDots().Remove(hitDot);
                Destroy(hitDot);
            }
        }
    }

    float CalculateScore(float errorDistance)
    {
        return Mathf.Clamp(100 - (errorDistance * 50), 0, 100);
    }
}