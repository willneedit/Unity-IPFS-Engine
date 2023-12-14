using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Humanizer;

public class NewBehaviourScript : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        var list = new List<string>() { "One", "Two", "Three" };
        Debug.Log(list.Humanize());
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
