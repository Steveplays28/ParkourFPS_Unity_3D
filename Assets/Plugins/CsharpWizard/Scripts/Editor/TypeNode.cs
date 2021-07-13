using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using System.Linq;
using System;
using System.Text;

namespace Pinwheel.CsharpWizard
{
    public partial class ClassWizard
    {
        /// <summary>
        /// Represent a type with declaration and implementation
        /// </summary>
        internal struct TypeNode
        {
            TypeDeclaration declaration;
            TypeImplementation implementation;

            public TypeNode Declare(TypeDeclaration d)
            {
                new TypeDeclaration.Builder().Validate(d); //an exception will be thrown if it cannot pass the test
                declaration = d;
                return this;
            }

            public TypeNode Implement(List<string> customCode = null)
            {
                TypeImplementation impl = new TypeImplementation();
                //implement all abstract methods in base class
                if (declaration.baseClass != null)
                {
                    MethodInfo[] m = declaration.baseClass.GetMethods().Where(x => x.IsAbstract).ToArray();
                    for (int i = 0; i < m.Length; ++i)
                    {
                        impl.ImplementMethod(m[i], true);
                    }
                }

                //implement all interfaces
                if (declaration.implementedInterface.HasElement())
                {
                    for (int i = 0; i < declaration.implementedInterface.Count; ++i)
                    {
                        Type t = declaration.implementedInterface[i];
                        PropertyInfo[] p = t.GetProperties();
                        impl.ImplementProperties(p);

                        //get all method except get/set methods and operators
                        MethodInfo[] m = t.GetMethods().Where(x => !x.IsSpecialName).ToArray();
                        impl.ImplementMethods(m);
                    }
                }

                impl.ImplementCustomCode(customCode);
                implementation = impl;
                return this;
            }

            /// <summary>
            /// Get the code
            /// </summary>
            /// <returns></returns>
            public override string ToString()
            {
                StringBuilder s = new StringBuilder();
                s.Append(declaration.ToString()).Append(Token.SPACE).Append(implementation.ToString());
                return s.ToString();
            }
        }
    }
}