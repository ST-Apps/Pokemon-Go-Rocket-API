using System.Threading.Tasks;

namespace System.Data.HashFunction.Utilities.UnifiedData
{
    internal class ArrayData
        : UnifiedData
    {
        protected readonly byte[] Data;


        /// <summary>
        ///     Initializes a new instance of the <see cref="ArrayData" /> class.
        /// </summary>
        /// <param name="data">The data to represent.</param>
        public ArrayData(byte[] data)
        {
            Data = data;
            BufferSize = Data.Length > 0
                ? Data.Length
                : 1;
        }

        /// <inheritdoc />
        public override long Length => Data.Length;


        /// <inheritdoc />
        public override void ForEachRead(Action<byte[], int, int> action)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));


            action(Data, 0, Data.Length);
        }

#if !NET40 || INCLUDE_ASYNC
        /// <inheritdoc />
        public override Task ForEachReadAsync(Action<byte[], int, int> action)
        {
            ForEachRead(action);

#if !INCLUDE_ASYNC
            return Task.FromResult(true);
#else
            return TaskEx.FromResult(true);
#endif
        }
#endif


        /// <inheritdoc />
        public override void ForEachGroup(int groupSize, Action<byte[], int, int> action,
            Action<byte[], int, int> remainderAction)
        {
            if (groupSize <= 0)
                throw new ArgumentOutOfRangeException(nameof(groupSize), "bufferSize must be greater than 0.");

            if (action == null)
                throw new ArgumentNullException(nameof(action));


            var remainderLength = Data.Length%groupSize;

            if (Data.Length - remainderLength > 0)
                action(Data, 0, Data.Length - remainderLength);

            if (remainderAction != null)
            {
                if (remainderLength > 0)
                    remainderAction(Data, Data.Length - remainderLength, remainderLength);
            }
        }

#if !NET40 || INCLUDE_ASYNC
        /// <inheritdoc />
        public override Task ForEachGroupAsync(int groupSize, Action<byte[], int, int> action,
            Action<byte[], int, int> remainderAction)
        {
            ForEachGroup(groupSize, action, remainderAction);

#if !INCLUDE_ASYNC
            return Task.FromResult(true);
#else
            return TaskEx.FromResult(true);
#endif
        }
#endif


        /// <inheritdoc />
        public override byte[] ToArray()
        {
            return Data;
        }

#if !NET40 || INCLUDE_ASYNC
        /// <inheritdoc />
        public override Task<byte[]> ToArrayAsync()
        {
#if !INCLUDE_ASYNC
            return Task.FromResult(
                ToArray());
#else
            return TaskEx.FromResult(
                ToArray());
#endif
        }
#endif
    }
}