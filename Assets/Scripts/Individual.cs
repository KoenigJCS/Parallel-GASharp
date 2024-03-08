using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Unity.Netcode;

public class Individual : INetworkSerializable, IComparable<Individual>
{
    
    public int chromLength;
    public int[] chromosome;
    public float fitness;
    public float objectiveFunction;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref chromLength);
        serializer.SerializeValue(ref fitness);
        serializer.SerializeValue(ref objectiveFunction);

        if(serializer.IsWriter)
        {
            serializer.GetFastBufferWriter().WriteValueSafe(chromosome);
        }
        else
        {
            serializer.GetFastBufferReader().ReadValueSafe(out chromosome);
        }
    }

    public Individual()
    {
        chromosome = new int[100];
        chromLength=100;
    }

    public Individual(int n_chromLength)
    {
        //InputHandler.inst.ThreadLog(chromLength.ToString());
        chromosome = new int[n_chromLength];
        this.chromLength = n_chromLength;
    }

    public void Init()
    {
        for(int i = 0; i < chromLength; i++) {
            chromosome[i] = Rand.inst.Flip01(0.5f);
        }
    }

    public void Mutate(float pm)
    {
        for(int i = 0; i < chromLength; i++) {
            chromosome[i] = Rand.inst.Flip(pm) ? 1 - chromosome[i] : chromosome[i];
        }
    }

    public override string ToString()
    {
        StringBuilder sb = new StringBuilder();
        for(int i = 0; i < chromLength; i++) {
            sb.Append(chromosome[i].ToString("0"));
        }
        sb.Append(", " + fitness);
        return sb.ToString();
    }

    public int CompareTo(Individual other)
    {
        if(other.fitness-this.fitness>0f)
            return 1;
        if(other.fitness-this.fitness>0f)
            return -1;
        return 0;
       //From high fitness to low
    }
}
