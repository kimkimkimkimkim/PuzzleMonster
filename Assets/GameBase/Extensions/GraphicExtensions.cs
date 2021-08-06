using UnityEngine;
using UnityEngine.UI;

namespace GameBase
{
    public static class GraphicExtensions
    {
        public static void SetAlpha(this Graphic self, float alpha)
        {
            var color = self.color;
            var newColor = new Color(color.r, color.g, color.b, alpha);
            self.color = newColor;
        }
    }
}
