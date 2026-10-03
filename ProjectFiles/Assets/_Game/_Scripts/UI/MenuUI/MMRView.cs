using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class MmrResultView : MonoBehaviour
{
    [Header("Main")]
    [SerializeField] Slider mmrSlider;
    [SerializeField] TextMeshProUGUI levelTxt;
    [SerializeField] float increaseAnimDuration = 1.2f, deacreaseAnimDuration = 0.75f;
    [SerializeField] Ease animEase = Ease.OutCubic;
    [SerializeField] float durationPauseUp = 0.5f;
    [SerializeField] float durationPauseDown = 0.25f;

    [Header("Level Jump")]
    [Tooltip("Если разница уровней больше этого — включается режим скачка.")]
    [SerializeField] int upLevelJumpThreshold = 3;
    [SerializeField] int downLevelJumpThreshold = 3;
    [Tooltip("Сколько уровней анимировать до скачка.")]
    [SerializeField] int upLevelsToAnimateBeforeJump = 2;
    [SerializeField] int downLevelsToAnimateBeforeJump = 2;

    [Header("FallTxt")]
    [SerializeField] TextMeshPool fallTxtPool;
    [SerializeField] RectTransform fallTxtSpawnAnchor;
    [SerializeField] Color decreaseColorFallTxt, increaseColorFallTxt;
    [SerializeField] float upDistanceFallTxt = 30f;
    [SerializeField] float leftDistanceFallTxt = 15f;
    [SerializeField] float durationFallTxtUp = 0.9f;
    [SerializeField] float holdDuration = 0.25f;
    [SerializeField] float fadeDuration = 0.4f;

    [Header("Level Text Effect")]
    [SerializeField] float levelPunchUpScale = 1.4f;
    [SerializeField] float levelPunchUpDuration = 0.25f;
    [SerializeField] float levelShakeDuration = 0.2f;
    [SerializeField] float levelShakeStrength = 8f;
    [SerializeField] float levelPunchDownDuration = 0.15f;
    [SerializeField] Ease levelPunchUpEase = Ease.OutBack;
    [SerializeField] Ease levelPunchDownEase = Ease.InBack;

    Coroutine animRoutine;
    Tween sliderTween;
    readonly List<TextMeshProUGUI> activeFallTxts = new();

    int pendingTargetMmr;
    bool hasPendingTarget;
    bool isCancelled;

    WaitForSeconds cachedPauseUp;
    WaitForSeconds cachedPauseDown;

    void Awake()
    {
        cachedPauseUp = new WaitForSeconds(durationPauseUp);
        cachedPauseDown = new WaitForSeconds(durationPauseDown);
    }

    public void Show(int oldMmr, int newMmr)
    {
        pendingTargetMmr = newMmr;
        hasPendingTarget = true;

        mmrSlider.minValue = 0;
        mmrSlider.maxValue = RatingCalculator.Instance.mmrPerLevel;

        if (animRoutine == null)
        {
            isCancelled = false;
            animRoutine = StartCoroutine(RunAnimations(oldMmr));
        }
    }


    #region Animation Common Logic
    IEnumerator RunAnimations(int startMmr)
    {
        int currentMmr = startMmr;

        while (hasPendingTarget)
        {
            hasPendingTarget = false;
            int target = pendingTargetMmr;

            yield return AnimateRoutine(currentMmr, target);
            currentMmr = target;
        }

        animRoutine = null;

        RatingService.Instance.UpdateOldMmrToCur();
    }

    IEnumerator AnimateRoutine(int oldMmr, int newMmr)
    {
        int delta = newMmr - oldMmr;
        if (delta != 0) DoFallTxt(delta);

        float perLevel = RatingCalculator.Instance.mmrPerLevel;
        int oldLevel = RatingCalculator.Instance.GetLevelMMR(oldMmr);
        int newLevel = RatingCalculator.Instance.GetLevelMMR(newMmr);

        if (oldLevel == newLevel)
        {
            SetLevelText(newLevel);
            yield return AnimateSlider(oldMmr % perLevel, newMmr % perLevel);
            yield break;
        }

        if (delta > 0)
            yield return AnimateUp(oldMmr, newMmr, oldLevel, newLevel);
        else
            yield return AnimateDown(oldMmr, newMmr, oldLevel, newLevel);
    }

    IEnumerator AnimateUp(int oldMmr, int newMmr, int currentLevel, int newLevel)
    {
        float perLevel = RatingCalculator.Instance.mmrPerLevel;
        SetLevelText(currentLevel);

        float startInLevel = oldMmr % perLevel;
        if (Mathf.Approximately(startInLevel, 0f) && oldMmr > 0)
            mmrSlider.value = 0f;
        else
            yield return AnimateSlider(startInLevel, perLevel);

        int totalLevels = newLevel - currentLevel;
        bool jumpMode = totalLevels > upLevelJumpThreshold;
        int levelsToAnimate = jumpMode
            ? Mathf.Min(upLevelsToAnimateBeforeJump, totalLevels - 1)
            : totalLevels;

        for (int i = 0; i < levelsToAnimate; i++)
        {
            if (isCancelled) yield break;
            yield return cachedPauseUp;
            if (isCancelled) yield break;

            currentLevel++;

            yield return AnimateLevelChange(currentLevel);
            mmrSlider.value = 0f;
            int targetInLevel = (!jumpMode && i == levelsToAnimate - 1)
                ? newMmr % (int)perLevel
                : (int)perLevel;

            if (targetInLevel > 0)
                yield return AnimateSlider(0, targetInLevel);
        }

        if (jumpMode && currentLevel < newLevel)
        {
            if (isCancelled) yield break;
            yield return cachedPauseUp;
            if (isCancelled) yield break;

            currentLevel = newLevel;
            yield return AnimateLevelChange(currentLevel);
            mmrSlider.value = 0f;

            int finalTarget = newMmr % (int)perLevel;
            if (finalTarget > 0)
                yield return AnimateSlider(0, finalTarget);
        }
    }

    IEnumerator AnimateDown(int oldMmr, int newMmr, int currentLevel, int newLevel)
    {
        float perLevel = RatingCalculator.Instance.mmrPerLevel;
        SetLevelText(currentLevel);

        yield return AnimateSlider(oldMmr % perLevel, 0f);

        int totalLevels = currentLevel - newLevel;
        bool jumpMode = totalLevels > downLevelJumpThreshold;
        int levelsToAnimate = jumpMode
            ? Mathf.Min(downLevelsToAnimateBeforeJump, totalLevels - 1)
            : totalLevels;

        for (int i = 0; i < levelsToAnimate; i++)
        {
            if (isCancelled) yield break;
            yield return cachedPauseDown;
            if (isCancelled) yield break;

            currentLevel--;
            SetLevelText(currentLevel);
            mmrSlider.value = perLevel;

            int targetInLevel = (!jumpMode && i == levelsToAnimate - 1)
                ? newMmr % (int)perLevel
                : 0;

            if (targetInLevel != (int)perLevel)
                yield return AnimateSlider(perLevel, targetInLevel);
        }

        if (jumpMode && currentLevel > newLevel)
        {
            if (isCancelled) yield break;
            yield return cachedPauseDown;
            if (isCancelled) yield break;

            currentLevel = newLevel;
            SetLevelText(currentLevel);
            mmrSlider.value = perLevel;

            int finalTarget = newMmr % (int)perLevel;
            if (finalTarget != (int)perLevel)
                yield return AnimateSlider(perLevel, finalTarget);
        }
    }

    IEnumerator AnimateSlider(float from, float to)
    {
        sliderTween?.Kill();
        sliderTween = null;

        if (isCancelled) yield break;

        mmrSlider.value = from;

        if (Mathf.Approximately(from, to)) yield break;

        float duration = to > from ? increaseAnimDuration : deacreaseAnimDuration;

        sliderTween = mmrSlider
            .DOValue(to, duration)
            .SetEase(animEase)
            .SetUpdate(true);

        while (sliderTween != null && sliderTween.IsActive() && !sliderTween.IsComplete())
        {
            if (isCancelled) yield break;
            yield return null;
        }

        sliderTween = null;
    }

    public void StopAnim()
    {
        RatingService.Instance.UpdateOldMmrToCur();
        isCancelled = true;
        hasPendingTarget = false;

        if (animRoutine != null)
        {
            StopCoroutine(animRoutine);
            animRoutine = null;
        }

        sliderTween?.Kill();
        sliderTween = null;

        for (int i = activeFallTxts.Count - 1; i >= 0; i--)
        {
            var txt = activeFallTxts[i];
            if (txt == null) continue;
            txt.DOKill();
            fallTxtPool.AddToPool(txt);
        }
        activeFallTxts.Clear();
    }
    #endregion

    void SetLevelText(int level)
    {
        if (levelTxt != null)
            levelTxt.text = level.ToString();
    }

    #region Some Effect
    void DoFallTxt(int delta)
    {
        if (delta == 0 || fallTxtPool == null) return;

        TextMeshProUGUI fallTxt = fallTxtPool.GetFromPool();
        if (fallTxt == null) return;

        activeFallTxts.Add(fallTxt);
        fallTxt.DOKill();

        RectTransform txtRect = fallTxt.rectTransform;

        txtRect.SetParent(fallTxtSpawnAnchor, false);
        txtRect.anchorMin = Vector2.zero;
        txtRect.anchorMax = Vector2.one;
        txtRect.pivot = new Vector2(0.5f, 0.5f);
        txtRect.sizeDelta = Vector2.zero;
        txtRect.offsetMin = Vector2.zero;
        txtRect.offsetMax = Vector2.zero;
        txtRect.anchoredPosition = Vector2.zero;
        txtRect.localRotation = Quaternion.identity;
        txtRect.localScale = Vector3.one;

        fallTxt.color = delta > 0 ? increaseColorFallTxt : decreaseColorFallTxt;
        fallTxt.text = delta > 0 ? $"+{delta}" : delta.ToString();
        fallTxt.alpha = 1f;

        Vector2 startPos = txtRect.anchoredPosition;
        float xJitter = MathOperations.GetInstance()
            .GetSafeRandom(-leftDistanceFallTxt, leftDistanceFallTxt);
        Vector2 targetPos = startPos + new Vector2(xJitter, upDistanceFallTxt);

        Sequence seq = DOTween.Sequence().SetUpdate(true);
        seq.Append(txtRect.DOAnchorPos(targetPos, durationFallTxtUp).SetEase(Ease.OutQuad));
        seq.AppendInterval(holdDuration);
        seq.Join(fallTxt.DOFade(0f, fadeDuration).SetEase(Ease.InQuad));

        seq.OnComplete(() =>
        {
            if (fallTxt == null) return;
            activeFallTxts.Remove(fallTxt);
            fallTxtPool.AddToPool(fallTxt);
        });
    }

    IEnumerator AnimateLevelChange(int newLevel)
    {
        if (levelTxt == null)
        {
            yield break;
        }

        Transform t = levelTxt.transform;
        t.DOKill();

        yield return t
            .DOScale(Vector3.one * levelPunchUpScale, levelPunchUpDuration)
            .SetEase(levelPunchUpEase)
            .SetUpdate(true)
            .WaitForCompletion();

        SetLevelText(newLevel);

        t.DOPunchPosition(Vector3.right * levelShakeStrength, levelShakeDuration, 10, 1f)
            .SetUpdate(true);

        yield return t
            .DOScale(Vector3.one, levelPunchDownDuration)
            .SetEase(levelPunchDownEase)
            .SetUpdate(true)
            .WaitForCompletion();
    }
    #endregion

    private void OnEnable()
    {
        Show(RatingService.Instance.Data.OldMmr, RatingService.Instance.Data.CurMmr);
        RatingService.Instance.OnMmrChanged += Show;
    }

    private void OnDisable()
    {
        RatingService.Instance.OnMmrChanged -= Show;
        StopAnim();
    }
}