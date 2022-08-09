/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System.Collections.Generic;
using UnityEngine;

namespace Piot.Surge.Tools
{
    public class Lagometer : MonoBehaviour
    {
        [SerializeField] private GUIStyle guiStyle;
        private readonly Queue<int> interpolationValues = new();

        private readonly Queue<int> values = new();
        private Texture2D? backgroundTexture;
        private int debugInterpolationCount;

        private int debugLastInterpolationValue = -20;
        private int debugNextInterpolationCountThreshold = 3;
        private Texture2D? dropTexture;
        private Texture2D? extrapolationTexture;
        private Texture2D? interpolationTexture;
        private Texture2D? receivedTexture;

        private void Awake()
        {
            backgroundTexture = CreateTextureForColor(new Color(0, 0, 0, 0.9f));
            const float alpha = 0.5f;
            dropTexture = CreateTextureForColor(new Color(1.0f, 0, 0, alpha));
            receivedTexture = CreateTextureForColor(new Color(0, 1.0f, 0, alpha));
            interpolationTexture = CreateTextureForColor(new Color(0, 0, 1.0f, alpha));
            extrapolationTexture = CreateTextureForColor(new Color(1.0f, 1.0f, 0, alpha));
        }

        private void FixedUpdate()
        {
            var y = 200;

            debugLastInterpolationValue += 25;
            debugInterpolationCount++;
            if (debugInterpolationCount >= debugNextInterpolationCountThreshold)
            {
                debugInterpolationCount = 0;
                debugNextInterpolationCountThreshold = Random.Range(2, 7);
                debugLastInterpolationValue = -100;
            }

            interpolationValues.Enqueue(debugLastInterpolationValue);
            if (interpolationValues.Count > 200)
            {
                interpolationValues.Dequeue();
            }

            if (Random.Range(0, 100) < 14)
            {
                return;
            }

            var value = 16 + Random.Range(0, 50);
            if (Random.Range(0, 100) < 8)
            {
                value = -value;
            }

            values.Enqueue(value);
            if (values.Count > 200)
            {
                values.Dequeue();
            }
        }

        private void OnGUI()
        {
            var basePos = new Vector2(300, 300);
            GUI.DrawTexture(new Rect(basePos.x, basePos.y - 300, 200 * 2, 340), backgroundTexture,
                ScaleMode.StretchToFill, true, 0);

            var i = 0;
            foreach (var value in values)
            {
                if (value < 0)
                {
                    DrawBar(i, 80, dropTexture);
                }
                else
                {
                    DrawBar(i, value, receivedTexture);
                }

                ++i;
            }

            i = 0;
            var lineY = -100;
            foreach (var value in interpolationValues)
            {
                DrawLine(i, lineY, value, value < 0 ? interpolationTexture : extrapolationTexture);

                ++i;
            }

            GUI.Label(new Rect(basePos.x, basePos.y, 200 * 2, 340), "hello", guiStyle);
        }

        private static Texture2D CreateTextureForColor(Color color)
        {
            var colorTexture = new Texture2D(1, 1);
            colorTexture.SetPixel(0, 0, color);
            colorTexture.wrapMode = TextureWrapMode.Repeat;
            colorTexture.Apply();
            return colorTexture;
        }

        private static void DrawBar(int index, int value, Texture2D texture)
        {
            var basePos = new Vector2(300, 300);
            var xOffset = index * 2;
            GUI.DrawTexture(new Rect(basePos.x + xOffset, basePos.y - value, 2, value), texture,
                ScaleMode.StretchToFill, true, 0);
        }

        private static void DrawLine(int index, int y, int value, Texture2D texture)
        {
            var basePos = new Vector2(300, 300);
            var xOffset = index * 2;
            var adjustedValue = value / 10;
            var pixelValue = adjustedValue == 0 ? 1 : adjustedValue;
            GUI.DrawTexture(new Rect(basePos.x + xOffset, basePos.y + y - adjustedValue, 3, pixelValue), texture,
                ScaleMode.StretchToFill, true, 0);
        }
    }
}