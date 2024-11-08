﻿// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using System.Threading;
using System.Threading.Tasks;

namespace Tsavorite.core
{
    /// <summary>
    /// The Tsavorite key-value store
    /// </summary>
    public partial class TsavoriteKV<TKey, TValue, TStoreFunctions, TAllocator> : TsavoriteBase
        where TStoreFunctions : IStoreFunctions<TKey, TValue>
        where TAllocator : IAllocator<TKey, TValue, TStoreFunctions>
    {
        /// <summary>
        /// Check if at least one (sync) request is ready for CompletePending to operate on
        /// </summary>
        /// <param name="sessionCtx"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        internal static ValueTask ReadyToCompletePendingAsync<TInput, TOutput, TContext>(ExecutionContext<TInput, TOutput, TContext> sessionCtx, CancellationToken token = default)
            => sessionCtx.WaitPendingAsync(token);

#if false // TODO CompletePendingAsync
        /// <summary>
        /// Complete outstanding pending operations that were issued synchronously
        /// Async operations (e.g., ReadAsync) need to be completed individually
        /// </summary>
        /// <returns></returns>
        internal async ValueTask CompletePendingAsync<TInput, TOutput, TContext, TSessionFunctionsWrapper, TKernelSession, TKeyLocker>(TSessionFunctionsWrapper sessionFunctions,
                                      ref TKernelSession kernelSession, CancellationToken token, CompletedOutputIterator<TKey, TValue, TInput, TOutput, TContext> completedOutputs)
            where TSessionFunctionsWrapper : ISessionFunctionsWrapper<TKey, TValue, TInput, TOutput, TContext, TStoreFunctions, TAllocator>
            where TKernelSession : IKernelSession
            where TKeyLocker : struct, IKeyLocker
        {
            while (true)
            {
                sessionFunctions.UnsafeResumeThread();
                try
                {
                    InternalCompletePendingRequests<TInput, TOutput, TContext, TSessionFunctionsWrapper, TKernelSession, TKeyLocker>(sessionFunctions, ref kernelSession, completedOutputs);
                }
                finally
                {
                    sessionFunctions.UnsafeSuspendThread();
                }

                await sessionFunctions.ExecutionCtx.WaitPendingAsync(token).ConfigureAwait(false);

                if (sessionFunctions.ExecutionCtx.HasNoPendingRequests)
                    return;

                InternalRefresh(sessionFunctions.ExecutionCtx);

                Thread.Yield();
            }
        }
#endif
    }
}