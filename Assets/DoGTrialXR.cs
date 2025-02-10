using UnityEngine;
using System.Collections;

public class DoGTrialXR : MonoBehaviour
{
    [Header("Stimulus Settings")]
    [Tooltip("Prefab with the DoG shader (e.g., a quad with the DoG material)")]
    public GameObject dogPrefab;

    [Tooltip("Duration (in seconds) for which the stimulus is visible (e.g., 0.24 for 240 ms)")]
    public float stimulusDuration = 0.24f;

    [Header("Radial Position Settings")]
    [Tooltip("Optional: A Transform that defines the center of the field of view (the dot). If unassigned, a point in front of the camera is used.")]
    public Transform centerDot;

    [Tooltip("If centerDot is not assigned, this distance from the camera defines the center position.")]
    public float centerDistance = 2.0f;

    [Tooltip("Minimum radial distance (in meters) from the center at which the stimulus can appear.")]
    public float minRadius = 0.1f;

    [Tooltip("Maximum radial distance (in meters) from the center at which the stimulus can appear.")]
    public float maxRadius = 1.0f;

    [Header("Trial Settings")]
    [Tooltip("Total number of trials")]
    public int numberOfTrials = 500;

    [Tooltip("Break duration (in seconds) between trials (e.g., 0.5 for 500 ms)")]
    public float breakDuration = 0.5f;

    [Header("Beep Settings")]
    [Tooltip("Optional: Beep AudioClip to play at each trial. If left empty, one will be generated.")]
    public AudioClip beepSound;

    [Tooltip("Frequency of the beep (in Hz) if generated")]
    public float beepFrequency = 1000f;

    [Tooltip("Duration of the beep (in seconds) if generated")]
    public float beepDuration = 0.1f;

    IEnumerator Start()
    {
        // If no beep AudioClip is assigned, generate one using a sine wave.
        if (beepSound == null)
        {
            beepSound = CreateBeepClip(beepFrequency, beepDuration);
        }

        for (int i = 0; i < numberOfTrials; i++)
        {
            // Determine the center of the field of view.
            Vector3 centerPos;
            if (centerDot != null)
            {
                centerPos = centerDot.position;
            }
            else if (Camera.main != null)
            {
                centerPos = Camera.main.transform.position + Camera.main.transform.forward * centerDistance;
            }
            else
            {
                Debug.LogWarning("No centerDot assigned and no Main Camera found. Using (0,0,0) as center.");
                centerPos = Vector3.zero;
            }

            // Randomize a radial offset in the plane perpendicular to the camera's forward vector.
            float randomRadius = Random.Range(minRadius, maxRadius);
            float randomAngle = Random.Range(0f, 2 * Mathf.PI);
            Vector3 offset = (Mathf.Cos(randomAngle) * Camera.main.transform.right +
                              Mathf.Sin(randomAngle) * Camera.main.transform.up) * randomRadius;

            // Compute the final stimulus position: center plus offset.
            Vector3 stimulusPos = centerPos + offset;

            // Compute the rotation so that the stimulus always faces the viewer.
            // Use the camera's position as the target.
            Vector3 viewerPos = Camera.main.transform.position;
            Quaternion baseRot = Quaternion.LookRotation(viewerPos - stimulusPos);
            // Apply an additional rotation offset (180° about Y) to compensate for the quad's default orientation.
            Quaternion offsetRot = Quaternion.Euler(0f, 180f, 0f);
            Quaternion stimulusRot = baseRot * offsetRot;

            // Play the beep at the viewer's (camera's) position.
            AudioSource.PlayClipAtPoint(beepSound, viewerPos);

            // Instantiate the DoG stimulus at the computed position and rotation.
            GameObject stimulus = Instantiate(dogPrefab, stimulusPos, stimulusRot);

            // Log trial info.
            Debug.Log("Trial " + (i + 1) + " | Stimulus Pos: " + stimulusPos + " | Viewer Pos: " + viewerPos);

            // Keep the stimulus visible for the specified duration.
            yield return new WaitForSeconds(stimulusDuration);

            // Destroy the stimulus.
            Destroy(stimulus);

            // Wait for the break duration before starting the next trial.
            yield return new WaitForSeconds(breakDuration);
        }

        Debug.Log("All trials complete.");
    }

    /// <summary>
    /// Creates an AudioClip containing a sine wave (a beep) with the given frequency and duration.
    /// </summary>
    /// <param name="frequency">Frequency in Hertz.</param>
    /// <param name="duration">Duration in seconds.</param>
    /// <returns>The generated AudioClip.</returns>
    AudioClip CreateBeepClip(float frequency, float duration)
    {
        int sampleRate = 44100; // Standard audio sample rate.
        int sampleLength = Mathf.CeilToInt(sampleRate * duration);
        AudioClip clip = AudioClip.Create("Beep", sampleLength, 1, sampleRate, false);
        float[] samples = new float[sampleLength];

        for (int i = 0; i < sampleLength; i++)
        {
            samples[i] = Mathf.Sin(2 * Mathf.PI * frequency * i / sampleRate);
        }
        clip.SetData(samples, 0);
        return clip;
    }
}
