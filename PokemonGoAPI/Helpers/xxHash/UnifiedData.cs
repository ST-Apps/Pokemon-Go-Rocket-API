using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Data.HashFunction.Utilities.UnifiedData
{
    /// <summary>
    /// Centralized methodology for accessing data used by Data.HashFunction.
    /// </summary>
    public abstract class UnifiedData
    {
        /// <summary>
        /// Length of data provided.
        /// </summary>
        /// <remarks>
        /// Implementors are allowed throw an exception if it is not possible to resolve the length of the data.
        /// </remarks>
        public abstract long Length { get; }

        /// <summary>
        /// Length of temporary buffers used, if they are needed.
        /// </summary>
        /// <remarks>
        /// Implementors are not required to use this value.
        /// </remarks>
        public virtual int BufferSize 
        {
            get { return _BufferSize; }
            set
            {
                if (value <= 0)
                    throw new ArgumentOutOfRangeException("value", "value must be greater than 0");

                _BufferSize = value;
            }
        }


        private int _BufferSize = 4096;



        /// <summary>
        /// Executes an action each time a chunk is read.
        /// </summary>
        /// <param name="action">Function to execute.</param>
        public abstract void ForEachRead(Action<byte[], int, int> action);


#if !NET40 || INCLUDE_ASYNC
        /// <inheritdoc cref="ForEachRead(Action{byte[], int, int})" />
        /// <returns>Task representing the asynchronous operation.</returns>
        public abstract Task ForEachReadAsync(Action<byte[], int, int> action);
#endif



        /// <summary>
        /// Executes an action one or more times, providing the data read as an array whose length is a multiple of groupSize.  
        /// Optionally runs an action on the final remainder group.
        /// </summary>
        /// <param name="groupSize">Length of the groups passed to the action.</param>
        /// <param name="action">Action to execute for each full group read.</param>
        /// <param name="remainderAction">Action to execute if the final group is less than groupSize.  Null values are allowed.</param>
        /// <remarks>remainderAction will not be run if the length of the data is a multiple of groupSize.</remarks>
        public abstract void ForEachGroup(int groupSize, Action<byte[], int, int> action, Action<byte[], int, int> remainderAction);
        
#if !NET40 || INCLUDE_ASYNC
        /// <inheritdoc cref="ForEachGroup(int, Action{byte[], int, int}, Action{byte[], int, int})" />
        /// <returns>Task representing the asynchronous operation.</returns>
        public abstract Task ForEachGroupAsync(int groupSize, Action<byte[], int, int> action, Action<byte[], int, int> remainderAction);
#endif



        /// <summary>
        /// Reads all data and converts it to an in-memory array.
        /// </summary>
        /// <returns>Array of bytes read from the data provider.</returns>
        public abstract byte[] ToArray();
        
#if !NET40 || INCLUDE_ASYNC
        /// <inheritdoc cref="ToArray()" />
        public abstract Task<byte[]> ToArrayAsync();
#endif
    }
}
