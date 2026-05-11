using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Visualizer : MonoBehaviour
{
    //reference to the system generator
    [SerializeField]
    private LSystemGenerator generator;
    
    //List of the saved position aka the stack
    List<Vector3> agentPositions = new List<Vector3>();
    //The hits, or saved agent/turtle positions
    [SerializeField]
    private GameObject agentprefab;

    //To show the drawed path of the agent
    [SerializeField] 
    private Material forLineDrawing;
    
    
    /*MODIFIABLE FIELDS*/
    private int stepSize = 8;
    private double theAngleOfTheAgent = 90;
    
    //Public methods
    public int getLenght()
    {
        //TODO: implement better logic that would protect from going of map or negatibve lenghts
        if (stepSize <= 0)
        {
            //for now return 1, later maybe return the abs(stepsSize);
            return 1;
        }
        else
        {
            return stepSize;
        }
    }

    public void setLenght(int lenght)
    {
        stepSize = lenght;
    }
    
    //enums for moving
    public enum Letters
    {
        Unknown = '1',
        Save = '[',
        Load = ']',
        Draw = 'F',
        TurnClockWise = '+',
        TurnCounterClockWise = '-'
    }

    void Start()
    {
        // string sentence = generator.GenerateTheSentence();//
        // Visualize(sentence);
    }

    private void Visualize(string sentence)
    {
        //we need a LIFO principal the Last saved position, is the first one kicked out
        Stack<HelperClass> savedPositions = new Stack<HelperClass>();
        
        //Starting position
        Vector3 curr_pos = Vector3.zero;
        //The current dierection of the agent
        Vector3 direction = Vector3.forward;
        //We save the current position in this and then the current position 
        //is set to be the postion where we draw to
        Vector3 positionFrom = Vector3.zero;
        
        //add the current position to the position list
        agentPositions.Add(curr_pos);

        foreach (var letter in sentence)
        {
            //get the encoding 
            Letters currentLetter = (Letters)letter;

            switch (currentLetter)
            {
                case Letters.Save:
                    //save the current position
                    savedPositions.Push(new HelperClass()
                    {
                        position = curr_pos,
                        direction = direction,
                        stepSize = stepSize,
                    });
                    break;
                case Letters.Load:
                    //if we have postion saved
                    if (savedPositions.Count > 0)
                    {
                        HelperClass temp = savedPositions.Pop();
                        curr_pos = temp.position;
                        direction = temp.direction;
                        stepSize = temp.stepSize;
                    }
                    break;
                case Letters.Draw:
                    positionFrom = curr_pos;
                    curr_pos += direction * stepSize;
                    DrawLine(positionFrom, curr_pos, Color.magenta);
                    //minimise the steps size
                    stepSize -= 1;
                    agentPositions.Add(curr_pos);
                    break;
                case Letters.TurnClockWise:
                    //create a new angle
                    direction = Quaternion.AngleAxis((float)theAngleOfTheAgent, Vector3.up) * direction;
                    break;
                case Letters.TurnCounterClockWise:
                    //Do the oposite
                    direction = Quaternion.AngleAxis(-(float)theAngleOfTheAgent, Vector3.up) * direction;
                    break;
            }
        }
        
        //spawn the prefabs at the position where agnet stopped
        foreach (var position in agentPositions)
        {
            // Debug.Log(position);
            Instantiate(agentprefab, position, Quaternion.identity);
        }
    }

    private void DrawLine(Vector3 start_position, Vector3 end_position, Color color)
    {
        GameObject line = new GameObject("Line");
        line.transform.position = start_position;
        LineRenderer lineRenderer = line.AddComponent<LineRenderer>();
        lineRenderer.material = forLineDrawing;
        lineRenderer.startColor = color;
        lineRenderer.endColor = color;
        lineRenderer.startWidth = 0.1f;
        lineRenderer.endWidth = 0.1f;
        lineRenderer.SetPosition(0, start_position);
        lineRenderer.SetPosition(1, end_position);
    }
    
    
}
