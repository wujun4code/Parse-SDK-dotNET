using System;
using System.Threading;
using System.Threading.Tasks;

namespace LeanCloud.Core.Internal
{
    public interface IAppRouterController
    {
        Task<AppRouterState> GetAsync(CancellationToken cancellationToken);
        /// <summary>
        /// Start refresh the app router. This method will run forever if called.
        /// </summary>
        /// <param name="delay"></param>
        /// <returns></returns>
        Task StartRefreshAsync(TimeSpan delay);
        /// <summary>
        /// Stop refresh app router.
        /// </summary>
        void StopRefresh();
    }
}
