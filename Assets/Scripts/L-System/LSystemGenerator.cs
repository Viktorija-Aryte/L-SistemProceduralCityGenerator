using System.Text;
using UnityEngine;

public class LSystemGenerator : MonoBehaviour
{
    public Rule[] rules;
    public string axiom = "F"; 

    [Range(0, 15)]
    public int iterationLimit = 1;

    [SerializeField]
    private bool randomizeBranching = false;

    [SerializeField]
    [Range(0, 1)]
    private float branchPruningProbability = 0.5f;
    
    private System.Collections.Generic.Dictionary<char, Rule> ruleCache;

    private void Awake()
    {
        CacheRules();
    }

    private void CacheRules()
    {
        ruleCache = new System.Collections.Generic.Dictionary<char, Rule>();
        foreach (var rule in rules)
        {
            if (rule.variable.Length > 0)
            {
                ruleCache[rule.variable[0]] = rule;
            }
        }
    }

    public string GenerateSentence()
    {
        if (ruleCache == null || ruleCache.Count == 0)
        {
            CacheRules();
        }
        
        string sentence = axiom;
        for (int i = 0; i < iterationLimit; i++)
        {
            sentence = ApplyRules(sentence, i);
        }
        return sentence;
    }

    private string ApplyRules(string input, int iteration)
    {
        StringBuilder output = new StringBuilder(input.Length * 2);

        foreach (char c in input)
        {
            if (ruleCache.TryGetValue(c, out Rule rule))
            {
                // Check if we should prune this branch
                if (randomizeBranching && iteration > 1 && Random.value < branchPruningProbability)
                {
                    output.Append(c); // Keep original character
                }
                else
                {
                    output.Append(rule.GetWord());
                }
            }
            else
            {
                output.Append(c);
            }
        }

        return output.ToString();
    }
}