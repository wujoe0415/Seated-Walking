using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IReader
{
    void Init();
    void Quit();
    void Read();
}

public abstract class DataReader : MonoBehaviour, IReader
{
    public virtual void Init()
    {

    }
    public virtual void Quit()
    {

    }
    public virtual void Read()
    {

    }
}

