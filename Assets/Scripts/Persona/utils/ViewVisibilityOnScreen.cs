using System;
using UnityEngine;

public class ViewVisibilityOnScreen
{
    public static float GetVisiblePercentage(RectTransform rectTransform, Canvas canvas)
    {
        // Get the Canvas RectTransform (assuming the Canvas is the parent of the image)
        RectTransform canvasRectTransform = canvas.GetComponent<RectTransform>();

        // Convert RectTransforms to screen space Rects
        Rect imageRectangle = GetScreenSpaceRect(rectTransform);
        Rect canvasRectangle = GetScreenSpaceRect(canvasRectTransform);

        // Calculate intersecting area
        Rect intersection = CalculateIntersection(imageRectangle, canvasRectangle);

        // Calculate and display intersecting area's size
        float visibleImageArea = imageRectangle.width * imageRectangle.height;
        float intersectingArea = intersection.width * intersection.height;
        float percentageVisible = (intersectingArea / visibleImageArea) * 100f;

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

    private static Rect CalculateIntersection(Rect rect1, Rect rect2)
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
