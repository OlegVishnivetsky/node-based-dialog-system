using DG.Tweening;
using System.Collections.Generic;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace cherrydev
{
    public class SentencePanel : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI dialogNameText;
        [SerializeField] private TextMeshProUGUI dialogText;
        [SerializeField] private Image dialogCharacterImage;

        [Space(7)]
        [SerializeField] private List<MemberImage> membersImages;
        [SerializeField] private RectTransform selectionImageTransform;
        [SerializeField] private float duration;
        [SerializeField] private Ease ease;

        public void SetUpEmotionImages(List<MemberInfo> memberInfos, int selectedMemberIndex)
        {
            StartCoroutine(SetUpEmotionImagesRoutine(memberInfos, selectedMemberIndex));
        }

        public IEnumerator SetUpEmotionImagesRoutine(List<MemberInfo> memberInfos, int selectedMemberIndex)
        {
            foreach (MemberImage image in membersImages)
            {
                image.gameObject.SetActive(false);
            }

            for (int i = 0; i < memberInfos.Count; i++)
            {
                membersImages[i].gameObject.SetActive(true);
                membersImages[i].SetEmotionSprite(memberInfos[i].sprite);

                yield return null;
            }

            selectionImageTransform
                .DOMove(membersImages[selectedMemberIndex].transform.position, duration)
                .SetEase(ease);
        }

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