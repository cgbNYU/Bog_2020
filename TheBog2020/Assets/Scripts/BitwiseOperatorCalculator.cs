using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using UnityEngine;

public class BitwiseOperatorCalculator : MonoBehaviour
{

    public int a;
    public int b;

    public int outputC;
    
    
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        outputC = a & b;    //when you make a over 255, c rolls over. Range of 255. Anything over, starts over at 0. Sort of like modulo, but diff. 
    }
}
