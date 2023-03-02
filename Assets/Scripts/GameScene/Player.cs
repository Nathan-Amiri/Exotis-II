using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using FishNet.Connection;
using TMPro;
using System;

public class Player : NetworkBehaviour
{
    //layers: -2 = background, -1 = editorgrid, 0 = terrain, 1 = players/some HUD, 2 = missiles/spells/more HUD

    public SpriteRenderer spriteRenderer; //assigned in inspector
    public SpriteRenderer coreSpriteRenderer; //^
    public Animator animator; //^
    public PlayerMovement playerMovement; //^, read by Setup and VenomAbilities

    [NonSerialized] public GameManager gameManager; //set by Setup
    [NonSerialized] public Animator countdownAnim; //^
    [NonSerialized] public TMP_Text countdownText; //^
    [NonSerialized] public TMP_Text winnerText; //^
    [NonSerialized] public PlayAgain playAgain; //^
    [NonSerialized] public PlayerHUD playerHUD; //^

    //colors from lightest to darkest:
    [NonSerialized] public Color32 frost = new(140, 228, 232, 255); //read by index
    [NonSerialized] public Color32 wind = new(205, 205, 255, 255); //^
    [NonSerialized] public Color32 lightning = new(255, 236, 0, 255); //^
    [NonSerialized] public Color32 flame = new(255, 122, 0, 255); //^
    [NonSerialized] public Color32 water = new(35, 182, 255, 255); //^
    [NonSerialized] public Color32 venom = new(23, 195, 0, 255); //^

    [NonSerialized] public Color32 lighterColor; //set by index, read by abilitybase
    [NonSerialized] public Color32 darkerColor; //^

    [NonSerialized] public float maxHealth = 15; //can be altered by index

    private float power = 3;
    private float range = 8;
    private readonly float rangeMultiplier = 1.2f; //speed multiplier is in PlayerMovement


    [NonSerialized] public string[] charSelectInfo = new string[4];

    [NonSerialized] public GameObject abilityParent; //set by Setup
    private AbilityBase ability1;
    private AbilityBase ability2;
    private AbilityBase ability3;

    [SyncVar]
    private float health;
    [SyncVar]
    [NonSerialized] public bool isImmune = false;

    private float maxHealthBarWidth;

    private float missileFillSpeed;
    private float missileAmount;
    private float maxMissileBarWidth;
    private bool onMissileCooldown;

    private bool startUpdate;

    private Vector2 mousePosition;

    public GameObject missile; //assigned in inspector

    public static int alivePlayers = 0; //number of players not eliminated. used by server only
    private bool isEliminated; //server only
    public delegate void OnGameEndAction();
    public static event OnGameEndAction OnGameEnd;

    private void OnEnable()
    {
        OnGameEnd += GameEnd;
        PlayAgain.OnPlayAgain += NewGame;
    }
    private void OnDisable()
    {
        OnGameEnd -= GameEnd;
        PlayAgain.OnPlayAgain -= NewGame;
    }

    public void OnSpawn(Index index)
    {
        name = charSelectInfo[0];

        if (IsOwner)
        {
            GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Dynamic;

            RpcSpawnAbility(ClientManager.Connection, charSelectInfo[1], 1);
            //RpcSpawnAbility(ClientManager.Connection, charSelectInfo[2], 2);
            //RpcSpawnAbility(ClientManager.Connection, charSelectInfo[3], 3);
        }

        index.LoadAttributes(this, charSelectInfo); //add stats and spells

        playerHUD.gameObject.SetActive(true);
        playerHUD.charImage.charShell.color = lighterColor;
        playerHUD.charImage.charCore.color = darkerColor;

        maxHealthBarWidth = playerHUD.healthBarPivot.transform.localScale.x;

        missileFillSpeed = 1;
        maxMissileBarWidth = playerHUD.missileBarPivot.transform.localScale.x;

        NewGame();

        startUpdate = true;
    }

    [ServerRpc]
    private void RpcSpawnAbility(NetworkConnection owner, string abilityName, int abilityNumber)
    {
        GameObject abilityObject = Instantiate(Resources.Load("Abilities/" + abilityName), abilityParent.transform) as GameObject;
        ServerManager.Spawn(abilityObject, owner);
        AbilityBase newAbility = abilityObject.GetComponent<AbilityBase>();
        RpcClientSpawnAbility(newAbility, abilityName, abilityNumber);
    }

    [ObserversRpc]
    private void RpcClientSpawnAbility(AbilityBase newAbility, string abilityName, int abilityNumber)
    {
        if (abilityNumber == 1)
            ability1 = newAbility;
        else if (abilityNumber == 2)
            ability2 = newAbility;
        else
            ability3 = newAbility;

        newAbility.transform.position = new Vector2(-15, 0);

        newAbility.OnSpawn(this, abilityName);
        Color32 spellColor = newAbility.spellColor; //set in newAbility.OnSpawn

        if (abilityNumber == 1)
        {
            playerHUD.spell1Image.color = spellColor;
            newAbility.spellGray = playerHUD.spell1Gray;
        }
        else if (abilityNumber == 2)
        {
            playerHUD.spell2Image.color = spellColor;
            newAbility.spellGray = playerHUD.spell2Gray;
        }
        else //abilityNumber == 3
        {
            playerHUD.spell3Image.color = spellColor;
            newAbility.spellGray = playerHUD.spell3Gray;
        }
    }

    public void NewGame() //run by PlayAgain
    {
        missileAmount = 3;

        StartCoroutine(UnlockPlayers());

        if (IsOwner)
        {
            winnerText.text = "";

            if (GameManager.playerNumber == 1)
                transform.position = new Vector2(-5.5f, -2.5f);
            else if (GameManager.playerNumber == 2)
                transform.position = new Vector2(5.5f, -2.5f);
            else if (GameManager.playerNumber == 3)
                transform.position = new Vector2(-7.5f, 3);
            else if (GameManager.playerNumber == 4)
                transform.position = new Vector2(7.5f, 3f);

            StartCoroutine(Countdown());
        }

        if (IsServer)
        {
            isEliminated = false;

            playerMovement.isStunned = true;
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

    private IEnumerator Countdown()
    {
        yield return new WaitForSeconds(.3f);
        countdownText.text = "3";
        countdownAnim.SetTrigger("TrCountdown");
        yield return new WaitForSeconds(.9f);
        countdownText.text = "2";
        countdownAnim.SetTrigger("TrCountdown");
        yield return new WaitForSeconds(.9f);
        countdownText.text = "1";
        countdownAnim.SetTrigger("TrCountdown");
        yield return new WaitForSeconds(.9f);
        countdownText.text = "Go!";
        countdownAnim.SetTrigger("TrCountdown");
    }

    private IEnumerator UnlockPlayers()
    {
        yield return new WaitForSeconds(3);
        isImmune = false;
        playerMovement.isStunned = false;
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
            spriteRenderer.color = lighterColor;
            coreSpriteRenderer.color = darkerColor;
        }

        if (!IsOwner)
            return;

        mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        if (Input.GetButtonDown("Missile") && IsClient && !playerMovement.isStunned && missileAmount >= 1 && !onMissileCooldown)
        {
            StartCoroutine(MissileCooldown());
            Vector2 fireDirection = (mousePosition - new Vector2(transform.position.x, transform.position.y)).normalized;
            CreateMissile(this, transform.position, fireDirection, 0f);
            RpcServerCreateMissile(this, transform.position, fireDirection, TimeManager.Tick);
        }

        Abilities();
    }

    private void HealthBar() //run in update
    {
        float proportion = maxHealth / health; //maxHealth / health should equal the same proportion as maxHealthBarWidth / healthBar's scale.x

        GameObject healthBarPivot = playerHUD.healthBarPivot;

        if (healthBarPivot.transform.localScale.x > maxHealthBarWidth / proportion + .05f)
            healthBarPivot.transform.localScale -= new Vector3(Time.deltaTime, 0);
        else if (healthBarPivot.transform.localScale.x < maxHealthBarWidth / proportion - .05f)
            healthBarPivot.transform.localScale += new Vector3(Time.deltaTime * 2, 0);
        else
            healthBarPivot.transform.localScale = new Vector2(maxHealthBarWidth / proportion, 1);

        if (healthBarPivot.transform.localScale.x < .05f)
        {
            if (playerHUD.healthBar.activeSelf)
                playerHUD.healthBar.SetActive(false);
        }
        else if (!playerHUD.healthBar.activeSelf)
            playerHUD.healthBar.SetActive(true);
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

    private IEnumerator BecomeImmune(float duration) //run on server
    {
        isImmune = true;
        yield return new WaitForSeconds(duration);
        isImmune = false;
    }

    public void StatChange(string stat, int amount) //stat changes occur on the client. amount = number of stages (-2, -1, 1, or 2)
    {
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
        playerMovement.isStunned = true;
        RpcRelocate(Owner);
        CheckForGameEnd();
    }
    [TargetRpc]
    private void RpcRelocate(NetworkConnection conn)
    {
        transform.position = new Vector2(50, 0);
    }

    [Server]
    private void CheckForGameEnd() //called on server for newly eliminated player classes
    {
        alivePlayers -= 1;

        if (alivePlayers == 1)
            OnGameEnd?.Invoke();
    }

    [Server]
    private void GameEnd() //called on server for all player classes
    {
        if (!isEliminated)
            isImmune = true;

        RpcBeginReset(!isEliminated);
    }

    [ObserversRpc]
    private void RpcBeginReset(bool isWinner) //called on all player classes on all clients
    {
        if (isWinner)
            winnerText.text = name + " Wins!";
        if (IsOwner)
            StartCoroutine(PlayAgainScreen());
    }

    private IEnumerator PlayAgainScreen()
    {
        yield return new WaitForSeconds(2);

        Color32[] lightAndDark = new Color32[2];
        lightAndDark[0] = lighterColor;
        lightAndDark[1] = darkerColor;

        playAgain.NewPlayAgain(this, lightAndDark);
    }

    private const float maxPassedTime = 0.3f; //never change this!

    [ServerRpc]
    private void RpcServerCreateMissile(Player caster, Vector3 firePosition, Vector2 fireDirection, uint tick)
    {
        if (!IsOwner)
        {
            float passedTime = (float)TimeManager.TimePassed(tick, false); //false prevents negative
            passedTime = Mathf.Min(maxPassedTime / 2f, passedTime);

            CreateMissile(caster, firePosition, fireDirection, passedTime);
        }

        RpcClientCreateMissile(caster, firePosition, fireDirection, tick);
    }
    [ObserversRpc]
    private void RpcClientCreateMissile(Player caster, Vector3 firePosition, Vector2 fireDirection, uint tick)
    {
        if (IsServer || IsOwner)
            return;

        float passedTime = (float)TimeManager.TimePassed(tick, false); //false prevents negative
        passedTime = Mathf.Min(maxPassedTime / 2f, passedTime);

        CreateMissile(caster, firePosition, fireDirection, passedTime);
    }
    private void CreateMissile(Player caster, Vector3 firePosition, Vector2 fireDirection, float passedTime)
    {
        MissileInfo missileInfo = ObjectPool.sharedInstance.GetPooledInfo();

        GameObject newMissile = missileInfo.obj;
        newMissile.SetActive(true);

        Missile missileScript = missileInfo.missile;
        StartCoroutine(RevealMissile(missileScript));

        missileScript.spriteRenderer.color = caster.darkerColor;
        missileScript.coreSprireRenderer.color = caster.lighterColor;


        missileScript.missilePower = caster.power;
        missileScript.player = caster;

        caster.missileAmount -= 1;

        //missileObject = newMissile; //used for missile timer



        float displacementMagnitude = passedTime / 2.778f; //(number of ticks fired missile has traveled) / 2.778 = the distance the missile has traveled
        Vector3 displacement = (caster.range / 10) * displacementMagnitude * fireDirection;
        Vector3 castPosition = firePosition + new Vector3(fireDirection.x, fireDirection.y) * .5f;
        newMissile.transform.position = castPosition += displacement;

        missileScript.rb.velocity = fireDirection * caster.range;
    }

    //missile timer code used to initially test the average distance a missile travels per tick:

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
        playerHUD.missileBarPivot.transform.localScale = new Vector2(maxMissileBarWidth / proportion, playerHUD.missileBarPivot.transform.localScale.y);

        if (missileAmount < 3)
            missileAmount += missileFillSpeed * Time.deltaTime;
        else
            missileAmount = 3;
    }

    private void Abilities() //run in update
    {
        if (playerMovement.isStunned) return;

        if (Input.GetButtonDown("Ability1")) SelectAbility(1);
        if (Input.GetButtonDown("Ability2")) SelectAbility(2);
        if (Input.GetButtonDown("Ability3")) SelectAbility(3);
    }

    private void SelectAbility(int abilityNumber)
    {
        AbilityBase currentAbility = ability1;
        if (abilityNumber == 2) currentAbility = ability2;
        else if (abilityNumber == 3) currentAbility = ability3;

        if (currentAbility.onCooldown)
            return;

        Vector2 casterPosition = new(transform.position.x, transform.position.y);
        Vector2 aimPoint = mousePosition;
        if (currentAbility.hasRange)
        {
            float mouseRange = Vector3.Distance(transform.position, mousePosition);
            if (mouseRange > currentAbility.spellRange)
            {
                Vector2 aimDirection = (mousePosition - casterPosition).normalized;
                aimPoint = casterPosition + (aimDirection * currentAbility.spellRange);
            }
        }

        currentAbility.TriggerAbility(casterPosition, aimPoint);
        RpcServerTriggerAbility(ClientManager.Connection, abilityNumber, casterPosition, aimPoint);
    }
    [ServerRpc]
    protected void RpcServerTriggerAbility(NetworkConnection caster, int abilityNumber, Vector2 casterPosition, Vector2 aimPoint)
    {
        RpcClientTriggerAbility(caster, abilityNumber, casterPosition, aimPoint);
    }
    [ObserversRpc]
    protected void RpcClientTriggerAbility(NetworkConnection caster, int abilityNumber, Vector2 casterPosition, Vector2 aimPoint)
    {
        if (caster == ClientManager.Connection)
            return;

        AbilityBase currentAbility = ability1;
        if (abilityNumber == 2) currentAbility = ability2;
        else if (abilityNumber == 3) currentAbility = ability3;

        currentAbility.TriggerAbility(casterPosition, aimPoint);
    }
}