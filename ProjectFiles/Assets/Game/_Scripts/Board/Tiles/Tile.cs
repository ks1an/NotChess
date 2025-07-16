using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public sealed class Tile : MonoBehaviour, IEffectable
{
    public Dictionary<ScriptableEffector, EffectorBehaviour> Effects { get; set; } = new();
    public bool banPutUnitsOnTile, banAttackTileByUnits, banLeaveTile;
    public Vector3 tileCenter;

    public void SetBans(bool banPutUnitsOnTile, bool banAttackTileByUnits, bool banLeaveTile)
    {
        this.banPutUnitsOnTile = banPutUnitsOnTile;
        this.banAttackTileByUnits = banAttackTileByUnits;
        this.banLeaveTile = banLeaveTile;
    }

    #region Effects

    public void AddEffect(EffectorBehaviour buff)
    {
        if (Effects.Count == 0)
        {
            MatchController.Instance.states.OnTurnEnded += OnTurnEnded;
            MatchController.Instance.states.OnGameRestarted += EndAllEffects;
        }

        if (Effects.ContainsKey(buff.Effect))
        {
            Effects[buff.Effect].Activate(tileCenter);
        }
        else
        {
            Effects.Add(buff.Effect, buff);
            buff.Activate(tileCenter);
        }
    }

    public void OnTurnEnded(int x, int y, Team teamTurned)
    {
        foreach (EffectorBehaviour activeBuff in Effects.Values.ToList())
        {
            activeBuff.OnTurnEnded();

            if (activeBuff.isFinished)
                Effects.Remove(activeBuff.Effect);
        }

        if (Effects.Count == 0)
        {
            MatchController.Instance.states.OnTurnEnded -= OnTurnEnded;
            MatchController.Instance.states.OnGameRestarted -= EndAllEffects;
        }
    }

    public void EndAllEffects()
    {
        if (Effects.Count > 0)
        {
            MatchController.Instance.states.OnTurnEnded -= OnTurnEnded;
            MatchController.Instance.states.OnGameRestarted -= EndAllEffects;
            foreach (EffectorBehaviour buff in Effects.Values.ToList())
            {
                buff.EndEffect();
                Effects.Remove(buff.Effect);
            }
        }
    }
    #endregion
}
