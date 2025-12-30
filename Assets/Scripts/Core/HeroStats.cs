using UnityEngine;

[System.Serializable]
public class HeroStats
{
    public int level = 1;
    public float currentXP = 0f;
    public float xpToNextLevel = 100f;
    public float maxHP = 100f;
    public float currentHP = 100f;
    public float attack = 10f;
    public float moveSpeed = 5f;

    public void AddXP(float amount)
    {
        currentXP += amount;

        while (currentXP >= xpToNextLevel)
        {
            currentXP -= xpToNextLevel;
            LevelUp();
        }
    }

    void LevelUp()
    {
        level++;
        maxHP += 10f;
        attack += 2f;
        moveSpeed += 0.1f;
        currentHP = maxHP;
        xpToNextLevel *= 1.2f;
    }

    public void TakeDamage(float damage)
    {
        currentHP -= damage;
        if (currentHP < 0f)
        {
            currentHP = 0f;
        }
    }

    public bool IsDead()
    {
        return currentHP <= 0f;
    }
}
