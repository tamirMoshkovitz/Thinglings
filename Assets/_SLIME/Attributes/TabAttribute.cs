using UnityEngine;
using System;

[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
public class TabAttribute : PropertyAttribute
{
    public string tabName;

    public TabAttribute(string name)
    {
        this.tabName = name;
    }
}