using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BlessingState
{
    public BlessingData blessing;
    public int currentLevel;
}

public class MetaProgressionManager : MonoBehaviour
{
    public static MetaProgressionManager Instance { get; private set; }

    [SerializeField] private List<BlessingState> blessings = new List<BlessingState>();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        Object.DontDestroyOnLoad(gameObject);
    }

    public int GetBlessingLevel(BlessingData blessing)
    {
        if (blessing == null)
            return 0;

        BlessingState state = blessings.Find(b => b.blessing == blessing);
        return state != null ? state.currentLevel : 0;
    }

    public void IncreaseBlessingLevel(BlessingData blessing)
    {
        if (blessing == null)
            return;

        BlessingState state = blessings.Find(b => b.blessing == blessing);
        if (state == null)
        {
            state = new BlessingState { blessing = blessing, currentLevel = 0 };
            blessings.Add(state);
        }

        if (state.currentLevel < blessing.maxLevel)
        {
            state.currentLevel++;
            // TODO: apply blessing effect globally (e.g., increase max units, gold gain, etc.)
            // TODO: save blessings to disk.
        }
    }
}
