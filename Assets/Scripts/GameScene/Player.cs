using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using FishNet.Connection;

public class Player : NetworkBehaviour
{
    //layers: -2 = background, -1 = editorgrid, 0 = terrain, 1 = players/some HUD, 2 = missiles/spells/more HUD

    public SpriteRenderer spriteRenderer; //assigned in inspector
    public SpriteRenderer coreSpriteRenderer; //^
    public Animator animator; //^
    public PlayerMovement playerMovement; //^, read by Setup

    //colors from lightest to darkest:
    [HideInInspector] public Color32 frost = new(140, 228, 232, 255); //read by index
    [HideInInspector] public Color32 wind = new(205, 205, 255, 255); //^
    [HideInInspector] public Color32 lightning = new(255, 236, 0, 255); //^
    [HideInInspector] public Color32 flame = new(255, 122, 0, 255); //^
    [HideInInspector] public Color32 water = new(35, 182, 255, 255); //^
    [HideInInspector] public Color32 venom = new(23, 195, 0, 255); //^

    [HideInInspector] public Color32 lighterColor; //set by index
    [HideInInspector] public Color32 darkerColor; //^

    [HideInInspector] public float maxHealth = 15; //can be altered by index

    private float power = 3;
    private float range = 10;
    private readonly float speedMultipler = 1.2f;
    private readonly float jumpMultiplier = 1.1f;
    private readonly float rangeMultiplier = 1.3f;


    [HideInInspector] public string[] charSelectInfo = new string[4];

    [SyncVar]
    private float health;
    [SyncVar]
    [HideInInspector] public bool isImmune = false;

    private float maxHealthBarWidth;

    private float missileFillSpeed;
    private float missileAmount;
    private float maxMissileBarWidth;

    private bool startUpdate;

    private Vector2 mousePosition;

    public GameObject missile; //set in inspector

    [HideInInspector] public GameObject playerHud;
    private GameObject healthBar; //is actually the health bar's pivot point
    private GameObject missileBar; //is actually the missile bar's pivot point

    [HideInInspector] public GameManager gameManager; //set by Setup
    public static int alivePlayers = 0; //number of players not eliminated. used by server only
    private bool isEliminated; //server only
    public delegate void OnGameEndAction();
    public static event OnGameEndAction OnGameEnd;
    private void OnEnable()
    {
        OnGameEnd += GameEnd;
    }
    private void OnDisable()
    {
        OnGameEnd -= GameEnd;
    }

    public void OnSpawn(Index index)
    {
        playerMovement.isStunned = true;

        name = charSelectInfo[0];
        index.LoadAttributes(this, charSelectInfo); //add stats and spells

        playerHud.SetActive(true);
        playerHud.transform.GetChild(0).GetComponent<SpriteRenderer>().color = lighterColor; //shell
        playerHud.transform.GetChild(0).GetChild(0).GetComponent<SpriteRenderer>().color = darkerColor; //core

        if (IsOwner)
            GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Dynamic;

        healthBar = playerHud.transform.GetChild(2).gameObject;
        missileBar = playerHud.transform.GetChild(3).GetChild(1).gameObject;
        
        health = maxHealth;
        maxHealthBarWidth = healthBar.transform.localScale.x;

        missileFillSpeed = 1;
        missileAmount = 3;
        maxMissileBarWidth = missileBar.transform.localScale.x;
        
        if (IsServer && IsOwner)
            for (int i = 0; i < gameManager.playerNumbers.Length; i++)
                if (gameManager.playerNumbers[i] != 0)
                    alivePlayers++;

        startUpdate = true;
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

        if (Input.GetButtonDown("Missile") && IsClient && !playerMovement.isStunned && missileAmount >= 1)
        {
            Vector2 fireDirection = (mousePosition - new Vector2(transform.position.x, transform.position.y)).normalized;
            CreateMissile(this, transform.position, fireDirection, 0f);
            RpcServerCreateMissile(this, transform.position, fireDirection, TimeManager.Tick);
        }
    }

    private void HealthBar() //run in update
    {
        float proportion = maxHealth / health; //maxHealth / health should equal the same proportion as maxHealthBarWidth / healthBar's scale.x

        if (healthBar.transform.localScale.x > maxHealthBarWidth / proportion)
            healthBar.transform.localScale -= new Vector3(Time.deltaTime, 0);
        else if (healthBar.transform.localScale.x < maxHealthBarWidth / proportion)
            healthBar.transform.localScale += new Vector3(Time.deltaTime, 0);
    }

    [Server]
    public void HealthChange(float amount)
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
        {
            StartCoroutine(BecomeImmune(.7f));
            playerMovement.BecomeStunned(.35f, false);
            RpcClientTakeDamage();
        }
    }

    [ObserversRpc]
    private void RpcClientTakeDamage()
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


    public void StatChange(string stat, int amount) //amount = number of stages (-2, -1, 1, or 2)
    {
        if (stat == "power")
            power += amount;
        else
        {
            bool multiply = amount > 0;
            amount = Mathf.Abs(amount);
            if (stat == "speed")
            {
                for (int i = 0; i < amount; i++)
                    if (multiply)
                    {
                        playerMovement.speed *= speedMultipler;
                        playerMovement.jumpForce *= jumpMultiplier;
                        playerMovement.lowJumpMultiplier /= jumpMultiplier;
                    }
                    else
                    {
                        playerMovement.speed /= speedMultipler;
                        playerMovement.jumpForce /= jumpMultiplier;
                        playerMovement.lowJumpMultiplier *= jumpMultiplier;
                    }
            }
            else if (stat == "range")
            {
                for (int i = 0; i < amount; i++)
                    if (multiply)
                        range *= rangeMultiplier;
                    else
                        range /= rangeMultiplier;
            }
        }
    }

    [Server]
    private void Eliminate()
    {
        isEliminated = true;
        RpcClientTakeDamage();
        playerMovement.BecomeStunned(0, true);
        RpcRelocate(Owner);
        CheckForGameEnd();
    }
    [TargetRpc]
    private void RpcRelocate(NetworkConnection conn)
    {
        transform.position = new Vector2(50, 0);
    }

    [Server]
    private void CheckForGameEnd()
    {
        alivePlayers -= 1;

        if (alivePlayers == 1)
            OnGameEnd?.Invoke();
    }

    [Server]
    private void GameEnd()
    {
        if (!isEliminated)
            Debug.Log(name + " WON!!");
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

        CreateMissile(caster, firePosition,  fireDirection, passedTime);
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



        //new code:
        if (name == "Nymph")
        {
            float displacementMagnitude = passedTime == 0 ? 0 : passedTime / 2.778f;
            Vector3 displacement = fireDirection * (displacementMagnitude * (caster.range / 10));
            Vector3 castPosition = firePosition + new Vector3(fireDirection.x, fireDirection.y) * .5f;
            newMissile.transform.position = castPosition += displacement;
            missileScript.rb.velocity = fireDirection * caster.range;
        }
        else        //old code:
        {
            newMissile.transform.position = transform.position + new Vector3(fireDirection.x, fireDirection.y) * .5f;
            missileScript.rb.velocity = fireDirection * caster.range;
        }






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
    //            Debug.Log((cachedMissilePosition - missileObject.transform.position).magnitude);
    //        cachedMissilePosition = missileObject.transform.position;
    //    }
    //    else if (missileObject == null)
    //        cachedMissilePosition = default;
    //}









    private IEnumerator RevealMissile(Missile missileScript)
    {
        yield return new WaitForSeconds(.01f);
        missileScript.spriteRenderer.enabled = true;
    }

    private void MissileBar() //run in update
    {
        float proportion = 3 / missileAmount; //3 is max missile amount. 3 / missileAmount should equal the same proportion as maxMissileBarWidth / missileBar's scale.x 
        missileBar.transform.localScale = new Vector2(maxMissileBarWidth / proportion, missileBar.transform.localScale.y);

        if (missileAmount < 3)
            missileAmount += missileFillSpeed * Time.deltaTime;
        else
            missileAmount = 3;
    }
}