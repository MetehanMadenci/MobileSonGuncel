using UnityEngine;

public class TestClick : MonoBehaviour
{
    private void OnMouseDown()
    {
        Debug.Log("TIKLANDI: " + gameObject.name);
        GetComponent<Renderer>().material.color = Color.yellow;
    }
}
