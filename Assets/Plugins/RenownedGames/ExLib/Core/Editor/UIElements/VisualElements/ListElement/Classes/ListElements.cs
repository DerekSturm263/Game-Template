/* ================================================================
   ----------------------------------------------------------------
   Project   :   ExLib
   Company   :   Renowned Games
   Developer :   Zinnur Davleev
   ----------------------------------------------------------------
   Copyright 2022 Tamerlan Shakirov All rights reserved.
   ================================================================ */

using System.Collections.Generic;
using UnityEngine.UIElements;

namespace RenownedGames.ExLibEditor.UIElements
{
    public sealed class ListElement : VisualElement, IListElement
    {
        public new class UxmlFactory : UxmlFactory<ListElement, UxmlTraits> { }

        private List<ListItem> list;
        private ScrollView container;

        public ListElement()
        {
            list = new List<ListItem>();
            StyleSheet styleSheet = EditorResources.Load<StyleSheet>("UIElements/Styles/StyleSheets/ListElement.uss");
            styleSheets.Add(styleSheet);
            AddToClassList("list-element");
        }

        /// <summary>
        /// Initializing the list elements.
        /// </summary>
        public void Initialize()
        {
            Clear();

            container = new ScrollView(ScrollViewMode.Vertical);
            Add(container);

            list.Sort(ItemCompare);

            Foldout lastFoldout = null;
            string lastCategory = string.Empty;

            for (int i = 0; i < list.Count; i++)
            {
                ListItem item = list[i];

                if (!string.IsNullOrEmpty(item.GetCategory()))
                {
                    if (lastCategory != item.GetCategory())
                    {
                        lastCategory = item.GetCategory();

                        lastFoldout = new Foldout();
                        lastFoldout.text = lastCategory;
                        lastFoldout.AddToClassList("list-element-foldout");

                        container.Add(lastFoldout);
                    }
                }
                else if (lastFoldout != null)
                {
                    lastFoldout = null;
                }

                Label element = new Label(item.GetName());
                element.tooltip = item.GetTooltip();
                element.userData = i;
                element.focusable = true;
                element.AddToClassList("list-element-item");
                element.RegisterCallback<ClickEvent>(OnClickEvent);


                VisualElement icon = new VisualElement();
                icon.style.backgroundColor = new StyleColor(item.GetColor());
                icon.AddToClassList("list-element-item-icon");

                element.Add(icon);

                if (lastFoldout != null)
                {
                    lastFoldout.Add(element);
                }
                else
                {
                    container.Add(element);
                }
            }
        }

        /// <summary>
        /// Add new item to list.
        /// </summary>
        public void AddItem(ListItem item)
        {
            list.Add(item);
        }

        /// <summary>
        /// Clears the list of items.
        /// </summary>
        public void ClearItems()
        {
            list.Clear();
        }

        /// <summary>
        /// Called when you click on an item.
        /// </summary>
        private void OnClickEvent(ClickEvent evt)
        {
            if (evt.propagationPhase != PropagationPhase.AtTarget)
                return;

            VisualElement element = evt.target as VisualElement;
            int index = (int)element.userData;

            list[index].OnClick?.Invoke();
        }

        /// <summary>
        /// The logic of sorting items.
        /// </summary>
        private int ItemCompare(ListItem lhs, ListItem rhs)
        {
            int compare = string.IsNullOrEmpty(lhs.GetCategory()).CompareTo(string.IsNullOrEmpty(rhs.GetCategory()));
            if (compare == 0)
            {
                compare = lhs.GetCategory().CompareTo(rhs.GetCategory());
                if (compare == 0)
                {
                    compare = lhs.GetName().CompareTo(rhs.GetName());
                }
            }
            return compare;
        }
    }
}