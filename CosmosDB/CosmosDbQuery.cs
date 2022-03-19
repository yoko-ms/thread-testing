// ----------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------

using Microsoft.Azure.Cosmos;

namespace TestThreading.CosmosDB
{
    /// <summary>
    /// Wrapper for document query results for Cosmos DB interface.
    /// </summary>
    public class CosmosDbQuery<T>
    {
        /// <summary>
        /// Gets a <see cref="FeedIterator{T}"/> object used to query document items for V3 interface.
        /// </summary>
        public FeedIterator<T> FeedIterator { get; private set; }

        /// <summary>
        /// Gets the saved query string.
        /// </summary>
        public string QueryString { get; private set; }

        /// <summary>
        ///  Initializes a new instance of the <see cref="CosmosDbQuery{T}"/> class with V3 interface.
        /// </summary>
        /// <param name="feedIterator">A <see cref="FeedIterator{T}"/> object.</param>
        /// <param name="queryString">Query string to save.</param>
        public CosmosDbQuery(FeedIterator<T> feedIterator, string queryString = null)
        {
            this.QueryString = queryString;
            this.FeedIterator = feedIterator;
        }

        /// <summary>
        /// Gets a value indicating whether returns if there are more results
        /// </summary>
        public bool HasMoreResults => this.FeedIterator.HasMoreResults;
    }
}
