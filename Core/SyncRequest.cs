namespace SyncUpgrades.Core;

internal record SyncRequest(SyncBundle Bundle) : ISyncRequest
{
    public void Run()
    {
        #if DEBUG
        Entry.LogSource.LogInfo($"[NETWORKING] [{nameof(this.Run)}] [{this.Bundle}]");
        #endif
        this.Bundle.Manager.SyncAllDictionaries();
    }

    public static ISyncRequest New(SyncBundle bundle)
        => new SyncRequest(bundle);
}