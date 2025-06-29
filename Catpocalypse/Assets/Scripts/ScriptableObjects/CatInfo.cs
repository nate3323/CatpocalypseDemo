using System.Collections;
using System.Collections.Generic;

using UnityEngine;


[CreateAssetMenu(fileName = "NewCatInfo", menuName = "New CatInfo Asset")]
public class CatInfo : ScriptableObject
{
    public CatTypes CatType;
    public string DisplayName;
    public Sizes Size;
    public Speeds Speed;
    public Ratings SpawnRate;
    public Ratings HP;
    public Ratings PayOnDistraction;
    public Sprite Icon;
    public Sprite CatImage;

    [Space(10)]
    [TextArea(4, 10)]
    public string Special = "None";
    
    [Space(10)]
    [TextArea(4, 10)]
    public string Description;
}
