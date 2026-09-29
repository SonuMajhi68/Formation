using UnityEditor;
using UnityEditor.Toolbars;
using UnityEngine;

namespace Formation.Toolbar
{
    public class FavoriteMainToolbar
    {
        [MainToolbarElement("Personal/Favorite", defaultDockPosition = MainToolbarDockPosition.Left)]
        public static MainToolbarElement GetProjectSettings()
        {
            var icon = EditorGUIUtility.IconContent("SettingsIcon").image as Texture2D;
            var content = new MainToolbarContent(icon, "Favorite Settings");

            return new MainToolbarDropdown(content, ShowDropdownMenu);
        }

        private static void ShowDropdownMenu(Rect rect)
        {
            GenericMenu menu = new GenericMenu();

            menu.AddItem(new GUIContent("Preferences"), false, () => SettingsService.OpenUserPreferences());
            menu.AddSeparator("");                                              // Optional separator line
            menu.AddItem(new GUIContent("Project Setting"), false, () => SettingsService.OpenProjectSettings());

            menu.DropDown(rect);
        }
    }
}
