using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using FishNet.Connection;
using TMPro;
using System;

public class Player : NetworkBehaviour
{
    //assigned in inspector
    public GameObject healthBarPivot;
    public GameObject healthBar;
    public GameObject missileBarPivot;
    public List<SpriteRenderer> spellBacks;
    public List<SpriteRenderer> spellIcons;
    public List<SpriteRenderer> spellGrays;

    //assigned in inspector, used by distortion
    public SpriteRenderer spriteRenderer;
    public SpriteRenderer coreRenderer;
    public Animator animator;

    //assigned in inspector, read by Setup and various spells
    public PlayerMovement playerMovement;

    //set by Setup
    [NonSerialized] public GameManager gameManager;
    [NonSerialized] public Animator countdownAnim;
    [NonSerialized] public TMP_Text countdownText;
    [NonSerialized] public TMP_Text winnerText;
    [NonSerialized] public MapManager mapManager;

    //read by index
    [NonSerialized] public Color32 water = new(35, 182, 255, 255);
    [NonSerialized] public Color32 flame = new(255, 122, 0, 255);
    [NonSerialized] public Color32 wind = new(205, 205, 255, 255);
    [NonSerialized] public Color32 lightning = new(255, 236, 0, 255);
    [NonSerialized] public Color32 frost = new(140, 228, 232, 255);
    [NonSerialized] public Color32 venom = new(23, 195, 0, 255);

    //set by index, read by spellbase
    [NonSerialized] public Color32 shellColor;
    [NonSerialized] public Color32 coreColor;

    [NonSerialized] public float maxHealth = 15; //can be altered by index

    private float power = 3;
    private float range = 8;
    private readonly float rangeMultiplier = 1.2f; //speed multiplier is in PlayerMovement


    [NonSerialized] public string[] charSelectInfo = new string[4];

    [NonSerialized] public GameObject spellParent; //set by Setup
    private SpellBase spell1;
    private SpellBase spell2;
    private SpellBase spell3;

    [SyncVar]
    private float health;
    [SyncVar]
    [NonSerialized] public bool isImmune = false;

    private float maxHealthBarWidth;

    private float missileFillSpeed;
    private float missileAmount;
    private float maxMissileBarWidth;
    private bool onMissileCooldown;

    [NonSerialized] public Infect infectSpell; //used by CreateMissile and Missile, null unless next missile is infected

    private bool startUpdate;

    [NonSerialized] public Vector2 mousePosition; //read by swoop

    public GameObject missile; //assigned in inspector

    public static int alivePlayers = 0; //number of players not eliminated. used by server only
    [NonSerialized] public bool isEliminated; //server only, read by Recharge
    public delegate void OnRoundEndAction();
    public static event OnRoundEndAction OnRoundEnd;

    private void OnEnable()
    {
        OnRoundEnd += RoundEnd;
        GameManager.OnNewRound += NewRound;
        GameManager.OnGameEnd += GameEnd;
    }
    private void OnDisable()
    {
        OnRoundEnd -= RoundEnd;
        GameManager.OnNewRound -= NewRound;
        GameManager.OnGameEnd -= GameEnd;
    }

    public void OnSpawn()
    {
        name = charSelectInfo[0];

        shellColor = (Color32)GetType().GetField(charSelectInfo[1]).GetValue(this);
        coreColor = (Color32)GetType().GetField(charSelectInfo[2]).GetValue(this);

        if (IsOwner)
        {
            PlayerScoreInfo newInfo = new()
            {
                shellColor = shellColor,
                coreColor = coreColor,
                username = PlayerPrefs.GetString("Username")
            };
            gameManager.RpcServerAddScoreInfo(newInfo, GameManager.playerNumber);


            GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Dynamic;

            RpcSpawnSpell(ClientManager.Connection, charSelectInfo[5], 0);
            RpcSpawnSpell(ClientManager.Connection, charSelectInfo[6], 1);
            RpcSpawnSpell(ClientManager.Connection, charSelectInfo[7], 2);

            mapManager.player = this;
        }


        if (charSelectInfo[3] == "health")
            maxHealth += 3;
        else if (charSelectInfo[3] == "power")
            StatChange("power", 1);

        if (charSelectInfo[4] == "speed")
            StatChange("speed", 1);
        else if (charSelectInfo[4] == "range")
            StatChange("range", 1);

        maxHealthBarWidth = healthBarPivot.transform.localScale.x;

        missileFillSpeed = 1;
        maxMissileBarWidth = missileBarPivot.transform.localScale.x;

        NewRound();

        startUpdate = true;
    }

    [ServerRpc]
    private void RpcSpawnSpell(NetworkConnection owner, string spellName, int spellNumber)
    {
        GameObject spellObject = Instantiate(Resources.Load("Spells/" + spellName), spellParent.transform) as GameObject;
        ServerManager.Spawn(spellObject, owner);
        SpellBase newSpell = spellObject.GetComponent<SpellBase>();
        RpcClientSpawnSpell(newSpell, spellName, spellNumber);
    }

    [ObserversRpc]
    private void RpcClientSpawnSpell(SpellBase newSpell, string spellName, int spellNumber)
    {
        if (spellNumber == 0)
            spell1 = newSpell;
        else if (spellNumber == 1)
            spell2 = newSpell;
        else
            spell3 = newSpell;

        newSpell.transform.position = new Vector2(-15, 0);

        newSpell.OnSpawn(this, spellName);
        Color32 spellColor = newSpell.spellColor; //set in newSpell.OnSpawn

        spellBacks[spellNumber].color = spellColor;
        //spellIcons[spellNumber].sprite = Resources.Load<Sprite>("SpellIcons/" + spellName);
        newSpell.spellGray = spellGrays[spellNumber];
    }

    public void NewRound() //run on all players on all clients
    {
        missileAmount = 3;
        StartCoroutine(UnlockPlayers());

        if (IsOwner)
        {
            mapManager.LoadNewMap();

            winnerText.text = "";

            StartCoroutine(Countdown());

            playerMovement.NewRound();
        }

        if (IsServer)
        {
            isEliminated = false;

            isImmune = true;
            health = maxHealth;
        }

        if (IsOwner && IsServer)
        {
            alivePlayers = 0;
            for (int i = 0; i < gameManager.playerNumbers.Length; i++)
                if (gameManager.playerNumbers[i] != 0)
                    alivePlayers++;
        }
    }

    public void SpawnPlayerOnMap(Vector2[] currentMapSpawnPositions) //called by MapManager after new map has been loaded
    {
        transform.position = currentMapSpawnPositions[GameManager.playerNumber - 1];
    }


    private IEnumerator Countdown()
    {
        string[] count = { "3", "2", "1", "Go!" };
        yield return new WaitForSeconds(.3f);

        for (int i = 0; i < 4; i++)
        {
            countdownText.text = count[i];
            countdownAnim.SetTrigger("TrCountdown");
            yield return new WaitForSeconds(.9f);
        }
    }

    private IEnumerator UnlockPlayers()
    {
        yield return new WaitForSeconds(3);
        if (IsServer)
            isImmune = false;
        if (IsOwner)
        {
            playerMovement.ToggleStun(false);
            playerMovement.ToggleFreeze(false);
        }
    }

    private void Update()
    {
        if (!startUpdate)
            return;

        HealthBar();
        MissileBar();

        //MissileTimer();

        if (!animator.enabled)
        {
            spriteRenderer.color = shellColor;
            coreRenderer.color = coreColor;
        }

        if (!IsOwner)
            return;

        mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        bool missileInput = Input.GetKeyDown(InputManager.InputIndex["Fire"]);
        if (missileInput && IsClient && !playerMovement.isStunned && missileAmount >= 1 && !onMissileCooldown)
        {
            StartCoroutine(MissileCooldown());
            Vector2 fireDirection = (mousePosition - new Vector2(transform.position.x, transform.position.y)).normalized;
            CreateMissile(transform.position, fireDirection, 0f);
            RpcServerCreateMissile(transform.position, fireDirection, TimeManager.Tick);
        }

        Abilities();
    }

    private void HealthBar() //run in update
    {
        float proportion = maxHealth / health; //maxHealth / health should equal the same proportion as maxHealthBarWidth / healthBar's scale.x

        if (healthBarPivot.transform.localScale.x > maxHealthBarWidth / proportion + .05f)
            healthBarPivot.transform.localScale -= new Vector3(Time.deltaTime, 0);
        else if (healthBarPivot.transform.localScale.x < maxHealthBarWidth / proportion - .05f)
            healthBarPivot.transform.localScale += new Vector3(Time.deltaTime * 2, 0);
        else
            healthBarPivot.transform.localScale = new Vector2(maxHealthBarWidth / proportion, 1);

        if (healthBarPivot.transform.localScale.x < .05f)
        {
            if (healthBar.activeSelf)
                healthBar.SetActive(false);
        }
        else if (!healthBar.activeSelf)
            healthBar.SetActive(true);
    }

    [Server]
    public void HealthChange(float amount) //damage changes occur on the server
    {
        if (isImmune)
            return;

        health += amount;

        if (health > maxHealth)
            health = maxHealth;
        else if (health <= 0)
        {
            health = 0;
            Eliminate();
            return;
        }

        if (amount < 0)
            RpcDamageAnimation();
    }

    [ObserversRpc]
    private void RpcDamageAnimation()
    {
        StartCoroutine(DamageAnimation(.7f));
    }

    private IEnumerator DamageAnimation(float duration)
    {
        animator.enabled = true;
        animator.SetTrigger("TakeDamage");
        MainCamera.screenShakeIntensity += 1;
        yield return new WaitForSeconds(duration);
        animator.StopPlayback();
        animator.enabled = false;
    }

    [Server]
    public IEnumerator BecomeImmune(float duration)
    {
        isImmune = true;
        yield return new WaitForSeconds(duration);
        isImmune = false;
    }

    public void StatChange(string stat, int amount) //stat changes occur on the client. amount = number of stages (-2, -1, 1, or 2)
    {
        if (isImmune)
            return;

        if (stat == "power")
            power += amount;
        else
        {
            bool multiply = amount > 0;
            amount = Mathf.Abs(amount);

            if (stat == "speed")
                playerMovement.SpeedChange(multiply, amount);
            else if (stat == "range")
                for (int i = 0; i < amount; i++)
                    if (multiply)
                        range *= rangeMultiplier;
                    else
                        range /= rangeMultiplier;
        }
    }



    [Server]
    private void Eliminate()
    {
        isEliminated = true;
        RpcClientEliminate(Owner);
        CheckForGameEnd();
    }
    [TargetRpc]
    private void RpcClientEliminate(NetworkConnection conn)
    {
        transform.position = new Vector2(50, 0);
        playerMovement.ToggleStun(true);
        playerMovement.ToggleFreeze(true);
    }

    [Server]
    private void CheckForGameEnd() //called on server for newly eliminated player classes
    {
        alivePlayers -= 1;

        if (alivePlayers == 1)
            OnRoundEnd?.Invoke();
    }

    [Server]
    private void RoundEnd() //called on server for all player classes
    {
        if (!isEliminated)
            isImmune = true;

        RpcBeginReset(!isEliminated);
    }

    [ObserversRpc]
    private void RpcBeginReset(bool isWinner) //run on all player classes on all clients
    {
        StartCoroutine(SpellGameEnd());
        if (isWinner)
        {
            winnerText.text = name + " Wins!";

            if (IsOwner)
                StartCoroutine(RoundEndDelay());
        }
    }

    private IEnumerator SpellGameEnd()
    {
        yield return new WaitForSeconds(2);
        spell1.GameEnd();
        spell2.GameEnd();
        spell3.GameEnd();
    }

    private IEnumerator RoundEndDelay() //run on winning client's owned player
    {
        yield return new WaitForSeconds(2.5f);

        gameManager.RpcServerRoundWon(GameManager.playerNumber);
        //if game is over, final scores will be invoked by gamemanager. Else, NewGame will be invoked instead
    }

    private void GameEnd(GameManager gm) //invoked by gamemanager on all players on all clients
    {
        if (IsOwner)
        {
            playerMovement.ToggleFreeze(true);
            playerMovement.ToggleStun(true);
        }
    }


    private const float maxPassedTime = 0.3f; //never change this!

    [ServerRpc]
    private void RpcServerCreateMissile(Vector3 firePosition, Vector2 fireDirection, uint tick)
    {
        if (!IsOwner)
        {
            float passedTime = (float)TimeManager.TimePassed(tick, false); //false prevents negative
            passedTime = Mathf.Min(maxPassedTime / 2f, passedTime);

            CreateMissile(firePosition, fireDirection, passedTime);
        }

        RpcClientCreateMissile(firePosition, fireDirection, tick);
    }
    [ObserversRpc]
    private void RpcClientCreateMissile(Vector3 firePosition, Vector2 fireDirection, uint tick)
    {
        if (IsServer || IsOwner)
            return;

        float passedTime = (float)TimeManager.TimePassed(tick, false); //false prevents negative
        passedTime = Mathf.Min(maxPassedTime / 2f, passedTime);

        CreateMissile(firePosition, fireDirection, passedTime);
    }
    private void CreateMissile(Vector3 firePosition, Vector2 fireDirection, float passedTime)
    {
        MissileInfo missileInfo = ObjectPool.sharedInstance.GetPooledInfo();

        GameObject newMissile = missileInfo.obj;
        newMissile.SetActive(true);

        Missile missileScript = missileInfo.missile;
        StartCoroutine(RevealMissile(missileScript));

        missileScript.spriteRenderer.color = coreColor;
        missileScript.coreSpriteRenderer.color = shellColor;


        missileScript.missilePower = power;
        missileScript.player = this;

        missileAmount -= 1;

        //missileObject = newMissile; //used for missile timer


        float displacementMagnitude = passedTime / 2.778f; //(number of ticks fired missile has traveled) / 2.778 = the distance the missile has traveled
        Vector3 displacement = (range / 10) * displacementMagnitude * fireDirection;
        Vector3 castPosition = firePosition + new Vector3(fireDirection.x, fireDirection.y) * .5f;
        newMissile.transform.position = castPosition += displacement;

        missileScript.rb.velocity = fireDirection * range;

        if (infectSpell != null)
            infectSpell.FireInfectedMissile();
    }

    //missile timer code used to initially test the average distance a missile travels per tick
    //(2.778 according to last test)
    //private int ticks = 0;
    //private GameObject missileObject;
    //private Vector3 cachedMissilePosition;
    //private void MissileTimer() //run in update
    //{
    //    if (missileObject != null && TimeManager.Tick > ticks)
    //    {
    //        ticks = (int)TimeManager.Tick;

    //        if (cachedMissilePosition != default)
    //            Debug.Log(Vector3.Distance(cachedMissilePosition, missileObject.transform.position);
    //        cachedMissilePosition = missileObject.transform.position;
    //    }
    //    else if (missileObject == null)
    //        cachedMissilePosition = default;
    //}

    private IEnumerator MissileCooldown()
    {
        onMissileCooldown = true;
        yield return new WaitForSeconds(.1f);
        onMissileCooldown = false;
    }

    private IEnumerator RevealMissile(Missile missileScript)
    {
        yield return new WaitForSeconds(.01f);
        missileScript.spriteRenderer.enabled = true;
    }

    private void MissileBar() //run in update
    {
        float proportion = 3 / missileAmount; //3 is max missile amount. 3 / missileAmount should equal the same proportion as maxMissileBarWidth / missileBar's scale.x 
        missileBarPivot.transform.localScale = new Vector2(maxMissileBarWidth / proportion, missileBarPivot.transform.localScale.y);

        if (missileAmount < 3)
            missileAmount += missileFillSpeed * Time.deltaTime;
        else
            missileAmount = 3;
    }

    private void Abilities() //run in update
    {
        if (playerMovement.isStunned) return;

        if (Input.GetKeyDown(InputManager.InputIndex["Spell1"])) SelectSpell(1);
        if (Input.GetKeyDown(InputManager.InputIndex["Spell2"])) SelectSpell(2);
        if (Input.GetKeyDown(InputManager.InputIndex["Spell3"])) SelectSpell(3);
    }

    private void SelectSpell(int spellNumber)
    {
        SpellBase currentSpell = spell1;
        if (spellNumber == 2) currentSpell = spell2;
        else if (spellNumber == 3) currentSpell = spell3;

        if (currentSpell.spellLock)
            return;

        Vector2 casterPosition = new(transform.position.x, transform.position.y);
        Vector2 aimPoint = mousePosition;

        currentSpell.TriggerSpell(casterPosition, aimPoint);
        RpcServerTriggerSpell(ClientManager.Connection, spellNumber, casterPosition, aimPoint);
    }
    [ServerRpc]
    protected void RpcServerTriggerSpell(NetworkConnection caster, int spellNumber, Vector2 casterPosition, Vector2 aimPoint)
    {
        RpcClientTriggerSpell(caster, spellNumber, casterPosition, aimPoint);
    }
    [ObserversRpc]
    protected void RpcClientTriggerSpell(NetworkConnection caster, int spellNumber, Vector2 casterPosition, Vector2 aimPoint)
    {
        if (caster == ClientManager.Connection)
            return;

        SpellBase currentSpell = spell1;
        if (spellNumber == 2) currentSpell = spell2;
        else if (spellNumber == 3) currentSpell = spell3;

        currentSpell.TriggerSpell(casterPosition, aimPoint);
    }
}