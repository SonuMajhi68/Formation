using Formation;
using Formation.ArmOCBridge;
using UnityEditor;
using UnityEditor.Toolbars;
using UnityEngine;
using UnityEngine.UIElements;

namespace Formation.Toolbar
{
    public class ArmOCBridgeMainToolbar
    {
        const string toolbarName = "Personal/ArmOC";

        [MainToolbarElement(toolbarName, defaultDockPosition = MainToolbarDockPosition.Right)]
        public static MainToolbarElement GetArmOCBridgeEditorWindow()
        {
            Texture2D customIcon = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Tools/Icons/Debug_register.svg");
            var button = new MainToolbarButton(new MainToolbarContent(customIcon, "ArmOC"), ArmOCBridgeEditor.OpenEditorWindow);

            EditorElementStyler.StyleElement<UnityEditor.Toolbars.EditorToolbarButton>(toolbarName, element =>
            {
                element.style.backgroundColor = new Color(0.235f, 0.235f, 0.235f);
                element.RegisterCallback<MouseEnterEvent>(evt => element.style.backgroundColor = new Color(0.4f, 0.4f, 0.4f));
                element.RegisterCallback<MouseLeaveEvent>(evt => element.style.backgroundColor = new Color(0.235f, 0.235f, 0.235f));

                var image = element.Q<Image>();
                if (image != null)
                {
                    image.style.width = 14f;
                    image.style.height = 14f;
                }
            });
            Debug.Log("");
            return button;
        }
    }
}




