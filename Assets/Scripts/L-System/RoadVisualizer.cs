using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;

public class RoadVisualizer : MonoBehaviour
{
    [SerializeField]
    private LSystemGenerator generator;
    
    public RoadParam roadParam;
    public Structure structureManager;
    
    [Header("Road Length Variation")]
    [SerializeField] private bool varyRoadLength = true;
    [SerializeField] private int minRoadLength = 4;
    [SerializeField] private int maxRoadLength = 12;
    
    [Header("Road Settings")]
    public int roadLength = 8;
    private int currentStepSize = 8;
    
    [Header("Agent Settings")]
    private const float ROTATION_ANGLE = 90f;
    private bool isPlacingRoad = false;
    
    [Header("Debug")]
    [SerializeField]
    private bool fixRoadConnections = true;
    
    private MemoryTracker memoryTracker = new MemoryTracker();
    private Stopwatch generationTimer;
    private Stopwatch phaseTimer;
    private long lSystemGenerationTime;
    private long roadPlacementTime;
    private long connectionFixingTime;
    private long structurePlacementTime;
    
    
    
    [ContextMenu("Debug L-System Setup")]
    private void DebugSetup()
    {
        UnityEngine.Debug.Log("=== L-SYSTEM DEBUG ===");
        UnityEngine.Debug.Log($"Axiom: '{generator.axiom}'");
        UnityEngine.Debug.Log($"Iterations: {generator.iterationLimit}");
        UnityEngine.Debug.Log($"Rules Count: {generator.rules?.Length ?? 0}");
    
        if (generator.rules != null)
        {
            for (int i = 0; i < generator.rules.Length; i++)
            {
                var rule = generator.rules[i];
                if (rule != null)
                {
                    UnityEngine.Debug.Log($"Rule {i}: Variable='{rule.variable}', Output='{rule.GetWord()}'");
                }
                else
                {
                    UnityEngine.Debug.LogError($"Rule {i} is NULL!");
                }
            }
        }
    
        string result = generator.GenerateSentence();
        UnityEngine.Debug.Log($"Final Result: {result}");
        UnityEngine.Debug.Log("=====================");
    }

    private void Start()
    {
        roadParam.OnRoadPlacementComplete += OnRoadPlaced;
        GenerateCity();
    }

    private void OnDestroy()
    {
        roadParam.OnRoadPlacementComplete -= OnRoadPlaced;
    }

    private void OnRoadPlaced()
    {
        isPlacingRoad = false;
    }

    // public void GenerateCity()
    // {
    //     // Reset for new generation
    //     currentStepSize = roadLength;
    //     roadParam.Reset();
    //     structureManager.Reset();
    //     
    //     string sentence = generator.GenerateSentence();
    //     memoryTracker.UpdateMemoryTracking();
    //
    //     generationTimer = Stopwatch.StartNew();
    //     UnityEngine.Debug.Log("=== STARTING CITY GENERATION ===");
    //     
    //     StartCoroutine(VisualizeCity(sentence));
    // }
    // In RoadVisualizer.cs, update GenerateCity():
    public void GenerateCity()
    {
        generationTimer = Stopwatch.StartNew();
        UnityEngine.Debug.Log("<color=yellow>=== STARTING CITY GENERATION ===</color>");
    
        // Reset for new generation
        currentStepSize = roadLength;
        roadParam.Reset();
        structureManager.Reset();
    
        // 🔴 ADD THIS - Record memory BEFORE generation
        memoryTracker.RecordBaseline();
        memoryTracker.LogDetailedMemory();
    
        // Phase 1: L-System Generation
        phaseTimer = Stopwatch.StartNew();
        string sentence = generator.GenerateSentence();
        lSystemGenerationTime = phaseTimer.ElapsedMilliseconds;
        UnityEngine.Debug.Log($"<color=cyan>[Phase 1] L-System Generated in {lSystemGenerationTime}ms</color>");
        UnityEngine.Debug.Log($"<color=cyan>  Generated String Length: {sentence.Length}</color>");
        UnityEngine.Debug.Log($"<color=cyan>  Axiom: {generator.axiom}</color>");
        UnityEngine.Debug.Log($"<color=cyan>  Iterations: {generator.iterationLimit}</color>");

        memoryTracker.UpdateMemoryTracking();

        StartCoroutine(VisualizeCity(sentence));
        // currentStepSize = roadLength;
        // roadParam.Reset();
        // structureManager.Reset();
        //
        // string sentence = generator.GenerateSentence();
        //
        // // DEBUG: Print the generated sentence
        // UnityEngine.Debug.Log($"<color=cyan>Generated L-System: {sentence}</color>");
        // UnityEngine.Debug.Log($"<color=cyan>Axiom: {generator.axiom}</color>");
        // UnityEngine.Debug.Log($"<color=cyan>Iterations: {generator.iterationLimit}</color>");
        //
        // memoryTracker.UpdateMemoryTracking();
        // generationTimer = Stopwatch.StartNew();
        // UnityEngine.Debug.Log("=== STARTING CITY GENERATION ===");
        //
        // StartCoroutine(VisualizeCity(sentence));
    }

    
    private IEnumerator VisualizeCity(string sentence)
    {
        Stack<AgentState> savedStates = new Stack<AgentState>();
        
        Vector3 currentPosition = Vector3.zero;
        Vector3 direction = Vector3.forward;
        
        foreach (char letter in sentence)
        {
            // Wait if road is still being placed
            while (isPlacingRoad)
            {
                yield return null;
            }
            
            Letters command = ParseCommand(letter);

            switch (command)
            {
                case Letters.Save:
                    savedStates.Push(new AgentState
                    {
                        position = currentPosition,
                        direction = direction,
                        stepSize = currentStepSize
                    });
                    break;
                    
                case Letters.Load:
                    if (savedStates.Count > 0)
                    {
                        AgentState state = savedStates.Pop();
                        currentPosition = state.position;
                        direction = state.direction;
                        currentStepSize = state.stepSize;
                    }
                    else
                    {
                        UnityEngine.Debug.LogWarning("Attempted to load state from empty stack");
                    }
                    break;
                
                case Letters.Draw:
                    Vector3 startPosition = currentPosition;
    
                    // Vary road length for more organic look
                    int actualLength = varyRoadLength 
                        ? Random.Range(minRoadLength, maxRoadLength + 1)
                        : currentStepSize;
    
                    currentPosition += direction * actualLength;
    
                    StartCoroutine(roadParam.PlaceRoadSegment(startPosition, direction, actualLength));
                    isPlacingRoad = true;
    
                    yield return null;
                    break;

                    
                case Letters.TurnClockwise:
                    direction = Quaternion.AngleAxis(ROTATION_ANGLE, Vector3.up) * direction;
                    break;
                    
                case Letters.TurnCounterClockwise:
                    direction = Quaternion.AngleAxis(-ROTATION_ANGLE, Vector3.up) * direction;
                    break;
                    
                case Letters.Unknown:
                default:
                    // Ignore unknown characters
                    break;
            }
            
            memoryTracker.UpdateMemoryTracking();
        }

        // Wait for all roads to finish placing
        while (isPlacingRoad)
        {
            yield return null;
        }
        
        yield return new WaitForSeconds(0.1f);
        
        // Fix road connections if enabled
        if (fixRoadConnections)
        {
            roadParam.UpdateRoadConnections();
            memoryTracker.UpdateMemoryTracking();
        }
        
        phaseTimer = Stopwatch.StartNew();
        UnityEngine.Debug.Log("<color=blue>[Phase 4] Placing Structures...</color>");
        yield return StartCoroutine(structureManager.PlaceStructures(roadParam.GetRoadPositions()));
        structurePlacementTime = phaseTimer.ElapsedMilliseconds;
        UnityEngine.Debug.Log($"<color=blue>[Phase 4] Structures Placed in {structurePlacementTime}ms</color>");
    
        // 🔴 ADD THIS - Log memory AFTER generation
        memoryTracker.LogIncrementalMemory();
        memoryTracker.LogDetailedMemory();


        
        generationTimer.Stop();
        PrintGenerationReport();
    }
    
    private Letters ParseCommand(char letter)
    {
        switch (letter)
        {
            case '[': return Letters.Save;
            case ']': return Letters.Load;
            case 'F': return Letters.Draw;
            case '+': return Letters.TurnClockwise;
            case '-': return Letters.TurnCounterClockwise;
            default: return Letters.Unknown;
        }
    }
    
    private void PrintGenerationReport()
    {
         float totalMemory = memoryTracker.GetPeakMemoryTracker();
        int totalObjects = structureManager.GetBuildingCount() + roadParam.GetRoadSegmentCount();
        long totalTime = generationTimer.ElapsedMilliseconds;
        
        UnityEngine.Debug.Log("<color=yellow>╔════════════════════════════════════════════╗</color>");
        UnityEngine.Debug.Log("<color=yellow>║     CITY GENERATION COMPLETE               ║</color>");
        UnityEngine.Debug.Log("<color=yellow>╚════════════════════════════════════════════╝</color>");
        
        UnityEngine.Debug.Log($"\n<b>⏱️ TIMING BREAKDOWN:</b>");
        UnityEngine.Debug.Log($"  Total Execution Time: <b>{totalTime}ms ({totalTime / 1000f:F3}s)</b>");
        UnityEngine.Debug.Log($"  ├─ L-System Generation:    {lSystemGenerationTime,6}ms ({GetPercentage(lSystemGenerationTime, totalTime):F1}%)");
        UnityEngine.Debug.Log($"  ├─ Road Placement:         {roadPlacementTime,6}ms ({GetPercentage(roadPlacementTime, totalTime):F1}%)");
        UnityEngine.Debug.Log($"  ├─ Connection Fixing:      {connectionFixingTime,6}ms ({GetPercentage(connectionFixingTime, totalTime):F1}%)");
        UnityEngine.Debug.Log($"  └─ Structure Placement:    {structurePlacementTime,6}ms ({GetPercentage(structurePlacementTime, totalTime):F1}%)");
        
        UnityEngine.Debug.Log($"\n<b>🏗️ GENERATION STATS:</b>");
        UnityEngine.Debug.Log($"  Road Length: {roadLength}");
        UnityEngine.Debug.Log($"  Iteration Limit: {generator.iterationLimit}");
        
        UnityEngine.Debug.Log($"\n<b>📊 OBJECT COUNTS:</b>");
        UnityEngine.Debug.Log($"  Total Objects: <b>{totalObjects}</b>");
        UnityEngine.Debug.Log($"  ├─ Roads:      {roadParam.GetRoadSegmentCount(),6}");
        UnityEngine.Debug.Log($"  └─ Buildings:  {structureManager.GetBuildingCount(),6}");
        
        UnityEngine.Debug.Log($"\n<b>💾 MEMORY USAGE:</b>");
        UnityEngine.Debug.Log($"  Peak Memory: <b>{totalMemory:F2} MB</b>");
        
        UnityEngine.Debug.Log($"\n<b>⚡ PERFORMANCE:</b>");
        UnityEngine.Debug.Log($"  Objects/Second: {(totalObjects / (totalTime / 1000f)):F2}");
        UnityEngine.Debug.Log($"  Avg Time/Object: {(totalTime / (float)totalObjects):F2}ms");
        
        UnityEngine.Debug.Log("<color=yellow>════════════════════════════════════════════</color>\n");
    }
    private float GetPercentage(long value, long total)
    {
        if (total == 0) return 0;
        return (value / (float)total) * 100f;
    }
    // Enums for L-System commands
    private enum Letters
    {
        Unknown,
        Save,       // '['
        Load,       // ']'
        Draw,       // 'F'
        TurnClockwise,        // '+'
        TurnCounterClockwise  // '-'
    }

    // Helper class to store agent state
    private class AgentState
    {
        public Vector3 position;
        public Vector3 direction;
        public int stepSize;
    }
}
