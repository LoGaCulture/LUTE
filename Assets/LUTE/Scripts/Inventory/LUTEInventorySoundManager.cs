using BogGames.Tools.Inventory;
using UnityEngine;

namespace LoGaCulture.LUTE
{
    /// <summary>
    /// Manager for the inventory sound effects.
    /// Listens for signals from the inventory system and plays the appropriate sound effect.
    /// The sound effects are normally stored on the specific inventory objects themselves.
    /// One could inherit from this class and then override these methods to play more than just sounds.
    /// </summary>
    public class LUTEInventorySoundManager : MonoBehaviour
    {
        SoundManager soundManager;

        private void Awake()
        {
            BogInventorySignals.OnInventoryItemUsed += PlayItemUsedSound;
            BogInventorySignals.OnInventoryItemUnlocked += PlayItemUnlockedSound;
            BogInventorySignals.OnInventoryItemLocked += PlayItemLockedSound;
            BogInventorySignals.OnInventoryItemAdded += PlayItemAddedSound;
            BogInventorySignals.OnInventoryItemRemoved += PlayItemRemovedSound;
            BogInventorySignals.OnInventoryItemMoved += PlayItemMovedSound;
            BogInventorySignals.OnInventoryMenuOpened += PlayMenuOpenedSound;
            BogInventorySignals.OnInventoryMenuClosed += PlayMenuClosedSound;

            soundManager = LogaManager.Instance.SoundManager;
        }

        private void OnDestroy()
        {
            BogInventorySignals.OnInventoryItemUsed -= PlayItemUsedSound;
            BogInventorySignals.OnInventoryItemUnlocked -= PlayItemUnlockedSound;
            BogInventorySignals.OnInventoryItemLocked -= PlayItemLockedSound;
            BogInventorySignals.OnInventoryItemAdded -= PlayItemAddedSound;
            BogInventorySignals.OnInventoryItemRemoved -= PlayItemRemovedSound;
            BogInventorySignals.OnInventoryItemMoved -= PlayItemMovedSound;
            BogInventorySignals.OnInventoryMenuOpened -= PlayMenuOpenedSound;
            BogInventorySignals.OnInventoryMenuClosed -= PlayMenuClosedSound;
        }

        protected virtual void PlayItemUsedSound(BogInventoryItem item)
        {
            if (item.UseSound != null && soundManager != null)
            {
                soundManager.PlaySound(item.UseSound, 1.0f);
            }
        }

        protected virtual void PlayItemUnlockedSound(BogInventoryItem item)
        {
            if (item.UnlockedSound != null && soundManager != null)
            {
                soundManager.PlaySound(item.UnlockedSound, 1.0f);
            }
        }

        protected virtual void PlayItemLockedSound(BogInventoryItem item)
        {
            if (item.LockedSound != null && soundManager != null)
            {
                soundManager.PlaySound(item.LockedSound, 1.0f);
            }
        }

        protected virtual void PlayItemAddedSound(BogInventoryItem item)
        {
            if (item.AddSound != null && soundManager != null)
            {
                soundManager.PlaySound(item.AddSound, 1.0f);
            }
        }

        protected virtual void PlayItemRemovedSound(BogInventoryItem item)
        {
            if (item.DropSound != null && soundManager != null)
            {
                soundManager.PlaySound(item.DropSound, 1.0f);
            }
        }

        protected virtual void PlayItemMovedSound(BogInventoryItem item)
        {
            if (item.MoveSound != null && soundManager != null)
            {
                soundManager.PlaySound(item.MoveSound, 1.0f);
            }
        }

        protected virtual void PlayMenuOpenedSound(BogInventoryCanvas canvas)
        {
            if (canvas.OpenSound != null && soundManager != null)
            {
                soundManager.PlaySound(canvas.OpenSound, 1.0f);
            }
        }

        protected virtual void PlayMenuClosedSound(BogInventoryCanvas canvas)
        {
            if (canvas.CloseSound != null && soundManager != null)
            {
                soundManager.PlaySound(canvas.CloseSound, 1.0f);
            }
        }
    }
}