using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text;
using Pinwheel;
using System.Threading;

namespace Pinwheel.CsharpWizard
{
    public partial class ClassWizard
    {
        /// <summary>
        /// Represent a namespace with types implemented inside
        /// </summary>
        internal struct NamespaceNode
        {
            public string name;
            public TypeNode[] types;

            public NamespaceNode SetName(string name)
            {
                this.name = name;
                return this;
            }

            /// <summary>
            /// Set the types to be implemented
            /// </summary>
            /// <param name="types"></param>
            /// <returns></returns>
            public NamespaceNode SetTypes(params TypeNode[] types)
            {
                this.types = types;
                return this;
            }

            /// <summary>
            /// Get the namespace declaration and implementation code
            /// </summary>
            /// <returns></returns>
            public override string ToString()
            {
                StringBuilder s = new StringBuilder();
                s.Append(Scope.NAMESPACE).Append(Token.SPACE).Append(name);
                s.Append(Token.SPACE).Append(Token.OPEN_CURLY).Append(Token.SPACE);
                if (types.HasElement())
                {
                    for (int i = 0; i < types.Length; ++i)
                    {
                        s.Append(types[i].ToString()).Append(Token.SPACE);
                    }
                }
                s.Append(Token.CLOSE_CURLY);

                return s.ToString();
            }
        }
    }
}