using UnityEngine;

public class CollectibleItem : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            GameManager.Instance.CollectItem();
            Destroy(gameObject);
        }
    }

    void Update()
    {
        transform.Rotate(0,90 * Time.deltaTime, 0); 
    }
}
