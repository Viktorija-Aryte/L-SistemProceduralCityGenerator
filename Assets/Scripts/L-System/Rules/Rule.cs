using UnityEngine;

[CreateAssetMenu(menuName = "ProceduralGeneration/L-System/Rule")]
public class Rule : ScriptableObject
{
    public string variable;
    
    [SerializeField]
    private string[] rules = null;

    [SerializeField] 
    private bool randomRules = false; // Fixed typo: radomRules -> randomRules

    /// <summary>
    /// Gets the rule output. If randomRules is enabled, returns a random rule from the array.
    /// </summary>
    public string GetWord()
    {
        if (rules == null || rules.Length == 0)
        {
            Debug.LogWarning($"Rule '{variable}' has no rules defined!");
            return string.Empty;
        }

        if (randomRules && rules.Length > 1)
        {
            int index = Random.Range(0, rules.Length);
            return rules[index];
        }
        
        return rules[0];
    }

    // Validate the rule in the editor
    private void OnValidate()
    {
        if (string.IsNullOrEmpty(variable))
        {
            Debug.LogWarning($"Rule in {name} has no variable defined!");
        }

        if (rules == null || rules.Length == 0)
        {
            Debug.LogWarning($"Rule '{variable}' in {name} has no replacement rules!");
        }
    }
}