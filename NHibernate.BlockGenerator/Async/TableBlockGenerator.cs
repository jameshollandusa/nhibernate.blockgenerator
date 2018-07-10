// This code is modeled on the auto-generated async code for the TableHiLoGenerator.

using System;
using System.Runtime.CompilerServices;
using NHibernate.Engine;

namespace NHibernate.Id
{
    using System.Threading;
    using System.Threading.Tasks;
    public partial class TableBlockGenerator
    {
        private readonly NHibernate.Util.AsyncLock _generate = new NHibernate.Util.AsyncLock();

        #region IIdentifierGenerator Members

        /// <summary>
        /// Generate a <see cref="Int64"/> for the identifier by selecting and updating a value in a table.
        /// </summary>
        /// <param name="session">The <see cref="ISessionImplementor"/> this id is being generated in.</param>
        /// <param name="obj">The entity for which the id is being generated.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the work</param>
        /// <returns>The new identifier as a <see cref="Int64"/>.</returns>
        [MethodImpl]
        public override async Task<object> GenerateAsync(ISessionImplementor session, object obj, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            using (await _generate.LockAsync())
            {
                // Note these checks are subtlely different from the similar code in TableHiLoGenerator but
                // this is by design and appropriate for the block generator algorithm.
                if (blockSize < 2)
                {
                    // Behave as if block size is 1, and don't allow an ID of 0.
                    //keep the behavior consistent even for boundary usages
                    long val = Convert.ToInt64(await (base.GenerateAsync(session, obj, cancellationToken)).ConfigureAwait(false));
                    if (val == 0)
                        val = Convert.ToInt64(await (base.GenerateAsync(session, obj, cancellationToken)).ConfigureAwait(false));
                    return IdentifierGeneratorFactory.CreateNumber(val, returnClass);
                }

                if (lo >= blockSize)
                {
                    hi = Convert.ToInt64(await (base.GenerateAsync(session, obj, cancellationToken)).ConfigureAwait(false));
                    // Block size is at least 2 here, so we can skip 0 and carry on.
                    lo = (hi == 0) ? 1 : 0;
                    log.Debug("New id value: {0}", hi);
                }

                return IdentifierGeneratorFactory.CreateNumber(hi + lo++, returnClass);
            }
        }

        #endregion
    }
}