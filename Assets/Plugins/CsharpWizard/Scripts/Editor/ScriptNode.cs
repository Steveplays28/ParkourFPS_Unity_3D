using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;

namespace Pinwheel.CsharpWizard
{
    public partial class ClassWizard
    {
        /// <summary>
        /// Represent a script file
        /// </summary>
        internal struct ScriptNode
        {
            public string compilationDirective;
            public UsingNode usingNode;
            public NamespaceNode namespaceNode;
            public List<TypeNode> typeNodes;

            public ScriptNode SetCompilationDirective(string directive)
            {
                compilationDirective = directive;
                return this;
            }

            public ScriptNode SetUsing(UsingNode u)
            {
                usingNode = u;
                return this;
            }

            public ScriptNode SetNamespace(NamespaceNode n)
            {
                namespaceNode = n;
                return this;
            }

            /// <summary>
            /// Use this when you don't want to wrap the type in a namespace
            /// </summary>
            /// <param name="t"></param>
            /// <returns></returns>
            public ScriptNode SetTypes(params TypeNode[] t)
            {
                if (!typeNodes.HasElement())
                    typeNodes = new List<TypeNode>();
                typeNodes.AddRange(t);
                return this;
            }

            /// <summary>
            /// Get the generated code
            /// </summary>
            /// <returns></returns>
            public override string ToString()
            {
                StringBuilder s = new StringBuilder();

                if (!string.IsNullOrEmpty(compilationDirective))
                {
                    s.Append("#if ").Append(compilationDirective).Append(Token.EOL);
                }

                s.Append(usingNode.ToString()).Append(Token.SPACE);
                if (!string.IsNullOrEmpty(namespaceNode.name))
                {
                    if (typeNodes.HasElement())
                        namespaceNode.SetTypes(typeNodes.ToArray());
                    s.Append(namespaceNode.ToString()).Append(Token.SPACE);
                }
                else
                {
                    if (typeNodes.HasElement())
                    {
                        for (int i = 0; i < typeNodes.Count; ++i)
                        {
                            s.Append(typeNodes[i].ToString()).Append(Token.SPACE);
                        }
                    }
                }

                if (!string.IsNullOrEmpty(compilationDirective))
                {
                    s.Append("#endif");
                }

                return s.ToString();
            }
        }
    }
}