using GameBase;
using PM.Enum.Battle;
using UnityEngine;

public class TimeManager : SingletonMonoBehaviour<TimeManager>
{
    public void Pause()
    {
        Time.timeScale = 0.0f;
    }

    /// <summary>
    /// 1倍速
    /// </summary>
    public void SpeedBy1()
    {
        Time.timeScale = 1.0f;
    }

    /// <summary>
    /// 2倍速
    /// </summary>
    public void SpeedBy2()
    {
        Time.timeScale = 2.0f;
    }

    public void SetSpeed(BattleSpeed battleSpeed)
    {
        switch (battleSpeed)
        {
            case BattleSpeed.One:
                Time.timeScale = 1.0f;
                break;

            case BattleSpeed.TwoTimes:
                Time.timeScale = 2.0f;
                break;

            case BattleSpeed.FourTimes:
                Time.timeScale = 4.0f;
                break;

            case BattleSpeed.None:
            default:
                Time.timeScale = 1.0f;
                break;
        }
    }
}