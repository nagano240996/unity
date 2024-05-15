using UnityEngine;

public class MyBlocks : MonoBehaviour
{
    public int Vertical = 0;
    public int Horizon = 0;

    private void Start()
    {
        GetComponent<MeshRenderer>().enabled = false;
    }
}
