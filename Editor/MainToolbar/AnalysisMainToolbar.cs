using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Toolbars;
using UnityEngine;
using UnityEngine.UIElements;


namespace Formation.Toolbar
{
    public class AnalysisMainToolbar
    {
        const string toolbarName = "Personal/Analysis";
        static readonly (string, string, string)[] s_Elements =
            new (string icon, string tooptip, string menuPath)[]
            {
                ("Assets/Tools/Icons/Debug_audit.svg"       , "Project Auditor"     ,"Window/Analysis/Project Auditor"),
                ("Assets/Tools/Icons/Debug_profiler.svg"    , "Profiler"            ,"Window/Analysis/Profiler"),
                ("Assets/Tools/Icons/Debug_memory.svg"      , "Memory Profiler"     ,"Window/Analysis/Memory Profiler"),
                ("Assets/Tools/Icons/Debug_frame.svg"       , "Frame Debugger"      ,"Window/Analysis/Frame Debugger"),
                ("Assets/Tools/Icons/Debug_rendering.svg"   , "Rendering Debugger"  ,"Window/Analysis/Rendering Debugger"),
                ("Assets/Tools/Icons/Debug_RGV.svg"         , "Render Graph Viewer" ,"Window/Analysis/Render Graph Viewer"),
                ("Assets/Tools/Icons/Debug_register.svg"    , "ArmOC Bridge"        ,"Tools/Arm Offline Compiler Bridge")
                //("Assets/Tools/Icons/Debug_graphic.svg", "Open the Profiler window","Window/Analysis/Profiler"),
            };

    #pragma warning disable UDR0001 // Domain Reload Analyzer
        static bool s_DisplayAsButtons = true;
    #pragma warning restore UDR0001 // Domain Reload Analyzer


        [MainToolbarElement(toolbarName, defaultDockPosition = MainToolbarDockPosition.Left)]
        static IEnumerable<MainToolbarElement> CreateAnalysisWindowsBar()
        {
            if (s_DisplayAsButtons)
            {
                foreach (var element in s_Elements)
                {
                    Texture2D icon = AssetDatabase.LoadAssetAtPath<Texture2D>(element.Item1);
                    yield return new MainToolbarButton(new MainToolbarContent(icon, element.Item2), () => EditorApplication.ExecuteMenuItem(element.Item3))
                    {
                        populateContextMenu = PopulateContextMenu,
                    };


                    EditorElementStyler.StyleElement<UnityEditor.Toolbars.EditorToolbarButton>(toolbarName, element =>
                    {
                        element.style.backgroundColor = new Color(0.235f, 0.235f, 0.235f);

                        element.RegisterCallback<MouseEnterEvent>(evt => element.style.backgroundColor = new Color(0.4f, 0.4f, 0.4f));
                        element.RegisterCallback<MouseLeaveEvent>(evt => element.style.backgroundColor = new Color(0.235f, 0.235f, 0.235f));

                        element.style.marginLeft = 1f;
                        element.style.marginRight = 1f;

                        element.style.borderTopLeftRadius = 2;
                        element.style.borderTopRightRadius = 2;
                        element.style.borderBottomLeftRadius = 2;
                        element.style.borderBottomRightRadius = 2;

                        if (element.ClassListContains("unity-editor-toolbar__button-strip-element--left"))
                        {
                            //element.style.marginLeft = 10f;
                            element.style.borderTopLeftRadius = 4;
                            element.style.borderBottomLeftRadius = 4;
                        }

                        if (element.ClassListContains("unity-editor-toolbar__button-strip-element--right"))
                        {
                            element.style.borderTopRightRadius = 4;
                            element.style.borderBottomRightRadius = 4;
                        }

                        var image = element.Q<Image>();
                        if (image != null)
                        {
                            image.style.width = 14f;
                            image.style.height = 14f;
                        }
                    });
                }
            }
            else
            {
                yield return new MainToolbarDropdown(
                    new MainToolbarContent("Analysis", "Open the list of available analysis windows"),
                    ShowDropdownMenu)
                {
                    populateContextMenu = PopulateContextMenu,
                };
            }
        }

        static void PopulateContextMenu(DropdownMenu menu)
        {
            menu.AppendAction(
                L10n.Tr(s_DisplayAsButtons ? "Display as Dropdown" : "Display as Buttons"),
                UpdateDisplayType);
        }

        static void UpdateDisplayType(DropdownMenuAction _)
        {
            s_DisplayAsButtons = !s_DisplayAsButtons;
            MainToolbar.Refresh(toolbarName);
        }

        static void ShowDropdownMenu(Rect dropDownRect)
        {
            var menu = new GenericMenu();
            foreach (var element in s_Elements)
            {
                menu.AddItem(new GUIContent(element.Item2), false, () =>
                {
                    EditorApplication.ExecuteMenuItem(element.Item3);
                });
            }
            menu.DropDown(dropDownRect);
        }
    }
}
