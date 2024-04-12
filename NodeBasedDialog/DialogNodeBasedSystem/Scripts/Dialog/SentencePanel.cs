using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace cherrydev
{
    public class SentencePanel : MonoBehaviour
    {
        [SerializeField] private Button PanalPrefab;                         // allows the answerButtonPrefab to be modified in gmaea 
        [SerializeField] private TextMeshProUGUI dialogNameText;
        [SerializeField] private TextMeshProUGUI dialogText;
        [SerializeField] private Image dialogCharacterImage;
        [SerializeField] private int TestOffSet;

        [SerializeField] private Transform target;                                  // Not sure 
        public PlayerMovement playerMovement;                                       // Reference to the PlayerMovement script
        private Vector3 centerOffset;                                               // Offset to center relative to the target and another object
        private Vector3 updatedPosition;                                               // Offset to center relative to the target and another object
        private Vector3 textOffset;                                               // A button to offset postion with respect to the player

        void Start()
        {
            if (playerMovement != null)
            {
                textOffset = PanalPrefab.transform.position - playerMovement.transform.position;

                // Update the y-position to 900
                Vector3 newPosition = transform.position;
                newPosition.y = 500f;
                Debug.Log("imbeing Called Ap");
                transform.position = newPosition;
                Debug.Log(newPosition.y);

            }
        }

        private void Update()
         {
        // late update is a function meant to be instead a actual update 
        // often times for calculations for postitioning and orientating                                                         
        if (target != null)
        {
            updatedPosition.x = target.position.x - 3f;
             updatedPosition.z = 500f;

            // Only update the X position while keeping the Y position unchanged
            transform.position = new Vector3(updatedPosition.x, transform.position.y, updatedPosition.z);
        }                                                                       
      }


        // Method to calculate the center offset
        // public void CalculateCenterOffset()
        // {
        //     if (target != null)
        //     {
        //         centerOffset = target.position - transform.position;                 
        //     }
        // }

        /// <summary>
        /// Setting dialogText max visible characters to zero
        /// </summary>
        public void ResetDialogText()
        {
            dialogText.maxVisibleCharacters = 0;
        }

        /// <summary>
        /// Set dialog text max visible characters to dialog text length
        /// </summary>
        /// <param name="text"></param>
        public void ShowFullDialogText(string text)
        {
            dialogText.maxVisibleCharacters = text.Length;
        }

        /// <summary>
        /// Assigning dialog name text, character image sprite and dialog text
        /// </summary>
        /// <param name="name"></param>
        public void Setup(string name, string text, Sprite sprite)
        {
            dialogNameText.text = name;
            dialogText.text = text;

            if (sprite == null)
            {
                dialogCharacterImage.color = new Color(dialogCharacterImage.color.r,
                    dialogCharacterImage.color.g, dialogCharacterImage.color.b, 0);
                return;
            }

            dialogCharacterImage.color = new Color(dialogCharacterImage.color.r,
                    dialogCharacterImage.color.g, dialogCharacterImage.color.b, 255);
            dialogCharacterImage.sprite = sprite;
        }

        /// <summary>
        /// Increasing max visible characters
        /// </summary>
        public void IncreaseMaxVisibleCharacters()
        {
            dialogText.maxVisibleCharacters++;
        }
    }
}
