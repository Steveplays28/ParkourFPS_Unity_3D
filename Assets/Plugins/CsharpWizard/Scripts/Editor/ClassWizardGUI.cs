using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEngine;

namespace Pinwheel.CsharpWizard
{
    public partial class ClassWizard : EditorWindow
    {
        private static Vector2 minimumSize = new Vector2(400, 350); //to keep its layout

        //these AnimatedValue is used to fade in/out editor section based on the current settings
        private static AnimFloat showClassNameError;
        private static AnimFloat showBaseClassError;
        private static AnimFloat showBaseClass;
        private static AnimFloat showBaseClassSuggestion;
        private static AnimFloat showNamespaceSuggestion;
        private static AnimFloat showInterfaces;
        private static AnimFloat showInterfaceSuggestion;
        private static AnimFloat showInterfaceError;
        private static AnimFloat showStaticAbstractSealed;
        private static AnimFloat showMenuPath;

        //a flag to indicate that the editor should repaint when the find suggestion job is completed in the second thread
        private static bool forceRepaint;

        //editor minor stuffs
        private const string namespaceControlName = "txtNamespace";
        private static Rect baseClassRect;
        private const string baseClassControlName = "txtBaseClass";
        private const string interfaceControlName = "txtInterfaces";
        private static Rect interfaceRect;
        private static Vector2 baseClassSuggestionScrollPos;
        private static Vector2 namespaceSuggestionScrollPos;
        private static Vector2 interfaceSuggestionScrollPos;
        private static int suggestionScrollViewVisibleCapacity = 5; //the maximum visible entries in suggestion scrollview
        private static int maximumSuggestion = 100; //limit the suggestion to show

        private static int accessModifierSelectedValue = 0; //0 is public, 1 is internal
        private static int staticAbstractSealedSelectedValue = 0; //0 is none, 1 is static, 2 is abstract, 3 is sealed

        [MenuItem("Assets/Create/C# Script with Wizard", priority = 80)]
        public static void ShowWindow()
        {
            ClassWizard w = GetWindow<ClassWizard>();
            w.minSize = minimumSize;
            w.titleContent = new GUIContent("CSharp Wizard");
            w.Show();
        }

        private void OnEnable()
        {
            onNamespaceNameChanged += OnNamespaceNameChanged;
            onClassNameChanged += OnClassNameChanged;
            onBaseClassNameChanged += OnBaseClassNameChanged;
            onInterfaceListChanged += OnInterfaceListChanged;

            showClassNameError = new AnimFloat(0);
            showClassNameError.valueChanged.AddListener(Repaint);
            showBaseClassError = new AnimFloat(0);
            showBaseClassError.valueChanged.AddListener(Repaint);
            showBaseClass = new AnimFloat(1);
            showBaseClass.valueChanged.AddListener(Repaint);
            showBaseClassSuggestion = new AnimFloat(0);
            showBaseClassSuggestion.valueChanged.AddListener(Repaint);
            showNamespaceSuggestion = new AnimFloat(0);
            showNamespaceSuggestion.valueChanged.AddListener(Repaint);
            baseClassSuggestion = new List<Type>();
            showInterfaceSuggestion = new AnimFloat(0);
            showInterfaceSuggestion.valueChanged.AddListener(Repaint);
            showInterfaceError = new AnimFloat(0);
            showInterfaceError.valueChanged.AddListener(Repaint);
            showInterfaces = new AnimFloat(0);
            showInterfaces.valueChanged.AddListener(Repaint);
            implementedInterfaces = new List<Type>();
            interfaceSuggesstion = new List<Type>();
            showStaticAbstractSealed = new AnimFloat(0);
            showStaticAbstractSealed.valueChanged.AddListener(Repaint);
            showMenuPath = new AnimFloat(0);
            showMenuPath.valueChanged.AddListener(Repaint);
            GetCurrentlySelectedFolder();
            InitTypes();
            LoadSettings();
            ClassName = string.Empty;

            wantsMouseMove = true; //give it a nicer look when hovering over suggestion entries
        }

        private void OnDisable()
        {
            onNamespaceNameChanged -= OnNamespaceNameChanged;
            onClassNameChanged -= OnClassNameChanged;
            onBaseClassNameChanged -= OnBaseClassNameChanged;
            onInterfaceListChanged -= OnInterfaceListChanged;
        }

        private void OnDestroy()
        {
            SaveSettings();
            scriptType = ScriptType.Class;
            onNamespaceNameChanged = delegate { };
            onClassNameChanged = delegate { };
            onBaseClassNameChanged = delegate { };
            onInterfaceListChanged = delegate { };
            NamespaceName = string.Empty;
            ClassName = string.Empty;
            BaseClassName = string.Empty;
            baseClass = null;
            InterfaceList = string.Empty;
            isInternal = false;
            isAbstract = false;
            isSealed = false;
            isStatic = false;
        }

        private void OnNamespaceNameChanged(string newName, string oldName)
        {
            //Search for namespace suggestion base on newName
            new Thread(() =>
            {
                namespaceSuggestion = SearchNamespace(newName);
                forceRepaint = true;
            }).Start();
        }

        private void OnClassNameChanged(string newName, string oldName)
        {
            //check for class name error
            showClassNameError.target = IsTypeExists(newName) ? 1 : 0;
        }

        private void OnBaseClassNameChanged(string newName, string oldName)
        {
            //check for base class error
            showBaseClassError.target =
                (!string.IsNullOrEmpty(newName) && !IsClassDerivable(newName, ref baseClassNameError)) ? 1 : 0;
            //search for base class suggestion based on newName
            new Thread(() =>
            {
                baseClassSuggestion = SearchType(newName);
                forceRepaint = true;
            }).Start();
        }

        private void OnInterfaceListChanged(string newName, string oldName)
        {
            //validate the interface list by remove interfaces that are not listed and duplicated entries
            implementedInterfaces = implementedInterfaces.Where(i => InterfaceList.Contains(i.Name)).Distinct().ToList();

            //also check for error
            string[] s = InterfaceList.Replace(" ", "").Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
            showInterfaceError.target =
                IsInterfacesExist(s, ref interfaceListError) ? 0 : 1;
            new Thread(() =>
            {
                try
                {
                    //search for suggestion based on the last listed interface name
                    if (s.HasElement())
                    {
                        string suggest = s[s.Length - 1];
                        interfaceSuggesstion = SearchInterfaces(suggest);
                        forceRepaint = true;
                    }
                }
                catch (Exception e)
                {
                    Debug.Log(e.ToString());
                }
            }).Start();
        }

        private void Update()
        {
            //we must constantly check for this flag because Repaint can only be call in main thread
            if (forceRepaint)
            {
                try
                {
                    Repaint();
                }
                catch { }
                forceRepaint = false;
            }
        }

        private void OnGUI()
        {
            DrawNamespace();
            DrawClassName();
            DrawBaseClass();
            DrawInterfaces();
            DrawCompilationDirective();
            DrawMenuPath();
            DrawScriptType();
            DrawAccessModifier();
            DrawStaticAbstractSealedModifier();
            DrawFormatting();
            EditorCommon.BrowseFolder("Directory", ref directory);
            if (EditorCommon.RightAnchoredButton("Create"))
            {
                string path = Generate();
                if (!string.IsNullOrEmpty(path))
                {
                    Close();
                    AssetDatabase.Refresh(); //import the file
                    Selection.activeObject = AssetDatabase.LoadAssetAtPath<MonoScript>(path); //select the newly created file
                }
            }

            DrawLeaveFeedback();

            if (Event.current.type == EventType.MouseDown)
            {
                GUI.FocusControl(string.Empty); //to hide the suggestion scroll view
            }
            if (Event.current.type == EventType.MouseMove)
            {
                Repaint();
            }
        }

        private void DrawNamespace()
        {
            GUI.SetNextControlName(namespaceControlName);
            NamespaceName = EditorGUILayout.TextField("Namespace", NamespaceName).Replace(" ", string.Empty);
            TryDrawNamespaceSuggestion();
        }

        private void DrawClassName()
        {
            ClassName = EditorGUILayout.TextField(string.Format("{0} name", ObjectNames.NicifyVariableName(scriptType.ToString())), ClassName).Replace(" ", string.Empty);

            EditorGUILayout.BeginFadeGroup(showClassNameError.value);
            if (showClassNameError.value != 0)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel(" ");
                GUIContent warningContent = new GUIContent("Type existed!", EditorCommon.WarningIcon);
                GUILayout.Label(warningContent);
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndFadeGroup();
        }

        private void DrawBaseClass()
        {
            showBaseClass.target = scriptType == ScriptType.Class ? 1 : 0;

            EditorGUILayout.BeginFadeGroup(showBaseClass.value);
            if (showBaseClass.value != 0)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel("Base class");
                GUI.SetNextControlName(baseClassControlName);
                baseClassRect = EditorGUILayout.GetControlRect(false);
                BaseClassName = EditorGUI.TextField(baseClassRect, BaseClassName).Replace(" ", string.Empty);
                EditorGUILayout.EndHorizontal();

                if (!TryDrawBaseClassSuggestion()) //show error warning if suggestion scrollview is hidden
                {
                    EditorGUILayout.BeginFadeGroup(showBaseClassError.value);
                    if (showBaseClassError.value != 0)
                    {
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.PrefixLabel(" ");
                        GUIContent warningContent = new GUIContent(baseClassNameError, EditorCommon.WarningIcon);
                        GUILayout.Label(warningContent);
                        EditorGUILayout.EndHorizontal();
                    }
                    EditorGUILayout.EndFadeGroup();
                }
            }
            EditorGUILayout.EndFadeGroup();
        }

        /// <summary>
        /// Show a list of suggestion for namespace
        /// </summary>
        /// <returns>true is the scrollview is shown, false if it is completely hidden</returns>
        private bool TryDrawNamespaceSuggestion()
        {
            if (!GUI.GetNameOfFocusedControl().Equals(namespaceControlName) || //only show when focus on the namespace textbox
                string.IsNullOrEmpty(NamespaceName) ||
                namespaceSuggestion == null ||
                namespaceSuggestion.Count == 0)
            {
                showNamespaceSuggestion.target = 0;
            }
            else
            {
                showNamespaceSuggestion.target = 1;
            }

            if (showNamespaceSuggestion.value != 0)
            {
                EditorGUILayout.BeginFadeGroup(showNamespaceSuggestion.value);
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel(" ");

                int lim = Mathf.Min(maximumSuggestion, namespaceSuggestion.Count);
                namespaceSuggestionScrollPos = EditorGUILayout.BeginScrollView(namespaceSuggestionScrollPos, GUILayout.Height(EditorCommon.standardHeight * suggestionScrollViewVisibleCapacity));
                try
                {
                    for (int i = 0; i < lim; ++i)
                    {
                        try
                        {
                            GUIStyle style =
                                namespaceSuggestion[i].Equals(NamespaceName) ? EditorCommon.SelectedFlatButton :
                                i % 2 == 0 ? EditorCommon.EvenFlatButton : EditorCommon.OddFlatButton;
                            if (GUILayout.Button(string.Empty, style, GUILayout.Height(EditorCommon.standardHeight)))
                            {
                                //select the namespace when user click on it, then hide the scrollview
                                NamespaceName = namespaceSuggestion[i];
                                GUI.FocusControl(string.Empty);
                            }
                            Rect r = GUILayoutUtility.GetLastRect();
                            GUI.Label(new RectOffset(3, 0, 0, 0).Remove(r), namespaceSuggestion[i]);
                        }
                        catch
                        {
                            continue;
                        }
                    }
                    if (lim < namespaceSuggestion.Count)
                    {
                        EditorGUILayout.LabelField("..."); //show an ellipsis if there are so many suggestions
                    }
                }
                catch { }
                EditorGUILayout.EndScrollView();
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndFadeGroup();
            }

            return showNamespaceSuggestion.value != 0;
        }

        /// <summary>
        /// Show a list of suggestion for base class
        /// </summary>
        /// <returns>true is the scrollview is shown, false if it is completely hidden</returns>
        private bool TryDrawBaseClassSuggestion()
        {
            if (!GUI.GetNameOfFocusedControl().Equals(baseClassControlName) //only show when focus on the base class textbox
                || string.IsNullOrEmpty(BaseClassName) ||
                baseClassSuggestion == null ||
                baseClassSuggestion.Count == 0)
            {
                showBaseClassSuggestion.target = 0;
            }
            else
            {
                showBaseClassSuggestion.target = 1;
            }

            if (showBaseClassSuggestion.value != 0)
            {
                EditorGUILayout.BeginFadeGroup(showBaseClassSuggestion.value);
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel(" ");

                int lim = Mathf.Min(maximumSuggestion, baseClassSuggestion.Count);
                baseClassSuggestionScrollPos = EditorGUILayout.BeginScrollView(baseClassSuggestionScrollPos, GUILayout.Height(EditorCommon.standardHeight * suggestionScrollViewVisibleCapacity));
                try
                {
                    for (int i = 0; i < lim; ++i)
                    {
                        try
                        {
                            GUIStyle style =
                                baseClassSuggestion[i].Equals(baseClass) ? EditorCommon.SelectedFlatButton :
                                i % 2 == 0 ? EditorCommon.EvenFlatButton : EditorCommon.OddFlatButton;
                            if (GUILayout.Button(string.Empty, style, GUILayout.Height(EditorCommon.standardHeight)))
                            {
                                //select the base class when user click on it, then hide the scrollview
                                BaseClassName = baseClassSuggestion[i].Name;
                                baseClass = baseClassSuggestion[i];
                                GUI.FocusControl(string.Empty);
                            }
                            //the base class name and its namespace is draw on top of the latest button
                            Rect r = GUILayoutUtility.GetLastRect();
                            GUI.Label(new RectOffset(3, 0, 0, 0).Remove(r), baseClassSuggestion[i].Name);
                            GUI.Label(new RectOffset(0, 3, 0, 0).Remove(r), baseClassSuggestion[i].Namespace, EditorCommon.RightAlignedItalicLabel);
                        }
                        catch
                        {
                            continue;
                        }
                    }
                    if (lim < baseClassSuggestion.Count)
                    {
                        EditorGUILayout.LabelField("..."); //show an ellipsis if there are so many suggestions
                    }
                }
                catch { }
                EditorGUILayout.EndScrollView();
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndFadeGroup();
            }

            return showBaseClassSuggestion.value != 0;
        }

        private void DrawInterfaces()
        {
            bool willShow = scriptType == ScriptType.Class || scriptType == ScriptType.Struct || scriptType == ScriptType.Interface;
            showInterfaces.target = willShow ? 1 : 0;

            EditorGUILayout.BeginFadeGroup(showInterfaces.value);
            if (showInterfaces.value != 0)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel("Interfaces");
                GUI.SetNextControlName(interfaceControlName);
                interfaceRect = EditorGUILayout.GetControlRect(false);
                InterfaceList = EditorGUI.TextField(interfaceRect, InterfaceList);
                EditorGUILayout.EndHorizontal();
                if (!TryDrawInterfaceSuggestion())
                {
                    //show interface error warning if the suggestion scrollview is hidden
                    EditorGUILayout.BeginFadeGroup(showInterfaceError.value);
                    if (showInterfaceError.value != 0)
                    {
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.PrefixLabel(" ");
                        GUIContent warningContent = new GUIContent(interfaceListError, EditorCommon.WarningIcon);
                        GUILayout.Label(warningContent);
                        EditorGUILayout.EndHorizontal();
                    }
                    EditorGUILayout.EndFadeGroup();
                }
            }
            EditorGUILayout.EndFadeGroup();
        }

        /// <summary>
        /// Show a list of suggestion for interfaces
        /// </summary>
        /// <returns>true is the scrollview is shown, false if it is completely hidden</returns>
        private bool TryDrawInterfaceSuggestion()
        {
            if (!GUI.GetNameOfFocusedControl().Equals(interfaceControlName) //only show when focus on interfaces textbox
                || string.IsNullOrEmpty(InterfaceList) ||
                interfaceSuggesstion == null ||
                interfaceSuggesstion.Count == 0)
            {
                showInterfaceSuggestion.target = 0;
            }
            else
            {
                showInterfaceSuggestion.target = 1;
            }

            if (showInterfaceSuggestion.value != 0)
            {
                EditorGUILayout.BeginFadeGroup(showInterfaceSuggestion.value);
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel(" ");

                int lim = Mathf.Min(maximumSuggestion, interfaceSuggesstion.Count);
                interfaceSuggestionScrollPos = EditorGUILayout.BeginScrollView(interfaceSuggestionScrollPos, GUILayout.Height(EditorCommon.standardHeight * suggestionScrollViewVisibleCapacity));
                try
                {
                    for (int i = 0; i < lim; ++i)
                    {
                        try
                        {
                            GUIStyle style =
                                implementedInterfaces.Contains(interfaceSuggesstion[i]) ? EditorCommon.SelectedFlatButton :
                                i % 2 == 0 ? EditorCommon.EvenFlatButton : EditorCommon.OddFlatButton;
                            if (GUILayout.Button(string.Empty, style, GUILayout.Height(EditorCommon.standardHeight)))
                            {
                                //append it into the selected interfaces, format the string and hide the scrollview
                                implementedInterfaces.Add(interfaceSuggesstion[i]);
                                implementedInterfaces.Distinct();
                                InterfaceList = implementedInterfaces.Select(x => x.Name).ListElementsToString(", ");
                                GUI.FocusControl(string.Empty);
                            }
                            Rect r = GUILayoutUtility.GetLastRect();
                            GUI.Label(new RectOffset(3, 0, 0, 0).Remove(r), interfaceSuggesstion[i].Name);
                            GUI.Label(new RectOffset(0, 3, 0, 0).Remove(r), interfaceSuggesstion[i].Namespace, EditorCommon.RightAlignedItalicLabel);
                        }
                        catch
                        {
                            continue;
                        }
                    }
                    if (lim < interfaceSuggesstion.Count)
                    {
                        EditorGUILayout.LabelField("..."); //show an ellipsis if there are so many suggestions
                    }
                }
                catch { }
                EditorGUILayout.EndScrollView();
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndFadeGroup();
            }

            return showInterfaceSuggestion.value != 0;
        }

        private void DrawCompilationDirective()
        {
            compilationDirective = EditorGUILayout.TextField("Directive (#if)", compilationDirective);
        }

        private void DrawMenuPath()
        {
            bool willShow = scriptType == ScriptType.Window || scriptType == ScriptType.ScriptableObject;
            showMenuPath.target = willShow ? 1 : 0;

            EditorGUILayout.BeginFadeGroup(showMenuPath.value);
            if (showMenuPath.value != 0)
            {
                menuPath = EditorGUILayout.TextField("Menu Path", menuPath);
            }
            EditorGUILayout.EndFadeGroup();
        }

        private void DrawScriptType()
        {
            scriptType = (ScriptType)EditorGUILayout.EnumPopup("Type", scriptType);
        }

        private void DrawAccessModifier()
        {
            accessModifierSelectedValue = EditorCommon.ToggleGroup(
                "Visibility",
                accessModifierSelectedValue,
                EditorCommon.standardWidth,
                "Public",
                "Internal");
            isInternal = accessModifierSelectedValue == 1;
        }

        private void DrawStaticAbstractSealedModifier()
        {
            showStaticAbstractSealed.target = scriptType == ScriptType.Class ? 1 : 0;

            if (showStaticAbstractSealed.value > 0)
            {
                EditorGUILayout.BeginFadeGroup(showStaticAbstractSealed.value);
                staticAbstractSealedSelectedValue = EditorCommon.ToggleGroup(
                    "Modifiers",
                    staticAbstractSealedSelectedValue,
                    EditorCommon.standardWidth,
                    "None",
                    "Static",
                    "Abstract",
                    "Sealed");
                isStatic = staticAbstractSealedSelectedValue == 1;
                isAbstract = staticAbstractSealedSelectedValue == 2;
                isSealed = staticAbstractSealedSelectedValue == 3;
                EditorGUILayout.EndFadeGroup();
            }
        }

        private void DrawLeaveFeedback()
        {
            float offset = 3;
            Rect r = new Rect(
                position.width - EditorCommon.standardWidth - offset,
                position.height - EditorCommon.standardHeight - offset,
                EditorCommon.standardWidth,
                EditorCommon.standardHeight);
            if (EditorCommon.LinkButton(r, "Leave feedback..."))
            {
                LeaveFeedback();
            }
        }

        private void LeaveFeedback()
        {
            //open the built-in mail app
            Utilities.MailTo(contactEmail, "CSharpWizard feedback", "");
        }

        private void DrawFormatting()
        {
            int formatOptionsCount = 3;
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Code formatting");
            Rect r = EditorGUILayout.GetControlRect(false, EditorCommon.standardHeight * formatOptionsCount);
            Rect toggleRect = new Rect(r.x, r.y, r.width, EditorCommon.standardHeight);

            int i = 0;
            toggleRect.position = r.position + new Vector2(0, i * EditorCommon.standardHeight);
            addSpaceAfterUsings = EditorGUI.ToggleLeft(toggleRect, "Add space after Usings", addSpaceAfterUsings);

            i += 1;
            toggleRect.position = r.position + new Vector2(0, i * EditorCommon.standardHeight);
            addSpaceBetweenMethods = EditorGUI.ToggleLeft(toggleRect, "Add space between methods", addSpaceBetweenMethods);

            i += 1;
            toggleRect.position = r.position + new Vector2(0, i * EditorCommon.standardHeight);
            openCurlyBracketOnNewLine = EditorGUI.ToggleLeft(toggleRect, "Open curly brackets on new line", openCurlyBracketOnNewLine);

            EditorGUILayout.EndHorizontal();
        }
    }
}