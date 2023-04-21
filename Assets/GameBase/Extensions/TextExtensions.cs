using UnityEngine;
using UnityEngine.UI;

namespace GameBase {
    public static class TextExtension {
        /// <summary>
        /// 指定したテキストに収まるように指定した省略記号で省略された文字列を返します
        /// </summary>
        public static void SetOmmitedText(this Text textComponent, string text, string ellipsis = "...") {
            var rectTransform = textComponent.rectTransform;
            var generator = textComponent.cachedTextGenerator;
            var settings = textComponent.GetGenerationSettings(rectTransform.rect.size);
            generator.Populate(text, settings);

            if (rectTransform.rect.width == 0 || rectTransform.rect.height == 0) {
                // Do nothing because the layout seems not to have been built yet.
                textComponent.text = text;
                return;
            }

            if (text.Length == 0) {
                textComponent.text = text;
                return;
            }

            if (textComponent.horizontalOverflow == HorizontalWrapMode.Wrap) {
                var height = generator.GetPreferredHeight(text, settings) / settings.scaleFactor;

                if (rectTransform.rect.size.y >= height) {
                    textComponent.text = text;
                    return;
                }

                while (true) {
                    text = text.Remove(text.Length - 1);
                    height = generator.GetPreferredHeight(text + ellipsis, settings) / settings.scaleFactor;

                    if (text.Length == 0) {
                        break;
                    }

                    if (rectTransform.rect.size.y >= height) {
                        text += ellipsis;
                        break;
                    }
                }
            }

            if (textComponent.horizontalOverflow == HorizontalWrapMode.Overflow) {
                var width = generator.GetPreferredWidth(text, settings) / settings.scaleFactor;

                if (rectTransform.rect.size.x >= width) {
                    textComponent.text = text;
                    return;
                }

                while (true) {
                    text = text.Remove(text.Length - 1);
                    width = generator.GetPreferredWidth(text + ellipsis, settings) / settings.scaleFactor;

                    if (text.Length == 0) {
                        break;
                    }

                    if (rectTransform.rect.size.x >= width) {
                        text += ellipsis;
                        break;
                    }
                }
            }

            textComponent.text = text;
        }
    }
}
