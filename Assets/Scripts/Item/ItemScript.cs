using UnityEngine;

/// <summary>
/// �D��}�� - mobias
/// </summary>

public class ItemScript : MonoBehaviour
{
    [SerializeField] private ItemData itemSO;
    private Transform player;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

        GameObject go = GameObject.FindWithTag("Player"); // ���]���a�O Player tag
        if (go != null) player = go.transform;

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void deleteItem()
    {
        Destroy(gameObject); // �P���ۨ��D�㪫��
    }

    public void itemEffect()
    {

    }

    public void addItemtoPocket(GameObject player)
    {

    }
}
