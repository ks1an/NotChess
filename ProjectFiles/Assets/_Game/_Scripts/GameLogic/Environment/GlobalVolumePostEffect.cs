using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(Volume))]
public partial class GlobalVolumePostEffect : MonoBehaviour
{
    Volume volume;

    void Awake()
    {
        volume = GetComponent<Volume>();
        volume.profile.TryGet(out vignette);
        volume.profile.TryGet(out chromeAbb);

        vignette.rounded.value = isRoundedVignetteFocus;
    }

    void Update()
    {
        UpdateVignette();
        UpdateChromeAbb();
    }
}

//Vignette
partial class GlobalVolumePostEffect
{
    [Header("DoOnVignetteFocus")]
    [SerializeField] AnimationCurve animCurveVignetteFocus;
    Vignette vignette;
    float timeVignetteFocus;
    bool needVigneteFocus, vigneteProcces;
    [SerializeField] bool isRoundedVignetteFocus;

    public void SetNeedVignetteFocus(bool b) => needVigneteFocus = b;
    
    void UpdateVignette()
    {
        if (needVigneteFocus)
        {
            timeVignetteFocus += Time.deltaTime;
            if (timeVignetteFocus < 0.05f) return;
            if (timeVignetteFocus >= animCurveVignetteFocus.keys[^1].time)
            {
                timeVignetteFocus = animCurveVignetteFocus.keys[^1].time;
                return;
            }
            vigneteProcces = true;
            vignette.intensity.value = animCurveVignetteFocus.Evaluate(timeVignetteFocus);
        }
        else if (vigneteProcces)
        {
            timeVignetteFocus -= Time.deltaTime;
            if (timeVignetteFocus <= animCurveVignetteFocus.keys[0].time)
            {
                timeVignetteFocus = animCurveVignetteFocus.keys[0].time;
                vigneteProcces = false;
            }
            vignette.intensity.value = animCurveVignetteFocus.Evaluate(timeVignetteFocus);
        }
    }
}

//chromeAbb
partial class GlobalVolumePostEffect
{
    [Header("DoChromeAbbPostEffect")]
    [SerializeField] AnimationCurve animCurveChromeAbb;
    ChromaticAberration chromeAbb;
    float timeChromeAbb;
    bool needChromeAbb, chromeAbbProcces;

    public void SetNeedChromeAbb(bool b) => needChromeAbb = b;

    void UpdateChromeAbb()
    {
        if (needChromeAbb)
        {
            timeChromeAbb += Time.deltaTime;
            if (timeChromeAbb >= animCurveChromeAbb.keys[^1].time)
            {
                timeChromeAbb = animCurveChromeAbb.keys[^1].time;
                return;
            }
            chromeAbbProcces = true;
            chromeAbb.intensity.value = animCurveChromeAbb.Evaluate(timeChromeAbb);
        }
        else if (chromeAbbProcces)
        {
            timeChromeAbb -= Time.deltaTime;
            if (timeChromeAbb <= animCurveChromeAbb.keys[0].time)
            {
                timeChromeAbb = 0;
                chromeAbbProcces = false;
                return;
            }
            chromeAbb.intensity.value = animCurveChromeAbb.Evaluate(timeChromeAbb);
        }
    }
}