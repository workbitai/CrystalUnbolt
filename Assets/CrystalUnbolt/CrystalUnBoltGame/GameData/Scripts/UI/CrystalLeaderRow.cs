using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CrystalLeaderRow : MonoBehaviour
{
    [Header("Text Elements")]
    public TMP_Text rankText;
    public TMP_Text nameText;
    public TMP_Text coinText;

    [Header("Image Elements")]
    public Image avatarImage;
    public Image avatarBG;        // Avatar background
    public Image rowBG;            // Row background
    public Image rankBG;           // Rank number background
    public Image coinIcon;         // Coin icon

    [Header("Special Rank Colors (Optional)")]
    public Color rank1Color = new Color(1f, 0.84f, 0f);      // Gold
    public Color rank2Color = new Color(0.75f, 0.75f, 0.75f); // Silver
    public Color rank3Color = new Color(0.8f, 0.5f, 0.2f);    // Bronze
    public Color defaultColor = Color.white;

    // Method to set data from manager
    public void SetData(int rank, string playerName, int coins, Sprite avatar)
    {
        // Set text data
        if (rankText) rankText.text = rank.ToString();
        if (nameText) nameText.text = playerName;
        if (coinText) coinText.text = coins.ToString("N0");
        
        // Set avatar sprite
        if (avatarImage && avatar) 
            avatarImage.sprite = avatar;

        // Apply special colors for top 3 ranks
        ApplyRankStyling(rank);
    }

    public void SetData(int rank, string playerName, int coins, Sprite avatar, Sprite rankSprite, Sprite avatarBgSprite, Sprite coinSprite, Sprite rowBgSprite)
    {
        // Set basic data
        SetData(rank, playerName, coins, avatar);

        // Set custom sprites
        if (rankBG && rankSprite) 
            rankBG.sprite = rankSprite;
        
        if (avatarBG && avatarBgSprite) 
            avatarBG.sprite = avatarBgSprite;
        
        if (coinIcon && coinSprite) 
            coinIcon.sprite = coinSprite;
        
        if (rowBG && rowBgSprite) 
            rowBG.sprite = rowBgSprite;
    }

    private void ApplyRankStyling(int rank)
    {
        Color targetColor = defaultColor;

        // Choose color based on rank
        switch (rank)
        {
            case 1:
                targetColor = rank1Color;
                break;
            case 2:
                targetColor = rank2Color;
                break;
            case 3:
                targetColor = rank3Color;
                break;
            default:
                targetColor = defaultColor;
                break;
        }

        // Apply color to rank background
        if (rankBG) 
            rankBG.color = targetColor;

        // Optional: Also color the rank text
        if (rankText && rank <= 3) 
            rankText.color = targetColor;
    }
}
