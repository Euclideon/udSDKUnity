using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class demoSwitcher : MonoBehaviour
{
    int activeChild = 0;
    // Start is called before the first frame update
    void Start()
    {
        List<GameObject> children = GetAllChildren();
        activeChild %= children.Count;
        for (int i = 0; i < children.Count; i++)
        {
            children[i].transform.position = children[activeChild].transform.position;
            children[i].SetActive(i == activeChild);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.E))
        {
            ++activeChild;
            Start();
        }
    }
    List<GameObject> GetAllChildren()
    {
        List<GameObject> list = new List<GameObject>();
        for (int i = 0; i < transform.childCount; i++)
        {
            list.Add(transform.GetChild(i).gameObject);
        }
        return list;
    }
}
