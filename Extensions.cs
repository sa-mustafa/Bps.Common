using System;

namespace Bps.Common
{
    public static class Extensions
    {
        public enum StringKinds
        {
            Bytes,
            Digits,
            Alphabetic,
            Alphanumeric
        }

        public static bool IsASCII(this string data)
        {
            return System.Text.Encoding.UTF8.GetByteCount(data) == data.Length;
        }

        public static bool IsLowerCase(this string data)
        {
            return data.ToLower() == data;
        }

        public static bool IsUpperCase(this string data)
        {
            return data.ToUpper() == data;
        }

        public static (int, StringKinds) GetKind(this string Data)
        {
            if (Data == null) return (-1, StringKinds.Bytes);

            int len = Data.Length;
            if (len == 0) return (0, StringKinds.Bytes);

            int digits = 0; // number of digits
            int alphas = 0; // number of alphabetic characters
            int asciis = 0; // number of non alphanumeric characters
            foreach (var c in Data)
            {
                if (char.IsDigit(c))
                    digits++;
                else if (char.IsLetter(c))
                    alphas++;
                else
                    asciis++;
            }

            if (len == digits)
                return (len, StringKinds.Digits);
            else if (len == alphas)
                return (len, StringKinds.Alphabetic);
            else if (len == alphas + digits)
                return (len, StringKinds.Alphanumeric);
            else
                return (len, StringKinds.Bytes);
        }

        /// <summary>
        /// Get instance of an attribute on a type.
        /// </summary>
        /// <typeparam name="AttribType">Specify the attribute type.</typeparam>
        /// <param name="TypeName">Specify the type name.</param>
        /// <returns>an instance of the attribute.</returns>
        public static AttribType GetAttribute<AttribType>(Type TypeName) where AttribType : Attribute
        {
            return Attribute.GetCustomAttribute(TypeName, typeof(AttribType)) as AttribType;
        }

        /// <summary>
        /// Get instance of an attribute on a field of a type.
        /// </summary>
        /// <typeparam name="AttribType">Specify the attribute type.</typeparam>
        /// <param name="TypeName">Specify the type name.</param>
        /// <param name="FieldName">Specify the field name.</param>
        /// <returns>an instance of the attribute.</returns>
        public static AttribType GetAttribute<AttribType>(Type TypeName, string FieldName) where AttribType : Attribute
        {
            return Attribute.GetCustomAttribute(TypeName.GetField(FieldName), typeof(AttribType)) as AttribType;
        }
    }
}