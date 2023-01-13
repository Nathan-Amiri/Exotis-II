using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet.Object;
using FishNet.Object.Synchronizing;

public class Player : NetworkBehaviour
{
    //layers: -2 = background, -1 = editorgrid, 0 = terrain, 1 = players/some HUD, 2 = missiles/spells/more HUD

    public SpriteRenderer spriteRenderer; //assigned in inspector
    public Animator animator; //^
    public PlayerMovement playerMovement; //^, read by Setup


    [HideInInspector] public float maxHealth = 15; //altered by index

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

    public void OnSpawn(Index index)
    {
        playerMovement.isStunned = true;

        name = charSelectInfo[0];
        index.LoadAttributes(this, charSelectInfo); //add stats and spells
        //spriteRenderer.sprite = Resources.Load<Sprite>("Elementals/" + name);
        playerHud.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("Elementals/" + name);

        if (IsOwner)
            GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Dynamic;

        healthBar = playerHud.transform.GetChild(2).gameObject;
        missileBar = playerHud.transform.GetChild(3).GetChild(1).gameObject;
        
        health = maxHealth;
        maxHealthBarWidth = healthBar.transform.localScale.x;

        missileFillSpeed = 1;
        missileAmount = 3;
        maxMissileBarWidth = missileBar.transform.localScale.x;

        startUpdate = true;
    }

    private void Update()
    {
        if (!startUpdate)
            return;

        HealthBar();
        MissileBar();

        if (!IsOwner)
            return;

        mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        if (Input.GetButtonDown("Missile") && IsClient && !playerMovement.isStunned && missileAmount >= 1)
        {
            Test();
            //Vector2 fireDirection = (mousePosition - new Vector2(transform.position.x, transform.position.y)).normalized;
            //RpcServerCreateMissile(this, fireDirection);
        }
    }
    [ServerRpc (RequireOwnership = false)]
    private void Test()
    {
        HealthChange(-1);
    }

    private void HealthBar() //run in update
    {
        float proportion = maxHealth / health; //maxHealth / health should equal the same proportion as maxHealthBarWidth / healthBar's scale.x

        if (healthBar.transform.localScale.x > maxHealthBarWidth / proportion)
            healthBar.transform.localScale -= new Vector3(Time.deltaTime, 0);
        else if (healthBar.transform.localScale.x < maxHealthBarWidth / proportion)
            healthBar.transform.localScale += new Vector3(Time.deltaTime, 0);
    }

    public void HealthChange(float amount) //run on server
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
            playerMovement.isStunned = true;
            playerMovement.BecomeStunned(.35f, false);
        }
    }

    private IEnumerator BecomeImmune(float duration) //run on server
    {
        isImmune = true;
        animator.SetTrigger("TakeDamage");
        yield return new WaitForSeconds(duration);
        animator.SetTrigger("Empty");
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

    private void Eliminate()
    {
        playerMovement.isStunned = true;
        playerMovement.BecomeStunned(0, true);
        Debug.Log(name + " has been eliminated");
        transform.position = new Vector2(50, 0);
    }

    [ServerRpc]
    private void RpcServerCreateMissile(Player caster, Vector2 fireDirection)
    {
        CreateMissile(caster, fireDirection);
        RpcClientCreateMissile(caster, fireDirection);
    }
    [ObserversRpc]
    private void RpcClientCreateMissile(Player caster, Vector2 fireDirection)
    {
        if (!IsServer)
            CreateMissile(caster, fireDirection);
    }
    private void CreateMissile(Player caster, Vector2 fireDirection)
    {        
        MissileInfo missileInfo = ObjectPool.sharedInstance.GetPooledInfo();

        GameObject newMissile = missileInfo.obj;
        newMissile.transform.position = transform.position + new Vector3(fireDirection.x, fireDirection.y) * .5f;
        newMissile.SetActive(true);

        Missile missileScript = missileInfo.missile;
        StartCoroutine(RevealMissile(missileScript));

        missileScript.rb.velocity = fireDirection * caster.range;

        missileScript.missilePower = caster.power;
        missileScript.player = caster;

        caster.missileAmount -= 1;
    }
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