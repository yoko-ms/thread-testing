// ----------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------

using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace TestThreading.CosmosDB
{
    /// <summary>
    ///     Policy metadata
    /// </summary>
    public class RecordMetadata
    {
        /// <summary>
        /// Gets or sets the policyId
        /// </summary>
        [JsonProperty(PropertyName = "policyId", NullValueHandling = NullValueHandling.Ignore)]
        [Obsolete("should use 'stringData' instead")]
        public string policyId { get; set; }

        /// <summary>
        /// Gets or sets the user flow type
        /// </summary>
        [JsonProperty(PropertyName = "userFlowType", NullValueHandling = NullValueHandling.Ignore)]
        [Obsolete("should use 'stringData' instead")]
        public string userFlowType { get; set; }

        /// <summary>
        /// Gets or sets the boolean values for policy
        /// </summary>
        [JsonProperty(PropertyName = "booleanData", NullValueHandling = NullValueHandling.Ignore)]
        public IDictionary<string, bool> booleanData { get; private set; } = new Dictionary<string, bool>();

        /// <summary>
        /// Gets or sets the string metadata values for the record
        /// </summary>
        [JsonProperty(PropertyName = "stringData", NullValueHandling = NullValueHandling.Ignore)]
        public IDictionary<string, string> StringData { get; private set; } = new Dictionary<string, string>();
    }
}
