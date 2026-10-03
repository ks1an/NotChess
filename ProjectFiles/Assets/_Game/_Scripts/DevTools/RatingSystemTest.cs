using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public sealed class RatingSystemTest : MonoBehaviour
{
    [SerializeField] Button addMmrBttn, SetMmrBttn;
    [SerializeField] int mmrForAdd, mmrForSet;

    private void Awake()
    {
        if (Application.isEditor == false)
        {
            Destroy(gameObject);
        }
    }
    private void Start()
    {
        addMmrBttn.onClick.AddListener(() => RatingService.Instance.AddMmr(mmrForAdd));
        SetMmrBttn.onClick.AddListener(() => RatingService.Instance.SetMmr(mmrForSet));
    }

    private void OnDestroy()
    {
        addMmrBttn.onClick.RemoveAllListeners();
        SetMmrBttn.onClick.RemoveAllListeners();
    }
}
