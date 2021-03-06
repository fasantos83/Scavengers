﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour {

	public float levelStartDelay = 2f;
	public float turnDelay = .1f;
	public static GameManager instance = null;
	public int playerFoodPoints = 100;
	[HideInInspector] public bool playersTurn = true;

	private Text levelText;
	private GameObject levelImage;
	private BoardManager boardScript;
	private int level = 1;
	private List<Enemy> enemies;
	private bool enemiesMoving;
	private bool doingSetup = true;
	private bool firstRun = true;

	void Awake() {
		if (instance == null) {
			instance = this;
		} else if (instance != this) {
			Destroy(gameObject);
		}

		DontDestroyOnLoad(gameObject);
		enemies = new List<Enemy>();
		boardScript = GetComponent<BoardManager>();
		InitGame();
	}

	void OnLevelFinishedLoading(Scene scene, LoadSceneMode mode){
		if (firstRun) {
			firstRun = false;
			return;
		}
		level++;
		InitGame();
	}

	void OnEnable(){
		SceneManager.sceneLoaded += OnLevelFinishedLoading;
	}

	void OnDisable(){
		SceneManager.sceneLoaded -= OnLevelFinishedLoading;
	}

	void InitGame(){
		doingSetup = true;
		levelImage = GameObject.Find("LevelImage");
		levelText = GameObject.Find("LevelText").GetComponent<Text>();
		levelText.text = "Day " + level; 
		levelImage.SetActive(true);
		Invoke("HideLevelImage", levelStartDelay);

		enemies.Clear();
		boardScript.SetupScene(level);
		RepositionCamera();
	}

	private void RepositionCamera(){
		Transform cameraTransform = Camera.main.transform;

		float offset = 0;
		if (boardScript.columns > 8) {
			offset++;
		}
		Vector3 newPosition = new Vector3(cameraTransform.position.x + offset, cameraTransform.position.y, cameraTransform.position.z);
		cameraTransform.position = newPosition;
	}

	private void HideLevelImage(){
		levelImage.SetActive(false);
		doingSetup = false;
	}

	public void GameOver(){
		levelText.text = "After " + level + " days, you starved.";
		levelImage.SetActive(true);
		StartCoroutine(RestartGame());
	}

	IEnumerator RestartGame(){
		yield return new WaitForSeconds(levelStartDelay);
		Destroy(gameObject);
		Destroy(SoundManager.instance.gameObject);
		SceneManager.LoadScene(0);
	}

	void Update () {
		if (playersTurn || enemiesMoving || doingSetup) {
			return;
		}

		StartCoroutine(MoveEnemies());
	}

	public void AddEnemyToList(Enemy script){
		enemies.Add(script);
	}

	IEnumerator MoveEnemies(){
		enemiesMoving = true;
		yield return new WaitForSeconds(turnDelay);
		if (enemies.Count == 0) {
			yield return new WaitForSeconds(turnDelay);
		}

		for (int i = 0; i < enemies.Count; i++) {
			enemies[i].MoveEnemy();
			yield return new WaitForSeconds(enemies[i].moveTime);
		}

		playersTurn = true;
		enemiesMoving = false;
	}
}
