using UnityEngine;
using UnityEngine.Profiling;

public class MemoryTracker
{
    private float peakMemory = 0f;
    private long baselineMemory = 0;
    private long baselineMono = 0;

    public void UpdateMemoryTracking()
    {
        float currentMemory = GetCurrentMemoryUsage();
        if (currentMemory > peakMemory)
        {
            peakMemory = currentMemory;
        }
    }

    public float GetPeakMemoryTracker()
    {
        return peakMemory;
    }

    private float GetCurrentMemoryUsage()
    {
        return Profiler.GetTotalAllocatedMemoryLong() / 1048576f; // Convert to MB
    }

    // NEW METHODS - Add these:
    
    /// <summary>
    /// Call this BEFORE generating the city to establish baseline
    /// </summary>
    public void RecordBaseline()
    {
        baselineMemory = Profiler.GetTotalAllocatedMemoryLong();
        baselineMono = Profiler.GetMonoUsedSizeLong();
        
        UnityEngine.Debug.Log("=== MEMORY BASELINE ===");
        UnityEngine.Debug.Log($"Total Unity Memory: {baselineMemory / 1048576f:F2} MB");
        UnityEngine.Debug.Log($"Mono Heap Used: {baselineMono / 1048576f:F2} MB");
        UnityEngine.Debug.Log("=======================");
    }
    
    /// <summary>
    /// Call this AFTER generating the city to see incremental cost
    /// </summary>
    public void LogIncrementalMemory()
    {
        long currentMemory = Profiler.GetTotalAllocatedMemoryLong();
        long currentMono = Profiler.GetMonoUsedSizeLong();
        
        long incrementalMemory = currentMemory - baselineMemory;
        long incrementalMono = currentMono - baselineMono;
        
        UnityEngine.Debug.Log("=== INCREMENTAL MEMORY USAGE ===");
        UnityEngine.Debug.Log($"Total Unity Memory: {currentMemory / 1048576f:F2} MB");
        UnityEngine.Debug.Log($"Incremental Cost: {incrementalMemory / 1048576f:F2} MB");
        UnityEngine.Debug.Log($"Mono Heap Used: {currentMono / 1048576f:F2} MB");
        UnityEngine.Debug.Log($"Incremental Mono: {incrementalMono / 1048576f:F2} MB");
        UnityEngine.Debug.Log("================================");
    }
    
    /// <summary>
    /// Detailed memory breakdown
    /// </summary>
    public void LogDetailedMemory()
    {
        long totalReserved = Profiler.GetTotalReservedMemoryLong();
        long totalAllocated = Profiler.GetTotalAllocatedMemoryLong();
        long totalUnused = Profiler.GetTotalUnusedReservedMemoryLong();
        
        long monoHeap = Profiler.GetMonoHeapSizeLong();
        long monoUsed = Profiler.GetMonoUsedSizeLong();
        
        long graphicsMemory = Profiler.GetAllocatedMemoryForGraphicsDriver();
        
        UnityEngine.Debug.Log("=== DETAILED MEMORY BREAKDOWN ===");
        UnityEngine.Debug.Log($"Total Reserved: {totalReserved / 1048576f:F2} MB");
        UnityEngine.Debug.Log($"Total Allocated: {totalAllocated / 1048576f:F2} MB");
        UnityEngine.Debug.Log($"Total Unused: {totalUnused / 1048576f:F2} MB");
        UnityEngine.Debug.Log($"Mono Heap Size: {monoHeap / 1048576f:F2} MB");
        UnityEngine.Debug.Log($"Mono Used: {monoUsed / 1048576f:F2} MB");
        UnityEngine.Debug.Log($"Graphics Driver: {graphicsMemory / 1048576f:F2} MB");
        UnityEngine.Debug.Log("=================================");
    }
}
