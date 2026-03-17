/* ================================================================
   ----------------------------------------------------------------
   Project   :   ExLib
   Company   :   Renowned Games
   Developer :   Zinnur Davleev
   ----------------------------------------------------------------
   Copyright 2022 Tamerlan Shakirov All rights reserved.
   ================================================================ */

using System;
using UnityEngine;

namespace RenownedGames.ExLibEditor.UIElements
{
    public struct ListItem
    {
        private string name;
        private string category;
        private string tooltip;
        private Color color;

        /// <summary>
        /// ListItem constructor.
        /// </summary>
        /// <param name="name">Name of item.</param>
        /// <param name="onClick">Called when item is clicked.</param>
        public ListItem(string name, Action onClick)
        {
            this.name = name;
            this.OnClick = onClick;
            category = string.Empty;
            tooltip = string.Empty;
            color = Color.white;
        }

        /// <summary>
        /// ListItem constructor.
        /// </summary>
        /// <param name="name">Name of item.</param>
        /// <param name="category">Category of item.</param>
        /// <param name="onClick">Called when item is clicked.</param>
        public ListItem(string name, string category, Action onClick) : this(name, onClick)
        {
            if (!string.IsNullOrEmpty(category))
            {
                this.category = category.Trim();
            }
        }

        /// <summary>
        /// ListItem constructor.
        /// </summary>
        /// <param name="name">Name of item.</param>
        /// <param name="category">Category of item.</param>
        /// <param name="tooltip">Tooltip of item, displayed when mouse over item.</param>
        /// <param name="onClick">Called when item is clicked.</param>
        public ListItem(string name, string category, string tooltip, Action onClick) : this(name, category, onClick)
        {
            if (!string.IsNullOrEmpty(tooltip))
            {
                this.tooltip = tooltip.Trim();
            }
        }

        /// <summary>
        /// ListItem constructor.
        /// </summary>
        /// <param name="name">Name of item.</param>
        /// <param name="category">Category of item.</param>
        /// <param name="tooltip">Tooltip of item, displayed when mouse over item.</param>
        /// <param name="color">Color of item, by default is white.</param>
        /// <param name="onClick">Called when item is clicked.</param>
        public ListItem(string name, string category, string tooltip, Color color, Action onClick) : this(name, category, tooltip, onClick)
        {
            this.color = color;
        }

        #region [Event Action]
        /// <summary>
        /// Called when item is clicked.
        /// </summary>
        public Action OnClick;
        #endregion

        #region [Getter / Setter]
        public string GetName()
        {
            return name;
        }

        public void SetName(string value)
        {
            name = value;
        }

        public string GetCategory()
        {
            return category;
        }

        public void SetCategory(string value)
        {
            category = value;
        }

        public string GetTooltip()
        {
            return tooltip;
        }

        public void SetTooltip(string value)
        {
            tooltip = value;
        }

        public Color GetColor()
        {
            return color;
        }

        public void SetColor(Color value)
        {
            color = value;
        }
        #endregion
    }
}
