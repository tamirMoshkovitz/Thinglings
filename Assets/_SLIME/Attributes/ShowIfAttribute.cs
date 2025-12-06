using UnityEngine;
using System;

[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
public class ShowIfAttribute : PropertyAttribute
{
    public string ConditionName { get; private set; }

    public ShowIfAttribute(string conditionName)
    {
        ConditionName = conditionName;
    }
}