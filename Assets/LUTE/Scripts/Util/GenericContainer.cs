//using System.Collections;
//using System.Collections.Generic;
//using MoreMountains.InventoryEngine;
//using UnityEngine;

//public class GenericContainer : MonoBehaviour
//{
//    protected Animator _animator;
//    protected ItemPicker[] _itemPickerList;

//    protected virtual void Start()
//    {
//        _animator = GetComponent<Animator>();
//        _itemPickerList = GetComponents<ItemPicker>();
//    }

//    public virtual void AddContents(ItemPicker[] itemPickers, Animator animator)
//    {
//        _itemPickerList = itemPickers;
//        _animator = animator;
//    }

//    public virtual void OpenContainer()
//    {
//        TriggerOpeningAnimation();
//        PickContainerContents();
//    }

//    public virtual void TriggerOpeningAnimation()
//    {
//        if (_animator == null)
//        {
//            return;
//        }
//        _animator.SetTrigger("Open");
//    }

//    protected virtual void PickContainerContents()
//    {
//        if (_itemPickerList.Length == 0)
//        {
//            return;
//        }
//        foreach (ItemPicker picker in _itemPickerList)
//        {
//            picker.Pick();
//        }
//    }
//}
