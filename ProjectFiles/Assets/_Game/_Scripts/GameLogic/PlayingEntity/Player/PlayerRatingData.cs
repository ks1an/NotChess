using System;

[Serializable]
public class PlayerRatingData
{
    public const int BaseMmr = 0;
    public const int KFactor = 32;

    public int CurMmr = BaseMmr;
    public int OldMmr = BaseMmr;

    public string SeasonKey = "";
    public int SeasonGamesPlayed = 0;
}