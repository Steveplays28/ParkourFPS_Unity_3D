using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text;
using System.Reflection;

namespace Pinwheel.CsharpWizard
{
    public partial class ClassWizard
    {
        /// <summary>
        /// Represent properties and methods declaration
        /// </summary>
        internal struct MemberDeclaration
        {
            public string accessModifier;
            public string inheritance; //abstract? sealed? or none of them?
            public string scope; //property or method
            public Type type;
            public string typeAlias;
            public string memberName;
            public List<ParameterInfo> paramList; //method only

            /// <summary>
            /// Get the declaration string
            /// </summary>
            /// <returns></returns>
            public override string ToString()
            {
                StringBuilder s = new StringBuilder()
                    .Append(accessModifier).Append(Token.SPACE)
                    .Append(inheritance).Append(Token.SPACE)
                    .Append(typeAlias).Append(Token.SPACE)
                    .Append(memberName);

                if (scope.Equals(Scope.METHOD))
                {
                    s.Append(Param.GetParamDeclaration(paramList.ToArray()));
                }

                return FormatLine(s.ToString());
            }

            /// <summary>
            /// Ensure the declaration is properly set
            /// User is freely set everything with no error until Build() is call
            /// </summary>
            internal struct Builder
            {
                MemberDeclaration declaration;

                internal Builder SetInheritance(string i)
                {
                    declaration.inheritance = i;
                    return this;
                }

                internal Builder SetScope(string s)
                {
                    declaration.scope = s;
                    return this;
                }

                internal Builder SetName(string name)
                {
                    declaration.memberName = name;
                    return this;
                }

                internal Builder SetType(Type t)
                {
                    declaration.type = t;
                    return this;
                }

                internal Builder SetParams(params ParameterInfo[] p)
                {
                    declaration.paramList = new List<ParameterInfo>();
                    declaration.paramList.AddRange(p);
                    return this;
                }

                /// <summary>
                /// Validate the declaration, if no error, return it
                /// </summary>
                /// <returns></returns>
                internal MemberDeclaration Build()
                {
                    SetDefaultValues(ref declaration);
                    CheckTokenValid();
                    CheckNameValid();
                    CheckScopeValid();

                    return declaration;
                }

                private void SetDefaultValues(ref MemberDeclaration d)
                {
                    if (string.IsNullOrEmpty(d.accessModifier))
                        d.accessModifier = AccessModifier.PUBLIC;
                    if (string.IsNullOrEmpty(d.inheritance))
                        d.inheritance = Inheritance.DEFAULT;
                    if (string.IsNullOrEmpty(d.scope))
                        d.scope = Scope.METHOD;
                    d.typeAlias = TypeAlias.GetName(d.type);
                }

                /// <summary>
                /// Check for unrecognized token
                /// </summary>
                private void CheckTokenValid()
                {
                    MemberDeclaration d = declaration;
                    if (!AccessModifier.IsValid(d.accessModifier))
                        throw new ArgumentException(string.Format("Invalid token '{0}'", d.accessModifier));
                    if (d.inheritance.Equals(Inheritance.SEALED))
                        throw new ArgumentException(string.Format("Invalid token '{0}'", d.inheritance));
                    if (!d.scope.Equals(Scope.METHOD) && !d.scope.Equals(Scope.PROPERTY))
                        throw new ArgumentException(string.Format("Invalid token '{0}'", d.scope));
                }

                /// <summary>
                /// Check if type name is valid
                /// </summary>
                private void CheckNameValid()
                {
                    MemberDeclaration d = declaration;
                    string n = d.memberName;
                    if (string.IsNullOrEmpty(n)) //not empty
                        throw new ArgumentException(string.Format("Invalid type name {0}", n));
                    if (!Char.IsLetter(n[0]) && !n[0].Equals('_')) //only starts with letter or underscores
                        throw new ArgumentException(string.Format("Invalid type name {0}", n));
                    for (int i = 1; i < n.Length; ++i)
                    {
                        if (!Char.IsLetterOrDigit(n[i]) && !n[i].Equals('_')) //not contains special character
                            throw new ArgumentException(string.Format("Invalid type name {0}", n));
                    }
                }

                /// <summary>
                /// Check if scope is valid
                /// </summary>
                private void CheckScopeValid()
                {
                    MemberDeclaration d = declaration;
                    if (d.scope.Equals(Scope.PROPERTY) && d.type == null) //type null is considered as void, and only available for method
                        throw new ArgumentException("Properties must have a type");
                    if (d.scope.Equals(Scope.PROPERTY) && d.paramList.HasElement()) //parameters only available for method
                        throw new ArgumentException("Properties cannot have parameter list");
                }
            }
        }
    }
}