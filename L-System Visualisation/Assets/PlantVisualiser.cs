using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

public class TransformInfo { //make this just a stack of transforms instead
    public Vector3 position; 
    public Quaternion rotation;
}

public class PlantVisualiser : MonoBehaviour
{

    [SerializeField] GameObject Branch = default; 

    [SerializeField] GameObject GeneratedTree = default;

    [SerializeField] GameObject LeafPrefab = default;

    public char axiom { get; set; }

    public string plantName { get; set; }
    
    public Dictionary<char,string> parcelableRules = new Dictionary<char, string>();

    public int maxIterations { get; set; }

    public float thetaRotationAngle { get; set; }

    Stack<TransformInfo> transformInfos;

    string currentString = "";

    public bool onInstanceGenerateListener { get; set; } 

    public bool onInstanceRecalcuateTreeStructure { get; set; }

    Transform startingPos;

    public int currentIteration { get; set; }

    public bool hasLeaves;

    public bool isStochastic = false;

    StochasticProbablity sp;

    public string preloadedInstructions;

    //TODO add a schotastic listener (to stop the tree generating each time you tick leaves etc) ✓
    //TODO **important** remove the top bit of generate code and only generate when you need to (not when selecting ANGLE or leaves) ✓
    //TODO currently shoctastic only works with strings that start with F and include an F loop through each string if it contains + or minus opposite it maybe make a class that holds a temp and old and have methods that set the new generated one ✓
    //TODO ask in meeting if 50/50 probablity is good enough for the extra schotactic marks and the fact that some rules dictate unmutateable 
    //TODO just use a transform stack instead of transform info

    void Start()
    {
        transform.position = GeneratedTree.transform.position;
        startingPos = GeneratedTree.transform;
        transformInfos = new Stack<TransformInfo>();
        currentIteration = maxIterations;
        sp = new StochasticProbablity(parcelableRules);
        hasLeaves = false;
        CacheGenerationalInstructions();
        onInstanceGenerateListener=true;
    }

    void Update()
    {
        if (onInstanceRecalcuateTreeStructure)
        {
            CacheGenerationalInstructions();
            onInstanceRecalcuateTreeStructure = false;
        }
        if(onInstanceGenerateListener){
            Generate();
            onInstanceGenerateListener = false;
        }    
    }

    void Generate(){

        if(GeneratedTree.transform.childCount!=0) ClearTreeAndPosition();

        for(int i = 0; i < preloadedInstructions.Length; i++){
            switch(preloadedInstructions[i]){
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
                    if (hasLeaves)
                    {
                        Vector3 iP = transform.position;
                        transform.Translate(new Vector3(0, 0.9f, 0));

                        GameObject leaf = Instantiate(LeafPrefab, GeneratedTree.transform);

                        leaf.name = "leaf: " + i;
                        leaf.GetComponent<LineRenderer>().SetPosition(0, iP);
                        leaf.GetComponent<LineRenderer>().SetPosition(1, transform.position);
                    }
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
    
    string ApplyStochasticProbablity(char c)
    {
        float rNum = Random.Range(0f, 1f);
        Dictionary<char, string> working;  
        if (rNum > 0.5f)
        {
            working = sp.GetOrignalRuleSet;
                
        } else {

            working = sp.GetInvertedOperatorRuleSet;

        }
        
        //UPDATE PARCEABLE RULES 
        return working.ContainsKey(c) ? working[c] : c.ToString();
    }

    void CacheGenerationalInstructions()
    {
        currentString = axiom.ToString();
        StringBuilder sb = new StringBuilder();

        for (int i = 0; i < currentIteration; i++)
        {
            foreach (char c in currentString)
            {
                if (isStochastic)
                {
                    sb.Append(ApplyStochasticProbablity(c));
                }
                else
                {
                    sb.Append(parcelableRules.ContainsKey(c) ? parcelableRules[c] : c.ToString());
                }
            }

            currentString = sb.ToString();
            sb.Clear();
        }

        preloadedInstructions = currentString;
        if (isStochastic)
        {
            isStochastic = false;
        }
    }

    IEnumerator MultithreadDelayGen(){
        yield return new WaitForEndOfFrame();
        currentIteration = maxIterations;
        hasLeaves = false;
        sp.SetOrignalProduction(parcelableRules);
        isStochastic = false;
        CacheGenerationalInstructions();
        Generate();
    }

    void OnEnable()
    {
        StartCoroutine(MultithreadDelayGen());  
    }

    public void ClearAll(){ //garbage collection

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

    public class StochasticProbablity
    {
        Dictionary<char, string> originalProduction;
        Dictionary<char, string> modifiedProduction = new Dictionary<char, string>();

        public StochasticProbablity(Dictionary<char,string> initalProductionSet)
        {
            originalProduction = initalProductionSet;
            foreach(KeyValuePair<char, string> production in originalProduction)
            {
                string temp = production.Value;
                if (production.Value.Contains('-') || production.Value.Contains('+'))
                {
                    temp = string.Empty; 
                    foreach(char c in production.Value)
                    {
                        if (c == '+') temp += '-';
                        else if (c == '-') temp += '+';
                        else temp += c;
                    }
                }

                modifiedProduction.Add(production.Key, temp);
            }
        }

        public Dictionary<char, string> GetOrignalRuleSet => originalProduction;

        public Dictionary<char, string> GetInvertedOperatorRuleSet => modifiedProduction;

        public void SetOrignalProduction(Dictionary<char,string> newProd) {
            originalProduction = newProd;
            UpdateInverseProd();
        }

        void UpdateInverseProd()
        {
            modifiedProduction.Clear();
            foreach (KeyValuePair<char, string> production in originalProduction)
            {
                string temp = production.Value;
                if (production.Value.Contains('-') || production.Value.Contains('+'))
                {
                    temp = string.Empty;
                    foreach (char c in production.Value)
                    {
                        if (c == '+') temp += '-';
                        else if (c == '-') temp += '+';
                        else temp += c;
                    }
                }

                modifiedProduction.Add(production.Key, temp);
            }
        }


    }
}
