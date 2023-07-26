using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.LiveCapture.Editor
{
    static class MenuUtility
    {
        public static void CreateMenu<TMember, TAttribute>(
            IEnumerable<(TMember, TAttribute[])> attributes,
            Action<TMember, TAttribute> addItem,
            Action<string> addSeparator
        )
            where TAttribute : MenuPathAttribute
        {
            var subMenuMinPriorities = new Dictionary<string, int>();
            var subMenuMaxPriorities = new Dictionary<string, int>();

            foreach (var (member, attribute) in attributes
                     .Select(tuple => (tuple.Item1, tuple.Item2.First()))
                     .OrderBy(tuple => tuple.Item2.Priority))
            {
                // add separators based on the priority, matching how the MenuItem attribute works
                var subMenu = attribute.ItemName.Substring(0, Mathf.Max(0, attribute.ItemName.LastIndexOf('/')));

                if (!subMenuMinPriorities.ContainsKey(subMenu))
                {
                    // separate new submenus from the preceding submenu
                    if (subMenuMinPriorities.Count > 0)
                    {
                        var preceding = subMenuMinPriorities.Select(x => (x.Value, x)).Max().x;

                        if (attribute.Priority - preceding.Value > 10)
                        {
                            var sharedCharCount = subMenu.Zip(preceding.Key, (c1, c2) => c1 == c2).TakeWhile(b => b).Count();
                            var sharedPath = subMenu.Substring(0, sharedCharCount) + "/";
                            addSeparator(sharedPath);
                        }
                    }

                    subMenuMinPriorities[subMenu] = attribute.Priority;
                }
                else if (attribute.Priority - subMenuMaxPriorities[subMenu] > 10)
                {
                    // separate items in the same submenu
                    addSeparator(subMenu == string.Empty ? string.Empty : subMenu + "/");
                }

                subMenuMaxPriorities[subMenu] = attribute.Priority;

                // add the item that creates the device
                addItem(member, attribute);
            }
        }

        public static GenericMenu CreateMenu<TMember, TAttribute>(
            IEnumerable<(TMember, TAttribute[])> attributes,
            Func<TMember, bool> isEnabled,
            Action<TMember, TAttribute> menuFunction
        )
            where TAttribute : MenuPathAttribute
        {
            var menu = new GenericMenu();

            Action<TMember, TAttribute> addMenuItem = (member, attribute) =>
            {
                var item = new GUIContent(attribute.ItemName);
                if (isEnabled(member))
                {
                    menu.AddItem(item, false, () =>
                    {
                        menuFunction?.Invoke(member, attribute);
                    });
                }
                else
                {
                    menu.AddDisabledItem(item);
                }
            };

            CreateMenu(attributes, addMenuItem, menu.AddSeparator);

            return menu;
        }

        public static void SetupMenu<TMember, TAttribute>(
            DropdownMenu menu,
            IEnumerable<(TMember, TAttribute[])> attributes,
            Func<TMember, bool> isEnabled,
            Action<TMember, TAttribute> menuFunction
        )
            where TAttribute : MenuPathAttribute
        {
            Action<TMember, TAttribute> addMenuItem = (member, attribute) =>
            {
                menu.AppendAction(
                    attribute.ItemName,
                    action => menuFunction?.Invoke(member, attribute),
                    action => isEnabled(member) ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled
                );
            };

            CreateMenu(attributes, addMenuItem, menu.AppendSeparator);
        }
    }
}
