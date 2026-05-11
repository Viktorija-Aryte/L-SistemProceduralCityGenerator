using UnityEngine;

public class ID : MonoBehaviour
{
    [SerializeField] private int id = 1;

    public int getID()
    {
        return id;
    }
}
