using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Singleton<T> where T : class, new()
{
    private static T m_instance;

    protected Singleton()
    {

    } 
    public T Instance()
    {
        if (m_instance != null)
            return m_instance;
        else return new T();
    }
}
