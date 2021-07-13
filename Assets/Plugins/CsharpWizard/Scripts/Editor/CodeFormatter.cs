using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Pinwheel.CsharpWizard
{
    public partial class ClassWizard
    {
        internal struct CodeFormatter
        {
            internal enum OpenCurlyBracketStyle
            {
                NewLine, Inline
            }

            public bool addSpaceAfterUsings;
            public bool addSpaceBetweenMethods;
            public OpenCurlyBracketStyle bracketStyle;

            /// <summary>
            /// Format the generated code
            /// </summary>
            /// <param name="s">Represent the code to format</param>
            /// <returns>Formatted code, broken into lines</returns>
            public string[] Format(ScriptNode s)
            {
                List<string> lines = new List<string>();
                string baseCode = s.ToString();

                //firstly, process the '{' token
                //if bracket style is NewLine, we put the token into a separated line
                //if braket style is Inline, we keep the token in its current line, and put the content after that onto a new line
                //do the same for each token
                if (bracketStyle == OpenCurlyBracketStyle.NewLine)
                {
                    baseCode = baseCode.Replace(Token.OPEN_CURLY, Token.EOL + Token.OPEN_CURLY + Token.EOL);
                }
                else
                {
                    baseCode = baseCode.Replace(Token.OPEN_CURLY, Token.OPEN_CURLY + Token.EOL);
                }

                //we break the line at each ';' token
                //also put each '}' token on a separated line
                baseCode = baseCode
                    .Replace(Token.SEMICOLON, Token.SEMICOLON + Token.EOL)
                    .Replace(Token.CLOSE_CURLY, Token.EOL + Token.CLOSE_CURLY + Token.EOL);

                //split the code by eol token, remove leading and trailing whitespace on each line, then remove empty line
                lines = baseCode.Split(new string[] { Token.EOL }, StringSplitOptions.RemoveEmptyEntries).ToList();
                for (int i = 0; i < lines.Count; ++i)
                {
                    lines[i] = lines[i].Trim();
                }
                lines.RemoveAll(l => string.IsNullOrEmpty(l));

                //now we have a collection of code lines with no spacing and indentation
                //add space after using if they want to
                if (addSpaceAfterUsings)
                {
                    for (int i = 0; i < lines.Count - 1; ++i)
                    {
                        //because 'using' clause can only appear on top of the script file, 
                        //so we can simply loop like this
                        //and check if the current line starts with 'using' and the next line does not
                        if (lines[i].StartsWith(OtherKeywords.USING) &&
                            !lines[i + 1].StartsWith(OtherKeywords.USING))
                        {
                            lines.Insert(i + 1, Token.EMPTY);
                            break;
                        }
                    }
                }

                //add space between methods if they want to
                if (addSpaceBetweenMethods)
                {
                    for (int i = 0; i < lines.Count - 1; ++i)
                    {
                        //we loop through each line and check if it is '}' token and the next one is not
                        if (lines[i].Contains(Token.CLOSE_CURLY) &&
                            !lines[i + 1].Contains(Token.CLOSE_CURLY) &&
                            !lines[i + 1].Contains(OtherKeywords.SET) && //if you remove this, the 'get', 'set' block will be separated, too
                            !lines[i + 1].Contains(OtherKeywords.ENDIF)) //prevent an empty line before #endif
                        {
                            lines.Insert(i + 1, Token.EMPTY);
                        }
                    }
                }

                //then add indentation for each line
                int tabCount = 0;
                for (int i = 0; i < lines.Count; ++i)
                {
                    if (lines[i].Contains(Token.CLOSE_CURLY))
                    {
                        tabCount -= 1;
                    }
                    lines[i] = TabString(lines[i], tabCount);
                    if (lines[i].Contains(Token.OPEN_CURLY))
                    {
                        tabCount += 1;
                    }
                }

                //nice!
                return lines.ToArray();
            }

        }
    }
}