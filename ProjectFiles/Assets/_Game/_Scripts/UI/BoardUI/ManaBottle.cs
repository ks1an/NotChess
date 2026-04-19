using TMPro;
using UnityEngine;

public class ManaBottle : MonoBehaviour
{
    [SerializeField] Liquid liquid;
    [SerializeField] TextMeshPro manaTxt;
    [SerializeField] float fillSpeed, pourSpeed;

    float curMana;
    int maxMana;

    public void SetSettings(int curMana, int maxMana)
    {
        liquid.gameObject.SetActive(false);

        this.maxMana = maxMana;
        this.curMana = curMana;
        if(manaTxt != null)
            manaTxt.text = curMana.ToString();

        liquid.gameObject.SetActive(true);
        DeacreaseMana(maxMana);
        IncreaseMana(curMana);
    }

    public void DeacreaseMana(int value)
    {
        if (value < 0)
        {
            Debug.LogError($"Reveived a negative number({value}) in the DeacreaseMana()");
            return;
        }

        curMana -= value;

        if (curMana < 0)
            curMana = 0;
        if(manaTxt != null)
            manaTxt.text = curMana.ToString();
        liquid.SetFillAmount(curMana / 10, pourSpeed);
    }

    public void IncreaseMana(int value)
    {
        if (value < 0)
        {
            Debug.LogError($"Reveived a negative number({value}) in the IncreaseStamina()");
            return;
        }

        curMana += value;

        if (curMana > maxMana)
            curMana = maxMana;
        if (manaTxt != null)
            manaTxt.text = curMana.ToString();
        liquid.SetFillAmount(curMana / 10, fillSpeed);
    }
}
