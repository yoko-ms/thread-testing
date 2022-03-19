// ----------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------

using System.Collections.Generic;
using Newtonsoft.Json;

namespace TestThreading.CosmosDB.TestRunners
{
    /// <summary>
    /// Tenant metadata.
    /// </summary>
    public class TenantMetadata
    {
        /// <summary>
        /// Gets or sets the unique identifier of the tenant.
        /// </summary>
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the initial domain hint.
        /// </summary>
        [JsonProperty(PropertyName = "initialDomainHint")]
        public string InitialDomainHint { get; set; }

        /// <summary>
        /// Gets or sets the region.
        /// </summary>
        /// <remarks>
        /// Region is from eSTS wellknown endpoint, tenant_region_scope field.
        /// </remarks>
        [JsonProperty(PropertyName = "region")]
        public string Region { get; set; }

        /// <summary>
        /// Gets the features.
        /// </summary>
        [JsonProperty(PropertyName = "settings")]
        public IDictionary<string, Dictionary<string, object>> Settings { get; internal set; }
    }
}
