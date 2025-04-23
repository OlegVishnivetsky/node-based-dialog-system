using UnityEngine;
using cherrydev;

namespace DialogNodeBasedSystem.Demo.Scripts
{
    public class TestDialogStarter : MonoBehaviour
    {
        [SerializeField] private DialogBehaviour _dialogBehaviour;
        [SerializeField] private DialogNodeGraph _dialogGraph;

        private void Start()
        {
            _dialogBehaviour.BindExternalFunction("Test", DebugExternal);
            _dialogBehaviour.StartDialog(_dialogGraph);
        }

        private void DebugExternal() => Debug.Log("External function works!");
    }
}