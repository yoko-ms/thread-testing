// ----------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------

namespace TestThreading.CosmosDB.TestRunners
{
    /// <summary>
    /// Represents a wrapper class which holds the actual object.
    /// </summary>
    /// <typeparam name="T">The type.</typeparam>
    public class NullableCachedItem<T>
    {
        /// <summary>
        /// Gets or sets the item.
        /// </summary>
        public T Item { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="NullableCachedItem{T}"/> class.
        /// </summary>
        /// <param name="item">The item.</param>
        public NullableCachedItem(T item)
        {
            this.Item = item;
        }
    }
}