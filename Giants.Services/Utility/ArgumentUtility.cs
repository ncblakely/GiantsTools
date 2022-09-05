namespace Giants.Services
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class ArgumentUtility
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CheckForNull(object var, [CallerArgumentExpression("var")] string varName = null)
        {
            if (var == null)
                throw new ArgumentNullException(varName);
        }


        public static void CheckStringForNullOrEmpty(
            string stringVar,
            [CallerArgumentExpression("stringVar")] string stringVarName = null)
        {
            ArgumentUtility.CheckStringForNullOrEmpty(stringVar, false, stringVarName);
        }

        public static void CheckForNonnegativeInt(int var, [CallerArgumentExpression("var")] string varName = null)
        {
            if (var < 0)
                throw new ArgumentOutOfRangeException(varName);
        }

        public static void CheckForNonPositiveInt(int var, [CallerArgumentExpression("var")] string varName = null)
        {
            if (var <= 0)
                throw new ArgumentOutOfRangeException(varName);
        }

        public static void CheckStringForNullOrEmpty(
            string stringVar,
            bool trim,
            [CallerArgumentExpression("stringVar")] string stringVarName = null)
        {
            ArgumentUtility.CheckForNull((object)stringVar, stringVarName);
            if (trim)
                stringVar = stringVar.Trim();
            if (stringVar.Length == 0)
                throw new ArgumentException("Empty string not allowed.", stringVarName);
        }

        public static void CheckStringLength(
            string stringVar,
            int maxLength,
            int minLength = 0,
            [CallerArgumentExpression("stringVar")] string stringVarName = null)
        {
            ArgumentUtility.CheckForNull((object)stringVar, stringVarName);
            if (stringVar.Length < minLength || stringVar.Length > maxLength)
                throw new ArgumentException("String length not allowed.", stringVarName);
        }

        public static void CheckEnumerableForNullOrEmpty(
            IEnumerable enumerable,
            [CallerArgumentExpression("enumerable")] string enumerableName = null)
        {
            ArgumentUtility.CheckForNull((object)enumerable, enumerableName);
            if (!enumerable.GetEnumerator().MoveNext())
                throw new ArgumentException("Collection cannot be null or empty.", enumerableName);
        }

        public static void CheckEnumerableForNullElement(
            IEnumerable enumerable,
            [CallerArgumentExpression("enumerable")] string enumerableName = null)
        {
            ArgumentUtility.CheckForNull((object)enumerable, enumerableName);
            foreach (object obj in enumerable)
            {
                if (obj == null)
                    throw new ArgumentException("NullElementNotAllowedInCollection", enumerableName);
            }
        }

        public static void CheckForEmptyGuid(Guid guid, [CallerArgumentExpression("guid")] string varName = null)
        {
            if (guid.Equals(Guid.Empty))
                throw new ArgumentException("EmptyGuidNotAllowed", varName);
        }

        public static void CheckBoundsInclusive(
            int value,
            int minValue,
            int maxValue,
            [CallerArgumentExpression("value")] string varName = null)
        {
            if (value < minValue || value > maxValue)
                throw new ArgumentOutOfRangeException(varName, "ValueOutOfRange");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CheckForOutOfRange<T>(
            T var,
            T minimum,
            [CallerArgumentExpression("var")] string varName = null)
            where T : IComparable<T>
        {
            ArgumentUtility.CheckForNull((object)var, varName);
            if (var.CompareTo(minimum) < 0)
                throw new ArgumentOutOfRangeException(varName, (object)var, "OutOfRange");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CheckForOutOfRange(
            int var,
            int minimum,
            int maximum,
            [CallerArgumentExpression("var")] string varName = null)
        {
            if (var < minimum || var > maximum)
                throw new ArgumentOutOfRangeException(varName, (object)var, "OutOfRange");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CheckForOutOfRange(
            long var,
            long minimum,
            long maximum,
            [CallerArgumentExpression("var")] string varName = null)
        {
            if (var < minimum || var > maximum)
                throw new ArgumentOutOfRangeException(varName, (object)var, "OutOfRange");
        }

        public static void CheckForDateTimeRange(
            DateTime var,
            DateTime minimum,
            DateTime maximum,
            [CallerArgumentExpression("var")] string varName = null)
        {
            if (var < minimum || var > maximum)
                throw new ArgumentOutOfRangeException(varName, (object)var, "OutOfRange");
        }
    }
}
