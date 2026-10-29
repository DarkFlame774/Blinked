using UnityEngine;

public class player : MonoBehaviour
{
    public bool keypick = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
    }

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("objetv ");
        if (collision.gameObject.tag == "Door" && keypick)
        {

            Destroy(collision.gameObject);
        }
    }
}
