using System.Collections.Generic;
using UnityEngine;
using InventorySystem;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI
{
    public class InventoryWindow : MonoBehaviour
    {
        [SerializeField]
        private GameObject SideItemsBar;

        [SerializeField]
        private GameObject FrontItemsBar;

        [SerializeField]
        private GameObject TopItemsBar;

        [SerializeField]
        private GameObject ItemSlotPrefab;

        [SerializeField]
        private GraphicRaycaster raycaster;

        private Inventory currentInventory;

        void Update()
        {
            if (Input.GetMouseButtonUp(0))
            {
                PointerEventData pointerEventData = new PointerEventData(GetComponent<EventSystem>());
                pointerEventData.position = Input.mousePosition;
                List<RaycastResult> results = new List<RaycastResult>();
                raycaster.Raycast(pointerEventData, results);

                foreach (RaycastResult result in results)
                {
                    Debug.Log("Hit " + result.gameObject.name);
                    InventorySlot slot = result.gameObject.GetComponent<InventorySlot>();
                    if (slot)
                    {
                        currentInventory.TryToPullItem(slot.ItemID);
                        break;
                    }
                }
                gameObject.SetActive(false);
            }
        }

        public void OpenWindow(Inventory inventory)
        {
            currentInventory = inventory;
            ClearBars();

            foreach (var item in currentInventory.Items)
            {
                GameObject parent = SideItemsBar;
                switch (item.ItemType)
                {
                    case InventoryItemType.SideItem:
                        parent = SideItemsBar;
                        break;
                    case InventoryItemType.FrontItem:
                        parent = FrontItemsBar;
                        break;
                    case InventoryItemType.TopItem:
                        parent = TopItemsBar;
                        break;
                    default:
                        break;
                }
                var slotObj = Instantiate(ItemSlotPrefab, parent.transform);
                InventorySlot slot = slotObj.GetComponent<InventorySlot>();
                slot.SetItemData(item.ID, item.ItemIcon);
            }
            gameObject.SetActive(true);
        }

        private void ClearBars()
        {
            foreach (Transform child in SideItemsBar.transform)
            {
                Destroy(child.gameObject);
            }

            foreach (Transform child in FrontItemsBar.transform)
            {
                Destroy(child.gameObject);
            }

            foreach (Transform child in TopItemsBar.transform)
            {
                Destroy(child.gameObject);
            }
        }
    }
}
