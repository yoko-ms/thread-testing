// ----------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------

using System;

namespace TestThreading.CosmosDB
{
    /// <summary>
    /// Data holder having the result of the last Cosmos DB query.
    /// </summary>
    /// <typeparam name="T">Data type.</typeparam>
    public class CosmosDbQueryResult<T>
    {
        /// <summary>
        /// Gets or sets the internal data.
        /// </summary>
        public T DataObject { get; set; }

        /// <summary>
        /// Gets or sets etag value.
        /// </summary>
        public string Etag { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CosmosDbQueryResult{T}"/> class.
        /// </summary>
        public CosmosDbQueryResult()
        {
            this.DataObject = default(T);
            this.Etag = String.Empty;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CosmosDbQueryResult{T}"/> class.
        /// </summary>
        /// <param name="dataObject">Data object read from doc db.</param>
        public CosmosDbQueryResult(T dataObject)
        {
            this.DataObject = dataObject;
            this.Etag = String.Empty;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CosmosDbQueryResult{T}"/> class.
        /// </summary>
        /// <param name="eTag">Etag string.</param>
        public CosmosDbQueryResult(string eTag)
        {
            this.DataObject = default(T);
            this.Etag = eTag;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CosmosDbQueryResult{T}"/> class.
        /// </summary>
        /// <param name="dataObject">Data object read from doc db.</param>
        /// <param name="eTag">Etag string.</param>
        public CosmosDbQueryResult(T dataObject, string eTag)
        {
            this.DataObject = dataObject;
            this.Etag = eTag;
        }
    }
}
