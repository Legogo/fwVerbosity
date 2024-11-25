using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace fwp.verbosity
{

    [System.Flags]
    public enum VerbositySectionUniversal
    {
        none            = 0,
        engine          = 1,
        loading         = 2,

        input           = 4,
        addressables    = 8,
        audio           = 16,
        localization    = 32,

        shader          = 64,
    }

    /// <summary>
    /// a manager to toggle sections of verbosity
    /// </summary>
    public class Verbosity
    {
        const string _ppref_prefix = "ppref_";

        // Enum type & bitmask
        static Dictionary<Type, int> toggles = new Dictionary<Type, int>();

        static public Enum getMaskEnum(Enum enumType)
        {
            int mv = getMaskInt(enumType.GetType());
            return (Enum)Enum.ToObject(enumType.GetType(), mv);
        }

        static int getMaskInt(Type enumType)
        {
            //Type t = enumType.GetType();
#if UNITY_EDITOR
            if (Application.isEditor) 
                return EditorPrefs.GetInt(
                    _ppref_prefix + enumType, 0);
#endif

            checkKey(enumType);

            return toggles[enumType];
        }

        static public bool isToggled(Enum enumSpecificValue)
        {
            int local = 0;

            Type t = enumSpecificValue.GetType();

#if UNITY_EDITOR
            local = EditorPrefs.GetInt(_ppref_prefix + t.ToString(), 0);
#else
            checkKey(enumSpecificValue.GetType());
            local = toggles[t];
#endif

            // dico/pref stored value
            var dVal = (Enum)Enum.ToObject(t, local);

            // given filter
            var fVal = (Enum)Enum.ToObject(t, enumSpecificValue);

            return dVal.HasFlag(fVal);
        }

        static void checkKey(Type enumType)
        {
            if (Application.isEditor)
                return;

            // in build
            if (!toggles.ContainsKey(enumType))
            {
                toggles.Add(enumType, 0);
            }
        }

        static public void toggle(Enum flag)
        {
            Type t = flag.GetType();
            int sVal = (int)Enum.ToObject(t, flag);

#if UNITY_EDITOR
            EditorPrefs.SetInt(_ppref_prefix + t.ToString(), sVal);
            Debug.Log(" <editor< " + t.ToString() + "#" + sVal);
#else
            checkKey(flag.GetType());
            toggles[t] = sVal;

            Debug.Log(" <<< " + t.ToString() + "#" + sVal);
#endif
        }


        static public void logNone(string content, object context = null, string hex = null) 
            => logEnum(VerbositySectionUniversal.none, content, context, hex);

        /// <summary>
        /// log universal
        /// </summary>
        static public void logUniversal(VerbositySectionUniversal section, string content, object context = null, string hex = null)
            => logEnum(section, content, context);

        static public void logFilter(Enum enumValue, string content, object context = null, string hex = null)
            => logEnum(enumValue, content, context, hex);

        const string _tab = "   ";
        const string _separator = ">";

        static protected void logEnum(Enum enumValue, string content, object context = null, string hex = null)
        {
            bool toggled = isToggled(enumValue);

            if (!toggled)
                return;

            // num
            string stamp = Time.frameCount + _separator;

            // cat
            if (!string.IsNullOrEmpty(hex)) stamp += "<color=#" + hex + ">";

            stamp += enumValue.ToString();

            if (!string.IsNullOrEmpty(hex)) stamp += "</color>";

            stamp += _separator;

            // name
            if(context != null)
            {
                stamp += context.GetType();
                UnityEngine.Object uo = context as UnityEngine.Object;
                if (uo != null) stamp += ":" + uo.name;
                stamp += _separator;
            }

            // separator
            stamp += _tab;

            //Debug.Log($"{tar}    <color=#888888> >>>> </color> " + content, tar as Object);
            Debug.Log(stamp + content, context as UnityEngine.Object);
        }
    }
}
