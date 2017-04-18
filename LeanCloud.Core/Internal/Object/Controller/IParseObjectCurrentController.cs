// Copyright (c) 2015-present, Parse, LLC.  All rights reserved.  This Source code is licensed under the BSD-style license found in the LICENSE file in the root directory of this Source tree.  An additional grant of patent rights can be found in the PATENTS file in the same directory.

using System;
using System.Threading;
using System.Threading.Tasks;

namespace LeanCloud.Core.Internal {
  /// <summary>
  /// <code>IAVObjectCurrentController</code> controls the single-instance <see cref="AVObject"/>
  /// persistence used throughout the code-base. Sample usages are <see cref="ParseUser.CurrentUser"/> and
  /// <see cref="ParseInstallation.CurrentInstallation"/>.
  /// </summary>
  /// <typeparam name="T">Type of object being persisted.</typeparam>
  public interface IAVObjectCurrentController<T> where T : AVObject {
    /// <summary>
    /// Persists current <see cref="AVObject"/>.
    /// </summary>
    /// <param name="obj"><see cref="AVObject"/> to be persisted.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task SetAsync(T obj, CancellationToken cancellationToken);

    /// <summary>
    /// Gets the persisted current <see cref="AVObject"/>.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task<T> GetAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Returns a <see cref="Task"/> that resolves to <code>true</code> if current
    /// <see cref="AVObject"/> exists.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task<bool> ExistsAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Returns <code>true</code> if the given <see cref="AVObject"/> is the persisted current
    /// <see cref="AVObject"/>.
    /// </summary>
    /// <param name="obj">The object to check.</param>
    /// <returns>True if <code>obj</code> is the current persisted <see cref="AVObject"/>.</returns>
    bool IsCurrent(T obj);

    /// <summary>
    /// Nullifies the current <see cref="AVObject"/> from memory.
    /// </summary>
    void ClearFromMemory();

    /// <summary>
    /// Clears current <see cref="AVObject"/> from disk.
    /// </summary>
    void ClearFromDisk();
  }
}
