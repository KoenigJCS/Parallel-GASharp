using System;
using System.Diagnostics;
using System.Threading;
using Unity.Collections;
using UnityEngine;

public class GA : MonoBehaviour
{


    public GAParameters gaParameters;
    public GA(GAParameters gap)
    {
        gaParameters = gap;
        Rand r = new Rand(gaParameters.seed);
    }

    public Population parents, children;
    public void Init()
    {
        InputHandler.inst.ThreadLog("Initializing GA");

       

        parents =  gameObject.AddComponent<Population>();
        parents.Init(gaParameters);
        children = gameObject.AddComponent<Population>();
        children.Init(gaParameters);

        //parents.Evaluate();
        parents.Statistics();
        parents.Report(0);
        InputHandler.inst.ThreadLog("Initialed GA");

    }

    public void SetParams(GAParameters parameters)
    {
        gaParameters = parameters;
        //Rand r = new Rand(gaParameters.seed);
    }


    public void Run()
    {
        Init();
        Evolve();
        Cleanup();

    }
    
    public void Evolve()
    {
        for(int i = 1; i < gaParameters.numberOfGenerations; i++) {
            parents.CHCGeneration(children);
            children.Statistics();
            children.Report(i);


            Population tmp = parents;
            parents = children;
            children = tmp;

        }
        //parents.Print();
    }
    
    public void Cleanup()
    {
        InputHandler.inst.ThreadLog("Cleaning up");
    }

    /// <summary>
    /// This function is called when the MonoBehaviour will be destroyed.
    /// </summary>
    void OnDestroy()
    {
        Destroy(parents);
        Destroy(children);
    }
}
