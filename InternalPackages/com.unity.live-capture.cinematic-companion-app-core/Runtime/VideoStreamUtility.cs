using UnityEngine;

namespace Unity.CompanionAppCommon
{
    public static class VideoStreamUtility
    {
        public static Vector2 ScreenPointToVideoStreamNormalized(Vector2 screenPoint, RectTransform rectTransform, Rect uvRect)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, screenPoint, null, out var localPoint);
            var normalized = Rect.PointToNormalized(rectTransform.rect, localPoint);
            normalized *= uvRect.size;
            normalized += uvRect.position;
            return normalized;
        }

        public static Vector2 VideoStreamNormalizedPointToScreen(Vector2 normalizedPoint, RectTransform rectTransform, Rect uvRect)
        {
            normalizedPoint -= uvRect.position;
            normalizedPoint /= uvRect.size;
            var localPoint = Rect.NormalizedToPoint(rectTransform.rect, normalizedPoint);
            var worldPoint = rectTransform.TransformPoint(localPoint);
            return RectTransformUtility.WorldToScreenPoint(null, worldPoint);
        }

        public static Rect GetUvRect(RectTransform rectTransform, Texture texture, bool crop)
        {
            var transformAspect = rectTransform.rect.width / rectTransform.rect.height;
            var textureAspect = texture.width / (float)texture.height;

            var size = Vector2.one;

            if (crop) // Crop.
            {
                if (transformAspect > textureAspect)
                {
                    size.y = textureAspect / transformAspect;
                }
                else
                {
                    size.x = transformAspect / textureAspect;
                }
            }
            else // LetterBox.
            {
                if (transformAspect > textureAspect)
                {
                    size.x = transformAspect / textureAspect;
                }
                else
                {
                    size.y = textureAspect / transformAspect;
                }
            }

            return FlipY(new Rect((1 - size.x) * .5f, (1 - size.y) * .5f, size.x, size.y));
        }

        static Rect FlipY(Rect rect)
        {
            var yMin = rect.yMin;
            var yMax = rect.yMax;
            rect.yMin = yMax;
            rect.yMax = yMin;
            return rect;
        }
    }
}
