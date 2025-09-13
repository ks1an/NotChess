using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public sealed class Tile : MonoBehaviour, IEffectable
{
    [HideInInspector] public Dictionary<ScriptableEffector, EffectorBehaviour> Effects { get; set; } = new();
    [HideInInspector] public bool banPutUnitsOnTile, banAttackTileByUnits, banLeaveTile;
    [HideInInspector] public DefendClass defendClass;
    [HideInInspector] public int xCord, yCord;
    [HideInInspector] public Vector3 tileCenter;

    EffectorBehaviour buffGivingDefend;

    public void SetBans(bool banPutUnitsOnTile, bool banAttackTileByUnits, bool banLeaveTile)
    {
        this.banPutUnitsOnTile = banPutUnitsOnTile;
        this.banAttackTileByUnits = banAttackTileByUnits;
        this.banLeaveTile = banLeaveTile;
    }

    public void SetDefendClass(DefendClass defClass, EffectorBehaviour buffGiving)
    {
        if ((int)defClass > (int)defendClass)
        {
            defendClass = defClass;
            buffGivingDefend = buffGiving;
        }
    }
    public bool TryGetAroundDefend(int attackClass)
    {
        if (attackClass < 0)
        {
            Debug.LogError("Trying attack with negative attack class");
            return false;
        }

        if (defendClass == 0 || buffGivingDefend == null)
            return true;
        else if (attackClass == (int)defendClass)
        {
            defendClass = DefendClass.None;
            buffGivingDefend.EndEffect();
            buffGivingDefend = null;
            return false;
        }
        else if (attackClass > (int)defendClass)
        {
            defendClass = DefendClass.None;
            buffGivingDefend.EndEffect();
            buffGivingDefend = null;
            return true;
        }
        return false;
    }

    #region Effects

    public void AddEffect(EffectorBehaviour buff)
    {
        if (Effects.Count == 0)
        {
            GameController.Instance.states.OnTurnEnded += OnTurnEnded;
            GameController.Instance.states.OnGameRestarted += EndAllEffects;
        }

        if (Effects.ContainsKey(buff.Effect))
        {
            Effects[buff.Effect].Activate(xCord, yCord);
        }
        else
        {
            Effects.Add(buff.Effect, buff);
            buff.Activate(xCord, yCord);
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
            GameController.Instance.states.OnTurnEnded -= OnTurnEnded;
            GameController.Instance.states.OnGameRestarted -= EndAllEffects;
        }
    }

    public void EndAllEffects()
    {
        if (Effects.Count > 0)
        {
            GameController.Instance.states.OnTurnEnded -= OnTurnEnded;
            GameController.Instance.states.OnGameRestarted -= EndAllEffects;
            foreach (EffectorBehaviour buff in Effects.Values.ToList())
            {
                buff.EndEffect();
                Effects.Remove(buff.Effect);
            }
        }
    }
    #endregion
}

public enum DefendClass
{
    None,
    Light,
    Average,
    Advanced,
    Strong
}
