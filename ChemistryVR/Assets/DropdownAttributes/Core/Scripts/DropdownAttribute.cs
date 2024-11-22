
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

public delegate string GetItemNameCallback(object baseMaster, object master);
/// <summary>
/// [Dropdown(path, displayProperty)]
/// 
/// - path:            the path of the List
/// - displayProperty: the property you want to display in the dropdown selection
/// 
/// </summary>
public class DropdownAttribute : PropertyAttribute
{
    public Type type = null;
    public string listPath = "";
    public string itemNameProperty = "";
    public List<Object> list = new List<Object>();
    public int selectedID = -1;
    public GetItemNameCallback getItemName = null;
    public DropdownAttribute(string listPath, string itemNameProperty)
    {//With property name to get name
        //[Dropdown("SkillDatabase.Instance.SkillList", "skillID")]
        this.listPath = listPath;
        getItemName = ((baseMaster, obj) =>
         {
             return ReflectionSystem.GetValue(baseMaster, obj, itemNameProperty)?.ToString();
         });
        this.itemNameProperty = itemNameProperty;
    }
    public DropdownAttribute(string listPath)
    {
        this.listPath = listPath;
        getItemName = ((baseMaster, master) =>
        {
            if (master is Object)
            {//is Unity Object
                return master.GetType().GetProperty("name").GetValue(master).ToString();
            }
            else if (master is string)
            {//string
                return master.ToString();
            }
            else if (master.GetType().IsPrimitive)
            {//short, long, int, bool, char, double, float
                return master.ToString();
            }
            else
            {//object
                return JsonUtility.ToJson(master);
            }
        });
    }
}
