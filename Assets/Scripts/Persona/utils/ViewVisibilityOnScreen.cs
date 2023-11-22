using System;
using UnityEngine;

namespace IO.Persona.MobileAds.Unity
{
    public class ViewVisibilityOnScreen
    {
        public static float GetVisiblePercentage(RectTransform targetRectTransform, RectTransform canvasRectTransform)
        {
            // Convert RectTransforms to screen space Rects
            Rect targetRectangle = GetScreenSpaceRect(targetRectTransform);
            Rect canvasRectangle = GetScreenSpaceRect(canvasRectTransform);

            // Calculate intersecting area
            Rect intersectionRectangle = GetIntersectionRect(targetRectangle, canvasRectangle);

            // Calculate and display intersecting area's size
            float targetRectangleArea = targetRectangle.width * targetRectangle.height;
            if (targetRectangleArea == 0) return 0f;
            float intersectionRectangleArea = intersectionRectangle.width * intersectionRectangle.height;
            float percentageVisible = (intersectionRectangleArea / targetRectangleArea) * 100f;

            return percentageVisible;
        }

        private static Rect GetScreenSpaceRect(RectTransform rectTransform)
        {
            Vector3[] corners = new Vector3[4];
            rectTransform.GetWorldCorners(corners);

            Vector3 bottomLeft = corners[0];
            Vector3 topRight = corners[2];

            Vector3 bottomLeftScreen = RectTransformUtility.WorldToScreenPoint(null, bottomLeft);
            Vector3 topRightScreen = RectTransformUtility.WorldToScreenPoint(null, topRight);

            return new Rect(bottomLeftScreen.x, bottomLeftScreen.y, topRightScreen.x - bottomLeftScreen.x, topRightScreen.y - bottomLeftScreen.y);
        }

        private static Rect GetIntersectionRect(Rect rect1, Rect rect2)
        {
            float x = Mathf.Max(rect1.xMin, rect2.xMin);
            float y = Mathf.Max(rect1.yMin, rect2.yMin);
            float width = Mathf.Min(rect1.xMax, rect2.xMax) - x;
            float height = Mathf.Min(rect1.yMax, rect2.yMax) - y;

            if (width < 0 || height < 0)
            {
                return Rect.zero; // No intersection
            }

            return new Rect(x, y, width, height);
        }
    }
}
