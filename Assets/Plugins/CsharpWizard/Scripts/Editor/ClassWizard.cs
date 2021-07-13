using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.AnimatedValues;
using System.IO;
using System.Linq;
using Pinwheel;
using System;
using System.Threading;
using System.Text;

namespace Pinwheel.CsharpWizard
{
    public partial class ClassWizard : EditorWindow
    {
        /// <summary>
        /// Indicate type of the script to be generated
        /// </summary>
        private enum ScriptType
        {
            Class, Struct, Interface, Enum, Inspector, InspectorDrawer, Window, ScriptableObject
        }

        private static string namespaceName;
        private static string className; //not only the name of the new class, but also the name of the new struct, interface or enum
        private static string baseClassName; //name of the class which new class will inherite from
        private static string baseClassNameError; //error when attempt to inherite from the base class, ex: base class is sealed, etc.
        private static string interfaceList; //list of interface names to implement, separated by comma
        private static string interfaceListError; //error when attempt to implement these interfaces
        private static ScriptType scriptType; //you want to generate a class, a struct, an interface or an enum?
        private static string directory; //where to save the new file
        private static bool isStatic; //is the new class static?
        private static bool isAbstract; //is the new class abstract?
        private static bool isSealed; //is the new class sealed?
        private static bool isInternal; //is the class marked as internal?

        private static string compilationDirective;
        private static string menuPath;

        private static bool addSpaceAfterUsings; //code formatting option
        private static bool addSpaceBetweenMethods; //code formatting option
        private static bool openCurlyBracketOnNewLine; //code formatting option

        private const string contactEmail = "pinwheel.customer@gmail.com"; //glad to hear from you :)

        //an event fired when you type something in the namespace textbox, used to find and give you some suggestion
        private static Action<string, string> onNamespaceNameChanged = delegate { };
        private static string NamespaceName
        {
            get
            {
                if (namespaceName == null)
                    namespaceName = string.Empty;
                return namespaceName;
            }
            set
            {
                string oldName = namespaceName;
                string newName = value;
                namespaceName = newName;
                if (!newName.Equals(oldName))
                    onNamespaceNameChanged(newName, oldName); //find namespace suggestion after this
            }
        }

        //an event fired when you type something in the class name textbox, used to check the new name for error like type existed
        private static Action<string, string> onClassNameChanged = delegate { };
        private static string ClassName
        {
            get
            {
                if (className == null)
                    className = string.Empty;
                return className;
            }
            set
            {
                string oldName = className;
                string newName = value;
                className = newName;
                if (!newName.Equals(oldName))
                    onClassNameChanged(newName, oldName); //check error after this
            }
        }

        //an event fired when you type something in the base class name textbox, used to find and give you some suggestion, as well as check for error
        private static Action<string, string> onBaseClassNameChanged = delegate { };
        private static string BaseClassName
        {
            get
            {
                if (baseClassName == null)
                    baseClassName = string.Empty;
                return baseClassName;
            }
            set
            {
                string oldName = baseClassName;
                string newName = value;
                baseClassName = newName;
                if (!newName.Equals(oldName))
                    onBaseClassNameChanged(newName, oldName); //check error and find suggestion after this
            }
        }
        //the actual base class to inherite from, set when you click on a base class suggestion, 
        //if not set, it will look for a class with name is what you typed in the base class name textbox
        private static Type baseClass;

        //an event fired when you type something in the interfaces textbox, used to find and give you some suggestion
        private static Action<string, string> onInterfaceListChanged = delegate { };
        //list of interface names to implement, separated by comma, it will be validated (remove non-existed, duplicated entries) in some situations
        private static string InterfaceList
        {
            get
            {
                if (interfaceList == null)
                    interfaceList = string.Empty;
                return interfaceList;
            }
            set
            {
                string oldName = interfaceList;
                string newName = value;
                interfaceList = newName;
                if (!newName.Equals(oldName))
                    onInterfaceListChanged(newName, oldName); //find suggestion after this
            }
        }
        //the actual list of interfaces to implement
        //an interface will be added when you click on a suggestion in the editor
        //this list will be validated in some situations
        //before generating, it will add interfaces listed in the InterfaceList string, also remove interfaces that are not listed by name
        //(in the case you click on an suggestion then remove it name from the textbox)
        private static List<Type> implementedInterfaces;
        //list of interface suggestions, re-generate when interface name changed
        private static List<Type> interfaceSuggesstion;
        //name of loaded types
        private static HashSet<string> loadedTypeNames;
        //list of loaded types
        private static List<Type> loadedTypes;
        //list of base class suggestions, re-generate when base class name changed
        private static List<Type> baseClassSuggestion;
        //list of namespace suggestions, re-generate when namespace name changed
        private static List<string> namespaceSuggestion;

        //get the path to the folder actively selected in Project window when you open the Wizard
        public static void GetCurrentlySelectedFolder()
        {
            UnityEngine.Object o = Selection.activeObject;
            if (o == null ||
                !AssetDatabase.IsValidFolder(AssetDatabase.GetAssetPath(o.GetInstanceID())))
            {
                directory = "Assets";
            }
            else
            {
                string assetPath = AssetDatabase.GetAssetPath(o.GetInstanceID());
                directory = assetPath;
            }
        }

        //get all loaded types, used to find suggestion, as well as check for error
        public static void InitTypes()
        {
            loadedTypes = new List<Type>();
            List<string> typeName = new List<string>();
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (var t in assembly.GetTypes())
                {
                    if (t.IsVisible && !t.IsGenericType)
                    {
                        typeName.Add(t.Name);
                        loadedTypes.Add(t);
                    }
                }
            }

            loadedTypeNames = new HashSet<string>(typeName);
        }

        //is it exist in the loaded types collection
        private static bool IsTypeExists(string typeName)
        {
            return loadedTypeNames != null && loadedTypeNames.Contains(typeName);
        }

        /// <summary>
        /// Check for class derivable as well as the specific error
        /// </summary>
        /// <param name="typeName">Name of the type to check</param>
        /// <param name="error">A string to carry the output error</param>
        /// <returns>Is it derivable or not?</returns>
        private static bool IsClassDerivable(string typeName, ref string error)
        {
            error = string.Empty;
            bool derivable = true;
            Type type = loadedTypes.Find((t) => { return t.Name.Equals(typeName) || t.FullName.Equals(typeName); });
            if (type == null)
            {
                derivable = false;
                error = "Class not found";
            }
            else
            {
                if (!type.IsClass)
                {
                    derivable = false;
                    error = string.Format("{0} is not a class", typeName);
                }
                else if (type.IsSealed)
                {
                    derivable = false;
                    error = "Class is sealed or static";
                }
            }

            return derivable;
        }

        /// <summary>
        /// Check for class derivable
        /// </summary>
        /// <param name="typeName">Name of the type to check</param>
        /// <returns>Is it derivable?</returns>
        private static bool IsClassDerivable(string typeName)
        {
            bool derivable = true;
            Type type = loadedTypes.Find((t) => { return t.Name.Equals(typeName) || t.FullName.Equals(typeName); });
            if (type == null)
            {
                derivable = false;
            }
            else
            {
                if (!type.IsClass)
                {
                    derivable = false;
                }
                else if (type.IsSealed)
                {
                    derivable = false;
                }
            }

            return derivable;
        }

        /// <summary>
        /// Check for class derivable
        /// </summary>
        /// <param name="type">Type to check</param>
        /// <returns>Is it derivable</returns>
        private static bool IsClassDerivable(Type type)
        {
            return type.IsClass && !type.IsSealed;
        }

        /// <summary>
        /// Check if a type is struct
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public static bool IsStruct(Type t)
        {
            return !t.IsClass && !t.IsEnum && !t.IsInterface;
        }

        /// <summary>
        /// Check if every interface listed in il is exist
        /// </summary>
        /// <param name="il">List of interface names to check</param>
        /// <param name="error">A string to carry the specific error</param>
        /// <returns>True of all interfaces exist, otherwise false</returns>
        public static bool IsInterfacesExist(string[] il, ref string error)
        {
            error = string.Empty;
            bool result = true;
            List<string> notFoundList = new List<string>();
            for (int i = 0; i < il.Length; ++i)
            {
                List<Type> match = loadedTypes.Where(t => t.Name.Equals(il[i]) && t.IsInterface).ToList();
                if (!match.HasElement())
                {
                    result = false;
                    notFoundList.Add(il[i]);
                }
            }

            if (!result)
            {
                StringBuilder b = new StringBuilder();
                b.Append("Interface(s) not found: ");
                b.Append(notFoundList.ListElementsToString(", "));
                error = b.ToString();
            }

            return result;
        }

        /// <summary>
        /// Validate base class and interface list
        /// If baseClass is set but its name does not match the name in base class name textbox, it will be set to null
        /// Add interfaces that are listed but not included, and remove duplicated entries
        /// </summary>
        public static void CompleteBaseClassAndInterfaceList()
        {
            if (baseClass != null && baseClass.Name != BaseClassName && baseClass.FullName != BaseClassName)
                baseClass = null;

            string[] s = InterfaceList.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < s.Length; ++i)
            {
                if (implementedInterfaces.Any(t => t.Name.Equals(s[i])))
                    continue;
                List<Type> possibleInterfaces = SearchInterfaces(s[i]);
                if (possibleInterfaces.HasElement())
                {
                    Type match = possibleInterfaces[0];
                    if (match != null)
                        implementedInterfaces.Add(match);
                }
            }
            implementedInterfaces = implementedInterfaces.Distinct().ToList();
        }

        /// <summary>
        /// Search for all type whose name matches a specific name, used to find suggestion
        /// The result will be sorted by namespace, with the following order: CurrentNamespace >> UnityEngine >> UnityEditor >> System >> Other
        /// </summary>
        /// <param name="typeName">Name to search</param>
        /// <returns></returns>
        public static List<Type> SearchType(string typeName)
        {
            try
            {
                List<Type> result = new List<Type>();
                List<Type> currentNamespaceTypes = new List<Type>();
                List<Type> unityEngineTypes = new List<Type>();
                List<Type> unityEditorTypes = new List<Type>();
                List<Type> systemTypes = new List<Type>();
                List<Type> otherTypes = new List<Type>();

                for (int i = 0; i < loadedTypes.Count; ++i)
                {
                    Type t = loadedTypes[i];
                    string n1 = t.Name.ToLower();
                    string n2 = typeName.ToLower();

                    if (n1.StartsWith(n2))
                    {
                        if (!string.IsNullOrEmpty(t.Namespace))
                        {
                            if (t.Namespace.StartsWith(NamespaceName))
                                currentNamespaceTypes.Add(t);
                            else if (t.Namespace.StartsWith("UnityEngine"))
                                unityEngineTypes.Add(t);
                            else if (t.Namespace.StartsWith("UnityEditor"))
                                unityEditorTypes.Add(t);
                            else if (t.Namespace.StartsWith("System"))
                                systemTypes.Add(t);
                            else
                                otherTypes.Add(t);
                        }
                        else
                        {
                            otherTypes.Add(t);
                        }
                    }
                    else if (n1.Contains(n2))
                    {
                        if (!string.IsNullOrEmpty(t.Namespace))
                        {
                            if (t.Namespace.StartsWith(NamespaceName))
                                currentNamespaceTypes.Add(t);
                            else if (t.Namespace.StartsWith("UnityEngine"))
                                unityEngineTypes.Add(t);
                            else if (t.Namespace.StartsWith("UnityEditor"))
                                unityEditorTypes.Add(t);
                            else if (t.Namespace.StartsWith("System"))
                                systemTypes.Add(t);
                            else
                                otherTypes.Add(t);
                        }
                        else
                        {
                            otherTypes.Add(t);
                        }
                    }
                }

                unityEngineTypes.Sort((t1, t2) => { return t1.Namespace.CompareTo(t2.Namespace); });
                unityEditorTypes.Sort((t1, t2) => { return t1.Namespace.CompareTo(t2.Namespace); });
                systemTypes.Sort((t1, t2) => { return t1.Namespace.CompareTo(t2.Namespace); });

                result.AddRange(currentNamespaceTypes);
                result.AddRange(unityEngineTypes);
                result.AddRange(unityEditorTypes);
                result.AddRange(systemTypes);
                result.AddRange(otherTypes);

                return result;
            }
            catch (Exception e)
            {
                Debug.Log(e.ToString());
                return new List<Type>();
            }
        }

        /// <summary>
        /// Searching for namespaces whose name matches a specific name
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        public static List<string> SearchNamespace(string n)
        {
            if (string.IsNullOrEmpty(n))
                return new List<string>();
            List<string> result = new List<string>();
            HashSet<string> tmp = new HashSet<string>();
            for (int i = 0; i < loadedTypes.Count; ++i)
            {
                if (string.IsNullOrEmpty(loadedTypes[i].Namespace))
                    continue;
                if (loadedTypes[i].Namespace.ToLower().StartsWith(n.ToLower()))
                    tmp.Add(loadedTypes[i].Namespace);
            }

            result = tmp.ToList();
            result.Sort((s1, s2) => { return s1.CompareTo(s2); });
            return result;
        }

        /// <summary>
        /// Similar to SearchType, but only search for interfaces
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static List<Type> SearchInterfaces(string s)
        {
            try
            {
                List<Type> result = new List<Type>();
                List<Type> currentNamespaceTypes = new List<Type>();
                List<Type> unityEngineTypes = new List<Type>();
                List<Type> unityEditorTypes = new List<Type>();
                List<Type> systemTypes = new List<Type>();
                List<Type> otherTypes = new List<Type>();

                for (int i = 0; i < loadedTypes.Count; ++i)
                {
                    Type t = loadedTypes[i];
                    if (!t.IsInterface)
                        continue;

                    string n1 = t.Name.ToLower();
                    string n2 = s.ToLower();

                    if (n1.StartsWith(n2))
                    {
                        if (!string.IsNullOrEmpty(t.Namespace))
                        {
                            if (t.Namespace.StartsWith(namespaceName))
                                currentNamespaceTypes.Add(t);
                            else if (t.Namespace.StartsWith("UnityEngine"))
                                unityEngineTypes.Add(t);
                            else if (t.Namespace.StartsWith("UnityEditor"))
                                unityEditorTypes.Add(t);
                            else if (t.Namespace.StartsWith("System"))
                                systemTypes.Add(t);
                            else
                                otherTypes.Add(t);
                        }
                        else
                        {
                            otherTypes.Add(t);
                        }
                    }
                    else if (n1.Contains(n2))
                    {
                        if (!string.IsNullOrEmpty(t.Namespace))
                        {
                            if (t.Namespace.StartsWith(namespaceName))
                                currentNamespaceTypes.Add(t);
                            else if (t.Namespace.StartsWith("UnityEngine"))
                                unityEngineTypes.Add(t);
                            else if (t.Namespace.StartsWith("UnityEditor"))
                                unityEditorTypes.Add(t);
                            else if (t.Namespace.StartsWith("System"))
                                systemTypes.Add(t);
                            else
                                otherTypes.Add(t);
                        }
                        else
                        {
                            otherTypes.Add(t);
                        }
                    }
                }

                unityEngineTypes.Sort((t1, t2) => { return t1.Namespace.CompareTo(t2.Namespace); });
                unityEditorTypes.Sort((t1, t2) => { return t1.Namespace.CompareTo(t2.Namespace); });
                systemTypes.Sort((t1, t2) => { return t1.Namespace.CompareTo(t2.Namespace); });

                result.AddRange(currentNamespaceTypes);
                result.AddRange(unityEngineTypes);
                result.AddRange(unityEditorTypes);
                result.AddRange(systemTypes);
                result.AddRange(otherTypes);

                return result;
            }
            catch (Exception e)
            {
                Debug.Log(e.ToString());
                return new List<Type>();
            }
        }

        /// <summary>
        /// Remove all redundant space in a line of code
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static string FormatLine(string s)
        {
            return s.Split(new string[] { " " }, System.StringSplitOptions.RemoveEmptyEntries)
                    .ListElementsToString(" ");
        }

        /// <summary>
        /// Add leading tab to a line of code
        /// </summary>
        /// <param name="s">Line to add</param>
        /// <param name="tabCount">Number of tabs to add</param>
        /// <returns>The tabbed line</returns>
        private static string TabString(string s, int tabCount)
        {
            StringBuilder b = new StringBuilder();
            for (int i = 0; i < tabCount; ++i)
            {
                b.Append("    ");
            }
            b.Append(s);
            return b.ToString();
        }

        /// <summary>
        /// Abracadabra !!!
        /// Turn key strokes and mouse clicks to beautiful code!
        /// </summary>
        /// <returns>Path of the file contains generated code</returns>
        public static string Generate()
        {
            bool willWrite = false;
            string pathSuffix =
                scriptType == ScriptType.Inspector ? "Inspector" :
                scriptType == ScriptType.InspectorDrawer ? "InspectorDrawer" :
                scriptType == ScriptType.Window ? "Window" : "";
            string path = Path.Combine(directory, string.Format("{0}{1}.cs", ClassName, pathSuffix));
            if (File.Exists(path))
            {
                if (EditorUtility.DisplayDialog(
                    "File existed",
                    "The file name is existed in the current directory, overwrite?",
                    "Yes", "No"))
                {
                    willWrite = true;
                }
            }
            else
            {
                willWrite = true;
            }
            if (!willWrite)
                return null;

            string[] text = null;
            if (scriptType == ScriptType.Class)
                text = GenerateClass();
            else if (scriptType == ScriptType.Struct)
                text = GenerateStruct();
            else if (scriptType == ScriptType.Interface)
                text = GenerateInterface();
            else if (scriptType == ScriptType.Enum)
                text = GenerateEnum();
            else if (scriptType == ScriptType.Inspector)
                text = GenerateInspector();
            else if (scriptType == ScriptType.InspectorDrawer)
                text = GenerateInspectorDrawer();
            else if (scriptType == ScriptType.Window)
                text = GenerateWindow();
            else if (scriptType == ScriptType.ScriptableObject)
                text = GenerateScriptableObject();

            File.WriteAllLines(path, text);
            return path;
        }

        /// <summary>
        /// Generate a class with the specified settings
        /// </summary>
        /// <returns>Code, broke into lines</returns>
        public static string[] GenerateClass()
        {
            CompleteBaseClassAndInterfaceList();
            Type actualBaseClass = GetBaseClass();
            //Declare it
            TypeDeclaration d = new TypeDeclaration.Builder()
                .SetAccessModifier(isInternal ? AccessModifier.INTERNAL : AccessModifier.PUBLIC)
                .SetIsStatic(isStatic)
                .SetInheritance(isAbstract ? Inheritance.ABSTRACT : isSealed ? Inheritance.SEALED : Inheritance.DEFAULT)
                .SetScope(Scope.CLASS)
                .SetName(ClassName)
                .SetBaseClass(actualBaseClass)
                .SetInterfaces(implementedInterfaces.ToArray())
                .Build();

            //Import regularly used namespaces, just call a Using(...) if you need more
            UsingNode un = new UsingNode()
                .Using("UnityEngine")
                .Using("System.Collections")
                .Using("System.Collections.Generic");

            //Import the base class namespace to simplify its name in the declaration string
            if (d.baseClass != null)
                un.Using(d.baseClass.Namespace);

            //The same for interfaces
            if (d.implementedInterface.HasElement())
            {
                for (int i = 0; i < d.implementedInterface.Count; ++i)
                {
                    un.Using(d.implementedInterface[i].Namespace);
                }
            }

            //Give it a hint to determine type names
            TypeAlias.SetUsingNode(un);

            List<string> code = new List<string>();
            if (actualBaseClass != null)
            {
                if (actualBaseClass.IsAssignableFrom(typeof(ScriptableObject)))
                {
                    code.Add(UnityMessage.RESET);
                }
                else if (actualBaseClass.IsAssignableFrom(typeof(MonoBehaviour)))
                {
                    code.Add(UnityMessage.RESET);
                    code.Add(UnityMessage.AWAKE);
                    code.Add(UnityMessage.START);
                    code.Add(UnityMessage.ON_ENABLE);
                    code.Add(UnityMessage.ON_DISABLE);
                    code.Add(UnityMessage.UPDATE);
                    code.Add(UnityMessage.ON_DESTROY);
                }
            }

            //Implement the type
            TypeNode tn = new TypeNode()
                .Declare(d)
                .Implement(code);

            //Put the new type into the namespace
            NamespaceNode nn = new NamespaceNode()
                .SetName(NamespaceName)
                .SetTypes(tn);

            //Put everything into a script
            ScriptNode sn = new ScriptNode()
                .SetCompilationDirective(compilationDirective)
                .SetUsing(un)
                .SetNamespace(nn)
                .SetTypes(tn); //also set type in case namespace is empty

            //Now make the code look awesome
            CodeFormatter formatter = new CodeFormatter();
            formatter.addSpaceAfterUsings = addSpaceAfterUsings;
            formatter.addSpaceBetweenMethods = addSpaceBetweenMethods;
            formatter.bracketStyle = openCurlyBracketOnNewLine ?
                CodeFormatter.OpenCurlyBracketStyle.NewLine :
                CodeFormatter.OpenCurlyBracketStyle.Inline;

            string[] script = formatter.Format(sn);

            return script; //phew...
        }

        /// <summary>
        /// Generate a struct with the specified setting
        /// The process is similar to GenerateClass, see it for details
        /// </summary>
        /// <returns></returns>
        public static string[] GenerateStruct()
        {
            CompleteBaseClassAndInterfaceList();
            TypeDeclaration d = new TypeDeclaration.Builder()
                .SetAccessModifier(isInternal ? AccessModifier.INTERNAL : AccessModifier.PUBLIC)
                .SetScope(Scope.STRUCT)
                .SetName(ClassName)
                .SetInterfaces(implementedInterfaces.ToArray())
                .Build();

            UsingNode un = new UsingNode()
                .Using("UnityEngine")
                .Using("System.Collections")
                .Using("System.Collections.Generic");

            if (d.implementedInterface.HasElement())
            {
                for (int i = 0; i < d.implementedInterface.Count; ++i)
                {
                    un.Using(d.implementedInterface[i].Namespace);
                }
            }

            TypeAlias.SetUsingNode(un);

            TypeNode tn = new TypeNode()
                .Declare(d)
                .Implement();

            NamespaceNode nn = new NamespaceNode()
                .SetName(NamespaceName)
                .SetTypes(tn);

            ScriptNode sn = new ScriptNode()
                .SetUsing(un)
                .SetNamespace(nn)
                .SetTypes(tn);

            CodeFormatter formatter = new CodeFormatter();
            formatter.addSpaceAfterUsings = addSpaceAfterUsings;
            formatter.addSpaceBetweenMethods = addSpaceBetweenMethods;
            formatter.bracketStyle = openCurlyBracketOnNewLine ?
                CodeFormatter.OpenCurlyBracketStyle.NewLine :
                CodeFormatter.OpenCurlyBracketStyle.Inline;

            string[] script = formatter.Format(sn);

            return script;
        }

        /// <summary>
        /// Generate an interface with the specified setting
        /// The process is similar to GenerateClass, see it for details
        /// </summary>
        /// <returns></returns>
        public static string[] GenerateInterface()
        {
            CompleteBaseClassAndInterfaceList();
            TypeDeclaration d = new TypeDeclaration.Builder()
                .SetAccessModifier(isInternal ? AccessModifier.INTERNAL : AccessModifier.PUBLIC)
                .SetScope(Scope.INTERFACE)
                .SetName(ClassName)
                .SetInterfaces(implementedInterfaces.ToArray())
                .Build();

            UsingNode un = new UsingNode()
                .Using("UnityEngine")
                .Using("System.Collections")
                .Using("System.Collections.Generic");

            if (d.implementedInterface.HasElement())
            {
                for (int i = 0; i < d.implementedInterface.Count; ++i)
                {
                    un.Using(d.implementedInterface[i].Namespace);
                }
            }

            TypeAlias.SetUsingNode(un);

            TypeNode tn = new TypeNode()
                .Declare(d);
            //.Implement(); --> no need to implement parent interfaces

            NamespaceNode nn = new NamespaceNode()
                .SetName(NamespaceName)
                .SetTypes(tn);

            ScriptNode sn = new ScriptNode()
                .SetUsing(un)
                .SetNamespace(nn)
                .SetTypes(tn);

            CodeFormatter formatter = new CodeFormatter();
            formatter.addSpaceAfterUsings = addSpaceAfterUsings;
            formatter.addSpaceBetweenMethods = addSpaceBetweenMethods;
            formatter.bracketStyle = openCurlyBracketOnNewLine ?
                CodeFormatter.OpenCurlyBracketStyle.NewLine :
                CodeFormatter.OpenCurlyBracketStyle.Inline;

            string[] script = formatter.Format(sn);

            return script;
        }

        /// <summary>
        /// Generate an enum with the specified setting
        /// The process is similar to GenerateClass, see it for details
        /// </summary>
        /// <returns></returns>
        public static string[] GenerateEnum()
        {
            TypeDeclaration d = new TypeDeclaration.Builder()
                .SetAccessModifier(isInternal ? AccessModifier.INTERNAL : AccessModifier.PUBLIC)
                .SetScope(Scope.ENUM)
                .SetName(ClassName)
                .Build();

            UsingNode un = new UsingNode()
                .Using("UnityEngine")
                .Using("System.Collections")
                .Using("System.Collections.Generic");

            TypeAlias.SetUsingNode(un);

            TypeNode tn = new TypeNode()
                .Declare(d)
                .Implement();

            NamespaceNode nn = new NamespaceNode()
                .SetName(NamespaceName)
                .SetTypes(tn);

            ScriptNode sn = new ScriptNode()
                .SetUsing(un)
                .SetNamespace(nn)
                .SetTypes(tn);

            CodeFormatter formatter = new CodeFormatter();
            formatter.addSpaceAfterUsings = addSpaceAfterUsings;
            formatter.addSpaceBetweenMethods = addSpaceBetweenMethods;
            formatter.bracketStyle = openCurlyBracketOnNewLine ?
                CodeFormatter.OpenCurlyBracketStyle.NewLine :
                CodeFormatter.OpenCurlyBracketStyle.Inline;

            string[] script = formatter.Format(sn);

            return script;
        }

        public static string[] GenerateInspector()
        {
            //Declare it
            string inspectorClassName = ClassName + "Inspector";
            string customEditorAttribute = string.Format("[CustomEditor(typeof({0}))]", ClassName);
            TypeDeclaration d = new TypeDeclaration.Builder()
                .SetAccessModifier(isInternal ? AccessModifier.INTERNAL : AccessModifier.PUBLIC)
                .SetIsStatic(false)
                .SetInheritance(Inheritance.DEFAULT)
                .SetScope(Scope.CLASS)
                .SetName(inspectorClassName)
                .SetBaseClass(typeof(Editor))
                .AddAttribute(customEditorAttribute)
                .Build();

            //Import regularly used namespaces, just call a Using(...) if you need more
            UsingNode un = new UsingNode()
                .Using("UnityEngine")
                .Using("System.Collections")
                .Using("System.Collections.Generic")
                .Using("UnityEditor");

            //Import the base class namespace to simplify its name in the declaration string
            if (d.baseClass != null)
                un.Using(d.baseClass.Namespace);

            //Give it a hint to determine type names
            TypeAlias.SetUsingNode(un);

            List<string> code = new List<string>();
            code.Add(string.Format("private {0} instance;", ClassName));
            code.Add(string.Format("private void OnEnable() {0} instance = target as {1}; {2}", Token.OPEN_CURLY, ClassName, Token.CLOSE_CURLY));
            code.Add("public override void OnInspectorGUI() { }");

            //Implement the type
            TypeNode tn = new TypeNode()
                .Declare(d)
                .Implement(code);

            //Put the new type into the namespace
            NamespaceNode nn = new NamespaceNode()
                .SetName(NamespaceName)
                .SetTypes(tn);

            //Put everything into a script
            ScriptNode sn = new ScriptNode()
                .SetCompilationDirective(compilationDirective)
                .SetUsing(un)
                .SetNamespace(nn)
                .SetTypes(tn); //also set type in case namespace is empty

            //Now make the code look awesome
            CodeFormatter formatter = new CodeFormatter();
            formatter.addSpaceAfterUsings = addSpaceAfterUsings;
            formatter.addSpaceBetweenMethods = addSpaceBetweenMethods;
            formatter.bracketStyle = openCurlyBracketOnNewLine ?
                CodeFormatter.OpenCurlyBracketStyle.NewLine :
                CodeFormatter.OpenCurlyBracketStyle.Inline;

            string[] script = formatter.Format(sn);

            return script; //phew...
        }

        public static string[] GenerateInspectorDrawer()
        {
            //Declare it
            string drawerClassName = ClassName + "InspectorDrawer";
            TypeDeclaration d = new TypeDeclaration.Builder()
                .SetAccessModifier(isInternal ? AccessModifier.INTERNAL : AccessModifier.PUBLIC)
                .SetIsStatic(false)
                .SetInheritance(Inheritance.DEFAULT)
                .SetScope(Scope.CLASS)
                .SetName(drawerClassName)
                .Build();

            //Import regularly used namespaces, just call a Using(...) if you need more
            UsingNode un = new UsingNode()
                .Using("UnityEngine")
                .Using("System.Collections")
                .Using("System.Collections.Generic")
                .Using("UnityEditor");

            //Give it a hint to determine type names
            TypeAlias.SetUsingNode(un);

            List<string> code = new List<string>();
            code.Add(string.Format("private {0} instance;", ClassName));
            code.Add(string.Format("private {0}({1} instance) {2} this.instance = instance; {3}", drawerClassName, ClassName, Token.OPEN_CURLY, Token.CLOSE_CURLY));
            code.Add(string.Format("public static {0} Create({1} instance) {2} return new {0}(instance); {3}", drawerClassName, ClassName, Token.OPEN_CURLY, Token.CLOSE_CURLY));
            code.Add("public void DrawGUI() { }");

            //Implement the type
            TypeNode tn = new TypeNode()
                .Declare(d)
                .Implement(code);

            //Put the new type into the namespace
            NamespaceNode nn = new NamespaceNode()
                .SetName(NamespaceName)
                .SetTypes(tn);

            //Put everything into a script
            ScriptNode sn = new ScriptNode()
                .SetCompilationDirective(compilationDirective)
                .SetUsing(un)
                .SetNamespace(nn)
                .SetTypes(tn); //also set type in case namespace is empty

            //Now make the code look awesome
            CodeFormatter formatter = new CodeFormatter();
            formatter.addSpaceAfterUsings = addSpaceAfterUsings;
            formatter.addSpaceBetweenMethods = addSpaceBetweenMethods;
            formatter.bracketStyle = openCurlyBracketOnNewLine ?
                CodeFormatter.OpenCurlyBracketStyle.NewLine :
                CodeFormatter.OpenCurlyBracketStyle.Inline;

            string[] script = formatter.Format(sn);

            return script; //phew...
        }

        public static string[] GenerateWindow()
        {
            //Declare it
            string windowClassName = ClassName + "Window";
            TypeDeclaration d = new TypeDeclaration.Builder()
                .SetAccessModifier(isInternal ? AccessModifier.INTERNAL : AccessModifier.PUBLIC)
                .SetIsStatic(false)
                .SetInheritance(Inheritance.DEFAULT)
                .SetScope(Scope.CLASS)
                .SetName(windowClassName)
                .SetBaseClass(typeof(EditorWindow))
                .Build();

            //Import regularly used namespaces, just call a Using(...) if you need more
            UsingNode un = new UsingNode()
                .Using("UnityEngine")
                .Using("System.Collections")
                .Using("System.Collections.Generic")
                .Using("UnityEditor");

            //Import the base class namespace to simplify its name in the declaration string
            if (d.baseClass != null)
                un.Using(d.baseClass.Namespace);

            //Give it a hint to determine type names
            TypeAlias.SetUsingNode(un);

            List<string> code = new List<string>();

            code.Add(string.Format("[MenuItem(\"{0}\")]{1}", menuPath, Token.EOL));
            code.Add("public static void ShowWindow()");
            code.Add("{");
            code.Add(string.Format("{0} window = GetWindow<{0}>(); window.titleContent = new GUIContent(\"{1}\"); window.Show();", windowClassName, ClassName));
            code.Add("}");

            code.Add(UnityMessage.ON_ENABLE);
            code.Add(UnityMessage.ON_DISABLE);
            code.Add(UnityMessage.ON_GUI);

            //Implement the type
            TypeNode tn = new TypeNode()
                .Declare(d)
                .Implement(code);

            //Put the new type into the namespace
            NamespaceNode nn = new NamespaceNode()
                .SetName(NamespaceName)
                .SetTypes(tn);

            //Put everything into a script
            ScriptNode sn = new ScriptNode()
                .SetCompilationDirective(compilationDirective)
                .SetUsing(un)
                .SetNamespace(nn)
                .SetTypes(tn); //also set type in case namespace is empty

            //Now make the code look awesome
            CodeFormatter formatter = new CodeFormatter();
            formatter.addSpaceAfterUsings = addSpaceAfterUsings;
            formatter.addSpaceBetweenMethods = addSpaceBetweenMethods;
            formatter.bracketStyle = openCurlyBracketOnNewLine ?
                CodeFormatter.OpenCurlyBracketStyle.NewLine :
                CodeFormatter.OpenCurlyBracketStyle.Inline;

            string[] script = formatter.Format(sn);

            return script; //phew...
        }

        public static string[] GenerateScriptableObject()
        {
            CompleteBaseClassAndInterfaceList();

            //Declare it
            string createAssetAttribute = string.Format("[CreateAssetMenu(menuName = \"{0}\")]", menuPath);
            TypeDeclaration d = new TypeDeclaration.Builder()
                .SetAccessModifier(isInternal ? AccessModifier.INTERNAL : AccessModifier.PUBLIC)
                .SetIsStatic(false)
                .SetInheritance(Inheritance.DEFAULT)
                .SetScope(Scope.CLASS)
                .SetName(ClassName)
                .SetBaseClass(typeof(ScriptableObject))
                .SetInterfaces(implementedInterfaces.ToArray())
                .AddAttribute(createAssetAttribute)
                .Build();

            //Import regularly used namespaces, just call a Using(...) if you need more
            UsingNode un = new UsingNode()
                .Using("UnityEngine")
                .Using("System.Collections")
                .Using("System.Collections.Generic");

            //Import the base class namespace to simplify its name in the declaration string
            if (d.baseClass != null)
                un.Using(d.baseClass.Namespace);

            //The same for interfaces
            if (d.implementedInterface.HasElement())
            {
                for (int i = 0; i < d.implementedInterface.Count; ++i)
                {
                    un.Using(d.implementedInterface[i].Namespace);
                }
            }

            //Give it a hint to determine type names
            TypeAlias.SetUsingNode(un);

            List<string> code = new List<string>();
            code.Add(UnityMessage.RESET);

            //Implement the type
            TypeNode tn = new TypeNode()
                .Declare(d)
                .Implement(code);

            //Put the new type into the namespace
            NamespaceNode nn = new NamespaceNode()
                .SetName(NamespaceName)
                .SetTypes(tn);

            //Put everything into a script
            ScriptNode sn = new ScriptNode()
                .SetCompilationDirective(compilationDirective)
                .SetUsing(un)
                .SetNamespace(nn)
                .SetTypes(tn); //also set type in case namespace is empty

            //Now make the code look awesome
            CodeFormatter formatter = new CodeFormatter();
            formatter.addSpaceAfterUsings = addSpaceAfterUsings;
            formatter.addSpaceBetweenMethods = addSpaceBetweenMethods;
            formatter.bracketStyle = openCurlyBracketOnNewLine ?
                CodeFormatter.OpenCurlyBracketStyle.NewLine :
                CodeFormatter.OpenCurlyBracketStyle.Inline;

            string[] script = formatter.Format(sn);

            return script; //phew...
        }

        /// <summary>
        /// Get the base class
        /// If null, looking for a type whose name matches the name in the base class name textbox
        /// </summary>
        /// <returns></returns>
        private static Type GetBaseClass()
        {
            if (baseClass != null)
            {
                return baseClass;
            }
            if (loadedTypes.HasElement())
            {
                return loadedTypes.Find(t => t.FullName.Equals(BaseClassName) || t.Name.Equals(BaseClassName));
            }
            return null;
        }

        /// <summary>
        /// Save editor settings
        /// </summary>
        private static void SaveSettings()
        {
            string CSHARP_WIZARD = "CsharpWizard";
            EditorPrefs.SetString(EditorCommon.GetProjectRelatedEditorPrefsKey(CSHARP_WIZARD, "NamespaceName"), NamespaceName);
            EditorPrefs.SetString(EditorCommon.GetProjectRelatedEditorPrefsKey(CSHARP_WIZARD, "BaseClassName"), BaseClassName);
            EditorPrefs.SetInt(EditorCommon.GetProjectRelatedEditorPrefsKey(CSHARP_WIZARD, "ScriptType"), (int)scriptType);
            EditorPrefs.SetString(EditorCommon.GetProjectRelatedEditorPrefsKey(CSHARP_WIZARD, "Interfaces"), InterfaceList);
            EditorPrefs.SetBool(EditorCommon.GetProjectRelatedEditorPrefsKey(CSHARP_WIZARD, "IsInternal"), isInternal);
            EditorPrefs.SetBool(EditorCommon.GetProjectRelatedEditorPrefsKey(CSHARP_WIZARD, "IsStatic"), isStatic);
            EditorPrefs.SetBool(EditorCommon.GetProjectRelatedEditorPrefsKey(CSHARP_WIZARD, "IsAbstract"), isAbstract);
            EditorPrefs.SetBool(EditorCommon.GetProjectRelatedEditorPrefsKey(CSHARP_WIZARD, "IsSealed"), isSealed);
            EditorPrefs.SetBool(EditorCommon.GetProjectRelatedEditorPrefsKey(CSHARP_WIZARD, "SpaceAfterUsings"), addSpaceAfterUsings);
            EditorPrefs.SetBool(EditorCommon.GetProjectRelatedEditorPrefsKey(CSHARP_WIZARD, "SpaceBewteenMethod"), addSpaceBetweenMethods);
            EditorPrefs.SetBool(EditorCommon.GetProjectRelatedEditorPrefsKey(CSHARP_WIZARD, "OpenBracketOnNewLine"), openCurlyBracketOnNewLine);
            EditorPrefs.SetString(EditorCommon.GetProjectRelatedEditorPrefsKey(CSHARP_WIZARD, "CompilationDirective"), compilationDirective);
        }

        /// <summary>
        /// Load editor settings
        /// </summary>
        private static void LoadSettings()
        {
            string CSHARP_WIZARD = "CsharpWizard";
            NamespaceName = EditorPrefs.GetString(EditorCommon.GetProjectRelatedEditorPrefsKey(CSHARP_WIZARD, "NamespaceName"), string.Empty);
            BaseClassName = EditorPrefs.GetString(EditorCommon.GetProjectRelatedEditorPrefsKey(CSHARP_WIZARD, "BaseClassName"), "MonoBehaviour");
            if (BaseClassName.Equals("MonoBehaviour"))
                baseClass = typeof(MonoBehaviour);
            scriptType = (ScriptType)EditorPrefs.GetInt(EditorCommon.GetProjectRelatedEditorPrefsKey(CSHARP_WIZARD, "ScriptType"), (int)ScriptType.Class);

            InterfaceList = EditorPrefs.GetString(EditorCommon.GetProjectRelatedEditorPrefsKey(CSHARP_WIZARD, "Interfaces"), string.Empty);
            isInternal = EditorPrefs.GetBool(EditorCommon.GetProjectRelatedEditorPrefsKey(CSHARP_WIZARD, "IsInternal"), false);
            isStatic = EditorPrefs.GetBool(EditorCommon.GetProjectRelatedEditorPrefsKey(CSHARP_WIZARD, "IsStatic"), false);
            isAbstract = EditorPrefs.GetBool(EditorCommon.GetProjectRelatedEditorPrefsKey(CSHARP_WIZARD, "IsAbstract"), false);
            isSealed = EditorPrefs.GetBool(EditorCommon.GetProjectRelatedEditorPrefsKey(CSHARP_WIZARD, "IsSealed"), false);
            addSpaceAfterUsings = EditorPrefs.GetBool(EditorCommon.GetProjectRelatedEditorPrefsKey(CSHARP_WIZARD, "SpaceAfterUsings"), true);
            addSpaceBetweenMethods = EditorPrefs.GetBool(EditorCommon.GetProjectRelatedEditorPrefsKey(CSHARP_WIZARD, "SpaceBewteenMethod"), true);
            openCurlyBracketOnNewLine = EditorPrefs.GetBool(EditorCommon.GetProjectRelatedEditorPrefsKey(CSHARP_WIZARD, "OpenBracketOnNewLine"), true);
            compilationDirective = EditorPrefs.GetString(EditorCommon.GetProjectRelatedEditorPrefsKey(CSHARP_WIZARD, "CompilationDirective"), string.Empty);
        }
    }
}