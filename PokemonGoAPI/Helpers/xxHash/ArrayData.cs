using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Data.HashFunction.Utilities.UnifiedData
{
    internal class ArrayData
        : UnifiedData
    {
        /// <inheritdoc />
        public override long Length { get { return _Data.Length; } }

        
        protected readonly byte[] _Data;


        /// <summary>
        /// Initializes a new instance of the <see cref="ArrayData"/> class.
        /// </summary>
        /// <param name="data">The data to represent.</param>
        public ArrayData(byte[] data)
        {
            _Data = data;
            BufferSize = (
                _Data.Length > 0 ?
                _Data.Length :
                1);
        }



        /// <inheritdoc />
        public override void ForEachRead(Action<byte[], int, int> action)
        {
            if (action == null)
                throw new ArgumentNullException("action");


            action(_Data, 0, _Data.Length);
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
        public override void ForEachGroup(int groupSize, Action<byte[], int, int> action, Action<byte[], int, int> remainderAction)
        {
            if (groupSize <= 0)
                throw new ArgumentOutOfRangeException("groupSize", "bufferSize must be greater than 0.");

            if (action == null)
                throw new ArgumentNullException("action");


            var remainderLength = _Data.Length % groupSize;

            if (_Data.Length - remainderLength > 0)
                action(_Data, 0, _Data.Length - remainderLength);

            if (remainderAction != null)
            {
                if (remainderLength > 0)
                    remainderAction(_Data, _Data.Length - remainderLength, remainderLength);
            }
        }

#if !NET40 || INCLUDE_ASYNC
        /// <inheritdoc />
        public override Task ForEachGroupAsync(int groupSize, Action<byte[], int, int> action, Action<byte[], int, int> remainderAction)
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
            return _Data;
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
