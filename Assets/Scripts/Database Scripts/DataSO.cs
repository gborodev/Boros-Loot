using System;
using UnityEngine;

public class DataSO : ScriptableObject
{
    [Header("Data Stats")]
    private string _id;

    [SerializeField] private string _name;

    public string ID => _id;
    public string Name => _name;

    public virtual DataSO GetInstance()
    {
        _id = Guid.NewGuid().ToString();

        return this;
    }
}
