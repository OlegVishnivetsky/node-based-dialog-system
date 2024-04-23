using UnityEngine;
using UnityEngine.UI;

public class MemberImage : MonoBehaviour
{
    [SerializeField] private Image emotionSpriteImage;

    public void SetEmotionSprite(Sprite emotionSprite)
    {
        emotionSpriteImage.sprite = emotionSprite;
    }
}