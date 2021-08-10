using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

/// <summary>Class <c>PlantVisualiser</c> This class handles the creation of plants screen based on given arguments (provided from iniGivenPlant.cs)
/// </summary>
public class PlantVisualiser : MonoBehaviour
{

    #region Tree Reference Data
    [SerializeField] GameObject branch = default; //prefab object (branch containing line renderer)

    [SerializeField] GameObject generatedTree = default; //"root" of the tree (holding fixed position and rotation data)

    [SerializeField] GameObject leafPrefab = default; //prefab object (leaf to create containing line renderer)
    #endregion

    public char Axiom { private get; set; } //initial character to begin exponential creation

    public string PlantName { get; set; } //given plain text name of plant name

    public Dictionary<char, string> parcelableRules = new Dictionary<char, string>();//append instructions relative to a character

    public int MaxIterations { get; set; } //the number of iterative rules looped through

    public float ThetaRotationAngle { get; set; } //rotation of branches

    Stack<TransformData> transformData; //active stack to keep track of position data

    string currentString = string.Empty; //data for handling string building of instructions 

    public bool OnInstanceGenerateListener { get; set; } //allow external scripts to generate a single instance of the tree

    public bool OnInstanceRecalcuateTreeStructure { get; set; } //allow external scripts to recalculate the string needed for generation

    Transform startingPos; //a starting position to use after string has been generated for resetting initial position of the tree generator

    public int CurrentIteration { get; set; } //allow external HUD setting of the given iteration

    public int BranchLengthScalar { get; set; } //a scalar value for increasing branch length 

    public bool HasLeaves { get; set; } //external HUD setting for leaf generation

    public bool UpdateExisitingAngles { get; set; } //external HUD setting for generation of existing angles

    public bool isStochastic = false; //external HUD setting for randomness in generation

    StochasticProbablity sp; //instance of randomness probability allow data coupling

    string preloadedInstructions; //instructions used for pre-calculation of tree generation


    #region Tree Generation Constants
    const float leafExpansionLen = 0.9f; //size of leaves

    const int branchLenNormalised = 1; //normalised length of tree

    const float rulesetProbability = 0.5f; //stochastic rule probability (must be between 0-1)
    #endregion


    void Start()
    {
        transform.position = generatedTree.transform.position; //set generation at root
        startingPos = generatedTree.transform; //cache starting position
        transformData = new Stack<TransformData>(); //a stack of transform data

        CurrentIteration = MaxIterations; //by default the whole tree is drawn
        BranchLengthScalar = branchLenNormalised;

        UpdateExisitingAngles = false;

        sp = new StochasticProbablity(parcelableRules);

        HasLeaves = false;

        CacheGenerationalInstructions(); //within the first frame instructions a cached to allow for smooth generation of update

        OnInstanceGenerateListener = true; //create an instance of generation 
    }

    void Update()
    {
        if (OnInstanceRecalcuateTreeStructure) 
        {
            CacheGenerationalInstructions(); //generate new instruction set
            OnInstanceRecalcuateTreeStructure = false; //a trigger to stop caching the same instructions
        }

        if (OnInstanceGenerateListener)
        { 
            Generate(); //call to delete all objects and reset positions
            OnInstanceGenerateListener = false; //a trigger to stop continual generation
        }

        if (UpdateExisitingAngles) 
        {
            UpdateExistingAngles(); //call to update all angles and positions 
            UpdateExisitingAngles = false; //a trigger to stop continual adjustment
        }
    }

    /// <summary>method <c>Generate</c> A method to re-draw objects to the screen </summary>
    void Generate()
    {

        if (generatedTree.transform.childCount != 0)
            ClearTreeAndPosition();

        DrawTree();
    }

    /// <summary>method <c>UpdateExistingAngles</c> Used on a generated tree for visual angle correction </summary>
    void UpdateExistingAngles()
    {

        transformData.Clear(); //GC for stack info

        //position is reset
        transform.position = generatedTree.transform.position;
        transform.rotation = startingPos.transform.rotation;


        DrawTree(false);
        
    }

    /// <summary>method <c>DreeTree</c> Draw the tree </summary>
    void DrawTree(bool isDrawingFullTreeFromScratch=true)
    {
        int counter = 0;

        for (int i = 0; i < preloadedInstructions.Length; i++)
        {

            switch (preloadedInstructions[i])
            {
                case 'F':

                    Vector3 initalPosition = transform.position;
                    transform.Translate(Vector3.up * BranchLengthScalar);

                    GameObject treeSegment = null;

                    //When drawing the tree there is no need to reinstantiate all branches
                    if (isDrawingFullTreeFromScratch) 
                    {
                        treeSegment = Instantiate(branch, generatedTree.transform);

                    }
                    else if(generatedTree.transform.childCount > counter)
                    {
                        treeSegment = generatedTree.transform.GetChild(counter).gameObject; //get the reference to the branch already there
                        counter++;
                    }

                    if (treeSegment)
                    {
                        treeSegment.GetComponent<LineRenderer>().SetPosition(0, initalPosition);
                        treeSegment.GetComponent<LineRenderer>().SetPosition(1, transform.position);
                    }

                    break;
                case '-':
                    transform.Rotate(Vector3.back * ThetaRotationAngle);
                    break;
                case '+':
                    transform.Rotate(Vector3.back * -ThetaRotationAngle);
                    break;
                case '[':
                    transformData.Push(new TransformData { position = transform.position, rotation = transform.rotation });
                    break;
                case ']':

                    if (HasLeaves)
                    {
                        Vector3 iP = transform.position;

                        int scale = isDrawingFullTreeFromScratch ? BranchLengthScalar : 1;

                        transform.Translate(new Vector3(0, leafExpansionLen * scale, 0));

                        GameObject segment = null;

                        if (isDrawingFullTreeFromScratch)
                        {
                            segment = Instantiate(branch, generatedTree.transform);

                        } else
                        {
                            segment = generatedTree.transform.GetChild(counter).gameObject;
                            counter++;
                        }

                        if (segment)
                        {
                            segment.GetComponent<LineRenderer>().SetPosition(0, iP);
                            segment.GetComponent<LineRenderer>().SetPosition(1, transform.position);
                        }
                    }

                    //go back to location of previous branching
                    TransformData popedT = transformData.Pop();
                    transform.position = popedT.position;
                    transform.rotation = popedT.rotation;

                    break;
            }

        }
    }


    /// <summary>method <c>ApplyStochasticProbablity</c> Application of different rule-sets via random values run on each character  </summary>
    string ApplyStochasticProbablity(char c)
    {
        float rNum = Random.Range(0f, 1f); //a number is generated between 0-1 
        Dictionary<char, string> instanceOfRuleset; //temp ruleset used for character parsing  

        instanceOfRuleset = rNum > rulesetProbability ? sp.GetOrignalRuleSet : sp.GetInvertedOperatorRuleSet;

        //with such method design it would be trivial to add more probability
        return instanceOfRuleset.ContainsKey(c) ? instanceOfRuleset[c] : c.ToString(); //return matching instructions for that character
    }

    /// <summary>method <c>CacheGenerationalInstructions</c> Final generation string is calculated and cached globally in the script  </summary>
    void CacheGenerationalInstructions()
    {
        currentString = Axiom.ToString(); //start with the base axiom
        StringBuilder sb = new StringBuilder();

        for (int i = 0; i < CurrentIteration; i++)
        {
            foreach (char c in currentString)
            {
                if (isStochastic)
                {
                    sb.Append(ApplyStochasticProbablity(c)); //append randomised character set
                }
                else
                {
                    sb.Append(parcelableRules.ContainsKey(c) ? parcelableRules[c] : c.ToString()); //append default character
                }
            }

            currentString = sb.ToString();
            sb.Clear(); //temp string to add is cleared
        }

        preloadedInstructions = currentString; //instructions are updated

        if (isStochastic)

            isStochastic = false; //to stop continual generation this value is stopped until clicked from the HUD script

    }


    /// <summary>  CoRoutine <c>DelayGeb</c> To account for thread execution a slight delay is added  </summary>
    IEnumerator DelayGen()
    {
        yield return new WaitForEndOfFrame(); //ensure all instructions are loaded by waiting till the end of the frame 

        //default data is set
        CurrentIteration = MaxIterations;
        HasLeaves = false;
        sp.SetOrignalProduction(parcelableRules);
        BranchLengthScalar = 1;
        isStochastic = false;

        //tree is generated
        CacheGenerationalInstructions();
        Generate();
    }

    void OnEnable() => StartCoroutine(DelayGen());

    /// <summary>method <c>ClearAll</c> Completely remove and clear all objects and data (used externally)  </summary>
    public void ClearAll()
    { 

        //Completely reset and remove plant properties for GC purpose
        parcelableRules.Clear();

        PlantName = string.Empty;

        Axiom = ' ';

        transform.position = generatedTree.transform.position;
        transform.rotation = startingPos.transform.rotation;

        MaxIterations = 0;

        if (generatedTree.transform.childCount == 0) 
            return;

        foreach (Transform child in generatedTree.transform)
        {
            Destroy(child.gameObject);
        }

    }

    /// <summary>method <c>ClearTreeAndPosition</c> Removal of just the tree objects, along with a position reset  </summary>
    public void ClearTreeAndPosition()
    {

        transform.position = generatedTree.transform.position;
        transform.rotation = startingPos.transform.rotation;

        foreach (Transform child in generatedTree.transform)
        {
            Destroy(child.gameObject);
        }
    }

    /// <summary>inner class <c>StochasticProbablity</c> Data class for handling different mutations of rulesets  </summary>
    public class StochasticProbablity //this class can easily be expanded to allow for many different mutations and rules
    {
        Dictionary<char, string> originalProduction; //after stochastic probablity the original instruction set is cached
        readonly Dictionary<char, string> modifiedProduction = new Dictionary<char, string>(); //inverted symbols version of prodcution ruleset

        public StochasticProbablity(Dictionary<char, string> initalProductionSet)
        {
            originalProduction = initalProductionSet; //inital production is cached
            UpdateRuleset();
        }

        public Dictionary<char, string> GetOrignalRuleSet => originalProduction; //return original ruleset

        public Dictionary<char, string> GetInvertedOperatorRuleSet => modifiedProduction; //return modified ruleset

        /// <summary>method <c>SetOrignalProduction</c> Update original production  </summary>
        public void SetOrignalProduction(Dictionary<char, string> newProd)
        {
            originalProduction = newProd;
            UpdateInverseProd();
        }

        /// <summary>method <c>UpdateInverseProd</c> If a new string has been generated its instructions will be updated  </summary>
        void UpdateInverseProd()
        {
            modifiedProduction.Clear();
            UpdateRuleset();
        }

        void UpdateRuleset()
        {
            foreach (KeyValuePair<char, string> production in originalProduction)
            {
                string temp = production.Value;
                //this O(N²) loop is not an issue; this is due to it running once for each plant, whilst async information on the plant is loaded
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

    [System.Serializable]
    /// <summary>inner class <c>TransformData</c> Storage for the bare-minimum needed for transform stack (i.e. not needing to saved scale data etc.)  </summary>
    public class TransformData
    {
        public Vector3 position;
        public Quaternion rotation;
    }
}
