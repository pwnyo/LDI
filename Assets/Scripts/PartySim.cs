using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PartySim : MonoBehaviour
{
    [System.Serializable]
    public class PartyChar
    {
        public string name;
        public int health;
        public Vector3Int stats;

        public PartyChar(string name, int health, Vector3Int stats)
        {
            this.name = name;
            this.health = health;
            this.stats = stats;
        }
        public void TakeDamage(int damage)
        {
            health -= damage;
            if (health <= 0)
            {
                health = 0;
                Debug.Log($"{name} died!");
            }
        }
    }
    public List<PartyChar> chars;
    public PartyChar enemy;
    List<PartyChar> charCopy;
    int currentIndex;
    bool hasTied;

    // Start is called before the first frame update
    void Start()
    {
        Restart();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Period))
        {
            Next();
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            Restart();
        }
    }
    void Restart()
    {
        charCopy = new List<PartyChar>();
        foreach (PartyChar pc in chars)
        {
            charCopy.Add(new PartyChar(pc.name, pc.health, pc.stats));
        }
    }
    void Next()
    {
        CalcDamage(charCopy[currentIndex], enemy);
        if (enemy.health <= 0)
        {
            Debug.Log("You won!");
            return;
        }

        if (charCopy[currentIndex].health <= 0)
        {
            charCopy.RemoveAt(currentIndex);
        }
        if (charCopy.Count == 0)
        {
            Debug.Log("You lost!");
            return;
        }
        currentIndex++;
        if (currentIndex >= charCopy.Count)
        {
            currentIndex = 0;
        }
    }
    void CalcDamage(PartyChar attacker, PartyChar defender)
    {
        hasTied = false;
        int strDmg, dexDmg, magDmg;
        strDmg = GetDamage(attacker.stats.x, defender.stats.x);
        dexDmg = GetDamage(attacker.stats.y, defender.stats.y);
        magDmg = GetDamage(attacker.stats.z, defender.stats.z);
        int damageTotal = strDmg + dexDmg + magDmg;
        defender.TakeDamage(damageTotal);

        Debug.Log($"{attacker.name} dealt {damageTotal} damage to {defender.name} ({strDmg}/{dexDmg}/{magDmg})!");
    }
    int GetDamage(int a, int b)
    {
        if (a == b && !hasTied)
        {
            hasTied = true;
            return 1;
        }
        else if (a > b)
        {
            return 1 + RollD6(a - b);
        }
        else
        {
            return 0;
        }
    }
    int RollD6(int difference)
    {
        int roll = Random.Range(1, 6);
        Debug.Log($"rolled d6: got {roll} + {difference}");
        if (Random.Range(1, 6) + difference >= 6)
        {
            return 1;
        }
        return 0;
    }
}
