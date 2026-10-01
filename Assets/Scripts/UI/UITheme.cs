using UnityEngine;
using UnityEngine.UI;

namespace RetwineMake.UI
{
    public static class UITheme
    {
        public static readonly Color Accent = new Color(0.82f, 0.64f, 0.22f);
        public static readonly Color AccentBright = new Color(0.95f, 0.78f, 0.35f);
        public static readonly Color TextPrimary = Color.white;
        public static readonly Color TextMuted = new Color(0.68f, 0.72f, 0.78f);
        public static readonly Color PanelBackground = new Color(0.06f, 0.07f, 0.09f, 0.88f);
        public static readonly Color ButtonNormal = new Color(1f, 1f, 1f, 0.07f);
        public static readonly Color ButtonHighlighted = new Color(0.82f, 0.64f, 0.22f, 0.38f);
        public static readonly Color ButtonPressed = new Color(0.82f, 0.64f, 0.22f, 0.65f);

        public static ColorBlock ButtonColors()
        {
            var cb = ColorBlock.defaultColorBlock;
            cb.normalColor = ButtonNormal;
            cb.highlightedColor = ButtonHighlighted;
            cb.pressedColor = ButtonPressed;
            cb.selectedColor = ButtonHighlighted;
            cb.disabledColor = new Color(1f, 1f, 1f, 0.03f);
            cb.fadeDuration = 0.08f;
            return cb;
        }
    }
}
