using Android.Content;
using Android.OS;
using Android.App;
using System;
using System.Collections.Generic;
namespace LeanCloud.Internal {
  /// <summary>
  /// A helper class to provide the ability to start a <see cref="Service"/> that ensures
  /// that the device does not go back to sleep during the <see cref="Service"/> work.
  /// </summary>
  /// Adopted and translated from http://grepcode.com/file/repository.grepcode.com/java/ext/com.google.android/android/4.3.1_r1/android/support/v4/content/WakefulBroadcastReceiver.java
  /// We can't implement WakefulBroadcastReceiver because C# doesn't support private inheritance.
  /// If ParsePushBroadcastReceiver were to use WakefulBroadcastReceiver, we'll be forced to publicize
  /// our WakefulBroadcastReceiver which isn't desirable.
  internal class AVWakefulHelper {
    private const string ExtraWakeLockId = "com.parse.wakelockid";
    private const int WakeLockTimeout = 60 * 1000;

    private static readonly Object activeWakeLocksMutex = new Object();
    private static readonly IDictionary<int, PowerManager.WakeLock> activeWakeLocks = new Dictionary<int, PowerManager.WakeLock>();

    private static int nextId = 1;

    /// <summary>
    /// Do a <see cref="Context.StartService(Intent)"/>, but holding a wake lock while the service starts.
    /// </summary>
    /// <remarks>
    /// This will modify the intent to hold an extra identifying the wake lock. When the service receives it
    /// in <see cref="Service.OnStartCommand"/>, it should pass back the <see cref="Intent"/> it receives there to
    /// <see cref="CompleteWakefulIntent(Intent)"/> in order to release the wake lock.
    /// </remarks>
    /// <param name="context">The <see cref="Context"/> in which it operate.</param>
    /// <param name="intent">The <see cref="Intent"/> with which to start the service, as per <see cref="Context.StartService(Intent)"/>.</param>
    /// <returns>The <see cref="ComponentName"/> of the <see cref="Service"/> being started.</returns>
    internal static ComponentName StartWakefulService(Context context, Intent intent) {
      lock (activeWakeLocksMutex) {
        int id = nextId;
        nextId++;
        if (nextId <= 0) {
          nextId = 1;
        }

        intent.PutExtra(ExtraWakeLockId, id);
        ComponentName comp = context.StartService(intent);
        if (comp == null) {
          return null;
        }

        PowerManager pm = PowerManager.FromContext(context);
        PowerManager.WakeLock wl = pm.NewWakeLock(WakeLockFlags.Partial, "wake: " + comp.FlattenToShortString());
        wl.SetReferenceCounted(false);
        wl.Acquire(WakeLockTimeout);
        activeWakeLocks[id] = wl;

        return comp;
      }
    }

    /// <summary>
    /// Finish the execution from previous <see cref="StartWakefulService(Context, Intent)"/>.
    /// </summary>
    /// <remarks>
    /// Any wake lock that was being held will now be released.
    /// </remarks>
    /// <param name="intent">The <see cref="Intent"/> that was originally generated by <see cref="StartWakefulService(Context, Intent)"/></param>
    /// <returns><c>true</c> if the intent is associated with a wake lock that is now released.
    /// Returns <c>false</c> if there was no wake lock specified for it.</returns>
    internal static bool CompleteWakefulIntent(Intent intent) {
      int id = intent.GetIntExtra(ExtraWakeLockId, 0);
      if (id == 0) {
        return false;
      }

      lock (activeWakeLocksMutex) {
        PowerManager.WakeLock wl = activeWakeLocks[id];
        if (wl != null) {
          wl.Release();
          activeWakeLocks.Remove(id);

          return true;
        }

        // We return true whether or not we actually found the wake lock
        // the return code is defined to indicate whether the Intent contained
        // an identifier for a wake lock that it was supposed to match.
        // We just log a warning here if there is no wake lock found, which could
        // happen for example if this function is called twice on the same
        // intent or the process is killed and restarted before processing the intent.
        Android.Util.Log.Warn("AVWakefulHelper", "No active wake lock id #" + id);
        return true;
      }
    }
  }
}