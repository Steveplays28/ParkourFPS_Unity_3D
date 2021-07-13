using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text;
using System.Linq;

namespace Pinwheel.CsharpWizard
{
    public partial class ClassWizard
    {
        internal struct TypeDeclaration
        {
            public List<string> attributes;
            public string accessModifier;
            public string staticToken; //static or not?
            public string inheritance; //abstract? sealed? or none of them?
            public string partialToken;
            public string scope; //class, struct, interface or enum
            public string typeName;
            public Type baseClass;
            public List<Type> implementedInterface;

            /// <summary>
            /// Get the declaration code
            /// </summary>
            /// <returns></returns>
            public override string ToString()
            {
                StringBuilder s = new StringBuilder();
                if (attributes != null)
                {
                    attributes.RemoveAll(att => string.IsNullOrEmpty(att));
                    for (int i=0;i<attributes.Count;++i)
                    {
                        s.Append(attributes[i]).Append(Token.EOL);
                    }
                }

                s.Append(accessModifier).Append(Token.SPACE)
                 .Append(staticToken).Append(Token.SPACE)
                 .Append(inheritance).Append(Token.SPACE)
                 .Append(partialToken).Append(Token.SPACE)
                 .Append(scope).Append(Token.SPACE)
                 .Append(typeName).Append(Token.SPACE);
                if (staticToken.Equals(OtherKeywords.DEFAULT) &&
                    (baseClass != null || implementedInterface.HasElement()))
                {
                    s.Append(Token.COLON).Append(Token.SPACE);

                    List<string> baseType = new List<string>();
                    if (baseClass != null)
                        baseType.Add(baseClass.Name);
                    if (implementedInterface.HasElement())
                    {
                        for (int i = 0; i < implementedInterface.Count; ++i)
                        {
                            baseType.Add(implementedInterface[i].Name);
                        }
                    }
                    s.Append(baseType.ListElementsToString(Token.COMMA + Token.SPACE));
                }

                return FormatLine(s.ToString());
            }

            /// <summary>
            /// Ensure the declaration is properly set
            /// User is freely set everything with no error until Build() is call
            /// </summary>
            internal struct Builder
            {
                TypeDeclaration declaration;

                internal Builder AddAttribute(string code)
                {
                    if (declaration.attributes == null)
                    {
                        declaration.attributes = new List<string>();
                    }
                    declaration.attributes.Add(code);
                    return this;
                }

                internal Builder SetAccessModifier(string modifier)
                {
                    declaration.accessModifier = modifier;
                    return this;
                }

                internal Builder SetIsStatic(bool isStatic)
                {
                    declaration.staticToken = isStatic ? OtherKeywords.STATIC : OtherKeywords.DEFAULT;
                    return this;
                }

                internal Builder SetInheritance(string i)
                {
                    declaration.inheritance = i;
                    return this;
                }

                internal Builder SetIsPartial(bool isPartial)
                {
                    declaration.partialToken = isPartial ? OtherKeywords.PARTIAL : OtherKeywords.DEFAULT;
                    return this;
                }

                internal Builder SetScope(string s)
                {
                    declaration.scope = s;
                    return this;
                }

                internal Builder SetName(string name)
                {
                    declaration.typeName = name;
                    return this;
                }

                /// <summary>
                /// Set the base class to inherit from, 
                /// if t is an interface, it will be move to implementedInterface instead, and leave the base class null
                /// </summary>
                /// <param name="t"></param>
                /// <returns></returns>
                internal Builder SetBaseClass(Type t)
                {
                    if (t != null)
                    {
                        if (t.IsInterface)
                        {
                            if (declaration.implementedInterface == null)
                                declaration.implementedInterface = new List<Type>();
                            declaration.implementedInterface.Insert(0, t);
                            declaration.baseClass = null;
                        }
                        else
                        {
                            declaration.baseClass = t;
                        }
                    }
                    return this;
                }

                /// <summary>
                /// Set a list of interfaces to inherite from
                /// It also recursively add the parent interface of each element
                /// Duplicated entries will be removed
                /// </summary>
                /// <param name="interfaces"></param>
                /// <returns></returns>
                internal Builder SetInterfaces(params Type[] interfaces)
                {
                    for (int i = 0; i < interfaces.Length; ++i)
                    {
                        SetInterfaces(interfaces[i].GetInterfaces());
                    }
                    if (declaration.implementedInterface == null)
                        declaration.implementedInterface = new List<Type>();
                    declaration.implementedInterface.AddRange(interfaces);
                    declaration.implementedInterface = declaration.implementedInterface.Where(i => i != null).Distinct().ToList();
                    return this;
                }
                
                /// <summary>
                /// Check for error, if none, return the declaration
                /// </summary>
                /// <returns></returns>
                internal TypeDeclaration Build()
                {
                    SetDefaultValues(ref declaration);
                    CheckTokenValid();
                    CheckStaticValid();
                    CheckInheritanceValid(ref declaration);
                    CheckNameValid();
                    return declaration;
                }

                internal void Validate(TypeDeclaration d)
                {
                    declaration = d;
                    Build();
                }

                private void SetDefaultValues(ref TypeDeclaration d)
                {
                    if (string.IsNullOrEmpty(d.accessModifier))
                        d.accessModifier = AccessModifier.PUBLIC;
                    if (string.IsNullOrEmpty(d.staticToken))
                        d.staticToken = OtherKeywords.DEFAULT;
                    if (string.IsNullOrEmpty(d.inheritance))
                        d.inheritance = Inheritance.DEFAULT;
                    if (string.IsNullOrEmpty(d.partialToken))
                        d.partialToken = OtherKeywords.DEFAULT;
                    if (string.IsNullOrEmpty(d.scope))
                        d.scope = Scope.CLASS;
                }

                /// <summary>
                /// Check for unrecognized token
                /// </summary>
                private void CheckTokenValid()
                {
                    TypeDeclaration d = declaration;
                    if (!AccessModifier.IsValid(d.accessModifier))
                        throw new ArgumentException(string.Format("Invalid token '{0}'", d.accessModifier));
                    if (!OtherKeywords.IsValid(d.staticToken))
                        throw new ArgumentException(string.Format("Invalid token '{0}'", d.staticToken));
                    if (!Inheritance.IsValid(d.inheritance))
                        throw new ArgumentException(string.Format("Invalid token '{0}'", d.inheritance));
                    if (!OtherKeywords.IsValid(d.partialToken))
                        throw new ArgumentException(string.Format("Invalid token '{0}'", d.partialToken));
                    if (!Scope.IsValid(d.scope))
                        throw new ArgumentException(string.Format("Invalid token '{0}'", d.scope));
                }

                /// <summary>
                /// Check for the validity of static keyword
                /// </summary>
                private void CheckStaticValid()
                {
                    TypeDeclaration d = declaration;
                    if (d.staticToken.Equals(OtherKeywords.DEFAULT))
                        return;
                    if (!d.scope.Equals(Scope.CLASS))
                        throw new ArgumentException("Only class can be static");
                    if (!d.inheritance.Equals(Inheritance.DEFAULT))
                        throw new ArgumentException("Static classes cannot be abstract or sealed");
                    if (d.baseClass != null &&
                        !d.baseClass.Equals(typeof(object)))
                        throw new ArgumentException("Static classes can only derive from object");
                    if (d.implementedInterface.HasElement())
                        throw new ArgumentException("Static classes cannot implement interfaces");
                }

                /// <summary>
                /// Check for the validity of abstract/sealed keyword as well as base class and interfaces
                /// </summary>
                /// <param name="d"></param>
                private void CheckInheritanceValid(ref TypeDeclaration d)
                {
                    if (!d.scope.Equals(Scope.CLASS) && !d.inheritance.Equals(Inheritance.DEFAULT))
                    {
                        throw new ArgumentException(string.Format("Only class can be abstract or sealed"));
                    }

                    if (d.baseClass != null && d.scope != Scope.CLASS && d.scope != Scope.INTERFACE)
                    {
                        throw new ArgumentException("Inheritance requires class or interface");
                    }

                    if (d.scope.Equals(Scope.INTERFACE) && d.baseClass != null)
                    {
                        throw new ArgumentException("Interfaces cannot derive from a class");
                    }

                    if (d.baseClass != null && !IsClassDerivable(d.baseClass))
                    {
                        throw new ArgumentException(string.Format("Invalid base class {0}", d.baseClass.Name));
                    }

                    if (d.implementedInterface.HasElement())
                    {
                        for (int i = 0; i < d.implementedInterface.Count; ++i)
                        {
                            if (!d.implementedInterface[i].IsInterface)
                                throw new ArgumentException("Invalid interface list");
                        }
                    }
                }

                /// <summary>
                /// Check if type name is valid
                /// </summary>
                private void CheckNameValid()
                {
                    TypeDeclaration d = declaration;
                    string n = d.typeName;
                    if (string.IsNullOrEmpty(n)) //not empty
                        throw new ArgumentException(string.Format("Invalid type name {0}", n));
                    if (!Char.IsLetter(n[0]) && !n[0].Equals('_')) //only starts with letter or underscore
                        throw new ArgumentException(string.Format("Invalid type name {0}", n));
                    for (int i = 1; i < n.Length; ++i)
                    {
                        if (!Char.IsLetterOrDigit(n[i]) && !n[i].Equals('_')) //not contains special character
                            throw new ArgumentException(string.Format("Invalid type name {0}", n));
                    }
                }
            }
        }
    }
}
