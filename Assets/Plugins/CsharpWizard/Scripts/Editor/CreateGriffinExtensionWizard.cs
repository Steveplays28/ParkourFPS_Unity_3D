#if UNITY_EDITOR && GRIFFIN && GRIFFIN_CSHARP_WIZARD
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using Pinwheel.CsharpWizard;
using static Pinwheel.CsharpWizard.ClassWizard;
using System.IO;
using Pinwheel.Griffin;

namespace Pinwheel.CsharpWizard.GriffinExtension
{
    public static class CreateGriffinExtensionWizard
    {
        private static string ExtensionName { get; set; }
        private static string PublisherName { get; set; }
        private static string Description { get; set; }
        private static string Version { get; set; }
        private static string Directory { get; set; }

        private const string GX_CREATE_EXTENSION_WIZARD = "http://bit.ly/36uONPA";

        public static string GetExtensionName()
        {
            return "Create Extension Wizard";
        }

        public static string GetPublisherName()
        {
            return "Pinwheel Studio";
        }

        public static string GetDescription()
        {
            return "Quickly generate skeletion code for a Griffin Extension script file.";
        }

        public static string GetVersion()
        {
            return "v1.0.0";
        }

        public static void OpenSupportLink()
        {
            string receiver = "customer@pinwheel.studio";
            string subject = "Griffin Extension - CSharp Wizard";
            string body = "YOUR_MESSAGE_HERE";
            string url = string.Format(
                "mailto:{0}" +
                "?subject={1}" +
                "&body={2}",
                receiver,
                subject.Replace(" ", "%20"),
                body.Replace(" ", "%20"));

            Application.OpenURL(url);
        }

        public static void OnGUI()
        {
            ExtensionName = EditorGUILayout.TextField("Extension Name", ExtensionName);
            PublisherName = EditorGUILayout.TextField("Publisher Name", PublisherName);
            Description = EditorGUILayout.TextField("Description", Description);
            Version = EditorGUILayout.TextField("Version", Version);
            string dir = Directory;
            EditorCommon.BrowseFolder("Directory", ref dir);
            if (!string.IsNullOrEmpty(dir))
            {
                Directory = dir;
            }

            GUI.enabled = 
                !string.IsNullOrEmpty(ExtensionName) &&
                !string.IsNullOrEmpty(PublisherName) &&
                !string.IsNullOrEmpty(Directory);
            if (GUILayout.Button("Create"))
            {
                GAnalytics.Record(GX_CREATE_EXTENSION_WIZARD);
                CreateScriptFile();
            }
            GUI.enabled = true;
        }

        private static void CreateScriptFile()
        {
            bool willWrite = false;
            string path = Path.Combine(Directory, string.Format("{0}.cs", ExtensionName.RemoveSpecialCharacters()));
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
                return;

            string[] text = GenerateCode();
            
            File.WriteAllLines(path, text);
            AssetDatabase.Refresh(); //import the file

            Object asset = AssetDatabase.LoadAssetAtPath<Object>(FileUtil.GetProjectRelativePath(path));
            Selection.activeObject = asset;
            EditorGUIUtility.PingObject(asset);
        }

        private static string[] GenerateCode()
        {
            //Declare it
            TypeDeclaration d = new TypeDeclaration.Builder()
                .SetAccessModifier(AccessModifier.PUBLIC)
                .SetIsStatic(true)
                .SetInheritance(Inheritance.DEFAULT)
                .SetScope(Scope.CLASS)
                .SetName(ExtensionName.RemoveSpecialCharacters())
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
            //add basic function for an extension
            code.Add(GetExtensionNameCode());
            code.Add(GetPublisherNameCode());
            code.Add(GetDescriptionCode());
            code.Add(GetVersionCode());
            code.Add(GetOpenUserGuideCode());
            code.Add(GetOpenSupportLinkCode());
            code.Add(GetButtonMethodCode());
            code.Add(GetOnGUICode());

            //Implement the type
            TypeNode tn = new TypeNode()
                .Declare(d)
                .Implement(code);

            //Put the new type into the namespace
            NamespaceNode nn = new NamespaceNode()
                .SetName(string.Format("{0}.GriffinExtension", PublisherName.RemoveSpecialCharacters()))
                .SetTypes(tn);

            //Put everything into a script
            ScriptNode sn = new ScriptNode()
                .SetCompilationDirective("GRIFFIN && UNITY_EDITOR")
                .SetUsing(un)
                .SetNamespace(nn)
                .SetTypes(tn); //also set type in case namespace is empty

            //Now make the code look awesome
            CodeFormatter formatter = new CodeFormatter();
            formatter.addSpaceAfterUsings = true;
            formatter.addSpaceBetweenMethods = true;
            formatter.bracketStyle = CodeFormatter.OpenCurlyBracketStyle.NewLine;

            string[] script = formatter.Format(sn);

            return script; //phew...
        }

        private static string GetExtensionNameCode()
        {
            string code = string.Format(
                "public static string GetExtensionName() {0} return \"{1}\"; {2}",
                Token.OPEN_CURLY, ExtensionName, Token.CLOSE_CURLY);
            return code;
        }

        private static string GetPublisherNameCode()
        {
            string code = string.Format(
                "public static string GetPublisherName() {0} return \"{1}\"; {2}",
                Token.OPEN_CURLY, PublisherName, Token.CLOSE_CURLY);
            return code;
        }

        private static string GetDescriptionCode()
        {
            string code = string.Format(
                "public static string GetDescription() {0} return \"{1}\"; {2}",
                Token.OPEN_CURLY, Description, Token.CLOSE_CURLY);
            return code;
        }

        private static string GetVersionCode()
        {
            string code = string.Format(
                "public static string GetVersion() {0} return \"{1}\"; {2}",
                Token.OPEN_CURLY, Version, Token.CLOSE_CURLY);
            return code;
        }

        private static string GetOpenUserGuideCode()
        {
            string code = string.Format(
                "public static void OpenUserGuide() {0} /*implement your code here*/ {1}",
                Token.OPEN_CURLY, Token.CLOSE_CURLY);
            return code;
        }

        private static string GetOpenSupportLinkCode()
        {
            string code = string.Format(
                "public static void OpenSupportLink() {0} /*implement your code here*/ {1}",
                Token.OPEN_CURLY, Token.CLOSE_CURLY);
            return code;
        }

        private static string GetButtonMethodCode()
        {
            string code = string.Format(
                "public static void Button_SampleMethod() {0} /*implement your code here*/ {1}",
                Token.OPEN_CURLY, Token.CLOSE_CURLY);
            return code;
        }

        private static string GetOnGUICode()
        {
            string code = string.Format(
                "public static void OnGUI() {0} /*implement your code here*/ {1}",
                Token.OPEN_CURLY, Token.CLOSE_CURLY);
            return code;
        }
    }
}

#endif
