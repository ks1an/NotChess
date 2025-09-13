using UnityEngine;

[RequireComponent(typeof(Light))]
public sealed class FlickeringLight : MonoBehaviour
{
    Light lightToFlicker;
    [SerializeField] bool isRegularFlicker;
    [SerializeField, Min(0f)] float minIntensity, maxIntensity;
    [SerializeField, Min(0f)] float timeBetweenIntensity;

    readonly MathOperations math = MathOperations.GetInstance();
    float curTimer, generalIntensity;
    int leftToFlick;

    void Awake()
    {
        if (lightToFlicker == null)
            lightToFlicker = GetComponent<Light>();
        generalIntensity = lightToFlicker.intensity;
    }

    void Update()
    {
        if (!(isRegularFlicker || leftToFlick > 0)) return;

        curTimer += Time.deltaTime;
        if (!(curTimer >= timeBetweenIntensity)) return;
        lightToFlicker.intensity = math.GetSafeRandom(minIntensity, maxIntensity);
        curTimer = 0f;

        if (leftToFlick > 0)
        {
            leftToFlick--;
            if (leftToFlick == 0)
                lightToFlicker.intensity = generalIntensity;
        }
    }

    public void StartFlickering(int flickAmount)
    {
        if (isRegularFlicker) return;
        leftToFlick = flickAmount;
    }
}
