using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class TransformInfo {
    public Vector3 position; 
    public Quaternion rotation;
}
public class PlantVisualiser : MonoBehaviour
{
        ///USE ENUM FOR PARACBLE RULE CHARACTER

    [SerializeField] GameObject Branch; 

    [SerializeField] GameObject GeneratedTree = default;

    [SerializeField] GameObject LeafPrefab;
    public char axiom { get; set; }

    public string plantName { get; set; }
    
    public Dictionary<char,string> parcelableRules = new Dictionary<char, string>();

    public int maxIterations { get; set; }

    public float thetaRotationAngle { get; set; } 

    Stack<TransformInfo> transformInfos;

    string currentString = "";

    // Start is called before the first frame update
    void Start()
    {
        ViewDataInput();
        transform.position = GameObject.Find("Tree").transform.position;
        transformInfos = new Stack<TransformInfo>();
       Generate(); 
    }

    void Generate(){
        currentString = axiom.ToString();
        StringBuilder sb = new StringBuilder();

        for(int i = 0; i < maxIterations; i++){
        foreach(char c in currentString){
            sb.Append(parcelableRules.ContainsKey(c) ? parcelableRules[c] : c.ToString());
         }
         currentString = sb.ToString();
         sb = new StringBuilder();
        }

        for(int i = 0; i < currentString.Length; i++){
            switch(currentString[i]){
                case 'F':
                    Vector3 initalPosition = transform.position;
                    transform.Translate(Vector3.up);

                    GameObject treeSegement = Instantiate(Branch);
                    treeSegement.transform.SetParent(GeneratedTree.transform);
                    treeSegement.GetComponent<LineRenderer>().SetPosition(0,initalPosition);
                    treeSegement.GetComponent<LineRenderer>().SetPosition(1,transform.position);
                break; 
                case 'X':
            
                break;
                case '-':
                transform.Rotate(Vector3.back * thetaRotationAngle);
                break;
                case '+':
                transform.Rotate(Vector3.forward * thetaRotationAngle);
                break;
                case '[':
                transformInfos.Push(new TransformInfo(){position = transform.position, rotation = transform.rotation});
                break;
                case ']':
                TransformInfo ti = transformInfos.Pop();
                transform.position = ti.position;
                transform.rotation = ti.rotation;
                break;
                case 'Y':

                break; 
                default:
                 Debug.LogError("CHARACTER NOT FOUND"+currentString[i].ToString()); 
                 break;
            }
        }
    }
    
    //DEBUG METHOD
    void ViewDataInput(){
        string TEMPDELETE = "";
         foreach(var c in parcelableRules){
            TEMPDELETE += "Key: "+c.Key+" | "+"Value: "+c.Value;
         }
        Debug.Log("| Plant name: "+plantName +" |  "+ axiom+" | "+TEMPDELETE+" | Max Iterations: "+maxIterations+" | Theta: "+thetaRotationAngle+" degrees");
    }

}
