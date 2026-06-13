using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace MackySoft.SerializeReferenceExtensions.Editor
{

    public class AdvancedTypePopupItem : AdvancedDropdownItem
    {

        public Type Type { get; }

        public AdvancedTypePopupItem (Type type, string name) : base(name)
        {
            Type = type;
        }

    }

    /// <summary>
    /// A type popup with a fuzzy finder.
    /// </summary>
    public class AdvancedTypePopup : AdvancedDropdown
    {

        private const int MaxNamespaceNestCount = 16;

        public static void AddTo (AdvancedDropdownItem root, IEnumerable<Type> types, bool showFullNamespace = false)
        {
            int itemCount = 0;

            // Add null item.
            var nullItem = new AdvancedTypePopupItem(null, TypeMenuUtility.NullDisplayName)
            {
                id = itemCount++
            };
            root.AddChild(nullItem);

            Type[] typeArray = types.OrderByType().ToArray();

            // Single namespace if the root has one namespace and the nest is unbranched.
            bool isSingleNamespace = !showFullNamespace;
            if (!showFullNamespace)
            {
                string[] namespaces = new string[MaxNamespaceNestCount];
                foreach (Type type in typeArray)
                {
                    string[] splittedTypePath = TypeMenuUtility.GetSplittedTypePath(type);
                    if (splittedTypePath.Length <= 1)
                    {
                        continue;
                    }
                    // If they explicitly want sub category, let them do.
                    if (TypeMenuUtility.GetAttribute(type) != null)
                    {
                        isSingleNamespace = false;
                        break;
                    }
                    for (int k = 0; (splittedTypePath.Length - 1) > k; k++)
                    {
                        string ns = namespaces[k];
                        if (ns == null)
                        {
                            namespaces[k] = splittedTypePath[k];
                        }
                        else if (ns != splittedTypePath[k])
                        {
                            isSingleNamespace = false;
                            break;
                        }
                    }

                    if (!isSingleNamespace)
                    {
                        break;
                    }
                }
            }

            // Add type items.
            foreach (Type type in typeArray)
            {
                string[] splittedTypePath = TypeMenuUtility.GetSplittedTypePath(type);
                if (splittedTypePath.Length == 0)
                {
                    continue;
                }

                AdvancedDropdownItem parent = root;

                // Add namespace items.
                if (!isSingleNamespace)
                {
                    for (int k = 0; (splittedTypePath.Length - 1) > k; k++)
                    {
                        AdvancedDropdownItem foundItem = GetItem(parent, splittedTypePath[k]);
                        if (foundItem != null)
                        {
                            parent = foundItem;
                        }
                        else
                        {
                            var newItem = new AdvancedDropdownItem(splittedTypePath[k])
                            {
                                id = itemCount++,
                            };
                            parent.AddChild(newItem);
                            parent = newItem;
                        }
                    }
                }

                // Add type item.
                var item = new AdvancedTypePopupItem(type, ObjectNames.NicifyVariableName(splittedTypePath[splittedTypePath.Length - 1]))
                {
                    id = itemCount++
                };
                parent.AddChild(item);
            }
        }

        private static AdvancedDropdownItem GetItem (AdvancedDropdownItem parent, string name)
        {
            foreach (AdvancedDropdownItem item in parent.children)
            {
                if (item.name == name)
                {
                    return item;
                }
            }
            return null;
        }

        private static readonly float HeaderHeight = EditorGUIUtility.singleLineHeight * 2f;

        private Type[] types;
        private bool showFullNamespace;

        public event Action<AdvancedTypePopupItem> OnItemSelected;

        public AdvancedTypePopup (IEnumerable<Type> types, int maxLineCount, AdvancedDropdownState state, bool showFullNamespace = false) : base(state)
        {
            SetTypes(types);
            this.showFullNamespace = showFullNamespace;
            minimumSize = new Vector2(minimumSize.x, EditorGUIUtility.singleLineHeight * maxLineCount + HeaderHeight);
        }

        public void SetTypes (IEnumerable<Type> types)
        {
            this.types = types.ToArray();
        }

        protected override AdvancedDropdownItem BuildRoot ()
        {
            GUI.skin = EditorGUIUtility.GetBuiltinSkin(EditorGUIUtility.isProSkin ? EditorSkin.Scene : EditorSkin.Inspector);
            var root = new AdvancedDropdownItem("Select Type");
            AddTo(root, types, showFullNamespace);
            return root;
        }

        protected override void ItemSelected (AdvancedDropdownItem item)
        {
            base.ItemSelected(item);
            if (item is AdvancedTypePopupItem typePopupItem)
            {
                OnItemSelected?.Invoke(typePopupItem);
            }
        }

    }
}