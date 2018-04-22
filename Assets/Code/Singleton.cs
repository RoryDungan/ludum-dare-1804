using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/// <summary>
/// Base class for singletons
/// </summary>
public class Singleton<T> where T : new()
{
    /// <summary>
    /// Singleton instance.
    /// </summary>
    public static T Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new T();
            }
            return instance;
        }
    }
    private static T instance;
}
