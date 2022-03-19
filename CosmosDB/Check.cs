// ----------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;

namespace TestThreading.CosmosDB
{
    /// <summary>
    ///     Provides methods to validate arguments and throw exceptions on validation failures.
    /// </summary>
    /// <remarks>
    ///     The <see cref="Check"/> class provides various methods to validate arguments and throw exceptions when the validation
    ///     condition fails. The methods in the <see cref="Check"/> class usually return the original value that was passed in so
    ///     that the validation and an assignment operation can be done simultaneously, as in the following example:
    ///     <code>
    ///         // This will throw an exception if the string is null or whitespace, otherwise assign it to outputValue
    ///         string outputValue = Check.NotNullOrWhiteSpace(@"inputValue", inputValue);
    ///     </code>
    /// </remarks>
    public static class Check
    {
        /// <summary>
        ///     Checks that the provided argument is not null, and throws an exception if it is.
        /// </summary>
        /// <typeparam name="T">
        ///     The <see cref="Type"/> of <paramref name="arg"/>.
        /// </typeparam>
        /// <param name="name">
        ///     Name of the argument to use when throwing the exception.
        /// </param>
        /// <param name="arg">
        ///     The argument to test.
        /// </param>
        /// <returns>
        ///     The <paramref name="arg"/> if it is not null, otherwise a <see cref="ArgumentNullException"/> is thrown.
        /// </returns>
        public static T NotNull<T>(string name, T arg)
        {
            if (arg == null)
            {
                throw new ArgumentNullException(name);
            }

            return arg;
        }

        /// <summary>
        ///     Checks that the provided argument is not null, and calls the provided method to throw an exception if the argument is null.
        /// </summary>
        /// <typeparam name="T">
        ///     The <see cref="Type"/> of <paramref name="arg"/>.
        /// </typeparam>
        /// <param name="message">
        ///     The message to be passed to the exception.
        /// </param>
        /// <param name="arg">
        ///     The argument to check.
        /// </param>
        /// <param name="newException">
        ///     A delegate that is invoked if the provided argument is null.
        /// </param>
        /// <returns>
        ///     The <paramref name="arg"/> if it is not null, otherwise a <see cref="ArgumentNullException"/> is thrown.
        /// </returns>
        public static T NotNull<T>(string message, T arg, Func<string, Exception> newException)
        {
            if (arg == null)
            {
                throw newException(message);
            }

            return arg;
        }

        /// <summary>
        ///     Checks that the provided string is not null or an <see cref="String.Empty"/> string, and throws an
        ///     exception if it is.
        /// </summary>
        /// <param name="name">
        ///     Name of the argument to use when throwing the exception.
        /// </param>
        /// <param name="arg">
        ///     The argument to test.
        /// </param>
        /// <param name="newException">
        ///     A delegate that is invoked if the provided argument is null.
        /// </param>
        /// <returns>
        ///     The <paramref name="arg"/> if it is not null or <see cref="String.Empty"/> string, otherwise an
        ///     <see cref="ArgumentException"/> is thrown.
        /// </returns>
        public static string NotNullOrEmpty(string name, string arg, Func<string, Exception> newException = null)
        {
            if (string.IsNullOrEmpty(arg))
            {
                if (newException == null)
                {
                    throw new ArgumentException($"Check_NotNullOrEmpty parameter name: {name}");
                }

                throw newException($"Check_NotNullOrEmpty parameter name: {name}");
            }

            return arg;
        }

        /// <summary>
        ///     Checks that the provided string is null or an <see cref="String.Empty"/> string, and throws an
        ///     exception if it is not.
        /// </summary>
        /// <param name="name">
        ///     Name of the argument to use when throwing the exception.
        /// </param>
        /// <param name="arg">
        ///     The argument to test.
        /// </param>
        /// <param name="newException">
        ///     A delegate that is invoked if the provided argument is null.
        /// </param>
        /// <returns>
        ///     The <paramref name="arg"/> if it is not null or <see cref="String.Empty"/> string, otherwise an
        ///     <see cref="ArgumentException"/> is thrown.
        /// </returns>
        public static string NullOrEmpty(string name, string arg, Func<string, Exception> newException = null)
        {
            if (!string.IsNullOrEmpty(arg))
            {
                if (newException == null)
                {
                    throw new ArgumentException($"Check_NullOrEmpty parameter name {name}");
                }

                throw newException($"Check_NullOrEmpty parameter name {name}");
            }

            return arg;
        }

        /// <summary>
        ///     Checks that the provided string is not null, an <see cref="String.Empty"/> string or consist only of white-space
        ///     characters, and throws an exception otherwise.
        /// </summary>
        /// <param name="name">
        ///     Name of the argument to use when throwing the exception.
        /// </param>
        /// <param name="arg">
        ///     The argument to test.
        /// </param>
        /// <param name="newException">
        ///     A delegate that is invoked if the provided argument is null.
        /// </param>
        /// <returns>
        ///     The <paramref name="arg"/> if it is not null, empty or consists only of white-space characters, otherwise an
        ///     <see cref="ArgumentException"/> is thrown.
        /// </returns>
        public static string NotNullOrWhiteSpace(string name, string arg, Func<string, Exception> newException = null)
        {
            if (string.IsNullOrWhiteSpace(arg))
            {
                if (newException == null)
                {
                    throw new ArgumentException($"Check_NotNullOrWhiteSpace parameter name {name}");
                }

                throw newException($"Check_NotNullOrWhiteSpace parameter name {name}");
            }

            return arg;
        }

        /// <summary>
        ///     Checks that the provided collection is not null and has at least one element, and throws an exception otherwise.
        /// </summary>
        /// <typeparam name="T">
        ///     The <see cref="Type"/> of items in <paramref name="arg"/>.
        /// </typeparam>
        /// <param name="name">
        ///     Name of the argument to use when throwing the exception.
        /// </param>
        /// <param name="arg">
        ///     The argument to test.
        /// </param>
        /// <returns>
        ///     The <paramref name="arg"/> if it is not null and has at least one element. An <see cref="ArgumentNullException"/>
        ///     is thrown if the collection is null and an <see cref="ArgumentException"/> is thrown if it does not consist of
        ///     any element.
        /// </returns>
        public static ICollection<T> NotNullOrEmpty<T>(string name, ICollection<T> arg)
        {
            if (arg == null)
            {
                throw new ArgumentNullException(name);
            }

            if (arg.Count == 0)
            {
                throw new ArgumentException($"Check_EmptyCollection parameter name: {name}");
            }

            return arg;
        }

        /// <summary>
        ///     Checks that the provided collection has only one element.
        /// </summary>
        /// <typeparam name="T">
        ///     The <see cref="Type"/> of items in <paramref name="arg"/>.
        /// </typeparam>
        /// <param name="name">
        ///     Name of the argument to use when throwing the exception.
        /// </param>
        /// <param name="arg">
        ///     The argument to test.
        /// </param>
        /// <returns>
        ///     The single item if the cndition is satisfied. Wlse throw <see cref="ArgumentException"/>
        /// </returns>
        public static T Single<T>(string name, ICollection<T> arg)
        {
            if (arg == null || arg.Count != 1)
            {
                throw new ArgumentException($"Check_SingleItemCollection parameter name: {name}");
            }

            return arg.First();
        }

        /// <summary>
        ///     Checks that the provided <see cref="Guid"/> does not have a value that is all zeros, and throws an
        ///     exception otherwise.
        /// </summary>
        /// <param name="name">
        ///     Name of the argument to use when throwing the exception.
        /// </param>
        /// <param name="arg">
        ///     The argument to test.
        /// </param>
        /// <returns>
        ///     The <paramref name="arg"/> if it does not have a value that is all zeros, otherwise an
        ///     <see cref="ArgumentException"/> is thrown.
        /// </returns>
        public static Guid NotEmpty(string name, Guid arg)
        {
            if (arg.Equals(Guid.Empty))
            {
                throw new ArgumentException($"Check_NotEmpty_Guid parameter name: {name}");
            }

            return arg;
        }

        /// <summary>
        ///     Check if two objects are equal
        /// </summary>
        /// <typeparam name="T">Type of the value to be compared</typeparam>
        /// <param name="name">
        ///     Name of the argument to use when throwing the exception.
        /// </param>
        /// <param name="arg">
        ///     The argument to test.
        /// </param>
        /// <param name="compareTo">
        ///     The number to compare <paramref name="arg"/> to.
        /// </param>
        /// <param name="newException">
        ///     A delegate that is invoked if the provided argument is null.
        /// </param>
        /// <returns>
        ///     <paramref name="arg"/> if its value is the same as the <paramref name="compareTo"/> value, otherwise an
        ///     <see cref="ArgumentOutOfRangeException"/> is thrown.
        /// </returns>
        public static T AreEqual<T>(string name, T arg, T compareTo, Func<string, Exception> newException)
        {
            if (!arg.Equals(compareTo))
            {
                throw newException($"Check_AreEqual {name} and {compareTo} failed");
            }

            return arg;
        }

        /// <summary>
        ///     Check if two objects are equal
        /// </summary>
        /// <typeparam name="T">Type of the value to be compared</typeparam>
        /// <param name="name">
        ///     Name of the argument to use when throwing the exception.
        /// </param>
        /// <param name="arg">
        ///     The argument to test.
        /// </param>
        /// <param name="compareTo">
        ///     The number to compare <paramref name="arg"/> to.
        /// </param>
        /// <returns>
        ///     <paramref name="arg"/> if its value is the same as the <paramref name="compareTo"/> value, otherwise an
        ///     <see cref="ArgumentOutOfRangeException"/> is thrown.
        /// </returns>
        public static T AreEqual<T>(string name, T arg, T compareTo)
        {
            if (!arg.Equals(compareTo))
            {
                throw new ArgumentOutOfRangeException(name, arg, $"Check_AreEqual {name} vs {compareTo} failed");
            }

            return arg;
        }

        /// <summary>
        ///     Checks that <paramref name="arg"/> is greater than the <paramref name="comparisonValue"/>, otherwise throws an
        ///     exception.
        /// </summary>
        /// <param name="name">
        ///     Name of the argument to use when throwing the exception.
        /// </param>
        /// <param name="arg">
        ///     The argument to test.
        /// </param>
        /// <param name="comparisonValue">
        ///     The value from which <paramref name="arg"/> must be greater.
        /// </param>
        /// <param name="newException">
        ///     A delegate that is invoked if the provided argument is null.
        /// </param>
        /// <returns>
        ///     The <paramref name="arg"/> if it is greater than the <paramref name="comparisonValue"/>, otherwise an
        ///     <see cref="ArgumentOutOfRangeException"/> is thrown.
        /// </returns>
        public static int GreaterThan(string name, int arg, int comparisonValue, Func<string, Exception> newException = null)
        {
            if (arg <= comparisonValue)
            {
                if (newException == null)
                {
                    throw new ArgumentOutOfRangeException(
                        name,
                        arg,
                        $"Check_GreaterThan {name}, {arg} vs {comparisonValue} failed");
                }

                throw newException($"Check_GreaterThan {name}, {arg} vs {comparisonValue} failed");
            }

            return arg;
        }

        /// <summary>
        ///     Checks that <paramref name="arg"/> is less than the <paramref name="comparisonValue"/>, otherwise throws an
        ///     exception.
        /// </summary>
        /// <param name="name">
        ///     Name of the argument to use when throwing the exception.
        /// </param>
        /// <param name="arg">
        ///     The argument to test.
        /// </param>
        /// <param name="comparisonValue">
        ///     The value from which <paramref name="arg"/> must be lesser.
        /// </param>
        /// <returns>
        ///     The <paramref name="arg"/> if it is less than the <paramref name="comparisonValue"/>, otherwise an
        ///     <see cref="ArgumentOutOfRangeException"/> is thrown.
        /// </returns>
        public static int LessThan(string name, int arg, int comparisonValue)
        {
            if (arg >= comparisonValue)
            {
                throw new ArgumentOutOfRangeException(name, arg, $"Check_LessThan {name}, {arg} vs {comparisonValue} failed");
            }

            return arg;
        }

        /// <summary>
        ///     Checks that <paramref name="arg"/> is greater than the <paramref name="min"/> value and lesser than the
        ///     <paramref name="max"/> value.
        /// </summary>
        /// <typeparam name="T">
        ///     The <see cref="Type"/> of the provided values.
        /// </typeparam>
        /// <param name="name">
        ///     Name of the argument to use when throwing the exception.
        /// </param>
        /// <param name="arg">
        ///     The argument to test.
        /// </param>
        /// <param name="min">
        ///     The value from which <paramref name="arg"/> must be greater.
        /// </param>
        /// <param name="max">
        ///     The value from which <paramref name="arg"/> must be lesser.
        /// </param>
        /// <returns>
        ///     The <paramref name="arg"/> if it is greater than <paramref name="min"/> and lesser than <paramref name="max"/>,
        ///     otherwise an <see cref="ArgumentOutOfRangeException"/> is thrown.
        /// </returns>
        public static T NotOutOfRange<T>(string name, T arg, T min, T max)
            where T : struct, IComparable<T>
        {
            if ((arg.CompareTo(min) < 0) || (arg.CompareTo(max) > 0))
            {
                throw new ArgumentOutOfRangeException(name, arg, $"Check_NotOutOfRange {name}, {min} vs {max} failed");
            }

            return arg;
        }

        /// <summary>
        ///     Checks that <paramref name="arg"/> is of the type <typeparamref name="T"/>, otherwise throws an exception.
        /// </summary>
        /// <typeparam name="T">
        ///     The <see cref="Type"/> of <paramref name="arg"/>.
        /// </typeparam>
        /// <param name="name">
        ///     Name of the argument to use when throwing the exception.
        /// </param>
        /// <param name="arg">
        ///     The argument to test.
        /// </param>
        /// <returns>
        ///     The <paramref name="arg"/> casted into type <typeparamref name="T"/> if the cast is valid, otherwise an
        ///     <see cref="ArgumentException"/> is thrown.
        /// </returns>
        public static T CastIsValid<T>(string name, object arg)
            where T : class
        {
            if (arg == null ||
                (arg.GetType() != typeof(T) &&
                !arg.GetType().GetTypeInfo().IsSubclassOf(typeof(T))))
            {
                ThrowInvalidTypeException(name, arg, typeof(T).Name);
            }

            return (T)arg;
        }

        /// <summary>
        ///     Checks that <paramref name="arg"/> is of the type <paramref name="expectedType"/>, otherwise throws an exception.
        /// </summary>
        /// <param name="name">
        ///     Name of the argument to use when throwing the exception.
        /// </param>
        /// <param name="arg">
        ///     The argument to test.
        /// </param>
        /// <param name="expectedType">
        ///     The <see cref="Type"/> of <paramref name="arg"/>.
        /// </param>
        /// <returns>
        ///     The <paramref name="arg"/> if it is of type <paramref name="expectedType"/> or one of its derived classes,if the cast is valid,
        ///     otherwise an <see cref="ArgumentException"/> is thrown.
        /// </returns>
        public static object CastIsValid(string name, object arg, Type expectedType)
        {
            if (arg == null ||
                expectedType == null ||
                (arg.GetType() != expectedType &&
                !arg.GetType().GetTypeInfo().IsSubclassOf(expectedType)))
            {
                string expectedTypeName = expectedType == null ? null : expectedType.Name;
                ThrowInvalidTypeException(name, arg, expectedTypeName);
            }

            return arg;
        }

        /// <summary>
        /// Returns exception message. Flattens aggregated exceptions
        /// </summary>
        /// <param name="exception">Exception to process</param>
        /// <returns>Exception message</returns>
        public static string GetExceptionMessage(Exception exception)
        {
            if (exception is AggregateException)
            {
                AggregateException aggeragteException = (AggregateException)exception;
                StringBuilder aggregateMessage = new StringBuilder();
                foreach (Exception ex in aggeragteException.Flatten().InnerExceptions)
                {
                    aggregateMessage.AppendLine(ex.Message);
                }

                return aggregateMessage.ToString();
            }
            else
            {
                return exception.Message;
            }
        }

        /// <summary>
        ///     Throws an exception for an invalid type.
        /// </summary>
        /// <param name="argName">
        ///     The argument name to put in the exception.
        /// </param>
        /// <param name="arg">
        ///     The argument that is of invalid type.
        /// </param>
        /// <param name="expectedTypeName">
        ///     The type that <paramref name="arg"/> was expected to be.
        /// </param>
        private static void ThrowInvalidTypeException(string argName, object arg, string expectedTypeName)
        {
            string argTypeName = arg != null ? arg.GetType().Name : null;
            throw new ArgumentException($"Check_TypeIsValid {argName}, {argTypeName} vs {expectedTypeName} is failed");
        }
    }
}
