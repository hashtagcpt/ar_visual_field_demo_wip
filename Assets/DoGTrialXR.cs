using UnityEngine;
using System.Collections;

public class DoGTrialXR : MonoBehaviour
{
    [Header("Stimulus Settings")]
    [Tooltip("Prefab with the DoG shader (e.g., a quad with the DoG material)")]
    public GameObject dogPrefab;

    [Tooltip("Duration (in seconds) for which the stimulus is visible (240 ms)")]
    public float stimulusDuration = 0.24f;  // 240 milliseconds

    [Tooltip("Minimum world-space coordinates for random placement")]
    public Vector3 positionMin = new Vector3(-2, 1, 2);

    [Tooltip("Maximum world-space coordinates for random placement")]
    public Vector3 positionMax = new Vector3(2, 3, 5);

    [Header("Trial Settings")]
    [Tooltip("Total number of trials")]
    public int numberOfTrials = 500;

    [Tooltip("Break duration (in seconds) between trials (500 ms)")]
    public float breakDuration = 0.5f;  // 500 milliseconds

    [Header("Beep Settings")]
    [Tooltip("Beep sound to play at the start of each trial. If none is assigned, one is created.")]
    public AudioClip beepSound;

    [Tooltip("Frequency of the beep in Hertz")]
    public float beepFrequency = 1000f;

    [Tooltip("Duration of the beep in seconds")]
    public float beepDuration = 0.1f;  // 100 milliseconds

    IEnumerator Start()
    {
        // If no beep AudioClip is assigned, create one with a sine wave.
        if (beepSound == null)
        {
            beepSound = CreateBeepClip(beepFrequency, beepDuration);
        }

        // Run the specified number of trials.
        for (int i = 0; i < numberOfTrials; i++)
        {
            // Choose a random position within the specified bounds.
            Vector3 randomPos = new Vector3(
                Random.Range(positionMin.x, positionMax.x),
                Random.Range(positionMin.y, positionMax.y),
                Random.Range(positionMin.z, positionMax.z)
            );

            // Play the beep sound at the random position.
            AudioSource.PlayClipAtPoint(beepSound, randomPos);

            // Instantiate the DoG stimulus at the random position.
            GameObject stimulus = Instantiate(dogPrefab, randomPos, Quaternion.identity);

            // Log the trial for debugging.
            Debug.Log("Trial " + (i + 1) + " at position " + randomPos);

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

        // Fill the samples array with a sine wave.
        for (int i = 0; i < sampleLength; i++)
        {
            samples[i] = Mathf.Sin(2 * Mathf.PI * frequency * i / sampleRate);
        }
        clip.SetData(samples, 0);
        return clip;
    }
}
