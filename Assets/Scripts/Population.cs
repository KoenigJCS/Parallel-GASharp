using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;


public class Population : MonoBehaviour
{
    GAParameters parameters;
    public List<Individual> members;
    public float min, max, avg, sumFitness;

    public Population(GAParameters p)
    {
        parameters = p;
        
        members = new List<Individual>(); // *2 for CHC implementation since children double popsize
        //evaluator = new AXnEvaluator(-5.12f, 5.11f, 1, 2, 10);
        evaluator = new AXnEvaluator(-1.28f, 1.27f, 1, 2, 8);
    }

    public void Init(GAParameters p)
    {
        parameters = p;
        members = new List<Individual>(); // *2 for CHC implementation since children double popsize
        //evaluator = new AXnEvaluator(-5.12f, 5.11f, 1, 2, 10);
        evaluator = new AXnEvaluator(-1.28f, 1.27f, 1, 2, 8);
        members.Clear();
        for(int i = 0; i < parameters.populationSize*2; i++) {
            members.Add(new Individual(parameters.chromosomeLength));
            members[i].Init();
        }
        Evaluate();
    }

    public void Generation(Population child)
    {
        int p1, p2;
        Individual parent1, parent2, child1, child2;
        for(int i = 0; i < members.Count; i += 2) {
            p1 = ProportionalSelector();
            p2 = ProportionalSelector();
            parent1 = members[p1];
            parent2 = members[p2];

            child1 = child.members[i]; // From the child's population
            child2 = child.members[i + 1];

            Reproduce(parent1, parent2, child1, child2);

        }
        child.Evaluate();
    }
    public void Reproduce(Individual parent1, Individual parent2, Individual child1, Individual child2)
    {
        for(int i = 0; i < parameters.chromosomeLength; i++) {
            child1.chromosome[i] = parent1.chromosome[i];
            child2.chromosome[i] = parent2.chromosome[i];
        }

        if(Rand.inst.Flip(parameters.pCross))
            XOver.OnePoint(parent1, parent2, child1, child2, parameters.chromosomeLength);

        child1.Mutate(parameters.pMut);
        child2.Mutate(parameters.pMut);
    }

    public void Halve(Population child)
    {
        members.Sort();
        //Array.Sort(members); //Individual defines sorting from high fitness to low
        for(int i = 0; i < parameters.populationSize; i++) {
            child.members[i] = members[i];
        }
    }

    public void CHCGeneration(Population child)
    {
        int p1, p2;
        Individual parent1, parent2, child1, child2;
        for(int i = 0; i < parameters.populationSize; i += 2) {
            p1 = ProportionalSelector();
            p2 = ProportionalSelector();
            parent1 = members[p1];
            parent2 = members[p2];

            child1 = members[parameters.populationSize + i]; //ADD to the parent's population
            child2 = members[parameters.populationSize + i + 1];

            Reproduce(parent1, parent2, child1, child2);
        }
        //int span = (members.Count-parameters.populationSize)/NetworkManager.ConnectedClients.Count;
        Evaluate(true);
        Halve(child); // sort and choose best half to make child population
    }

    public void Report(int gen)
    {
        //GraphMgr.inst.AddPoint(gen, avg, max);

        string report = gen + ": " + min + ", " + avg + ", " + max;
        InputHandler.inst.ThreadLog(report);

        using(StreamWriter w = File.AppendText("outfile")) {
            w.WriteLine(report);
        }


    }

    public void Statistics()
    {
        Statistics(0, parameters.populationSize);
    }
    public void Statistics(int start, int end)
    {
        float fit;
        min = max = sumFitness = members[start].fitness;
        for(int i =  start + 1; i < end; i++) {
            fit = members[i].fitness;
            sumFitness += fit;
            if(fit < min) min = fit;
            if(fit > max) max = fit;
        }
        avg = sumFitness/(end - start);

    }

    public int ProportionalSelector() // always on members[0 .. population size]
    {
        int index = -1;
        float sum = 0;
        float limit = (float) Rand.inst.rand.NextDouble() * sumFitness;
        do {
            index = index + 1;
            sum += members[index].fitness;
        } while (sum < limit && index < parameters.populationSize - 1);
        return index;
    }

    public void Evaluate(bool isHalf = false)
    {
        StartCoroutine(EvalCorutine(isHalf));
    }

    public IEnumerator EvalCorutine(bool isHalf)
    {
        InputHandler.inst.context=this;
        InputHandler.inst.finishedEvals=0;
        foreach (var processor in InputHandler.inst.processors)
        {
            InputHandler.inst.inactiveProcessors.Enqueue(processor);
        }
        int i = isHalf ? parameters.populationSize : 0;
        while (i<members.Count)
        {
            if(InputHandler.inst.inactiveProcessors.Count==0)
                continue;
            SubPopulationProcessor waitingProcessor = InputHandler.inst.inactiveProcessors.Dequeue();
            //UnityEngine.Debug.Log("AT: "+waitingProcessor.myID.Value);
            ClientRpcParams clientRpcParams = new ClientRpcParams
            {
                Send = new ClientRpcSendParams
                {
                    TargetClientIds = new ulong[]{waitingProcessor.myID.Value}
                }
            };
            ToHostMessage temp = new(members[i].chromosome,i);
            waitingProcessor.RunProcessorClientRpc(temp,clientRpcParams);
            i++;

        }
        float timeout = 0f;
        while(InputHandler.inst.finishedEvals<(members.Count-1) && timeout < 10f)
        {
            yield return new WaitForSeconds(.1f);
            timeout+=Time.deltaTime;
        }
    }

    
    private void EvaluateOnThread()
    {
        
    }

    private AXnEvaluator evaluator;// Constructed in Population constructor

    // [ClientRpc]
    // public void EvaluateClientRpc(int span, bool isHalf=false)
    // {
    //     int start = isHalf ? (int)NetworkManager.LocalClientId * span : (int)NetworkManager.LocalClientId * span + parameters.populationSize;
    //     int end = start+span;
    //     end = end==members.Count-1 ? end++ : end;
    //     for(int i = start; i < end; i++) {
    //         //members[i].fitness = Evaluator.Evaluate(members[i]); // MaxOnes
    //         //members[i].fitness = evaluator.F3(members[i]);   // F3
    //         members[i].fitness = evaluator.F4(members[i]);   // F4
    //     }
    // }

    // [ServerRpc]
    // public void WriteEvaluationServerRpc(int span,ServerRpcParams serverRpcParams = default)
    // {

    // }

    public void Print()
    {
        for(int i = 0; i < parameters.populationSize; i++) {
            InputHandler.inst.ThreadLog(members[i].ToString());
        }
    }
}
