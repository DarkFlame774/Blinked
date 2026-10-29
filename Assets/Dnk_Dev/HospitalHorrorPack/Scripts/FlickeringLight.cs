using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Light))]
public class FlickerLightRandom : MonoBehaviour
{
    public float baseIntensity = 1f;         // nominal intensity
    public float minMultiplier = 0f;         // min intensity = base * minMultiplier
    public float maxMultiplier = 1.3f;       // max intensity = base * maxMultiplier
    public float minInterval = 0.02f;        // shortest time between changes
    public float maxInterval = 0.25f;        // longest time between changes
    public bool flickerEnabled = true;
    public bool affectRange = false;
    public float rangeVariation = 0.2f;      // fraction of original range
    public bool affectColor = false;
    public Color minColor = Color.white;
    public Color maxColor = Color.white;

    Light _light;
    float _originalRange;
    Color _originalColor;

    void Awake()
    {
        _light = GetComponent<Light>();
        _originalRange = _light.range;
        _originalColor = _light.color;
    }

    void OnEnable()
    {
        StopAllCoroutines();
        if (flickerEnabled) StartCoroutine(FlickerRoutine());
    }

    void OnDisable()
    {
        StopAllCoroutines();
        ResetLight();
    }

    IEnumerator FlickerRoutine()
    {
        while (true)
        {
            float t = Random.value;
            float mult = Mathf.Lerp(minMultiplier, maxMultiplier, t);
            _light.intensity = baseIntensity * mult;

            if (affectRange)
            {
                float rMult = 1f + (Mathf.Lerp(-rangeVariation, rangeVariation, Random.value));
                _light.range = _originalRange * rMult;
            }

            if (affectColor)
            {
                _light.color = Color.Lerp(minColor, maxColor, Random.value);
            }

            float wait = Random.Range(minInterval, maxInterval);
            yield return new WaitForSeconds(wait);
        }
    }

    public void ResetLight()
    {
        _light.intensity = baseIntensity;
        _light.range = _originalRange;
        _light.color = _originalColor;
    }
}
