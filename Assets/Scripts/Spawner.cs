using UnityEngine;

// 3-модульдің қосымша тапсырмасы: тиындарды кодпен шығару.
// Бос нысанға жапсырып, Coin Prefab өрісіне Assets/Prefabs/Coin префабын салыңыз.
public class Spawner : MonoBehaviour
{
    public GameObject coinPrefab;

    void Start()
    {
        for (int i = 0; i < 5; i++)
            Instantiate(coinPrefab, new Vector3(i * 2f - 4f, -2.8f, 0f), Quaternion.identity);
    }
}
