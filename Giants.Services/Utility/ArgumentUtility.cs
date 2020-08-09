namespace Giants.Services
{
    // Decompiled with JetBrains decompiler
    // Type: Microsoft.VisualStudio.Services.Common.ArgumentUtility
    // Assembly: Microsoft.VisualStudio.Services.Common, Version=16.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
    // MVID: 8C174B92-2E1F-4F71-9E6B-FC8D9F2C517A

    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class ArgumentUtility
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CheckForNull(object var, string varName)
        {
            if (var == null)
                throw new ArgumentNullException(varName);
        }


        public static void CheckStringForNullOrEmpty(
            string stringVar,
            string stringVarName)
        {
            ArgumentUtility.CheckStringForNullOrEmpty(stringVar, stringVarName, false);
        }

        public static void CheckForNonnegativeInt(int var, string varName)
        {
            if (var < 0)
                throw new ArgumentOutOfRangeException(varName);
        }

        public static void CheckForNonPositiveInt(int var, string varName)
        {
            if (var <= 0)
                throw new ArgumentOutOfRangeException(varName);
        }

        public static void CheckStringForNullOrEmpty(
            string stringVar,
            string stringVarName,
            bool trim)
        {
            ArgumentUtility.CheckForNull((object)stringVar, stringVarName);
            if (trim)
                stringVar = stringVar.Trim();
            if (stringVar.Length == 0)
                throw new ArgumentException("Empty string not allowed.", stringVarName);
        }

        public static void CheckStringLength(
            string stringVar,
            string stringVarName,
            int maxLength,
            int minLength = 0)
        {
            ArgumentUtility.CheckForNull((object)stringVar, stringVarName);
            if (stringVar.Length < minLength || stringVar.Length > maxLength)
                throw new ArgumentException("String length not allowed.", stringVarName);
        }

        public static void CheckEnumerableForNullOrEmpty(
            IEnumerable enumerable,
            string enumerableName)
        {
            ArgumentUtility.CheckForNull((object)enumerable, enumerableName);
            if (!enumerable.GetEnumerator().MoveNext())
                throw new ArgumentException("Collection cannot be null or empty.", enumerableName);
        }

        public static void CheckEnumerableForNullElement(
            IEnumerable enumerable,
            string enumerableName)
        {
            ArgumentUtility.CheckForNull((object)enumerable, enumerableName);
            foreach (object obj in enumerable)
            {
                if (obj == null)
                    throw new ArgumentException("NullElementNotAllowedInCollection", enumerableName);
            }
        }

        public static void CheckForEmptyGuid(Guid guid, string varName)
        {
            if (guid.Equals(Guid.Empty))
                throw new ArgumentException("EmptyGuidNotAllowed", varName);
        }

        public static void CheckBoundsInclusive(
            int value,
            int minValue,
            int maxValue,
            string varName)
        {
            if (value < minValue || value > maxValue)
                throw new ArgumentOutOfRangeException(varName, "ValueOutOfRange");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CheckForOutOfRange<T>(
            T var,
            string varName,
            T minimum)
            where T : IComparable<T>
        {
            ArgumentUtility.CheckForNull((object)var, varName);
            if (var.CompareTo(minimum) < 0)
                throw new ArgumentOutOfRangeException(varName, (object)var, "OutOfRange");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CheckForOutOfRange(
            int var,
            string varName,
            int minimum,
            int maximum)
        {
            if (var < minimum || var > maximum)
                throw new ArgumentOutOfRangeException(varName, (object)var, "OutOfRange");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CheckForOutOfRange(
            long var,
            string varName,
            long minimum,
            long maximum)
        {
            if (var < minimum || var > maximum)
                throw new ArgumentOutOfRangeException(varName, (object)var, "OutOfRange");
        }

        public static void CheckForDateTimeRange(
            DateTime var,
            string varName,
            DateTime minimum,
            DateTime maximum)
        {
            if (var < minimum || var > maximum)
                throw new ArgumentOutOfRangeException(varName, (object)var, "OutOfRange");
        }

        public static void EnsureIsNull(object var, string varName)
        {
            if (var != null)
                throw new ArgumentException("NullValueNecessary");
        }
    }
}
