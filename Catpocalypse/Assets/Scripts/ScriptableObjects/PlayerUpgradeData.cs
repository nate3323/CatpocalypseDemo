using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class PlayerUpgradeData : ScriptableObject
{
    [SerializeField, Tooltip("How many reward upgrades can the player get")] private int _maxRewardUpgrades;
    public int MaxRewardUpgrades { get { return _maxRewardUpgrades; } }


    [Header("Cucumber Tower Upgrades")]
    [SerializeField, Tooltip("How much the cucumber fire rate is improved by")] private float _CucumberFireRateUpgrade = 1.2f;
    [SerializeField, Tooltip("How much the tower rotation speed is improved")] private float _CucumberAimingSpeedUpgrade = 1.3f;
    [SerializeField, Tooltip("How much the tower range is increased by")] private float _CucumberRangeUpgrade = 1.4f;
    [SerializeField, Tooltip("How much the tower range is increased by")] private float _CucumberSuperAOEUpgrade = 1.4f;
    [SerializeField, Tooltip("How many secondary cucumbers spawn from the Nested Cucumber Upgrade")] private float _CucumberNestedUpgrade = 5;

    public float CucumberFireRateUpgrade { get { return _CucumberFireRateUpgrade; } }
    public float CucumberAimSpeedUpgrade { get { return _CucumberAimingSpeedUpgrade; } }
    public float CucumberRangeUpgrade {  get { return _CucumberRangeUpgrade; } }
    public float CucumberSuperAOEUpgrade { get { return _CucumberSuperAOEUpgrade; } }
    public float CucumberNestedUpgrade {  get { return _CucumberNestedUpgrade; } }


    [Header("Laser Pointer Tower Upgrades")]
    [SerializeField, Tooltip("How much the drag speed of the laser pointer tower is increased")] private float _LaserPointerDragSpeedUpgrade = 1.15f;
    [SerializeField, Tooltip("How much the tower range is increased by")] private float _LaserPointerRangeUpgrade = 1.2f;
    [SerializeField, Tooltip("How much the tower distraction amount is increased by")] private float _LaserPointerDistractionUpgrade = 1.2f;
    [SerializeField, Tooltip("How much the stun time of Sudden Flash increases by")] private float _LaserPointerFlashStunUpgrade = 2f;
    [SerializeField, Tooltip("How many more cats a laser can target")] private int _LaserPointerNumberOfTargetsUpgrade = 1;

    public float LaserPointerDragSpeedUpgrade {  get { return _LaserPointerDragSpeedUpgrade; } }
    public float LaserRangeUpgrade { get { return _LaserPointerRangeUpgrade;  } }
    public float LaserDistractionUpgrade {  get { return _LaserPointerDistractionUpgrade; } }
    public float LaserPointerFlashUpgrade { get { return _LaserPointerFlashStunUpgrade; } }
    public float LaserPointerNumberOfTargetsUpgrade { get { return LaserPointerNumberOfTargetsUpgrade; } }


    [Header("Nonallergic Tower Upgrades")]
    [SerializeField, Tooltip("How much the cost to build Non-Allergic Towers is reduced")] private float _NABuildCostReduction = .85f;
    [SerializeField, Tooltip("How much a Non-Allergic Person's move speed is increased")] private float _NAMoveSpeedUpgrade = 1.1f;
    [SerializeField, Tooltip("How much a Non-Allergic Person's distract value is increased")] private float _NADistractionUpgrade = 1.25f;
    [SerializeField, Tooltip("How much longer Food Call lasts")] private float _NAFoodCallUpgrade = 1.3f;
    [SerializeField, Tooltip("Number of cats a Non-Allergic Person can permanently distract")] private int _NAPermanentDistractionUpgrade = 1;

    public float NABuildCostReduction {  get { return _NABuildCostReduction; } }
    public float NAMoveSpeedUpgrade {  get { return _NAMoveSpeedUpgrade; } }
    public float NADistractionUpgrade { get {return _NADistractionUpgrade; } }
    public float NAFoodCallUpgrade {  get {  return _NAFoodCallUpgrade; } }
    public float NAPermanentDistractionUpgrade { get { return _NAPermanentDistractionUpgrade; } }

    [Header("Scratching Post Tower Upgrades")]

    [SerializeField, Tooltip("Upgrades the radius of the scratching post")] private float _ScratchingPostRangeUpgrade = 1.1f;
    [SerializeField, Tooltip("Upgrades how often the Scratching Post Tower Fires")] private float _ScratchingPostFireRateUpgrade = .75f;
    [SerializeField, Tooltip("Upgrades how long a Scratching Post Tower lasts")] private float _ScratchingPostDurabilityUpgrade = 1.30f;
    [SerializeField, Tooltip("Upgrades the amount of time a cat is stunned by the Irresistible Scratching Post")] private float _ScratchingPostStunLengthUpgrade = 2f;
    [SerializeField, Tooltip("Upgrade gives the Scratching Posts impact damage in an AOE")] private float _ScratchingPostImpactDamageUpgrade = 5f;

    public float ScratchingPostRangeUpgrade {  get { return _ScratchingPostRangeUpgrade; } }
    public float ScratchingPostFireRateUpgrade {  get { return _ScratchingPostFireRateUpgrade; } }
    public float ScratchingPostDurabilityUpgrade { get { return _ScratchingPostDurabilityUpgrade; } }
    public float ScratchingPostStunLengthUpgrade { get { return _ScratchingPostStunLengthUpgrade; } }
    public float ScratchingPostImpactDamageUpgrade {  get {  return _ScratchingPostImpactDamageUpgrade; } }

    [Header("String Waver Tower Upgrades")]
    [SerializeField, Tooltip("How much the frequency of the String Waver Tower AOE is increased")] private float _StringWaverFrequencyUpgrade = 1.15f;
    [SerializeField, Tooltip("How much larger the range of the String Waver Tower is increased")] private float _StringWaverRangeUpgrade = 1.2f;
    [SerializeField, Tooltip("How much the String Waver distraction value is increased")] private float _StringWaverDistractValueUpgrade = 1.25f;
    [SerializeField, Tooltip("How much more the String Fling distraction value is increased")] private float _StringWaverStringFlingUpgrade = 1.4f;

    public float StringWaverFrequencyUpgrade { get { return _StringWaverFrequencyUpgrade; } }
    public float StringWaverRangeUpgrade {  get { return _StringWaverRangeUpgrade; } }
    public float StringWaverDistractValueUpgrade { get { return _StringWaverDistractValueUpgrade; } }
    public float StringWaverStringFlingUpgrade {  get { return _StringWaverStringFlingUpgrade; } }

    [Header("Yarn Thrower Tower Upgrades")]
    [SerializeField, Tooltip("How much to increase the Yarn Thrower tower fire rate")] private float _YarnThrowerFireRateUpgrade = 1.15f;

    public float YarnThrowerFireRateUpgrade {  get { return _YarnThrowerFireRateUpgrade;  } }

    [Header("Robot Upgrades")]
    [SerializeField, Tooltip("The upgrade to the robot movement speed")] private float _RobotSpeedUpgrade = 1.15f;
    [SerializeField, Tooltip("Upgrade to the robot projectile speed")] private float _RobotLaunchUpgrade = 1.2f;
    [SerializeField, Tooltip("Upgrade to the robot fire rate")] private float _RobotFireRateUpgrade = .3f;

    public float RobotSpeedUpgrade { get { return _RobotSpeedUpgrade; } }
    public float RobotLaunchUpgrade {  get { return _RobotLaunchUpgrade; } }
    public float RobotFireRateUpgrade { get { return _RobotFireRateUpgrade; } }

    [Header("Reward Upgrades")]
    [SerializeField, Tooltip("How much the reward for distracting cats increases by per upgrade")] private List<float> _rewardIncrease;
    public float RewardUpgrade { get { return _rewardIncrease[PlayerDataManager.Instance.CurrentData.catRewardUpgrades]; } }

    [Header("Fortification Upgrades")]
    [SerializeField, Tooltip("The amount to increase the player's starting health by")] private int _MaxHealthUpgrade;
    [SerializeField, Tooltip("How many non-allergic guards are spawned at the fortification")] private int _FortificationGuardNumber;
    [SerializeField, Tooltip("Upgrades the resistance to cat cuteness a player has")] private float _CutenessResistanceUpgrade;
    [SerializeField, Tooltip("How much faster hairballs will be removed by in seconds")] private float _HairballRemovalUpgrade;
    [SerializeField, Tooltip("How much the Chatterbox Upgrade can distract cats")] private int _ChatterboxUpgradeDistractionAmount;

    public int MaxHealthUpgrade{ get { return _MaxHealthUpgrade; }}
    public int FortificationGuardNumber { get { return _FortificationGuardNumber;} }
    public float CutenessResistanceUpgrade {  get { return _CutenessResistanceUpgrade; }}
    public float HairballRemovalSpeed { get { return _HairballRemovalUpgrade; }}
    public int ChatterboxUpgradeDistractionAmount { get { return _ChatterboxUpgradeDistractionAmount; }}

}
