// ----------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------

namespace TestThreading.CosmosDB.TestRunners
{
    /// <summary>
    ///     Represents the type of the secret, which is passed to the data mappers so correct
    ///     storage location or mechanism can be determined depending on the secret type.
    /// </summary>
    public enum SecretType
    {
        /// <summary>
        ///     Represents a X509Certificate.
        /// </summary>
        X509Certificate,

        /// <summary>
        ///     Represents a secret.
        /// </summary>
        Secret,

        /// <summary>
        ///     Represents a JWT key container.
        /// </summary>
        JwkKeyContainer,

        /// <summary>
        ///     Represents a U-Prove root key.
        /// </summary>
        UProveKey
    }

    /// <summary>
    /// Extensionts for ClaimsPrincipalType
    /// </summary>
    public static class SecretTypeTypeExtensions
    {
        /// <summary>
        /// To lower case string
        /// </summary>
        /// <param name="type">Secret type</param>
        /// <returns>Lower case string</returns>
        public static string ToLowerInvariantString(this SecretType type)
        {
            return type.ToString().ToLowerInvariant();
        }
    }
}