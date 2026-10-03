using System.Collections;
using UnityEngine;

[ExecuteInEditMode]
public class Liquid : MonoBehaviour
{
    public enum UpdateMode { Normal, UnscaledTime }
    public UpdateMode updateMode;

    [SerializeField] bool disableWobbleOnMobile = true;
    [SerializeField] bool disableWobbleInMenu = true;
    [SerializeField] float MaxWobble = 0.03f;
    [SerializeField] float WobbleSpeedMove = 1f;
    [SerializeField][Range(0, 1)] float fillAmount = 0.5f;
    [SerializeField] float Recovery = 1f;
    [SerializeField] float Thickness = 1f;
    [Range(0, 1)] public float CompensateShapeAmount;
    [SerializeField] Mesh mesh;
    [SerializeField] Renderer rend;
    [SerializeField] bool invertFillDirection = true;

    float cachedLowestY;
    float cachedHighestY;
    bool pointsCached = false;

    MaterialPropertyBlock mpb;

    Vector3 pos;
    Vector3 lastPos;
    Vector3 velocity;
    Quaternion lastRot;
    Vector3 angularVelocity;
    float wobbleAmountX;
    float wobbleAmountZ;
    float wobbleAmountToAddX;
    float wobbleAmountToAddZ;
    float pulse;
    float sinewave;
    float time = 0.5f;
    Vector3 comp;
    float previousFillAmount = -1f;
    bool forceUpdate = false;

    float lastWobbleX = float.NaN;
    float lastWobbleZ = float.NaN;
    Vector3 lastFillAmount = new();
    float lastFillPercentage = float.NaN;

    #region Before liquid
    void Start()
    {
        EnsureInit();
        previousFillAmount = fillAmount;
        forceUpdate = true;
    }

    void EnsureInit()
    {
        mpb ??= new MaterialPropertyBlock();

        if (rend == null || mesh == null)
            GetMeshAndRend();

        if (mesh != null && !pointsCached)
            CacheExtremes();
    }

    private void OnValidate()
    {
        GetMeshAndRend();
        if (mesh != null) CacheExtremes();
        fillAmount = Mathf.Clamp01(fillAmount);

        if (previousFillAmount != fillAmount)
        {
            forceUpdate = true;
            previousFillAmount = fillAmount;
        }
    }

    void GetMeshAndRend()
    {
        if (mesh == null) mesh = GetComponent<MeshFilter>().sharedMesh;
        if (rend == null) rend = GetComponent<Renderer>();
    }

    void CacheExtremes()
    {
        if (mesh == null) { pointsCached = false; return; }

        Bounds b = mesh.bounds;
        Vector3 min = b.min;
        Vector3 max = b.max;

        cachedLowestY = float.MaxValue;
        cachedHighestY = float.MinValue;

        CheckY(transform.TransformPoint(min.x, min.y, min.z));
        CheckY(transform.TransformPoint(max.x, min.y, min.z));
        CheckY(transform.TransformPoint(min.x, max.y, min.z));
        CheckY(transform.TransformPoint(max.x, max.y, min.z));
        CheckY(transform.TransformPoint(min.x, min.y, max.z));
        CheckY(transform.TransformPoint(max.x, min.y, max.z));
        CheckY(transform.TransformPoint(min.x, max.y, max.z));
        CheckY(transform.TransformPoint(max.x, max.y, max.z));

        pointsCached = true;
    }

    void CheckY(Vector3 p)
    {
        if (p.y < cachedLowestY) cachedLowestY = p.y;
        if (p.y > cachedHighestY) cachedHighestY = p.y;
    }
    #endregion

    void Update()
    {
        EnsureInit();
        if (mesh == null || rend == null || mpb == null)
            return; 


        if (forceUpdate || fillAmount != previousFillAmount)
        {
            UpdateFillHeight();
            forceUpdate = false;
            previousFillAmount = fillAmount;
        }

        float deltaTime = updateMode == UpdateMode.Normal ? Time.deltaTime : Time.unscaledDeltaTime;
        bool isMenu = SceneLoader.Instance != null && SceneLoader.Instance.IsMenuScene();

        if (!(disableWobbleOnMobile && Application.isMobilePlatform)
    && !(disableWobbleInMenu && isMenu))
        {
            time += deltaTime;

            if (deltaTime != 0)
            {
                wobbleAmountToAddX = Mathf.Lerp(wobbleAmountToAddX, 0, deltaTime * Recovery);
                wobbleAmountToAddZ = Mathf.Lerp(wobbleAmountToAddZ, 0, deltaTime * Recovery);

                pulse = 2 * Mathf.PI * WobbleSpeedMove;
                sinewave = Mathf.Lerp(sinewave, Mathf.Sin(pulse * time),
                    deltaTime * Mathf.Clamp(velocity.magnitude + angularVelocity.magnitude, Thickness, 10));

                wobbleAmountX = wobbleAmountToAddX * sinewave;
                wobbleAmountZ = wobbleAmountToAddZ * sinewave;

                velocity = (lastPos - transform.position) / deltaTime;
                angularVelocity = GetAngularVelocity(lastRot, transform.rotation);

                wobbleAmountToAddX += Mathf.Clamp(
                    (velocity.x + (velocity.y * 0.2f) + angularVelocity.z + angularVelocity.y) * MaxWobble,
                    -MaxWobble, MaxWobble);
                wobbleAmountToAddZ += Mathf.Clamp(
                    (velocity.z + (velocity.y * 0.2f) + angularVelocity.x + angularVelocity.y) * MaxWobble,
                    -MaxWobble, MaxWobble);
            }

            UpdateMaterialIfChanged();
        }
        else
        {
            if (wobbleAmountX != 0 || wobbleAmountZ != 0)
            {
                wobbleAmountX = 0;
                wobbleAmountZ = 0;
                UpdateMaterialIfChanged();
            }
        }

        UpdatePos(deltaTime);

        lastPos = transform.position;
        lastRot = transform.rotation;
    }

    void UpdateMaterialIfChanged()
    {
        if (rend == null || mpb == null) return;

        if (!Mathf.Approximately(wobbleAmountX, lastWobbleX) ||
            !Mathf.Approximately(wobbleAmountZ, lastWobbleZ))
        {
            rend.GetPropertyBlock(mpb);
            mpb.SetFloat("_WobbleX", wobbleAmountX);
            mpb.SetFloat("_WobbleZ", wobbleAmountZ);
            rend.SetPropertyBlock(mpb);
            lastWobbleX = wobbleAmountX;
            lastWobbleZ = wobbleAmountZ;
        }
    }

    void UpdatePos(float deltaTime)
    {
        if (!pointsCached) CacheExtremes();

        Vector3 meshCenter = mesh.bounds.center;
        Vector3 worldPos = transform.TransformPoint(meshCenter);

        float calculatedFillHeight = CalculateFillHeight();

        if (CompensateShapeAmount > 0)
        {
            if (deltaTime != 0)
                comp = Vector3.Lerp(comp, worldPos - new Vector3(0, cachedLowestY, 0), deltaTime * 10);
            else
                comp = worldPos - new Vector3(0, cachedLowestY, 0);

            pos = worldPos - transform.position -
                  new Vector3(0, calculatedFillHeight - (comp.y * CompensateShapeAmount), 0);
        }
        else
        {
            pos = worldPos - transform.position - new Vector3(0, calculatedFillHeight, 0);
        }

        bool posChanged = !Approximately(pos, lastFillAmount);
        bool percentChanged = !Mathf.Approximately(fillAmount, lastFillPercentage);

        if (posChanged || percentChanged)
        {
            rend.GetPropertyBlock(mpb);
            mpb.SetVector("_FillAmount", pos);
            mpb.SetFloat("_FillPercentage", fillAmount);
            rend.SetPropertyBlock(mpb);
            lastFillAmount = pos;
            lastFillPercentage = fillAmount;
        }
    }
    static bool Approximately(Vector3 a, Vector3 b)
    {
        return Mathf.Approximately(a.x, b.x)
            && Mathf.Approximately(a.y, b.y)
            && Mathf.Approximately(a.z, b.z);
    }

    void UpdateFillHeight()
    {
        if (rend == null || rend.sharedMaterial == null) return;
        if (!pointsCached) CacheExtremes();

        float calculatedFillHeight = CalculateFillHeight();
        Vector3 worldPos = transform.TransformPoint(mesh.bounds.center);

        if (CompensateShapeAmount > 0)
        {
            comp = worldPos - new Vector3(0, cachedLowestY, 0);
            pos = worldPos - transform.position -
                  new Vector3(0, calculatedFillHeight - (comp.y * CompensateShapeAmount), 0);
        }
        else
        {
            pos = worldPos - transform.position - new Vector3(0, calculatedFillHeight, 0);
        }

        rend.GetPropertyBlock(mpb);
        mpb.SetVector("_FillAmount", pos);
        mpb.SetFloat("_FillPercentage", fillAmount);
        rend.SetPropertyBlock(mpb);
    }


    float CalculateFillHeight()
    {
        float totalHeight = cachedHighestY - cachedLowestY;
        float targetHeight = cachedLowestY + (totalHeight * fillAmount);

        if (invertFillDirection)
            targetHeight = cachedHighestY - (totalHeight * fillAmount);

        return targetHeight;
    }

    public void SetFillAmount(float targetFill, float duration)
    {
        StopAllCoroutines();
        if (duration == 0)
        {
            fillAmount = targetFill;
            forceUpdate = true;
        }
        else
            StartCoroutine(AnimateFill(targetFill, duration));
    }

    private IEnumerator AnimateFill(float target, float duration)
    {
        float startFill = fillAmount;
        float t = 0;

        while (t < duration)
        {
            fillAmount = Mathf.Lerp(startFill, target, t / duration);
            forceUpdate = true;
            t += Time.deltaTime;
            yield return null;
        }

        fillAmount = target;
        forceUpdate = true;
    }

    Vector3 GetAngularVelocity(Quaternion foreLastFrameRotation, Quaternion lastFrameRotation)
    {
        var q = lastFrameRotation * Quaternion.Inverse(foreLastFrameRotation);
        if (Mathf.Abs(q.w) > 1023.5f / 1024.0f)
            return Vector3.zero;

        float gain;
        if (q.w < 0.0f)
        {
            var angle = Mathf.Acos(-q.w);
            gain = -2.0f * angle / (Mathf.Sin(angle) * Time.deltaTime);
        }
        else
        {
            var angle = Mathf.Acos(q.w);
            gain = 2.0f * angle / (Mathf.Sin(angle) * Time.deltaTime);
        }
        Vector3 angularVelocity = new(q.x * gain, q.y * gain, q.z * gain);

        if (float.IsNaN(angularVelocity.z))
            angularVelocity = Vector3.zero;

        return angularVelocity;
    }
}