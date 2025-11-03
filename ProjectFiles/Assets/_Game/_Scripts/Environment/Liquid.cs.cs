using System.Collections;
using UnityEngine;

[ExecuteInEditMode]
public class Liquid : MonoBehaviour
{
    public enum UpdateMode { Normal, UnscaledTime }
    public UpdateMode updateMode;

    [SerializeField]
    float MaxWobble = 0.03f;
    [SerializeField]
    float WobbleSpeedMove = 1f;
    [SerializeField]
    [Range(0, 1)]
    float fillAmount = 0.5f;
    [SerializeField]
    float Recovery = 1f;
    [SerializeField]
    float Thickness = 1f;
    [Range(0, 1)]
    public float CompensateShapeAmount;
    [SerializeField]
    Mesh mesh;
    [SerializeField]
    Renderer rend;

    [SerializeField]
    bool invertFillDirection = true;

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

    void Start()
    {
        GetMeshAndRend();
        previousFillAmount = fillAmount;
        forceUpdate = true;
    }

    private void OnValidate()
    {
        GetMeshAndRend();
        fillAmount = Mathf.Clamp01(fillAmount);

        if (previousFillAmount != fillAmount)
        {
            forceUpdate = true;
            previousFillAmount = fillAmount;
        }
    }

    void GetMeshAndRend()
    {
        if (mesh == null)
        {
            mesh = GetComponent<MeshFilter>().sharedMesh;
        }
        if (rend == null)
        {
            rend = GetComponent<Renderer>();
        }
    }

    void Update()
    {
        if (forceUpdate || fillAmount != previousFillAmount)
        {
            UpdateFillHeight();
            forceUpdate = false;
            previousFillAmount = fillAmount;
        }

        float deltaTime = 0;
        switch (updateMode)
        {
            case UpdateMode.Normal:
                deltaTime = Time.deltaTime;
                break;

            case UpdateMode.UnscaledTime:
                deltaTime = Time.unscaledDeltaTime;
                break;
        }

        time += deltaTime;

        if (deltaTime != 0)
        {
            wobbleAmountToAddX = Mathf.Lerp(wobbleAmountToAddX, 0, (deltaTime * Recovery));
            wobbleAmountToAddZ = Mathf.Lerp(wobbleAmountToAddZ, 0, (deltaTime * Recovery));

            pulse = 2 * Mathf.PI * WobbleSpeedMove;
            sinewave = Mathf.Lerp(sinewave, Mathf.Sin(pulse * time), deltaTime * Mathf.Clamp(velocity.magnitude + angularVelocity.magnitude, Thickness, 10));

            wobbleAmountX = wobbleAmountToAddX * sinewave;
            wobbleAmountZ = wobbleAmountToAddZ * sinewave;

            velocity = (lastPos - transform.position) / deltaTime;

            angularVelocity = GetAngularVelocity(lastRot, transform.rotation);

            wobbleAmountToAddX += Mathf.Clamp((velocity.x + (velocity.y * 0.2f) + angularVelocity.z + angularVelocity.y) * MaxWobble, -MaxWobble, MaxWobble);
            wobbleAmountToAddZ += Mathf.Clamp((velocity.z + (velocity.y * 0.2f) + angularVelocity.x + angularVelocity.y) * MaxWobble, -MaxWobble, MaxWobble);
        }

        rend.sharedMaterial.SetFloat("_WobbleX", wobbleAmountX);
        rend.sharedMaterial.SetFloat("_WobbleZ", wobbleAmountZ);

        UpdatePos(deltaTime);

        lastPos = transform.position;
        lastRot = transform.rotation;
    }

    void UpdatePos(float deltaTime)
    {
        Vector3 worldPos = transform.TransformPoint(new Vector3(mesh.bounds.center.x, mesh.bounds.center.y, mesh.bounds.center.z));

        float calculatedFillHeight = CalculateFillHeight();

        if (CompensateShapeAmount > 0)
        {
            if (deltaTime != 0)
            {
                comp = Vector3.Lerp(comp, (worldPos - new Vector3(0, GetLowestPoint(), 0)), deltaTime * 10);
            }
            else
            {
                comp = (worldPos - new Vector3(0, GetLowestPoint(), 0));
            }

            pos = worldPos - transform.position - new Vector3(0, calculatedFillHeight - (comp.y * CompensateShapeAmount), 0);
        }
        else
        {
            pos = worldPos - transform.position - new Vector3(0, calculatedFillHeight, 0);
        }

        rend.sharedMaterial.SetVector("_FillAmount", pos);
        rend.sharedMaterial.SetFloat("_FillPercentage", fillAmount);
    }

    void UpdateFillHeight()
    {
        if (rend != null && rend.sharedMaterial != null)
        {
            float calculatedFillHeight = CalculateFillHeight();
            Vector3 worldPos = transform.TransformPoint(new Vector3(mesh.bounds.center.x, mesh.bounds.center.y, mesh.bounds.center.z));

            if (CompensateShapeAmount > 0)
            {
                comp = (worldPos - new Vector3(0, GetLowestPoint(), 0));
                pos = worldPos - transform.position - new Vector3(0, calculatedFillHeight - (comp.y * CompensateShapeAmount), 0);
            }
            else
            {
                pos = worldPos - transform.position - new Vector3(0, calculatedFillHeight, 0);
            }

            rend.sharedMaterial.SetVector("_FillAmount", pos);
            rend.sharedMaterial.SetFloat("_FillPercentage", fillAmount);
        }
    }

    float CalculateFillHeight()
    {
        float lowestY = GetLowestPoint();
        float highestY = GetHighestPoint();
        float totalHeight = highestY - lowestY;

        float targetHeight = lowestY + (totalHeight * fillAmount);

        if (invertFillDirection)
        {
            targetHeight = highestY - (totalHeight * fillAmount);
        }

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
        float time = 0;

        while (time < duration)
        {
            fillAmount = Mathf.Lerp(startFill, target, time / duration);
            forceUpdate = true;
            time += Time.deltaTime;
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
        {
            angularVelocity = Vector3.zero;
        }
        return angularVelocity;
    }

    float GetLowestPoint()
    {
        float lowestY = float.MaxValue;
        Vector3[] vertices = mesh.vertices;

        for (int i = 0; i < vertices.Length; i++)
        {
            Vector3 position = transform.TransformPoint(vertices[i]);
            if (position.y < lowestY)
            {
                lowestY = position.y;
            }
        }
        return lowestY;
    }

    float GetHighestPoint()
    {
        float highestY = float.MinValue;
        Vector3[] vertices = mesh.vertices;

        for (int i = 0; i < vertices.Length; i++)
        {
            Vector3 position = transform.TransformPoint(vertices[i]);
            if (position.y > highestY)
            {
                highestY = position.y;
            }
        }
        return highestY;
    }
}