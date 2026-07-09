using CustomLitJson.Extensions;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class KV
{
    public string key;
    public string val;
    public int iVal;
    [JsonIgnore]
    public object objVal;
}