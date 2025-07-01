// Copyright (c) 2019-2025 Five Squared Interactive. All rights reserved.

using UnityEngine;
using TMPro;

namespace FiveSQD.StraightFour.Entity
{
    /// <summary>
    /// Class for a text entity.
    /// </summary>
    public class TextEntity : UIElementEntity
    {
        /// <summary>
        /// Canvas object for the text entity.
        /// </summary>
        public Canvas canvasObject;

        /// <summary>
        /// Text object for the text entity.
        /// </summary>
        public TextMeshProUGUI textObject;

        /// <summary>
        /// Scale factor for the text entity.
        /// </summary>
        private static readonly float scaleFactor = 1;

        /// <summary>
        /// Get the text for the text entity.
        /// </summary>
        /// <returns>The text for the text entity.</returns>
        public string GetText()
        {
            return textObject.text;
        }

        /// <summary>
        /// Get the font size for the text entity.
        /// </summary>
        /// <returns>The font size for the text entity.</returns>
        public int GetFontSize()
        {
            return (int) (textObject.fontSize / scaleFactor);
        }

        /// <summary>
        /// Get the color for the text entity.
        /// </summary>
        /// <returns>The color for the text entity.</returns>
        public Color GetColor()
        {
            return textObject.color;
        }

        /// <summary>
        /// Set the text for the text entity.
        /// </summary>
        /// <param name="text">Text to apply to the text entity.</param>
        /// <returns>Whether or not the operation was successful.</returns>
        public bool SetText(string text)
        {
            textObject.text = text;

            return true;
        }

        /// <summary>
        /// Set the font size for the text entity.
        /// </summary>
        /// <param name="size">Size to apply to the text entity.</param>
        /// <returns>Whether or not the operation was successful.</returns>
        public bool SetFontSize(int size)
        {
            textObject.fontSize = size * scaleFactor;

            return true;
        }

        /// <summary>
        /// Set the color for the text entity.
        /// </summary>
        /// <param name="color">Color to apply to the text entity.</param>
        /// <returns>Whether or not the operation was successful.</returns>
        public bool SetColor(Color color)
        {
            textObject.color = color;

            return true;
        }

        /// <summary>
        /// Set the margins for the text entity.
        /// </summary>
        /// <param name="margins">Margins to apply to the text entity.</param>
        /// <returns>Whether or not the operation was successful.</returns>
        public bool SetMargins(Vector4 margins)
        {
            textObject.margin = margins;

            return true;
        }

        /// <summary>
        /// Get the margins for the text entity.
        /// </summary>
        /// <returns>The margins for the text entity.</returns>
        public Vector4 GetMargins()
        {
            return textObject.margin;
        }

        /// <summary>
        /// Initialize this entity. This should only be called once.
        /// </summary>
        /// <param name="idToSet">ID to apply to the entity.</param>
        /// <param name="parentCanvas">Canvas to place the text entity on.</param>
        public override void Initialize(System.Guid idToSet, UIEntity parentCanvas)
        {
            base.Initialize(idToSet, parentCanvas);

            canvasObject = gameObject.AddComponent<Canvas>();
            textObject = gameObject.AddComponent<TextMeshProUGUI>();

            RectTransform rt = gameObject.GetComponent<RectTransform>();
            if (rt == null)
            {
                rt = gameObject.AddComponent<RectTransform>();
            }
            rt.position = Vector3.zero;
            rt.anchorMin = rt.anchorMax = Vector2.zero;
            uiElementRectTransform = rt;

            if (parentCanvas != null)
            {
                SetParent(parentCanvas);
            }

            SetColor(Color.black);

            MakeHidden();
        }
    }
}