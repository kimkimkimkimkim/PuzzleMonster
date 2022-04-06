using GameBase;
using UnityEngine;

[ResourcePath("UI/Window/Window-GachaResult")]
public class GachaResultWindowUIScript : WindowBase
{
    [SerializeField] GameObject _okButtonBase;

   public override void Init(WindowInfo info)
    {
        base.Init(info);
    }

    public override void Open(WindowInfo info)
    {
    }

    public override void Back(WindowInfo info)
    {
    }

    public override void Close(WindowInfo info)
    {
        base.Close(info);
    }
}
