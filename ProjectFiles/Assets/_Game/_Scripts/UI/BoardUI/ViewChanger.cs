using DG.Tweening;
using UnityEngine;
using UnityEngine.Splines;
using UnityEngine.UI;

public class ViewChanger : MonoBehaviour
{
    [SerializeField] Button NextStateBttn, PrevStateBttn;
    [SerializeField] float durationTransit;
    int curState;

    #region CardFocusState
    [Header("CardFocusPosSettings")]
    [SerializeField] Vector3 camCardFocusRot;
    [SerializeField] Vector3 camCardFocusPos;
    [SerializeField] SplineContainer playerHandSpline_FocusCard, enemyHandSpline_FocusCard;
    [SerializeField] Vector3 playerCardHandFocusPos, playerCardHandFocusRot;
    [SerializeField] Vector3 enemyCardHandFocusPos, enemyCardHandFocusRot;
    public void SetCardFocusState()
    {
        curState = 0;
        SetCam(camCardFocusRot, camCardFocusPos);
        SetCardHand(PlayerCardHand.Instance, playerHandSpline_FocusCard, playerCardHandFocusPos, playerCardHandFocusRot);
        SetCardHand(EnemyDeck.Instance.hand, enemyHandSpline_FocusCard, enemyCardHandFocusPos, enemyCardHandFocusRot);
    }
    #endregion

    #region DefState
    [Header("DefPosSettings")]
    [SerializeField] Vector3 camDefRot;
    [SerializeField] Vector3 camDefPos;
    [SerializeField] SplineContainer playerHandSpline_Def, enemyHandSpline_Def;
    [SerializeField] Vector3 playerCardHandDefPos, playerCardHandDefRot;
    [SerializeField] Vector3 enemyCardHandDefPos, enemyCardHandDefRot;
    public void SetDefState(bool needFast = false)
    {
        curState = 1;
        SetCam(camDefRot, camDefPos, needFast);
        SetCardHand(PlayerCardHand.Instance, playerHandSpline_Def, playerCardHandDefPos, playerCardHandDefRot, needFast);
        SetCardHand(EnemyDeck.Instance.hand, enemyHandSpline_Def, enemyCardHandDefPos, enemyCardHandDefRot, needFast);
    }
    void SetFastDefState() => SetDefState(true);
    #endregion

    #region OrthoBoardState
    [Header("OrthoBoardPosSettings")]
    [SerializeField] Vector3 camRotOrthoBoard;
    [SerializeField] Vector3 camPosOrthoBoard;
    [SerializeField] SplineContainer playerHandSpline_OrthoBoard, enemyHandSpline_OrthoBoard;
    [SerializeField] Vector3 playerCardHandOrthoBoardPos, playerCardHandOrthoBoardRot;
    [SerializeField] Vector3 enemyCardHandOrthoBoardPos,  enemyCardHandOrthoBoardRot;
    public void SetOrthoBoardState()
    {
        curState = 2;
        SetCam(camRotOrthoBoard, camPosOrthoBoard);
        SetCardHand(PlayerCardHand.Instance, playerHandSpline_OrthoBoard, playerCardHandOrthoBoardPos, playerCardHandOrthoBoardRot);
        SetCardHand(EnemyDeck.Instance.hand, enemyHandSpline_OrthoBoard, enemyCardHandOrthoBoardPos, enemyCardHandOrthoBoardRot);
    }
    #endregion


    private void Awake()
    {
        #region SplinesSet
        playerHandSpline_Def = playerHandSpline_Def == null ? PlayerCardHand.Instance.GetSpline() : playerHandSpline_Def;
        enemyHandSpline_Def = enemyHandSpline_Def == null ? EnemyDeck.Instance.hand.GetSpline() : enemyHandSpline_Def;

        playerHandSpline_FocusCard = playerHandSpline_FocusCard == null ? PlayerCardHand.Instance.GetSpline() : playerHandSpline_FocusCard;
        enemyHandSpline_FocusCard = enemyHandSpline_FocusCard == null ? EnemyDeck.Instance.hand.GetSpline() : enemyHandSpline_FocusCard;

        playerHandSpline_OrthoBoard = playerHandSpline_OrthoBoard == null ? PlayerCardHand.Instance.GetSpline() : playerHandSpline_OrthoBoard;
        enemyHandSpline_OrthoBoard = enemyHandSpline_OrthoBoard == null ? EnemyDeck.Instance.hand.GetSpline() : enemyHandSpline_OrthoBoard;
        #endregion

        curState = 0; ChangeState(1);

        NextStateBttn.onClick.AddListener(() => { ChangeState(1); });
        PrevStateBttn.onClick.AddListener(() => { ChangeState(-1); });
        SceneLoader.Instance.OnSomeSceneStartLoading += SetFastDefState;
    }

    void ChangeState(int plusIndex)
    {
        curState += plusIndex;
        if (curState < 0) curState = 2;
        if (curState > 2) curState = 0;

        switch (curState)
        {
            case 0:
                SetCardFocusState();
                PrevStateBttn.gameObject.SetActive(false);
                NextStateBttn.gameObject.SetActive(true);
                break;
            case 1:
                SetDefState();
                PrevStateBttn.gameObject.SetActive(true);
                NextStateBttn.gameObject.SetActive(true);
                break;
            case 2:
                SetOrthoBoardState();
                PrevStateBttn.gameObject.SetActive(true);
                NextStateBttn.gameObject.SetActive(false);
                break;
        }
    }

    void  SetCam(Vector3 camRot, Vector3 camPos, bool fastSet = false)
    {
        Camera.main.gameObject.transform.DOKill(false);
        Camera.main.gameObject.transform.DORotate(camRot, durationTransit);
        Camera.main.gameObject.transform.DOMove(camPos, durationTransit);
        if (fastSet)
            Camera.main.gameObject.transform.DOComplete(true);
    }

    void SetCardHand(HandObject hand, SplineContainer spline, Vector3 newPos, Vector3 newRot, bool fastSet = false)
    {
        hand.gameObject.transform.DOComplete(false);
        hand.gameObject.transform.DOMove(newPos, durationTransit);
        hand.gameObject.transform.DORotate(newRot, durationTransit).OnComplete(() => { StartCoroutine(hand.SetSpline(spline)); });
        if (fastSet)
            hand.gameObject.transform.DOComplete(true);
    }

    private void OnDisable()
    {
        SceneLoader.Instance.OnSomeSceneStartLoading -= SetFastDefState;
    }
}
