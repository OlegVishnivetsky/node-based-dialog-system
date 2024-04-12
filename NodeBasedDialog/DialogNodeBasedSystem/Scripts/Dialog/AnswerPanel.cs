using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace cherrydev{                                                            

    public class AnswerPanel : MonoBehaviour{
    
        [SerializeField] private Button answerButtonPrefab;                         // allows the answerButtonPrefab to be modified in gmaea 
        [SerializeField] private Transform parentTransform;                         // Not sure 
        [SerializeField] private Transform target;                                  // Not sure 
        [SerializeField] private float TestOffSet = 6.5f;

        private List<Button> buttons = new List<Button>();                          // a private list of buttons, being init 
        private List<TextMeshProUGUI> buttonTexts = new List<TextMeshProUGUI>();    // a private list of button test, being int 
        public PlayerMovement playerMovement;                                       // Reference to the PlayerMovement script
        private Vector3 buttonOffset;                                               // A button to offset postion with respect to the player
        private Vector3 centerOffset;                                               // Offset to center relative to the target and another object
        private Vector3 updatedPosition;                                               // Offset to center relative to the target and another object
        private Vector3 textOffset;                                               // A button to offset postion with respect to the player



        void Start()
        {
            if (playerMovement != null)
            {
                textOffset = answerButtonPrefab.transform.position - playerMovement.transform.position;

                // Update the y-position to 900
                Vector3 newPosition = transform.position;
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
             updatedPosition.x = target.position.x + 6.5f;
             updatedPosition.z = 500f;

            // Only update the X position while keeping the Y position unchanged
            transform.position = new Vector3(updatedPosition.x, transform.position.y, updatedPosition.z);
        }                                                                       
      }

        // Method to calculate the center offset
        public void CalculateCenterOffset(){                                         // another maybe usless offset calcution but inside a function 
            if (target != null){                                                     // if target in this case play exist                                     
                centerOffset = target.position - transform.position;                 // center offset is equal to player position - its moved postion 
            }
        }

                                                                                     //Instantiate answer buttons based on max amount of answer buttons
        public void SetUpButtons(int maxAmountOfAnswerButtons){                      // a function to set up buttons whatever that means
                                                                                     // said funtion takes the max amount of desired buttons
            for (int i = 0; i < maxAmountOfAnswerButtons; i++){                      // a loop ittrating though the buttons until the arugment is met
                Button answerButton = Instantiate(answerButtonPrefab, parentTransform); // answerButton is equal to item brought into existence with the arugments of a buttonPreFab and parentTransform meaning the position  
                buttons.Add(answerButton);                                           // simply add buttons specfily the answerButton 
                buttonTexts.Add(answerButton.GetComponentInChildren<TextMeshProUGUI>()); //a button text is being added to the childern of buttons via TextMeshProUGUI
            }
        }

        public Button GetButtonByIndex(int index){                                   // Returning button by index
            return buttons[index];                                                   // reuturning buttion with selected indiex 
        }

        public TextMeshProUGUI GetButtonTextByIndex(int index){                      // Returning button text by index
            return buttonTexts[index];                                               // Returning button text with selected index
        }

        public void AddButtonOnClickListener(int index, UnityAction action){         // Not Sure
            buttons[index].onClick.AddListener(action);                              // An action is preformed when a button is clicked and is sorted via a index 
        } 

        public void EnableCertainAmountOfButtons(int amount){                        // Enable certain amount of buttons
            if (buttons.Count == 0){
                Debug.LogWarning("Please assign button list!");                      // if buttons dont exist simply give up and retuirn 
                return;                                                              // simple return
            }
            for (int i = 0; i < amount; i++){                                        // itrating though all the buttons 
                buttons[i].gameObject.SetActive(true);                               // if button is equal to index set it to true 
            }
        }

        public void DisalbleAllButtons(){                                              
            foreach (Button button in buttons)                                       
                button.gameObject.SetActive(false);                                  
            }
        }
    }
