using System.Collections;
using System.Collections.Generic;
using GameBase;
using UnityEngine;

public class DummyWindowBase : MonoBehaviour
{
    public RectTransform _windowFrameRT;
    public RectTransform _fullScreenBaseRT; // セーフエリアにかかわらず画面サイズで表示されるUI
}
