using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System;
using System.Text;

namespace Pinwheel.CsharpWizard
{
    public static class Utilities
    {
        public static List<T> ToList<T>(this T[] array)
        {
            List<T> list = new List<T>();
            for (int i = 0; i < array.Length; ++i)
            {
                list.Add(array[i]);
            }
            return list;
        }

        public static string ListElementsToString<T>(this IEnumerable<T> list, string separator)
        {
            IEnumerator<T> i = list.GetEnumerator();
            System.Text.StringBuilder s = new System.Text.StringBuilder();
            if (i.MoveNext())
                s.Append(i.Current.ToString());
            while (i.MoveNext())
                s.Append(separator).Append(i.Current.ToString());
            return s.ToString();
        }

        public static bool HasElement<T>(this IEnumerable<T> list)
        {
            return list != null && list.GetEnumerator().MoveNext();
        }

        public static string ToUpperFirstLetter(this string s)
        {
            if (s.Length > 1)
            {
                return s[0].ToString().ToUpper() + s.Substring(1);
            }
            else
            {
                return s.ToUpper();
            }
        }

        public static string RemoveSpecialCharacters(this string str)
        {
            StringBuilder sb = new StringBuilder();
            foreach (char c in str)
            {
                if ((c >= '0' && c <= '9') || (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || c == '.' || c == '_')
                {
                    sb.Append(c);
                }
            }
            return sb.ToString();
        }

        public static void MailTo(string receiverEmail, string subject, string body)
        {
            subject = EscapeURL(subject);
            body = EscapeURL(body);
            Application.OpenURL("mailto:" + receiverEmail + "?subject=" + subject + "&body=" + body);
        }

        public static string EscapeURL(string url)
        {
#pragma warning disable 0618
            return WWW.EscapeURL(url).Replace("+", "%20");
#pragma warning restore 0618
        }
    }
}