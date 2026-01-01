using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LoGaCulture.LUTE
{
    public class StickerCollectionMenuItem : MonoBehaviour
    {
        [SerializeField] protected Image border;
        [SerializeField] protected Color lockedColour;

        private Image stickerImg;
        private TextMeshProUGUI stickerNameText;
        private string itemID;
        private string itemName;

        public string ItemID { get => itemID; }

        public void Initialise(string name, Sprite img, string ID)
        {
            if (img == null || name == null || ID == null) return;

            stickerImg = null;

            // Get all Image components that are children of the current game object
            Image[] images = GetComponentsInChildren<Image>();

            // Loop through the Image components and assign the first one that is not the border image property
            foreach (Image image in images)
            {
                if (image != border)
                {
                    stickerImg = image;
                    break;
                }
            }

            stickerNameText = GetComponentInChildren<TextMeshProUGUI>();

            if (stickerImg == null || stickerNameText == null) return;

            stickerImg.sprite = img;
            stickerNameText.text = name;
            itemID = ID;

            itemName = name;
        }

        public void SetUnlocked(bool unlocked)
        {
            if (stickerImg == null) return;

            stickerImg.color = unlocked ? Color.white : lockedColour;
            stickerNameText.text = unlocked ? itemName : "???";
        }
    }
}
