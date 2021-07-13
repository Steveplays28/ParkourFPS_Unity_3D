using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System.Linq;

namespace Pinwheel.CsharpWizard
{
    public partial class ClassWizard
    {
        internal struct UsingNode
        {
            List<string> usings;

            /// <summary>
            /// Add a namespace
            /// </summary>
            /// <param name="u"></param>
            /// <returns></returns>
            public UsingNode Using(string u)
            {
                if (!usings.HasElement())
                    usings = new List<string>();
                if (!string.IsNullOrEmpty(u))
                    usings.Add(u);
                return this;
            }

            /// <summary>
            /// Check if a namespace is included
            /// </summary>
            /// <param name="u"></param>
            /// <returns></returns>
            public bool Contains(string u)
            {
                return usings != null && usings.Contains(u);
            }

            /// <summary>
            /// Get the code
            /// </summary>
            /// <returns></returns>
            public override string ToString()
            {
                StringBuilder s = new StringBuilder();
                if (usings.HasElement())
                {
                    usings = new HashSet<string>(usings).ToList();
                    for (int i = 0; i < usings.Count; ++i)
                    {
                        s.Append(OtherKeywords.USING).Append(Token.SPACE).Append(usings[i]).Append(Token.SEMICOLON).Append(Token.SPACE);
                    }
                }
                return s.ToString();
            }
        }
    }
}