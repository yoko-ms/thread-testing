// ----------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Net;

namespace TestThreading.CosmosDB
{
    /// <summary>
    /// Represents the metadata with each Cosmos DB request.
    /// </summary>
    public class CosmosDbRequestMetadata
    {
        /// <summary>
        /// Gets or sets the Etag.
        /// </summary>
        public string Etag { get; set; }

        /// <summary>
        /// Gets or sets the activity ID.
        /// </summary>
        /// <remarks>
        /// Activity ID can be used to track the request at Cosmos DB service side.
        /// </remarks>
        public string ActivityId { get; set; }

        /// <summary>
        /// Gets or sets the request charge(RU).
        /// </summary>
        public double? RequestCharge { get; set; }

        /// <summary>
        /// Gets or sets the last modified date time.
        /// </summary>
        public DateTime? LastModifiedDateTime { get; set; }

        /// <summary>
        /// Gets or sets the request latency.
        /// </summary>
        public TimeSpan? RequestLatency { get; set; }

        /// <summary>
        /// Gets or sets the status code.
        /// </summary>
        public HttpStatusCode? StatusCode { get; set; }

        /// <summary>
        /// Gets or sets the document size in KB.
        /// </summary>
        /// <remarks>It's incorrectly named. It's a total length of documents in the same collection.</remarks>
        public long? DocumentSizeInKB { get; set; }

        /// <summary>
        /// Gets or sets additional note.
        /// </summary>
        /// <remarks>
        /// Currently used for capture exceptions while collecting the metadata.
        /// </remarks>
        public string Note { get; set; }

        /// <summary>
        /// Gets or sets the request diagnostic string.
        /// </summary>
        /// <remarks>
        /// String containing diagnostic information:
        /// TargetEndpoint; RequestStartTime; ResponseTime; Number of regions attempted; StorePhysicalAddress; PartitionKeyRangeId; StatusCode; IsGone; IsNotFound; IsInvalidPartition; RequestCharge; ItemLSN; SessionToken; ResourceType; OperationType; AddressResolution: StartTime, EndTime; 
        /// </remarks>
        public string RequestDiagnosticsString { get; set; }

        /// <summary>
        /// Gets or sets the count of documents being returned.
        /// </summary>
        /// <remarks>
        /// count of documents returned 
        /// </remarks>
        public int? Count { get; set; }
    }
}
