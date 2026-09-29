using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Toolbars;
using UnityEngine;

namespace Formation.Toolbar
{
    public class TimeScaleMainToolbar
    {
        private const float minTimeScale = 0f;
        private const float maxTimeScale = 5f;
        private const string toolbarId = "Personal/TimeScale";


        [MainToolbarElement(toolbarId, defaultDockPosition = MainToolbarDockPosition.Middle)]
        public static IEnumerable<MainToolbarElement> GetTimeScale()
        {
            List<MainToolbarElement> toolbar = new List<MainToolbarElement>();


            // TimeScale Slider
            var content1 = new MainToolbarContent("Time Scale", "Time Scale");
            var slider = new MainToolbarSlider(content1, Time.timeScale, minTimeScale, maxTimeScale, OnSliderValueChanged);

            slider.populateContextMenu = (menu) =>
            {
                menu.AppendAction("Reset", _ =>
                {
                    Time.timeScale = 1f;
                    MainToolbar.Refresh(toolbarId);
                });
            };

            EditorElementStyler.StyleElement<UnityEditor.Toolbars.EditorToolbarSlider>("Personal/TimeScale", (element) => element.style.marginLeft = 10f);


            // Reset button
            var icon = EditorGUIUtility.IconContent("Refresh").image as Texture2D;
            var content2 = new MainToolbarContent(icon, "Reset");
            var button = new MainToolbarButton(content2, () =>
            {
                Time.timeScale = 1f;
                MainToolbar.Refresh(toolbarId);
            });

            toolbar.Add(slider);
            toolbar.Add(button);

            return toolbar;
        }

        static void OnSliderValueChanged(float newValue)
        {
            Time.timeScale = newValue;
        }
    }
}
