using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Slider))]
public class SnappedSlider : MonoBehaviour
{
    [SerializeField] Slider slider;
    [SerializeField] int steps = 10;
    [SerializeField] bool notifyOnSnap = true;

    public Slider Slider => slider;
    public event System.Action<float> OnValueChanged;

    void Awake()
    {
        if (slider == null) slider = GetComponent<Slider>();

        slider.onValueChanged.AddListener(HandleChange);
        slider.SetValueWithoutNotify(Snap(slider.value));
    }

    void HandleChange(float value)
    {
        float snapped = Snap(value);

        if (!Mathf.Approximately(value, snapped))
            slider.SetValueWithoutNotify(snapped);

        if (notifyOnSnap)
            OnValueChanged?.Invoke(snapped);
    }

    float Snap(float value)
    {
        if (steps <= 0) return value;
        float step = 1f / steps;
        return Mathf.Clamp01(Mathf.Round(value / step) * step);
    }

    public void DisplayValue(float value)
    {
        slider.SetValueWithoutNotify(Snap(value));
    }

    public void ClearListeners() => OnValueChanged = null;
}
