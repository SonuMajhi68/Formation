using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Formation
{
    public class EditorElementStyler
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Domain reload", "UDR0005:Domain Reload Analyzer", Justification = "<Pending>")]
        public static void StyleElement<T>(string elementName, System.Action<T> styleAction) where T : VisualElement
        {
            EditorApplication.delayCall += () => {
                var container = FindElementByName(elementName);
                List<T> targetElement = container?.Query<T>().ToList();

                if (targetElement != null)
                {
                    foreach (var target in targetElement)
                    {
                        styleAction(target);
                    }
                }
            };
        }


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Domain reload", "UDR0005:Domain Reload Analyzer", Justification = "<Pending>")]
        public static void StyleElement<T>(string elementName, string editorWindow, System.Action<T> styleAction) where T : VisualElement
        {
            EditorApplication.delayCall += () => {
                var container = FindElementByName(elementName, editorWindow);
                T targetElement = container.Query<T>().First();

                if (targetElement != null)
                {
                    styleAction(targetElement);
                }
            };
        }


        static VisualElement FindElementByName(string name, string editorWindow = null)
        {
            var windows = Resources.FindObjectsOfTypeAll<EditorWindow>();
            EditorWindow[] targetWindow = windows;

            if (editorWindow != null)
            {
                foreach (var window in windows)
                {
                    if (window.titleContent.text.Contains(editorWindow))
                    {
                        targetWindow = null;
                        targetWindow = new EditorWindow[0];
                        targetWindow[0] = window;

                        break;
                    }
                }
            }

            foreach (var window in targetWindow)
            {
                var root = window.rootVisualElement;
                if (root == null) continue;

                VisualElement element;
                if ((element = root.FindElementByName(name)) != null) return element;
                if ((element = root.FindElementByTooltip(name)) != null) return element;
            }
            return null;
        }

    }

}