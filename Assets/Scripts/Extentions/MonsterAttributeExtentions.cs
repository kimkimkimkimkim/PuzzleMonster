using PM.Enum.Monster;
using UnityEngine;

public static class MonsterAttributeExtends
{
    public static Color Color(this MonsterAttribute attribute)
    {
        switch (attribute)
        {
            case MonsterAttribute.Red:
                return new Color(0.1529412f, 0.03921569f, 0.03137255f);
            case MonsterAttribute.Blue:
                return new Color(0.007843138f, 0.07450981f, 0.2039216f);
            case MonsterAttribute.Green:
                return new Color(0.07843138f, 0.1803922f, 0.1333333f);
            case MonsterAttribute.Yellow:
                return new Color(0.9647059f, 0.8823529f, 0.6117647f);
            case MonsterAttribute.Purple:
                return new Color(0.1294118f, 0.05098039f, 0.2039216f);
            default:
                return new Color(0, 0, 0);
        }
    }
}