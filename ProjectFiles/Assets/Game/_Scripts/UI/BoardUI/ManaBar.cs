using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class ManaBar : MonoBehaviour
{
    [SerializeField] Slider manaSlider;
    [SerializeField] TextMeshProUGUI manaTxt;
    [SerializeField] float fillSpeed, pourSpeed;

    #region Effects
    [Header("Decrease Effect")]
    [SerializeField] Slider deacreaseEffectSlider;
    [SerializeField]
    float deacreaseAnimTime,
        deacreaseAnimDelayTime;

    [Header("Increase Effect")]
    [SerializeField] Slider increaseEffectSlider;
    [SerializeField] float increaseAnimTime,
        increaseAnimDelayTime;
    bool isIncreaseAnimPlaying;
    #endregion

    float curMana;
    int maxMana;

    public void SetSettings()
    {
        maxMana = MatchController.Instance.settings.maxMana;
        curMana = 0;

        manaSlider.maxValue = maxMana;
        deacreaseEffectSlider.maxValue = maxMana;
        increaseEffectSlider.maxValue = maxMana;

        manaSlider.value = 0;
        deacreaseEffectSlider.value = 0;
        increaseEffectSlider.value = 0;
        manaTxt.text = "0";
    }

    void Update()
    {
        if(manaSlider.value < curMana)
        {
            if (!isIncreaseAnimPlaying)
                manaSlider.value += fillSpeed * Time.deltaTime;

            if (manaSlider.value > curMana)
                manaSlider.value = curMana;
        }
        else if(manaSlider.value > curMana + 0.1f)
        {
            if (!isIncreaseAnimPlaying)
                manaSlider.value -= pourSpeed * Time.deltaTime;

            if (manaSlider.value < curMana)
                manaSlider.value = curMana;
        }
    }

    #region PublicMethods
    public void DeacreaseMana(int value)
    {
        if(value < 0)
        {
            Debug.LogError($"Reveived a negative number({value}) in the DeacreaseMana()");
            return;
        }

        deacreaseEffectSlider.value = curMana;
        curMana -= value;
        increaseEffectSlider.value = curMana;

        if (curMana < 0)
            curMana = 0;
        manaTxt.text = curMana.ToString();

        DeacreaseEffectAnim();
    }

    public void IncreaseMana(int value)
    {
        if (value < 0)
        {
            Debug.LogError($"Reveived a negative number({value}) in the IncreaseStamina()");
            return;
        }

        increaseEffectSlider.value = curMana;
        curMana += value;
        if (curMana > maxMana)
            curMana = maxMana;
        manaTxt.text = curMana.ToString();

        IncreaseEffectAnim();
    }
    #endregion

    #region Effects

    void DeacreaseEffectAnim()
    {
        deacreaseEffectSlider.DOComplete();
        deacreaseEffectSlider.DOValue(curMana, deacreaseAnimTime).SetDelay(deacreaseAnimDelayTime);
    }

    void IncreaseEffectAnim()
    {
        increaseEffectSlider.DOComplete();
        isIncreaseAnimPlaying = true;
        increaseEffectSlider.DOValue(curMana, increaseAnimTime).SetDelay(increaseAnimDelayTime).
            OnComplete(SetFalseIncreaseAnimPlaying);
    }

    void SetFalseIncreaseAnimPlaying() => isIncreaseAnimPlaying = false;
    #endregion
}
