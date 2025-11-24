// Assets/_RemoteConfig/RCBootstrap.cs
using Firebase;
using System.Threading;
using System.Threading.Tasks;

public static class RCBootstrap
{
    static Task<DependencyStatus> _t;
    public static Task<DependencyStatus> Ensure() =>
        _t ??= FirebaseApp.CheckAndFixDependenciesAsync();

    // NEW: RC calls ko queue karo
    public static readonly SemaphoreSlim RcLock = new(1, 1);
}


// FirebaseBootstrap removed - using RCBootstrap.Ensure() as single source
