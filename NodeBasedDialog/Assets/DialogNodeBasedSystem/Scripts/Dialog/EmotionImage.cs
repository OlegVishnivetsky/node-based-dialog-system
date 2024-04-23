using UnityEngine;
using UnityEngine.UI;

public class EmotionImage : MonoBehaviour
{
    [SerializeField] private Image emotionSpriteImage;
    [SerializeField] private Sprite noneSprite;

    public void SetEmotionSprite(Sprite emotionSprite)
    {
        emotionSpriteImage.sprite = emotionSprite;
    }

    public void ResetEmotionSprite()
    {
        emotionSpriteImage.sprite = noneSprite;
    }
}