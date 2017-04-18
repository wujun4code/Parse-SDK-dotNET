// Copyright (c) 2015-present, Parse, LLC.  All rights reserved.  This Source code is licensed under the BSD-style license found in the LICENSE file in the root directory of this Source tree.  An additional grant of patent rights can be found in the PATENTS file in the same directory.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace LeanCloud.Core.Internal
{
    public interface IAVObjectController
    {
        //Task<IObjectState> FetchAsync(IObjectState state,
        //    string sessionToken,
        //    CancellationToken cancellationToken);

        Task<IObjectState> FetchAsync(IObjectState state,
            IDictionary<string,object> queryString,
            string sessionToken,
            CancellationToken cancellationToken);

        Task<IObjectState> SaveAsync(IObjectState state,
            IDictionary<string, IAVFieldOperation> operations,
            string sessionToken,
            CancellationToken cancellationToken);

        IList<Task<IObjectState>> SaveAllAsync(IList<IObjectState> states,
            IList<IDictionary<string, IAVFieldOperation>> operationsList,
            string sessionToken,
            CancellationToken cancellationToken);

        Task DeleteAsync(IObjectState state,
            string sessionToken,
            CancellationToken cancellationToken);

        IList<Task> DeleteAllAsync(IList<IObjectState> states,
            string sessionToken,
            CancellationToken cancellationToken);
    }
}
