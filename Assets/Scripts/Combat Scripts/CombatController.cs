using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatController : MonoBehaviour
{
    [Header("Database")]
    [SerializeField] private Database _database;

    [Header("Panels")]
    [SerializeField] private Transform _combatPanel;

    [Header("Prefabs")]
    [SerializeField] private CombatStage _combatStagePrefab;

    [Header("Combat Control Variables")]
    [SerializeField][Range(0.1f, 1f)] private float _combatMovingTime = 1f;

    [SerializeField] private List<Color> _visibilityMasks;

    private readonly int frontY = -100;
    private readonly int startY = 300;
    private readonly int stageSpacing = 200;

    private List<CombatStage> _combatStages;
    private readonly int minStageSize = 3;

    private List<EnemyData> _enemies;
    private const int MIN_ENEMY_COUNT = 1;
    private const int MAX_ENEMY_COUNT = 3;

    private void OnEnable()
    {
        GameEvents.StageEvents.OnStageStarted += InitializeEnemies;
        GameEvents.StageEvents.OnStageCleared += StageClear;
    }

    private void InitializeEnemies()
    {
        _enemies = new List<EnemyData>();

        foreach (EnemyData enemy in _database.enemies)
        {
            if (enemy.LevelRequirement <= GameManager.instance.GameLevel)
            {
                _enemies.Add(enemy);
            }
        }

        InitializeStageBegin();
    }

    private void InitializeStageBegin()
    {
        _combatStages = new List<CombatStage>();

        while (_combatStages.Count < minStageSize)
        {
            Vector3 position = new Vector3(0, startY + (_combatStages.Count * stageSpacing), 0);

            StageInstantiate(position);
        }

        StartCoroutine(StageMoverCoroutine());
    }

    private void StageClear(CombatStage stage)
    {
        _combatStages.Remove(stage);
        Destroy(stage.gameObject);

        Vector3 instantiatePosition = new Vector3(0, _combatStages[^1].Position.y + stageSpacing, 0);
        StageInstantiate(instantiatePosition);

        StartCoroutine(StageMoverCoroutine());
    }

    private IEnumerator StageMoverCoroutine()
    {
        //Ba�lang��ta mevcut a�amalar�n g�r�n�rl���n� ayarl�yoruz
        for (int i = 0; i < _combatStages.Count; i++)
        {
            CombatStage stage = _combatStages[i];

            stage.VisibilityMask = _visibilityMasks[i];
        }

        //S�re� boyunca mevcut a�amalar� ilerletiyoruz.
        float timer = 0;
        while (timer < _combatMovingTime)
        {
            timer += Time.deltaTime;

            float normalizedTime = timer / _combatMovingTime;

            for (int i = 0; i < _combatStages.Count; i++)
            {
                CombatStage stage = _combatStages[i];

                Vector3 newPosition = new Vector3(0, frontY + (i * stageSpacing), 0);
                Vector3 newScale = new Vector3(1f - ((float)i / (_combatStages.Count - 1)), 1f - ((float)i / (_combatStages.Count - 1)), 1);

                stage.Position = Vector3.Lerp(stage.LastPosition, newPosition, normalizedTime);
                stage.Scale = Vector3.Lerp(stage.LastScale, newScale, normalizedTime);
            }

            yield return null;
        }

        //��lemler bitti�inde bulunduklar� pozisyonu kay�t ediyoruz
        for (int i = 0; i < _combatStages.Count; i++)
        {
            CombatStage stage = _combatStages[i];

            stage.LastPosition = stage.Position;
            stage.LastScale = stage.Scale;
        }

        GameEvents.StageEvents.OnStageSelected?.Invoke(_combatStages[0]);
    }


    private void StageInstantiate(Vector3 position)
    {
        CombatStage combatStage = Instantiate(_combatStagePrefab, _combatPanel, true);
        combatStage.Position = position;

        combatStage.LastPosition = combatStage.Position;
        combatStage.LastScale = combatStage.Scale;

        int enemyCount = Random.Range(MIN_ENEMY_COUNT, MAX_ENEMY_COUNT + 1);

        List<EnemyData> randomEnemies = new List<EnemyData>();
        for (int i = 0; i < enemyCount; i++)
        {
            EnemyData enemy = _enemies[Random.Range(0, _enemies.Count)];

            if (enemy != null)
            {
                randomEnemies.Add(enemy);
            }
        }

        combatStage.InitializeSlots(randomEnemies);

        _combatStages.Add(combatStage);
    }
}
