using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

namespace InventorySystem
{
    [RequireComponent(typeof(Rigidbody)), RequireComponent(typeof(Collider))]
    public class InventoryItem : MonoBehaviour
    {
        public bool IsUsed { get; set; } = false;

        public int ID = 0;
        public string ItemName = "noName";
        public Sprite ItemIcon;

        [SerializeField]
        private float weight = 0f;

        [SerializeField]
        internal InventoryItemType ItemType = InventoryItemType.None;

        [SerializeField]
        private Transform connectionPoint;

        private Inventory currentInventory = null;

        private Vector3 mOffset;
        private float mouseZCoord;
        private new Rigidbody rigidbody;

        private void OnValidate()
        {
            GetComponent<Rigidbody>().mass = weight;
        }

        private void Awake()
        {
            rigidbody = GetComponent<Rigidbody>();
        }

        public void ConnectToHandle(GameObject handle)
        {
            IsUsed = true;
            StartCoroutine(MoveItem(handle.transform.rotation, handle.transform.position, () =>
            {
                var joint = gameObject.AddComponent<FixedJoint>();
                joint.connectedBody = handle.transform.parent.GetComponent<Rigidbody>();
            }));
        }

        public void ReleaseHandle()
        {
            Vector3 endPosition = transform.TransformPoint(new Vector3(0f, 0.5f, -0.5f));
            Destroy(GetComponent<FixedJoint>());

            StartCoroutine(MoveItem(transform.rotation, endPosition));
            IsUsed = false;
        }

        private IEnumerator MoveItem(Quaternion endRotation, Vector3 endPosition, Action onCompleted = null)
        {
            rigidbody.isKinematic = true;
            GetComponent<Collider>().isTrigger = true;

            while (transform.rotation != endRotation || transform.position != endPosition)
            {
                transform.rotation = Quaternion.Lerp(transform.rotation, endRotation, 10 * 1f * Time.deltaTime);
                transform.position = Vector3.Lerp(transform.position, endPosition, 10 * 1f * Time.deltaTime);
                yield return null;
            }
            onCompleted?.Invoke();
            GetComponent<Collider>().isTrigger = false;
            rigidbody.isKinematic = false;
        }

        private Vector3 GetMouseAsWorldPoint()
        {
            Vector3 mousePoint = Input.mousePosition;
            mousePoint.z = mouseZCoord;
            return Camera.main.ScreenToWorldPoint(mousePoint);
        }

        private void OnMouseDown()
        {
            if (!EventSystem.current.IsPointerOverGameObject() && !IsUsed)
            {
                mouseZCoord = Camera.main.WorldToScreenPoint(gameObject.transform.position).z;
                mOffset = gameObject.transform.position - GetMouseAsWorldPoint();

                LockObject(true);
            }
        }

        private void OnMouseDrag()
        {
            if (!EventSystem.current.IsPointerOverGameObject() && !IsUsed)
            {
                transform.position = GetMouseAsWorldPoint() + mOffset;
            }
        }

        private void OnMouseUp()
        {
            if (currentInventory != null && !currentInventory.TryToAddItem(this))
            {
                currentInventory = null;
            }
            LockObject(false);
        }

        private void OnTriggerEnter(Collider other)
        {
            Inventory inventory = other.GetComponent<Inventory>();
            if (inventory)
                currentInventory = inventory;
        }

        private void LockObject(bool isLocked)
        {
            GetComponent<Collider>().isTrigger = isLocked;
            rigidbody.useGravity = !isLocked;
            rigidbody.velocity = Vector3.zero;
            rigidbody.angularVelocity = Vector3.zero;
        }
    }
}
