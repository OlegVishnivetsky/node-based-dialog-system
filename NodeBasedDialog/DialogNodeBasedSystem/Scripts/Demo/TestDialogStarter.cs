using UnityEngine;
using cherrydev;

public class TestDialogStarter : MonoBehaviour
{
    [SerializeField] private DialogBehaviour dialogBehaviour;
    [SerializeField] private DialogNodeGraph dialogGraph;

    private Collider2D collider;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("DialogTrigger"))
        {
            Debug.Log("Entered trigger volume");
            Debug.Log("I passed");

            dialogBehaviour.BindExternalFunction("Test", DebugExternal);
            dialogBehaviour.StartDialog(dialogGraph);

            // Destroy the collider
            collider = collision;
            Destroy(collider.gameObject);
        }
    }

    private void DebugExternal()
    {
        Debug.Log("External function works!");
    }
}
