using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;
using System.Text;
using System.Linq;

namespace Pinwheel.CsharpWizard
{
    public partial class ClassWizard
    {
        /// <summary>
        /// Represent the implementation of a specific type
        /// </summary>
        internal struct TypeImplementation
        {
            private List<PropertyInfo> properties;
            private List<MethodInfo> methods;
            private List<bool> isMethodOverride;
            private List<string> customCode;

            /// <summary>
            /// Implement a method, set isOverride flag to true to implement an abstract method, false to implement an interface
            /// </summary>
            /// <param name="m"></param>
            /// <param name="isOverride"></param>
            /// <returns></returns>
            internal TypeImplementation ImplementMethod(MethodInfo m, bool isOverride = false)
            {
                if (!methods.HasElement())
                {
                    methods = new List<MethodInfo>();
                    isMethodOverride = new List<bool>();
                }
                methods.Add(m);
                isMethodOverride.Add(isOverride);
                return this;
            }

            /// <summary>
            /// Implement a set of methods with isOverride flag set to false
            /// </summary>
            /// <param name="ms"></param>
            /// <returns></returns>
            internal TypeImplementation ImplementMethods(MethodInfo[] ms)
            {
                for (int i = 0; i < ms.Length; ++i)
                {
                    ImplementMethod(ms[i]);
                }
                return this;
            }

            internal TypeImplementation ImplementProperty(PropertyInfo p)
            {
                if (!properties.HasElement())
                    properties = new List<PropertyInfo>();
                properties.Add(p);
                return this;
            }

            internal TypeImplementation ImplementProperties(PropertyInfo[] ps)
            {
                for (int i = 0; i < ps.Length; ++i)
                {
                    ImplementProperty(ps[i]);
                }
                return this;
            }

            internal TypeImplementation ImplementCustomCode(List<string> code)
            {
                customCode = code;
                return this;
            }

            /// <summary>
            /// Get the implementation code
            /// </summary>
            /// <returns></returns>
            public override string ToString()
            {
                StringBuilder s = new StringBuilder();
                s.Append(Token.OPEN_CURLY).Append(Token.SPACE);
                //first implement properties
                if (properties.HasElement())
                {
                    for (int i = 0; i < properties.Count; ++i)
                    {
                        PropertyInfo p = properties[i];
                        MemberDeclaration declaration = new MemberDeclaration.Builder()
                            .SetScope(Scope.PROPERTY)
                            .SetType(p.PropertyType)
                            .SetName(p.Name)
                            .Build();
                        MethodInfo getMethod = p.GetGetMethod();
                        MethodInfo setMethod = p.GetSetMethod();
                        string implementation = MemberImplementation.EmptyBlock;
                        if (getMethod != null && setMethod != null) //the property contains both get and set
                        {
                            implementation = MemberImplementation.PropertyGetSetNotImplemented;
                        }
                        else if (getMethod != null && setMethod == null) //the property only contains get
                        {
                            implementation = MemberImplementation.PropertyGetNotImplemented;
                        }
                        s.Append(declaration).Append(Token.SPACE).Append(implementation).Append(Token.SPACE);
                    }
                }

                //then implement methods
                if (methods.HasElement())
                {
                    for (int i = 0; i < methods.Count; ++i)
                    {
                        MethodInfo m = methods[i];
                        ParameterInfo[] pa = m.GetParameters();
                        MemberDeclaration declaration = new MemberDeclaration.Builder()
                            .SetScope(Scope.METHOD)
                            .SetInheritance(isMethodOverride[i] ? Inheritance.OVERRIDE : Inheritance.DEFAULT)
                            .SetType(m.ReturnType)
                            .SetName(m.Name)
                            .SetParams(pa)
                            .Build();
                        string implementation = MemberImplementation.MethodNotImplemented;

                        s.Append(declaration).Append(Token.SPACE).Append(implementation).Append(Token.SPACE);
                    }
                }

                if (customCode.HasElement())
                {
                    for (int i = 0; i < customCode.Count; ++i)
                    {
                        s.Append(customCode[i]).Append(Token.SPACE);
                    }
                }

                s.Append(Token.CLOSE_CURLY);
                return s.ToString();
            }
        }
    }
}