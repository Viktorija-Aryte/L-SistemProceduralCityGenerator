using System.Text;
using UnityEngine;
public class RuleGenerator : MonoBehaviour
{
    // Work on this
    [SerializeField] private int ruleLenght;
    private char[] rulesSymbol = new char[]{ 'F', '[', ']', '-', '+' };

    [SerializeField] private int howManyRules = 5;
    private StringBuilder stringBuilder;

    [SerializeField] private int mustHaveFAmount = 2;
    private int counter;
    void Start()
    {
        stringBuilder = new StringBuilder();
        this.counter = 0;
        // GenerateRules(ruleLenght);
        // Debug.Log("Random Senetence: " + stringBuilder.ToString());
    }
    
    // private string GenerateRules(int length)
    // {
    //     // stringBuilder.Clear();
    //     //memorise the last symbol
    //     char lastSymbol = ' ';
    //     char nextSymbol = ' ';
    //     System.Random random = new System.Random();
    //
    //     for (int i = 0; i < length; i++)
    //     {
    //         //while the placement is not allowed do generate
    //         do
    //         {
    //             nextSymbol = rulesSymbol[random.Next(rulesSymbol.Length)];
    //         } while (BadConnection(lastSymbol, nextSymbol));
    //         
    //         // stringBuilder.Append(nextSymbol);
    //         lastSymbol = nextSymbol;
    //     }
    //     // return stringBuilder.ToString();
    // }
    
    private bool BadConnection(char first, char second)
    {

        if ((first == 'F' && second == 'F')
            || (first == '+' && second == '-')
            || (first == '-' && second == '+'))
        {
            return true;
        }
        return false;
    }

    // public void GenerateAListOfRules()
    // {
    //     for (int i = 0; i < howManyRules; i++)
    //     {
    //         Debug.Log("Generated string: " + GenerateRules(ruleLenght));
    //     }
    // }
}
