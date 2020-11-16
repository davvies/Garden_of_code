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

    [SerializeField] GameObject Branch = default; 

    [SerializeField] GameObject GeneratedTree = default;

    [SerializeField] GameObject LeafPrefab = default;

    public char axiom;

    public string plantName;
    
    public Dictionary<char,string> parcelableRules = new Dictionary<char, string>();

    public int maxIterations;

    public float thetaRotationAngle;

    Stack<TransformInfo> transformInfos;

    string currentString = "";

    public bool onInstanceGenerateListener ; 

    Transform startingPos;

    public int currentIteration;

    void Start()
    {
        transform.position = GeneratedTree.transform.position;
        startingPos = GeneratedTree.transform;
        transformInfos = new Stack<TransformInfo>();
        currentIteration = maxIterations;
        onInstanceGenerateListener=true;
    }

    void Update()
    {
        if(onInstanceGenerateListener){
            Debug.Log("Generating");
            Generate();
            onInstanceGenerateListener = false;
        }    
    }
    void Generate(){

        if(GeneratedTree.transform.childCount!=0) ClearTreeAndPosition();

        currentString = axiom.ToString();
        StringBuilder sb = new StringBuilder();

        for(int i = 0; i < currentIteration; i++){
        foreach(char c in currentString){
            sb.Append(parcelableRules.ContainsKey(c) ? parcelableRules[c] : c.ToString());
         }
         currentString = sb.ToString();
         sb.Clear();
        }

        for(int i = 0; i < currentString.Length; i++){
            switch(currentString[i]){
                case 'F':
                    Vector3 initalPosition = transform.position;
                    transform.Translate(Vector3.up);

                    GameObject treeSegement = Instantiate(Branch, GeneratedTree.transform);
    
                    treeSegement.name = "branch: "+i;
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
    
    IEnumerator MultithreadDelayGen(){
        yield return new WaitForEndOfFrame();
        currentIteration = maxIterations;
        Generate();
    }

    void OnEnable()
    {
        StartCoroutine(MultithreadDelayGen());  
    }

    public void ClearAll(){ //garbage collection
        Debug.LogWarning("**TREE IS BEING CLEARED**");

        parcelableRules.Clear();
        plantName = string.Empty;
        axiom = ' ';
        transform.position = GeneratedTree.transform.position;
        transform.rotation = startingPos.transform.rotation;
        maxIterations = 0;
        if(GeneratedTree.transform.childCount==0) return;

        foreach(Transform child in GeneratedTree.transform){
            Destroy(child.gameObject);
        }
        
    }

    public void ClearTreeAndPosition(){
        transform.position = GeneratedTree.transform.position;
        transform.rotation = startingPos.transform.rotation;

        foreach(Transform child in GeneratedTree.transform){
            Destroy(child.gameObject);
        }
    }
    //AUX DEBUG METHOD
    void ViewDataInput(){
        string TEMPDELETE = "";
         foreach(var c in parcelableRules){
            TEMPDELETE += "Key: "+c.Key+" | "+"Value: "+c.Value;
         }
        Debug.Log("| Plant name: "+plantName +" |  "+ axiom+" | "+TEMPDELETE+" | Max Iterations: "+maxIterations+" | Theta: "+thetaRotationAngle+" degrees");
    }

}
