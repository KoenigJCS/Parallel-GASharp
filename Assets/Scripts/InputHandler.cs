using Unity.Jobs;
using Unity.Collections;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using TMPro;
using System.Threading;
using System.Runtime.CompilerServices;
using System.Text;
using Unity.Netcode;

public struct GAParameters
{
    public int populationSize{ get; set; }
    public int chromosomeLength{ get; set; }
    public int numberOfGenerations{ get; set; }
    public float pCross{ get; set; }
    public float pMut{ get; set; }
    public int seed{ get; set; }
    public GAParameters(int populationSize,int chromosomeLength,
    int numberOfGenerations,int pCross,int pMut,int seed)
    {
        this.populationSize=populationSize;
        this.chromosomeLength=chromosomeLength;
        this.numberOfGenerations=numberOfGenerations;
        this.pCross=pCross;
        this.pMut=pMut;
        this.seed=seed;
    }
}


public class InputHandler : MonoBehaviour
{
    public static InputHandler inst;

    [SerializeField]
    public List<SubPopulationProcessor> processors;

    [SerializeField]
    public Queue<SubPopulationProcessor> inactiveProcessors;
    public Population context;
    private void Awake()
    {
        inst = this; 
        Submit.onClick.AddListener(() => 
        {
            OnSubmit();
        });
        PopulationSize.text="100";
        NumberOfGenerations.text="200";
        ChromosomeLength.text="250";
        Px.text=".95";
        Pm.text=".05";
        Seed.text="0";
    }
    private Thread GAThread;
    private int GAResult;
    public GAParameters parameters;

    int[] tempValues;

    // Start is called before the first frame update
    void Start()
    {
        tempValues=new int[10];
        //GUIMgr.inst.State = GAState.GAInput;
        parameters = default;
        processors = new();
        inactiveProcessors = new();
    }

    // Update is called once per frame
    void Update()
    {
        // if(GUIMgr.inst.State == GAState.GARunning) {
        //     GraphMgr.inst.PlotGraph();
        // }
    }

    [SerializeField] private TMP_InputField PopulationSize;
    [SerializeField] private TMP_InputField NumberOfGenerations;
    [SerializeField] private TMP_InputField ChromosomeLength;
    [SerializeField] private TMP_InputField Px;
    [SerializeField] private TMP_InputField Pm;
    [SerializeField] private TMP_InputField Seed;

    public Button Submit;
    public bool isThreaded = false;

    
    public void OnSubmit()
    {

        parameters.populationSize = int.Parse(PopulationSize.text);
        parameters.numberOfGenerations = int.Parse(NumberOfGenerations.text);
        parameters.chromosomeLength = int.Parse(ChromosomeLength.text);
        parameters.pCross = float.Parse(Px.text);
        parameters.pMut = float.Parse(Pm.text);
        parameters.seed = int.Parse(Seed.text);
    

        Debug.Log("GAParameters: " +  parameters.populationSize + ", " + 
            parameters.numberOfGenerations + ", " + parameters.chromosomeLength + ", " +
            parameters.pCross + ", " + parameters.pMut + ", " + parameters.seed);
        //GUIMgr.inst.State = GAState.GARunning;
        StartJob();
//        GraphMgr.inst.SetAxisLimits(parameters.numberOfGenerations, 0, parameters.chromosomeLength);
    }
    //---------------------------------------------------------------------------------------
    
    void StartJob()
    {
        if(isThreaded)
        {
            GAThread = new Thread(GAStarter);
            GAThread.Start();
        }
        else
        {
            GAStarter();
        }
        //GUIMgr.inst.State = GAState.GARunning;
    }
    GA ga;
    public void GAStarter()
    {
        ga = gameObject.AddComponent<GA>();
        ga.SetParams(parameters);
        ga.Run();
        Debug.Log("GA done: ");
        Destroy(ga);
    }

    private void OnDestroy()
    {
        if(GAThread != null) GAThread.Join();
    }
    //---------------------------------------------------------------------------------------

    public string LogSemaphore = "1";
    public void ThreadLog(string msg)
    {
        lock(LogSemaphore) {
            Debug.Log("--->GA: " + msg);

        }
    }

    public int finishedEvals = 0;    

    public void SetFintess(float fitness,int index,SubPopulationProcessor processor)
    {
        //Not a fan of this :(
        inactiveProcessors.Enqueue(processor);
        int tempI = (int)processor.myID.Value;
        finishedEvals++;
        tempValues[tempI]++;
        processor.gameObject.GetComponentInChildren<TextMeshPro>().text=tempValues[tempI].ToString();
        context.members[index].fitness=fitness;
    }
}
