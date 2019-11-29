using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Singleton<T> where T : class, new()
{
    private static T m_instance = null;

    public T Instance()
    {
        if (m_instance == null)
            m_instance = new T();
        return m_instance;
    }  
}
