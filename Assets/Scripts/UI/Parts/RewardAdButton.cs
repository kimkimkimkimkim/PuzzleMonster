using UnityEngine;
using UnityEngine.UI;

public class RewardAdButton : Button
{
    [SerializeField] protected long _rewardAdId;

    public long rewardAdId { get { return _rewardAdId; } }

}
