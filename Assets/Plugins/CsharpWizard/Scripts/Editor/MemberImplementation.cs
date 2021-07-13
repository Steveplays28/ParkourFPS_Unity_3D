using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Pinwheel.CsharpWizard
{
    public partial class ClassWizard
    {
        /// <summary>
        /// Simply throw exception because we only need the skeleton code
        /// </summary>
        internal struct MemberImplementation
        {
            internal static string ThrowNotImplementException
            {
                get
                {
                    string s = string.Format("throw new {0}();", TypeAlias.GetName(typeof(NotImplementedException)));
                    return s;
                }
            }

            internal static string PropertyGetSetNotImplemented
            {
                get
                {
                    string s = ("{ get { {0} } set { {0} } }").Replace("{0}", ThrowNotImplementException);
                    return s;
                }
            }

            internal static string PropertyGetNotImplemented
            {
                get
                {
                    string s = ("{ get { {0} } }").Replace("{0}", ThrowNotImplementException);
                    return s;
                }
            }

            internal static string MethodNotImplemented
            {
                get
                {
                    string s = ("{ {0} }").Replace("{0}", ThrowNotImplementException);
                    return s;
                }
            }

            internal static string EmptyBlock
            {
                get
                {
                    return "{ }";
                }
            }
        }
    }
}