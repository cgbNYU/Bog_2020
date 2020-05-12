using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PerlinNoiseVisualization : MonoBehaviour
{
    public GameObject marker;

    public int startingVal;

    public float widthScale;

    public float heightScale;

    public int nodeCount;


    // Start is called before the first frame update
    void Start()
    {
            for (float i = startingVal; i <= startingVal + nodeCount; i += .2f)
            {
                for (float j = startingVal; j <= startingVal + nodeCount; j += .2f)
                {
                    GameObject tempMarker = Instantiate(marker);

                    float xPos = i * widthScale;
                    float zPos = j * widthScale;
                    float yPos = Perlin.Noise(i, j) * heightScale;

                    float xMask = i + 545.24f;
                    float zMask = j + 545.24f;

                    float yMask = Perlin.Noise(xMask / 5, zMask / 5);

                    float xOct = i + 25.87f;
                    float zOct = j + 25.87f;

                    float yOct = Perlin.Noise(xOct / 3, zOct / 3);


                    Vector3 markerPos = new Vector3(xPos, (yPos * yMask) + yOct * 5, zPos);

                    tempMarker.transform.position = markerPos;
                }
            }
        }
    }











//            for (float j = 0; j <= startingVal + nodeCount; j+= 0.2f) //.01f looks cool //hjigher num is smaller
//            
//            {
//                GameObject tempMarker = Instantiate(marker);
//            
//                //where temp marker needs to be 
//                float xPos = i * widthScale;
//                float yPos = Perlin.Noise(i,j) * heightScale;    //
//                float zPos = (j) * widthScale;    //3D    //Vector3 markerPos = new Vector3(xPos, yPos, 0);    //2 D
//
//                float xMask = i + 545.24f;
//                float zMask = j + 545.24f;
//                
//                float yMask = Perlin.Noise(xMask / 5, zMask / 5);
//
//                float xOct = j + 25.87f;
//                float zOct = j + 25.87f;
//                
//                float yOct = Perlin.Noise(xOct / 3 , xOct / 3);
//                
//                Vector3 markerPos = new Vector3(xPos, (yPos * yMask) + yOct * 5, zPos);    ////Vector3 markerPos = new Vector3(xPos, yPos, zPos);
//                
//                tempMarker.transform.position = markerPos;  
//            }
//            
//       
//            
//        }
//    }
//
//}
