using System;
using System.Threading;
using PokemonGo.RocketAPI;

namespace PokemonGo_UWP.Utils
{
    public class AsyncSynchronizationContext : SynchronizationContext
    {
        public static AsyncSynchronizationContext Register()
        {
            var syncContext = Current;
            if (syncContext == null)
                throw new InvalidOperationException("Ensure a synchronization context exists before calling this method.");

            var customSynchronizationContext = syncContext as AsyncSynchronizationContext;

            if (customSynchronizationContext == null)
            {
                customSynchronizationContext = new AsyncSynchronizationContext(syncContext);
                SetSynchronizationContext(customSynchronizationContext);
            }

            return customSynchronizationContext;
        }

        private readonly SynchronizationContext _syncContext;

        public AsyncSynchronizationContext(SynchronizationContext syncContext)
        {
            _syncContext = syncContext;
        }

        public override SynchronizationContext CreateCopy()
        {
            return new AsyncSynchronizationContext(_syncContext.CreateCopy());
        }

        public override void OperationCompleted()
        {
            _syncContext.OperationCompleted();
        }

        public override void OperationStarted()
        {
            _syncContext.OperationStarted();
        }

        public override void Post(SendOrPostCallback d, object state)
        {
            _syncContext.Post(WrapCallback(d), state);
        }

        public override void Send(SendOrPostCallback d, object state)
        {
            _syncContext.Send(d, state);
        }

        private static SendOrPostCallback WrapCallback(SendOrPostCallback sendOrPostCallback)
        {
            return state =>
            {
                Exception exception = null;

                try
                {
                    sendOrPostCallback(state);
                }
                catch (Exception ex)
                {
                    exception = ex;
                }

                if (exception != null)
                    Logger.Write(exception.Message);
            };
        }
    }
}
