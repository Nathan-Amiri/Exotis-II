using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet.Object;
using FishNet.Object.Synchronizing;

public class Player : NetworkBehaviour
{
    //layers: -2 = background, -1 = editorgrid, 0 = terrain, 1 = players/some HUD, 2 = missiles/spells/more HUD

    private int playerNumber;

    private float maxHealth;
    private float power;
    //private float speed;
    private float range;

    [SyncVar]
    private float health;
    private float maxHealthBarWidth;

    private float missileFillSpeed;
    private float missileAmount;
    private float maxMissileBarWidth;

    private bool startUpdate;

    private Vector2 mousePosition;

    public GameObject missile; //set in inspector

    private GameObject playerHud;
    private GameObject healthBar; //is actually the health bar's pivot point
    private GameObject missileBar; //is actually the missile bar's pivot point

    public override void OnStartClient()
    {
        base.OnStartClient();

        OnSpawn();
    }

    private void OnSpawn()
    {
        //these values need to be set by teambuilder
        playerNumber = IsHost ? (IsOwner ? 1 : 2) : (IsOwner ? 2 : 1);
        maxHealth = 15;
        power = 3;
        //speed = 1;
        range = 10;

        if (IsOwner)
            GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Dynamic;

        playerHud = GameObject.Find("HUD").transform.GetChild(playerNumber - 1).gameObject;
        healthBar = playerHud.transform.GetChild(3).gameObject;
        missileBar = playerHud.transform.GetChild(4).GetChild(1).gameObject;
        
        health = maxHealth;
        maxHealthBarWidth = healthBar.transform.localScale.x;

        missileFillSpeed = 1;
        missileAmount = 3;
        maxMissileBarWidth = missileBar.transform.localScale.x;

        if (playerNumber == 1)
        {
            GameObject.Find("EditorGrid").SetActive(false);
            GameObject.Find("NetworkHudCanvas").SetActive(false);
        }

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

        if (Input.GetButtonDown("Missile") && missileAmount >= 1)
        {
            Vector2 fireDirection = (mousePosition - new Vector2(transform.position.x, transform.position.y)).normalized;
            RpcServerCreateMissile(this, fireDirection);
        }
    }

    private void HealthBar() //run in update
    {
        float proportion = maxHealth / health; //maxHealth / health should equal the same proportion as maxHealthBarWidth / healthBar's scale.x

        if (healthBar.transform.localScale.x > maxHealthBarWidth / proportion)
            healthBar.transform.localScale -= new Vector3(Time.deltaTime, 0);
        else if (healthBar.transform.localScale.x < maxHealthBarWidth / proportion)
            healthBar.transform.localScale += new Vector3(Time.deltaTime, 0);

        if (health > maxHealth)
            health = maxHealth;
    }

    public void HealthChange(float amount)
    {
        health += amount;

        if (health <= 0)
            Eliminate();
    }

    private void Eliminate()
    {
        Debug.Log(name + " has been eliminated");
        //move player away but don't despawn player. Makes rematching easier and ensures that health bar fades down after elimination
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