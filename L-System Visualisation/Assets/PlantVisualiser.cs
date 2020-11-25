using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

/// <summary>Class <c>PlantVisualiser</c> This class handles the creation of plants screen based on given arguments (provided from iniGivenPlant.cs)
/// </summary>
public class PlantVisualiser : MonoBehaviour
{

    [SerializeField] GameObject Branch = default; //prefab object (branch containing line renderer)

    [SerializeField] GameObject GeneratedTree = default; //"root" of the tree (holding fixed position and rotation data)

    [SerializeField] GameObject LeafPrefab = default; //prefab object (leaf to create containing line renderer)

    public char axiom { get; set; } //inital character to begin expotential creation

    public string plantName { get; set; } //given plain text name of plant name
    
    public Dictionary<char,string> parcelableRules = new Dictionary<char, string>(); //append instructions relative to a character

    public int maxIterations { get; set; } //the number of itererative rules looped through

    public float thetaRotationAngle { get; set; } //rotation of branches

    Stack<TransformData> transformInfos; //active stack to keep track of position data

    string currentString = string.Empty; //data for handling string building of instructions 

    public bool onInstanceGenerateListener { get; set; } //allow external scripts to generate a single instance of the tree

    public bool onInstanceRecalcuateTreeStructure { get; set; } //allow external scripts to recalculate the string needed for generation

    Transform startingPos; //a starting position to use after string has been generated for resetting inital position of the tree generator

    public int currentIteration { get; set; } //allow external HUD setting of the given iteration

    public int branchLengthScalar { get; set; } //a scalar value for increasing branch length 

    public bool hasLeaves { get; set; } //external HUD setting for leaf generation

    public bool isStochastic = false; //external HUD setting for randomness in generation

    StochasticProbablity sp; //instance of randomness probablity allow data coupling

    public string preloadedInstructions; //instructions used for precalculation of tree generation

    public bool updateExisitngAngles { get; set; } //external HUD setting for generation of exisitng angles

    void Start()
    {
        transform.position = GeneratedTree.transform.position; //set generation at root
        startingPos = GeneratedTree.transform; //cache starting position
        transformInfos = new Stack<TransformData>(); //a stack of transform data
   
        currentIteration = maxIterations; //by default the whole tree is drawn
        branchLengthScalar = 1; //normalised length of tree
        updateExisitngAngles = false; 
        sp = new StochasticProbablity(parcelableRules);
        hasLeaves = false;
        CacheGenerationalInstructions(); //within the first frame instructions a cached to allow for smooth generation of update
        onInstanceGenerateListener=true; //create an instance of genertion 
    }

    void Update()
    {
        if (onInstanceRecalcuateTreeStructure) //a call to recalculate the instructions for building a tree
        {
            CacheGenerationalInstructions(); //generate new instruction set
            onInstanceRecalcuateTreeStructure = false; //a trigger to stop cacheing the same instructions
        }
        if(onInstanceGenerateListener){ //generate a visual tree 
            Generate(); //call to delete all objects and reset positions
            onInstanceGenerateListener = false; //a trigger to continually generation
        }
        if (updateExisitngAngles) //recalculate exisiting angles and positions of generated tree
        {
            UpdateExisitngAngles(); //call to update all angles and positions 
            updateExisitngAngles = false; //a trigger to stop continual adjustment
        }
    }

    /// <summary>method <c>Generate</c> A method to re-draw objects to the screen </summary>
    void Generate(){
        int branchCounter = 0; //branch index tracker
        int leafCounter = 0; //leaf index tracker
        if (GeneratedTree.transform.childCount!=0) ClearTreeAndPosition(); //tree clearing
        for(int i = 0; i < preloadedInstructions.Length; i++){
            switch(preloadedInstructions[i]){
                case 'F':
                    Vector3 initalPosition = transform.position; //inital position is cached 
                    transform.Translate(Vector3.up * branchLengthScalar); //position of generation tutle graphic is moved by scalar factor in a forward direction

                    GameObject treeSegement = Instantiate(Branch, GeneratedTree.transform); //a copy of prefab is instantiated
    
                    treeSegement.name = "branch: "+branchCounter; //editor indexing 
                    branchCounter += 1;
                    treeSegement.GetComponent<LineRenderer>().SetPosition(0,initalPosition);
                    treeSegement.GetComponent<LineRenderer>().SetPosition(1,transform.position);
      
                break; 
                case '-':
                    transform.Rotate(Vector3.back * thetaRotationAngle); //rotation realtive to axis
                break;
                case '+':
                    transform.Rotate(Vector3.back * -thetaRotationAngle); //negative rotation relative to axis
                break;
                case '[':
                    transformInfos.Push(new TransformData { position = transform.position, rotation = transform.rotation }); //position added to stack
                break;
                case ']':
                    if (hasLeaves)
                    {
                        Vector3 iP = transform.position;
                        transform.Translate(new Vector3(0, 0.9f*branchLengthScalar, 0));

                        GameObject leaf = Instantiate(LeafPrefab, GeneratedTree.transform);

                        leaf.name = "leaf: " + leafCounter;
                        leafCounter += 1;
                        leaf.GetComponent<LineRenderer>().SetPosition(0, iP);
                        leaf.GetComponent<LineRenderer>().SetPosition(1, transform.position);
                    }

                    TransformData popedT = transformInfos.Pop(); //info is popped of the stack
                    transform.position = popedT.position;
                    transform.rotation = popedT.rotation;
                  
                break;
            }
        }
    }

    /// <summary>method <c>ApplyStochasticProbablity</c> Application of different rulesets via random values run on each character  </summary>
    string ApplyStochasticProbablity(char c)
    {
        float rNum = Random.Range(0f, 1f); //a number is generated between 0-1 
        Dictionary<char, string> instanceOfRuleset; //temp ruleset used for character parsing  
        if (rNum > 0.5f) //a 50-50 probablity is used on each character this creates a vast amount of different generations
        {
            instanceOfRuleset = sp.GetOrignalRuleSet; 
                
        } else {

            instanceOfRuleset = sp.GetInvertedOperatorRuleSet; //if the number is lower than 0.5 the inverted ruleset is used 

        }
        //with such method design it would be trivial to add more probablity
        return instanceOfRuleset.ContainsKey(c) ? instanceOfRuleset[c] : c.ToString(); //return matching instructions for that character
    }

    /// <summary>method <c>CacheGenerationalInstructions</c> Final generation string is calculated and cached globally in the script  </summary>
    void CacheGenerationalInstructions()
    {
        currentString = axiom.ToString(); //start with the base axiom
        StringBuilder sb = new StringBuilder(); 

        for (int i = 0; i < currentIteration; i++)
        {
            foreach (char c in currentString)
            {
                if (isStochastic)
                {
                    sb.Append(ApplyStochasticProbablity(c)); //append randomised character set
                }
                else
                {
                    sb.Append(parcelableRules.ContainsKey(c) ? parcelableRules[c] : c.ToString()); //if the character set contains key append that else append character
                }
            }

            currentString = sb.ToString(); 
            sb.Clear(); //temp string to add is cleared
        }

        preloadedInstructions = currentString; //instructions are updated
        if (isStochastic)
        {
            isStochastic = false; //to stop continually generation this value is stopped until clicked from the HUD script
        }
    }

    /// <summary>method <c>UpdateExisitingAngles</c> All current branches and leaves are updated to match a potential angle update  </summary>
    void UpdateExisitngAngles()
    {

        transformInfos.Clear(); //GC for stack info

        //position is reset
        transform.position = GeneratedTree.transform.position;
        transform.rotation = startingPos.transform.rotation;

        int counter = 0;

        for (int i = 0; i < preloadedInstructions.Length; i++)
        {

            switch (preloadedInstructions[i])
            {
                case 'F':
                    Vector3 initalPosition = transform.position;
                    transform.Translate(Vector3.up * branchLengthScalar);

                    if (GeneratedTree.transform.childCount > counter)
                    {

                        GameObject treeSegement = GeneratedTree.transform.GetChild(counter).gameObject;
                        counter += 1;
                        treeSegement.GetComponent<LineRenderer>().SetPosition(0, initalPosition);
                        treeSegement.GetComponent<LineRenderer>().SetPosition(1, transform.position);
                      //  Debug.Log("getting: " + treeSegement.name);
                    }
                    break;
                case '-':
                    transform.Rotate(Vector3.back * thetaRotationAngle);
                    break;
                case '+':
                    transform.Rotate(Vector3.back * -thetaRotationAngle);
                    break;
                case '[':
                    transformInfos.Push(new TransformData { position = transform.position, rotation = transform.rotation });
                    break;
                case ']':
                    if (hasLeaves)
                    {
                        Vector3 iP = transform.position;
                        transform.Translate(new Vector3(0, 0.9f, 0));

                        GameObject leaf = GeneratedTree.transform.GetChild(counter).gameObject;
                        counter += 1;
                        leaf.name = "leaf: " + i;
                        leaf.GetComponent<LineRenderer>().SetPosition(0, iP);
                        leaf.GetComponent<LineRenderer>().SetPosition(1, transform.position);
                    }

                    TransformData popedT = transformInfos.Pop();
                    transform.position = popedT.position;
                    transform.rotation = popedT.rotation;

                    break;
            }//asumption all input strings are vast and we assume that the functionality for a given character c is such that c handles no operation
            
        }
    } //This method could be handled via bools within generate but for code readablity purposes it's intuative to seperate responsiblity

    /// <summary>Async Method <c>UpdateExisitingAngles</c> To account for thread execution, inital generation is multi-threaded  </summary>
    IEnumerator MultithreadDelayGen(){
        yield return new WaitForEndOfFrame(); //ensure all instructions are loaded by waiting till the end of the frame 
        
        //default data is set
        currentIteration = maxIterations;
        hasLeaves = false;
        sp.SetOrignalProduction(parcelableRules);
        branchLengthScalar = 1;
        isStochastic = false;

        //tree is generated
        CacheGenerationalInstructions();
        Generate();
    }

    void OnEnable()
    {
        StartCoroutine(MultithreadDelayGen());  
    }

    /// <summary>method <c>ClearAll</c> Completely remove and clear all objects and data (used externally)  </summary>
    public void ClearAll(){ //garbage collection

        //Completely reset and remove plant propeties for GC purpose
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

    /// <summary>method <c>ClearTreeAndPosition</c> Removal of just the tree objects, along with a position reset  </summary>
    public void ClearTreeAndPosition(){
        
     
        transform.position = GeneratedTree.transform.position;
        transform.rotation = startingPos.transform.rotation;

        foreach(Transform child in GeneratedTree.transform){
            Destroy(child.gameObject);
        }
    }

    /// <summary>inner class <c>Stochastic</c> Data class for handling different mutations of rulesets  </summary>
    public class StochasticProbablity //this class can easily be expanded to allow for many different mutations and rules
    {
        Dictionary<char, string> originalProduction; //after stochastic probablity the original instruction set is cached
        Dictionary<char, string> modifiedProduction = new Dictionary<char, string>(); //inverted symbols version of prodcution ruleset

        public StochasticProbablity(Dictionary<char,string> initalProductionSet)
        {
            originalProduction = initalProductionSet; //inital production is cached
            foreach(KeyValuePair<char, string> production in originalProduction)
            {
                string temp = production.Value;
                //this O(N²) loop is not an issue; this is due to it running once for each plant, whilst async information on the plant is loaded
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

        public Dictionary<char, string> GetOrignalRuleSet => originalProduction; //return original ruleset

        public Dictionary<char, string> GetInvertedOperatorRuleSet => modifiedProduction; //return modified ruleset

        /// <summary>method <c>SetOrignalProduction</c> Update orignal production  </summary>
        public void SetOrignalProduction(Dictionary<char,string> newProd) {
            originalProduction = newProd;
            UpdateInverseProd();
        }

        /// <summary>method <c>UpdateInverseProd</c> If a new string has been generated its producted will be updated  </summary>
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

    /// <summary>inner class <c>TransformData</c> Stoage for the bare-minimum needed for transform stack (i.e. not needing to saved scale data etc.)  </summary>
    public class TransformData
    {
        public Vector3 position;
        public Quaternion rotation;
    }
}
