using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using TMPro;

// This fixes the ambiguity between System.Random and UnityEngine.Random by
// telling it to use the Unity one.
using Random = UnityEngine.Random;


[RequireComponent(typeof(StateMachine))]
public class CatBase : MonoBehaviour
{
    public static bool ShowDistractednessBar = true;

    // This event is static so we don't need to subscribe the money manager to every cat instance's OnCatDied event.
    public static event EventHandler OnCatDied;
    public static event EventHandler OnCatReachGoal;
    
    public CatTypes _catType;
    [Tooltip("The cuteness value is how much this type of cat increases the cuteness meter.")]
    [Min(0)]
    [SerializeField] protected int _CutenessValue = 5;

    [Min(0)]
    [SerializeField] protected float distractionThreshold = 50; //The amount of distraction it takes to fully distract the cat

    public float DistractionThreshold
    {
        get { return distractionThreshold; }
        set { distractionThreshold = value; }
    }

    [Min(0f)]
    [SerializeField] protected float damageToPlayer = 2f; //How much health the cat takes from the player

    [Header("Cat Movement")]
    [Tooltip("The Cat Speed")]
    [Min(0f)]
    public float speed;
    
    [Tooltip("Controls the cat's navigation")]
    public NPCNavigationController NavController;

    [Tooltip("How much money to player gets for distracting this type of cat.")]
    [SerializeField] protected float distractReward = 50;

    [Header("Distractedness Meter")]
    [SerializeField] protected float _DistractednessMeterHeightAboveCat = 2f;
    [SerializeField] protected GameObject _DistractednessMeterPrefab;

    protected float distraction = 0; //How distracted the cat is currently
    protected bool isDistracted = false; // If the cat has been defeated or not.

    protected PlayerHealthManager healthManager;

    protected GameObject _DistractednessMeterGO;
    protected UnityEngine.UI.Image _DistractednessMeterBarImage;
    protected TextMeshPro _DistractednessMeterLabel;

    public List<AudioClip> sounds = new List<AudioClip>();
    private AudioSource catAudio;

    public List<AudioClip> purrs = new List<AudioClip>();

    public List<GameObject> slowingEntities;
    public List<GameObject> stoppingEntities;
    public bool isATarget = false;

    private StateMachine _stateMachine;
    public bool spedUp = false;

    [SerializeField]
    private PlayerUpgradeData _upgradeData;
    [SerializeField]
    private int _cutenessReduction = 2;

    public bool _affectedByParticles = false;
    // Start is called before the first frame update
    void Start()    
    {
        IsDead = false;
        catAudio = GetComponent<AudioSource>();
        InitDistractednessMeter();
        int index = Random.Range(0, sounds.Count - 1);

        catAudio.clip = sounds[index];
        catAudio.Play();
        speed = NavController.agent.speed;
        healthManager = GameObject.FindGameObjectWithTag("Goal").gameObject.GetComponent<PlayerHealthManager>();


        if (_stateMachine == null)
        {
            _stateMachine = GetComponent<StateMachine>();
            if (_stateMachine == null)
                throw new Exception($"The cat \"{gameObject.name}\" does not have a StateMachine component!");

            InitStateMachine();
        }
    }

    /// <summary>
    /// This function is overriden by subclasses to allow them to setup the state machine with their own states.
    /// </summary>
    protected virtual void InitStateMachine()
    {
        // Create tower states.
        CatState_Idle_Base idleState = new CatState_Idle_Base(this);
        CatState_Moving movingState = new CatState_Moving(this);
        CatState_Slowed slowedState = new CatState_Slowed(this);
        CatState_Stopped stoppedState = new CatState_Stopped(this);


        // Create and register transitions.
        // ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

        _stateMachine.AddTransitionFromState(movingState, new Transition(slowedState, () => slowingEntities.Count > 0 && stoppingEntities.Count == 0));
        _stateMachine.AddTransitionFromAnyState(new Transition(stoppedState, () => stoppingEntities.Count > 0));
        _stateMachine.AddTransitionFromState(stoppedState, new Transition(slowedState, () => stoppingEntities.Count == 0 &&
                                                                                             slowingEntities.Count > 0));

        _stateMachine.AddTransitionFromAnyState(new Transition(movingState, () => slowingEntities.Count == 0 && stoppingEntities.Count == 0));

        // ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------


        // Tell state machine to write in the debug console every time it exits or enters a state.
        _stateMachine.EnableDebugLogging = true;

        // This is necessary since we only have one state and no transitions for now.
        // Mouse over the AllowUnknownStates property for more info.
        _stateMachine.AllowUnknownStates = true;


        // Set the starting state.
        _stateMachine.SetState(idleState);
    }

    // Update is called once per frame
    void Update()
    {
        if (distraction >= distractionThreshold && isDistracted == false)
        {
            Distracted();
            return;
        }

        _DistractednessMeterGO.SetActive(ShowDistractednessBar);
    }

    private void InitDistractednessMeter()
    {
        Transform distractednessMeter = Instantiate(_DistractednessMeterPrefab).transform;
        _DistractednessMeterGO = distractednessMeter.gameObject;

        distractednessMeter.SetParent(transform, true); // I'm parenting it this way rather than using the Instantiate() function above, because I need it to not inherit scale from the cat.
        distractednessMeter.localPosition = new Vector3(0, _DistractednessMeterHeightAboveCat, 0);

        _DistractednessMeterBarImage = distractednessMeter.Find("DistractednessBar").GetComponent<UnityEngine.UI.Image>();
        _DistractednessMeterLabel = distractednessMeter.Find("DistractednessLabel").GetComponent<TextMeshPro>();

        UpdateDistractednessMeter();
    }

    private void UpdateDistractednessMeter()
    {        
        _DistractednessMeterBarImage.fillAmount = (float) distraction / distractionThreshold;
        _DistractednessMeterLabel.text = $"Distractedness: {distraction} of {distractionThreshold}";
    }

    protected void Distracted()
    {
        isDistracted = true;
    }

    
    //I am intending this function to be called from either the tower or the projectile that the tower fires
    public void DistractCat(float distractionValue, Tower targetingTower)
    {

        if (PlayerCutenessManager.Instance.CurrentCutenessChallenge == PlayerCutenessManager.CutenessChallenges.CatsGetHarderToDistract)
        {
            float debuffPercent = PlayerCutenessManager.Instance.CuteChallenge_CatsGetHarderToDistract_DebuffPercent;
            distractionValue = distractionValue * debuffPercent;
        }
        distraction += distractionValue;
        UpdateDistractednessMeter();

        if (distraction >= distractionThreshold)
        {
            StartCoroutine(Sound());
            if(targetingTower != null)
            {
                targetingTower.targets.Remove(this.gameObject);
            }     
        }
    }
    public void FortificationCatDistraction(float distractionValue,Fortifications fort)
    {
        if (PlayerCutenessManager.Instance.CurrentCutenessChallenge == PlayerCutenessManager.CutenessChallenges.CatsGetHarderToDistract)
        {
            float debuffPercent = PlayerCutenessManager.Instance.CuteChallenge_CatsGetHarderToDistract_DebuffPercent;
            distractionValue = distractionValue * debuffPercent;
        }
        distraction += distractionValue;
        UpdateDistractednessMeter();
        if (distraction >= distractionThreshold)
        {
            StartCoroutine(Sound());
            if (fort != null)
            {
                fort.targets.Remove(this.gameObject);
            }
        }
    }

    protected void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Goal"))
        {
            healthManager.TakeDamage(damageToPlayer);
            

            KillCat(2);
        }
    }
    protected void KillCat(int type)
    {
        if (IsDead)
            return;


        // Prevents this function from running twice in rare cases, causing this cat's death to count as more than one.
        IsDead = true;
        if(type == 1)
        {
            // Fire the OnCatDied event.
            OnCatDied?.Invoke(this, EventArgs.Empty);
        }
        else if(type == 2)  
        {
            OnCatReachGoal?.Invoke(this, EventArgs.Empty);
        }

        
        Destroy(_DistractednessMeterBarImage.transform.parent.gameObject);

        // Destroy the cat.
        Destroy(gameObject);

    }

    public int Cuteness { get { return _CutenessValue; } }
    IEnumerator Sound()
    {
        NavController.agent.speed = 0;
        int index = Random.Range(0, purrs.Count - 1);
        catAudio.clip = purrs[index];
        catAudio.Play();
        yield return new WaitForSeconds(0.5f);

        KillCat(1);
    }

    public float Distraction { get { return distraction; } set { distraction = value; } }
    public float DistractionReward { get { return distractReward; } }
    public bool IsDead { get; private set; }
}