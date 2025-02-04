using UnityEngine;
using System.Collections.Generic;

public class DotSpawner : MonoBehaviour
{
    [Header("Settings")]
    public GameObject dotPrefab;
    public float spawnInterval = 2f;
    public float dotLifetime = 3f;
    public float spawnRadius = 3f;

    [Header("Colors")]
    public Color blackColor = Color.black;
    public Color whiteColor = Color.white;
    public float emissionIntensity = 2f;

    private List<GameObject> activeDots = new List<GameObject>();
    private Camera vrCamera;
    private MaterialPropertyBlock materialProps;

    void Start()
    {
        vrCamera = Camera.main;
        materialProps = new MaterialPropertyBlock();
        StartCoroutine(SpawnDotsRoutine());
    }

    System.Collections.IEnumerator SpawnDotsRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);
            SpawnDot();
        }
    }

    void SpawnDot()
    {
        Vector3 spawnPosition = vrCamera.transform.position +
            vrCamera.transform.forward * spawnRadius +
            Random.insideUnitSphere * 2f;

        GameObject dot = Instantiate(dotPrefab, spawnPosition, Quaternion.identity);
        ConfigureDotAppearance(dot);
        activeDots.Add(dot);
        StartCoroutine(RemoveDot(dot, dotLifetime));
    }

    void ConfigureDotAppearance(GameObject dot)
    {
        Renderer renderer = dot.GetComponent<Renderer>();
        bool isWhite = Random.value > 0.5f;

        // Set shader properties using MaterialPropertyBlock
        materialProps.SetColor("_MainColor", isWhite ? whiteColor : blackColor);
        materialProps.SetColor("_EmissionColor", (isWhite ? whiteColor : blackColor) * emissionIntensity);
        materialProps.SetFloat("_Cutoff", 0.5f);

        renderer.SetPropertyBlock(materialProps);
    }

    System.Collections.IEnumerator RemoveDot(GameObject dot, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (activeDots.Contains(dot))
        {
            activeDots.Remove(dot);
            Destroy(dot);
        }
    }

    public List<GameObject> GetActiveDots() => activeDots;
}