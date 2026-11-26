using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ResourceView : MonoBehaviour
{
    [SerializeField] Image icon;
    [SerializeField] TextMeshProUGUI text;
    public ResourceType resourceType;

    [Header("Animation")]
    [SerializeField] int upscaleTextOnAnim = 2;
    [SerializeField] float animationDuration = 0.25f;

    private Coroutine _animationCoroutine;
    private float _originalFontSize = -1f;

    private void Awake()
    {
        if (text != null)
        {
            _originalFontSize = text.fontSize;
        }
    }

    public void SetResourceView(Sprite spriteIcon, int value, ResourceType resType, bool force = false)
    {
        if (spriteIcon != null) icon.sprite = spriteIcon;
        else icon.gameObject.SetActive(false);

        UpdateText(value, force);
        resourceType = resType;
    }

    public void UpdateText(int newValue, bool force = false)
    {
        if (text == null) return;

        if (!text.gameObject.activeSelf)
        {
            text.gameObject.SetActive(true);
        }

        if (_originalFontSize < 0)
        {
            _originalFontSize = text.fontSize;
        }

        if (_animationCoroutine != null)
        {
            StopCoroutine(_animationCoroutine);
            _animationCoroutine = null;
            text.fontSize = _originalFontSize;
        }

        int currentValue = newValue;

        if (!force && int.TryParse(text.text, out int parsedCurrent))
        {
            currentValue = parsedCurrent;
        }

        if (force || currentValue == newValue)
        {
            text.text = newValue.ToString();
            text.fontSize = _originalFontSize;
        }
        else
        {
            _animationCoroutine = StartCoroutine(AnimateValueRoutine(currentValue, newValue));
        }
    }

    private IEnumerator AnimateValueRoutine(int startValue, int targetValue)
    {
        text.fontSize = _originalFontSize + upscaleTextOnAnim;

        int step = targetValue > startValue ? 1 : -1;
        int current = startValue;

        int diff = Mathf.Abs(targetValue - startValue);
        float delay = diff > 0 ? Mathf.Min(0.03f, animationDuration / diff) : 0f;

        while (current != targetValue)
        {
            current += step;
            text.text = current.ToString();

            if (delay > 0f)
                yield return new WaitForSeconds(delay);
            else
                yield return null;
        }

        text.text = targetValue.ToString();
        text.fontSize = _originalFontSize;
        _animationCoroutine = null;
    }
}