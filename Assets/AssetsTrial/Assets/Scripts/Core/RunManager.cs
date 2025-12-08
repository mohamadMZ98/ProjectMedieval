using UnityEngine;

public class RunManager : MonoBehaviour
{
    public static RunManager Instance { get; private set; }

    [SerializeField] private PlayerController hero;
    [SerializeField] private ArmyManager armyManager;
    [SerializeField] private WaveSpawner waveSpawner;

    [SerializeField] private float maxRunTimeSeconds = 1200f; // 20 minutes

    private float runTime = 0f;
    private bool isRunning = false;
    private int currentGold = 0;

    public bool IsRunning => isRunning;
    public float RunTime => runTime;
    public float RunProgress
    {
        get
        {
            if (maxRunTimeSeconds <= 0f) return 0f;
            return Mathf.Clamp01(runTime / maxRunTimeSeconds);
        }
    }

    public Transform HeroTransform => hero != null ? hero.transform : null;
    public PlayerController Hero => hero;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    void Start()
    {
        StartRun();
    }

    void Update()
    {
        if (!isRunning)
            return;

        runTime += Time.deltaTime;

        if (runTime >= maxRunTimeSeconds)
        {
            EndRun(true);
        }
    }

    public void StartRun()
    {
        runTime = 0f;
        isRunning = true;

        if (armyManager != null && hero != null)
        {
            armyManager.InitializeArmy(hero);
        }

        if (hero != null)
        {
            hero.EnableControl(true);
        }

        Debug.Log("Run started.");
    }

    public void EndRun(bool victory)
    {
        if (!isRunning)
            return;

        isRunning = false;

        if (hero != null)
        {
            hero.EnableControl(false);
        }

        Debug.Log($"Run ended. Victory = {victory}. RunTime = {runTime} seconds. Gold = {currentGold}");

        // TODO: transition back to town scene and pass rewards.
    }

    public void OnHeroDied()
    {
        EndRun(false);
    }

    public void AddHeroXP(float amount)
    {
        if (hero != null && hero.Stats != null)
        {
            hero.Stats.AddXP(amount);
        }
    }

    public void AddGold(int amount)
    {
        currentGold += amount;
    }
}
