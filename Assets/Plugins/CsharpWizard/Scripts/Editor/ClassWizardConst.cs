using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;
using System.Text;

namespace Pinwheel.CsharpWizard
{
    public partial class ClassWizard
    {
        internal struct Token
        {
            public const string EMPTY = "";
            public const string OPEN_CURLY = "{";
            public const string CLOSE_CURLY = "}";
            public const string OPEN_BRACKET = "[";
            public const string CLOSE_BRACKET = "]";
            public const string OPEN_PARENTHESE = "(";
            public const string CLOSE_PERENTHESE = ")";
            public const string COLON = ":";
            public const string SEMICOLON = ";";
            public const string DOT = ".";
            public const string COMMA = ",";
            public const string SPACE = " ";
            public const string LT = "<";
            public const string GT = ">";
            public const string EOL = "\n";
            public const string TAB = "\t";
            public const string QUOTE = "\"";
            public const string AMP = "&";
        }

        internal struct Scope
        {
            public const string NAMESPACE = "namespace";
            public const string CLASS = "class";
            public const string STRUCT = "struct";
            public const string ENUM = "enum";
            public const string INTERFACE = "interface";
            public const string PROPERTY = "property";
            public const string METHOD = "method";

            public static bool IsValid(string s)
            {
                return
                    s.Equals(NAMESPACE) ||
                    s.Equals(CLASS) ||
                    s.Equals(STRUCT) ||
                    s.Equals(ENUM) ||
                    s.Equals(INTERFACE);
            }
        }

        internal struct AccessModifier
        {
            public const string PUBLIC = "public";
            public const string PRIVATE = "private";
            public const string PROTECTED = "protected";
            public const string INTERNAL = "internal";
            public const string PROTECTED_INTERNAL = "protected internal";

            public static bool IsValid(string s)
            {
                return
                    s.Equals(PUBLIC) ||
                    s.Equals(PRIVATE) ||
                    s.Equals(PROTECTED) ||
                    s.Equals(INTERNAL) ||
                    s.Equals(PROTECTED_INTERNAL);
            }
        }

        internal struct Inheritance
        {
            public const string DEFAULT = "";
            public const string ABSTRACT = "abstract";
            public const string SEALED = "sealed";
            public const string VIRTUAL = "virtual";
            public const string OVERRIDE = "override";

            public static bool IsValid(string s)
            {
                return
                    s.Equals(DEFAULT) ||
                    s.Equals(ABSTRACT) ||
                    s.Equals(SEALED) ||
                    s.Equals(VIRTUAL) ||
                    s.Equals(OVERRIDE);
            }
        }

        internal struct OtherKeywords
        {
            public const string DEFAULT = "";
            public const string USING = "using";
            public const string STATIC = "static";
            public const string PARTIAL = "partial";
            public const string GET = "get";
            public const string SET = "set";
            public const string ENDIF = "#endif";

            public static bool IsValid(string s)
            {
                return
                    s.Equals(DEFAULT) ||
                    s.Equals(USING) ||
                    s.Equals(STATIC) ||
                    s.Equals(PARTIAL) ||
                    s.Equals(GET) ||
                    s.Equals(SET) ||
                    s.Equals(ENDIF);
            }
        }

        internal struct Param
        {
            public const string DEFAULT = "";
            public const string REF = "ref";
            public const string OUT = "out";
            public const string PARAMS = "params";

            public static bool IsValid(string s)
            {
                return
                    s.Equals(DEFAULT) ||
                    s.Equals(REF) ||
                    s.Equals(OUT) ||
                    s.Equals(PARAMS);
            }

            /// <summary>
            /// Is the param goes with the ref keyword?
            /// </summary>
            /// <param name="p"></param>
            /// <returns></returns>
            public static bool IsRef(ParameterInfo p)
            {
                return p.ParameterType.Name.Contains(Token.AMP) && !p.IsOut;
            }

            /// <summary>
            /// Is the param goes with the out keyword?
            /// </summary>
            /// <param name="p"></param>
            /// <returns></returns>
            public static bool IsOut(ParameterInfo p)
            {
                return p.ParameterType.Name.Contains(Token.AMP) && p.IsOut;
            }

            /// <summary>
            /// Is the param goes with the params keyword?
            /// </summary>
            /// <param name="p"></param>
            /// <returns></returns>
            public static bool IsParams(ParameterInfo p)
            {
                return p.IsDefined(typeof(ParamArrayAttribute), false);
            }

            /// <summary>
            /// Get a generic name for a parameter
            /// Use parameterInfo.Name for a more meaningful name
            /// </summary>
            /// <param name="t"></param>
            /// <returns></returns>
            public static string GetParamName(ParameterInfo t)
            {
                string alias = TypeAlias.GetName(t.ParameterType).RemoveSpecialCharacters().ToUpperFirstLetter();
                return string.Format("p{0}", alias);
            }

            /// <summary>
            /// Get a generic name for a list of parameters, if they are duplicated, a number will be appended
            /// </summary>
            /// <param name="t"></param>
            /// <returns></returns>
            public static List<string> GetParamNames(List<ParameterInfo> t)
            {
                List<string> n = new List<string>();
                for (int i = 0; i < t.Count; ++i)
                {
                    int j = 0;
                    string name = string.Empty;
                    do
                    {
                        name = string.Format("{0}{1}", GetParamName(t[i]), (j > 0 ? j.ToString() : string.Empty));
                        j += 1;
                    }
                    while (n.Contains(name));
                    n.Add(name);
                }
                return n;
            }

            /// <summary>
            /// Get parameter declaration for a method
            /// </summary>
            /// <param name="p"></param>
            /// <returns></returns>
            public static string GetParamDeclaration(ParameterInfo[] p)
            {
                StringBuilder s = new StringBuilder();
                s.Append(Token.OPEN_PARENTHESE);
                if (p.HasElement())
                {
                    List<string> declaration = new List<string>();
                    for (int i = 0; i < p.Length; ++i)
                    {
                        string outRefParam =
                            IsOut(p[i]) ? OUT :
                            IsRef(p[i]) ? REF :
                            IsParams(p[i]) ? PARAMS :
                            DEFAULT;
                        string d = string.Format("{0} {1} {2}",
                            outRefParam,
                            TypeAlias.GetName(p[i].ParameterType).Replace(Token.AMP, Token.EMPTY),
                            p[i].Name);
                        declaration.Add(d.Trim());
                    }
                    s.Append(declaration.ListElementsToString(Token.COMMA + Token.SPACE));
                }
                s.Append(Token.CLOSE_PERENTHESE);
                return s.ToString();
            }
        }

        internal struct TypeAlias
        {
            //https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/built-in-types-table
            public const string VOID = "void";
            public const string BOOL = "bool";
            public const string BYTE = "byte";
            public const string CHAR = "char";
            public const string DECIMAL = "decimal";
            public const string DOUBLE = "double";
            public const string FLOAT = "float";
            public const string INT = "int";
            public const string LONG = "long";
            public const string OBJECT = "object";
            public const string STRING = "string";

            public const string BOOL_ARRAY = "bool[]";
            public const string BYTE_ARRAY = "byte[]";
            public const string CHAR_ARRAY = "char[]";
            public const string DECIMAL_ARRAY = "decimal[]";
            public const string DOUBLE_ARRAY = "double[]";
            public const string FLOAT_ARRAY = "float[]";
            public const string INT_ARRAY = "int[]";
            public const string LONG_ARRAY = "long[]";
            public const string OBJECT_ARRAY = "object[]";
            public const string STRING_ARRAY = "string[]";

            public static UsingNode usingNode;

            private static Dictionary<Type, string> typeNameMap;
            private static Dictionary<Type, string> TypeNameMap
            {
                get
                {
                    if (!typeNameMap.HasElement())
                    {
                        //some type like ushort, uint, etc. is currently not included
                        //you can add them into this dictionary
                        typeNameMap = new Dictionary<Type, string>()
                        {
                            {typeof(void), VOID },
                            {typeof(bool), BOOL },
                            {typeof(byte), BYTE },
                            {typeof(char), CHAR },
                            {typeof(decimal), DECIMAL },
                            {typeof(double), DOUBLE },
                            {typeof(float), FLOAT },
                            {typeof(int), INT },
                            {typeof(long), LONG },
                            {typeof(object), OBJECT },
                            {typeof(string), STRING },

                            {typeof(bool[]), BOOL_ARRAY },
                            {typeof(byte[]), BYTE_ARRAY },
                            {typeof(char[]), CHAR_ARRAY },
                            {typeof(decimal[]), DECIMAL_ARRAY },
                            {typeof(double[]), DOUBLE_ARRAY },
                            {typeof(float[]), FLOAT_ARRAY },
                            {typeof(int[]), INT_ARRAY },
                            {typeof(long[]), LONG_ARRAY },
                            {typeof(object[]), OBJECT_ARRAY },
                            {typeof(string[]), STRING_ARRAY },
                        };
                    }

                    return typeNameMap;
                }
            }

            /// <summary>
            /// Give a hint to determine if the namespace should be included in type name or not
            /// </summary>
            /// <param name="un"></param>
            public static void SetUsingNode(UsingNode un)
            {
                usingNode = un;
            }

            /// <summary>
            /// Get type name
            /// If the type is primitive, return it primitive name (e.g: int, long, etc.)
            /// </summary>
            /// <param name="t"></param>
            /// <returns></returns>
            public static string GetName(Type t)
            {
                if (t == null)
                    return VOID;
                if (TypeNameMap.ContainsKey(t))
                    return TypeNameMap[t];
                foreach (var ti in TypeNameMap.Keys)
                {
                    if (!ti.IsArray && !ti.IsByRef)
                    {
                        if (ti.MakeByRefType().Equals(t))
                            return TypeNameMap[ti];
                    }
                }

                if (t.IsGenericType)
                {
                    Type[] innerTypes = t.GetGenericArguments();
                    string[] innerTypeNames = new string[innerTypes.Length];
                    for (int i = 0; i < innerTypes.Length; ++i)
                    {
                        innerTypeNames[i] = GetName(innerTypes[i]);
                    }

                    string name = string.Format("{0}<{1}>",
                        t.Name.Remove(t.Name.IndexOf("`")),
                        innerTypeNames.ListElementsToString(","));
                    return name;
                }


                if (t == typeof(UnityEngine.Object)) //quick fix for ambigous between System.Object and UnityEngine.Object
                {
                    return usingNode.Contains("System") ? t.FullName : t.Name;
                }
                else
                    return usingNode.Contains(t.Namespace) ? t.Name : t.FullName;
            }
        }

        internal struct UnityMessage
        {
            public const string AWAKE = "public void Awake() { }";
            public const string START = "public void Start() { }";
            public const string UPDATE = "public void Update() { }";
            public const string ON_DESTROY = "public void OnDestroy() { }";
            public const string ON_ENABLE = "public void OnEnable() { }";
            public const string ON_DISABLE = "public void OnDisable() { }";
            public const string ON_GUI = "public void OnGUI() { }";
            public const string ON_VALIDATE = "public void OnValidate() { }";
            public const string RESET = "public void Reset() { }";
        }
    }
}