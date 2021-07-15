using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FateGames;
using TMPro;

public class MainLevelManager : LevelManager
{
    private static MainLevelManager instance;
    private Character character = null;
    public int NumberOfCollectedGems = 0;
    private Bag[] bags = null;
    private Thief[] thiefs = null;
    private Transform bonus = null;
    public float GemMultiplier = 1;
    [SerializeField] private TextMeshProUGUI gemText = null;
    public static MainLevelManager _Instance { get => instance; }
    public Character Character { get => character; }

    private new void Awake()
    {
        base.Awake();
        if (!instance)
            instance = this;
        character = FindObjectOfType<Character>();
        bonus = GameObject.FindGameObjectWithTag("Bonus").transform;
        bags = FindObjectsOfType<Bag>();
        thiefs = FindObjectsOfType<Thief>();
        gemText.text = GameManager.GEM.ToString();
    }

    private void Update()
    {
        CheckThiefPositions();
        if (!character.OnBonus && character.transform.position.z > bonus.position.z)
            character.OnBonus = true;
    }

    private void CheckThiefPositions()
    {
        for (int i = 0; i < thiefs.Length; i++)
        {
            if (thiefs[i].transform.position.z < character.transform.position.z + 12)
                thiefs[i].StartRunning();
        }
    }

    public override void StartLevel()
    {
        character.ChangeState(Character.State.WALKING);
    }
    public override void FinishLevel(bool success)
    {
        GameManager.Instance.State = GameManager.GameState.FINISHED;
        if (success)
            GameManager.GEM += (int)(NumberOfCollectedGems * GemMultiplier);
        LeanTween.delayedCall(1, () =>
        {
            character.ChangeState(Character.State.FAIL);
            GameManager.Instance.FinishLevel(success);
        });
    }



}
