using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;

namespace InventorySystem
{
    public class Inventory : MonoBehaviour
    {
        [SerializeField]
        private InventoryWindow inventoryWindow;

        [SerializeField]
        private List<ItemHandle> itemHandles = new List<ItemHandle>();

        public readonly List<InventoryItem> Items = new List<InventoryItem>();
        public ItemUpdateEvent ItemPutEvent = new ItemUpdateEvent();
        public ItemUpdateEvent ItemPullEvent = new ItemUpdateEvent();

        public bool TryToAddItem(InventoryItem item)
        {
            ItemHandle closestHandle = null;
            Vector3 itemPos = item.transform.position;
            float minDistance = float.PositiveInfinity;

            foreach (var handle in itemHandles)
            {
                if (closestHandle == null || (itemPos - handle.Item.transform.position).sqrMagnitude < minDistance)
                {
                    if (handle.ItemType == item.ItemType && !handle.CurrentItem)
                    {
                        minDistance = (itemPos - handle.Item.transform.position).sqrMagnitude;
                        closestHandle = handle;
                    }
                }
            }

            if (closestHandle != null)
            {
                StartCoroutine(UpdateItemStatus(item, ItemAction.put, closestHandle));
                return true;
            }
            else
                return false;
        }

        public bool TryToPullItem(int itemID)
        {
            InventoryItem item = Items.Find(i => i.ID == itemID);
            ItemHandle handle = itemHandles.Find(h => h.CurrentItem && h.CurrentItem.ID == itemID);
            if (item && handle != null)
            {
                StartCoroutine(UpdateItemStatus(item, ItemAction.pull, handle));
                return true;
            }
            else
                return false;
        }

        private void OnMouseDown()
        {
            if (!EventSystem.current.IsPointerOverGameObject() && Items.Count > 0)
                inventoryWindow.OpenWindow(this);
        }

        private IEnumerator UpdateItemStatus(InventoryItem item, ItemAction action, ItemHandle handle)
        {
            WWWForm form = new WWWForm();
            form.AddField("id", item.ID);
            form.AddField("action", action.ToString());

            UnityWebRequest www = UnityWebRequest.Post("https://dev3r02.elysium.today/inventory/status", form);
            www.SetRequestHeader("Authorization", "BMeHG5xqJeB4qCjpuJCTQLsqNGaqkfB6");
            www.timeout = 2; //introduced after server crash on 21 Sep
            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError)
            {
                Debug.Log(www.error);
            }
            else
            {
                switch (action)
                {
                    case ItemAction.put:
                        item.ConnectToHandle(handle.Item);
                        Items.Add(item);
                        handle.CurrentItem = item;
                        ItemPutEvent?.Invoke(item.ID);
                        break;
                    case ItemAction.pull:
                        item.ReleaseHandle();
                        Items.Remove(item);
                        handle.CurrentItem = null;
                        ItemPullEvent?.Invoke(item.ID);
                        break;
                    default:
                        break;
                }

                Debug.Log(www.downloadHandler.text);
            }
        }

        [System.Serializable]
        internal class ItemHandle
        {
            [HideInInspector]
            public InventoryItem CurrentItem = null;
            public GameObject Item;
            public InventoryItemType ItemType;
        }

        [System.Serializable]
        public class ItemUpdateEvent : UnityEvent<int>
        {
        }
    }
}
