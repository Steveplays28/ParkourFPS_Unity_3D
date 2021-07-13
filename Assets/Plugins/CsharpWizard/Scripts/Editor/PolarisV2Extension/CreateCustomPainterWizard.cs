#if UNITY_EDITOR && GRIFFIN && GRIFFIN_CSHARP_WIZARD
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using Pinwheel.CsharpWizard;
using static Pinwheel.CsharpWizard.ClassWizard;
using System.IO;
using Type = System.Type;
using Pinwheel.Griffin.PaintTool;
using Pinwheel.Griffin;

namespace Pinwheel.CsharpWizard.GriffinExtension
{
    public static class CreateCustomPainterWizard
    {
        private const int PAINTER_TYPE_GEOMETRY_TEXTURE = 0;
        private const int PAINTER_TYPE_FOLIAGE = 1;
        private const int PAINTER_TYPE_OBJECT = 2;

        private static int[] painterTypeValues = new int[] { 0, 1, 2 };
        private static string[] painterTypeLabels = new string[] { "Geometry - Texture Painter", "Foliage Painter", "Object Painter" };

        private static string PainterName { get; set; }
        private static int PainterType { get; set; }
        private static string Directory { get; set; }

        private const string GX_CREATE_CUSTOM_PAINTER_WIZARD = "http://bit.ly/3205n6j";

        public static string GetExtensionName()
        {
            return "Create Custom Painter Wizard";
        }

        public static string GetPublisherName()
        {
            return "Pinwheel Studio";
        }

        public static string GetDescription()
        {
            return "Quickly generate skeletion code for a custom painter script file.";
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
            PainterName = EditorGUILayout.TextField("Painter Name", PainterName);
            PainterType = EditorGUILayout.IntPopup("Type", PainterType, painterTypeLabels, painterTypeValues);
            string dir = Directory;
            EditorCommon.BrowseFolder("Directory", ref dir);
            if (!string.IsNullOrEmpty(dir))
            {
                Directory = dir;
            }

            GUI.enabled =
                !string.IsNullOrEmpty(PainterName) &&
                !string.IsNullOrEmpty(Directory);
            if (GUILayout.Button("Create"))
            {
                GAnalytics.Record(GX_CREATE_CUSTOM_PAINTER_WIZARD);
                CreateScriptFile();
            }
            GUI.enabled = true;
        }

        private static void CreateScriptFile()
        {
            bool willWrite = false;
            string path = Path.Combine(Directory, string.Format("{0}.cs", PainterName.RemoveSpecialCharacters()));
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
            Type painterInterface =
                PainterType == PAINTER_TYPE_GEOMETRY_TEXTURE ? typeof(IGTexturePainter) :
                PainterType == PAINTER_TYPE_FOLIAGE ? typeof(IGFoliagePainter) :
                PainterType == PAINTER_TYPE_OBJECT ? typeof(IGObjectPainter) :
                null;
            //Declare it
            TypeDeclaration d = new TypeDeclaration.Builder()
                .SetAccessModifier(AccessModifier.PUBLIC)
                .SetIsStatic(false)
                .SetInheritance(Inheritance.DEFAULT)
                .SetScope(Scope.CLASS)
                .SetName(PainterName.RemoveSpecialCharacters())
                .SetInterfaces(painterInterface)
                .Build();

            //Import regularly used namespaces, just call a Using(...) if you need more
            UsingNode un = new UsingNode()
                .Using("UnityEngine")
                .Using("System.Collections")
                .Using("System.Collections.Generic")
                .Using("Pinwheel.Griffin")
                .Using("Pinwheel.Griffin.PaintTool");

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

            //Implement the type
            TypeNode tn = new TypeNode()
                .Declare(d)
                .Implement(code);

            //Put the new type into the namespace
            //NamespaceNode nn = new NamespaceNode()
            //    .SetName("CustomPainter")
            //    .SetTypes(tn);

            //Put everything into a script
            ScriptNode sn = new ScriptNode()
                .SetCompilationDirective("GRIFFIN")
                .SetUsing(un)
              //.SetNamespace(nn)
                .SetTypes(tn); //also set type in case namespace is empty

            //Now make the code look awesome
            CodeFormatter formatter = new CodeFormatter();
            formatter.addSpaceAfterUsings = true;
            formatter.addSpaceBetweenMethods = true;
            formatter.bracketStyle = CodeFormatter.OpenCurlyBracketStyle.NewLine;

            string[] script = formatter.Format(sn);

            return script; //phew...
        }
    }
}

#endif
