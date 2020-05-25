using System.Collections;
using System.Collections.Generic;
using AlmenaraGames;
using UnityEngine;

public class SpitterScript : MonoBehaviour
{
    public enum SpitState
    {
        Normal,
        Auto,
        Spread,
        Sniper
    }

    private float _timer;
    private bool _canShoot;

    [Header("Timers")]
    public float NormalTime;
    public float AutoTime;
    public float SpreadTime;
    public float SniperTime;

    [Header("Ammo")] 
    public int AutoAmmo;
    public int SpreadAmmo;
    public int SniperAmmo;
    private int _ammo;

    [Header("Spit Prefabs")] 
    public GameObject NormalSpit;
    public GameObject AutoSpit;
    public GameObject SpreadSpit;
    public GameObject SniperSpit;
    public AudioObject SpitSound;

    private SpitState _spitState;

    private int _playerID;

    private int _teamID;
    // Start is called before the first frame update
    void Start()
    {
        _spitState = SpitState.Normal;
        _timer = 0;
        _canShoot = true;
        PlayerController pc = GetComponentInParent<PlayerController>();
        _playerID = pc.PlayerID;
        _teamID = pc.TeamID;
    }

    // Update is called once per frame
    void Update()
    {
        _timer -= Time.deltaTime;
        if (_timer <= 0)
        {
            _canShoot = true;
        }
    }

    public void Spit()
    {
        if (_canShoot)
        {
            _canShoot = false;
            _ammo--;
            //Play Sound
            MultiAudioManager.PlayAudioObject(SpitSound, transform.position);
            switch (_spitState)
            {
                case SpitState.Normal:
                    GameObject spit = Instantiate(NormalSpit, transform.position, transform.rotation);
                    spit.GetComponent<SpitHitBox>().PlayerID = _playerID;
                    spit.GetComponent<SpitHitBox>().TeamID = _teamID;
                    _timer = NormalTime;
                    break;
                case SpitState.Auto:
                    GameObject autoSpit = Instantiate(AutoSpit, transform.position, transform.rotation);
                    autoSpit.GetComponent<SpitHitBox>().PlayerID = _playerID;
                    autoSpit.GetComponent<SpitHitBox>().TeamID = _teamID;
                    _timer = AutoTime;
                    if (_ammo <= 0)
                    {
                        ChangeState(SpitState.Normal);
                    }
                    break;
                case SpitState.Spread:
                    GameObject spreadSpit = Instantiate(NormalSpit, transform.position, transform.rotation);
                    spreadSpit.GetComponent<SpitHitBox>().PlayerID = _playerID;
                    spreadSpit.GetComponent<SpitHitBox>().TeamID = _teamID;
                    _timer = SpreadTime;
                    if (_ammo <= 0)
                    {
                        ChangeState(SpitState.Normal);
                    }
                    break;
                case SpitState.Sniper:
                    GameObject sniperSpit = Instantiate(NormalSpit, transform.position, transform.rotation);
                    sniperSpit.GetComponent<SpitHitBox>().PlayerID = _playerID;
                    sniperSpit.GetComponent<SpitHitBox>().TeamID = _teamID;
                    _timer = SniperTime;
                    if (_ammo <= 0)
                    {
                        ChangeState(SpitState.Normal);
                    }
                    break;
                default:
                    break;
            }

        }
    }

    public void ChangeState(SpitState newState)
    {
        _spitState = newState;
        switch (_spitState)
        {
            case SpitState.Normal:
                _timer = NormalTime;
                break;
            case SpitState.Auto:
                _timer = AutoTime;
                _ammo = AutoAmmo;
                break;
            case SpitState.Spread:
                _timer = SpreadTime;
                _ammo = SpreadAmmo;
                break;
            case SpitState.Sniper:
                _timer = SniperTime;
                _ammo = SniperAmmo;
                break;
            default:
                break;
        }
    }

    public void SpitterReset()
    {
        _ammo = 0;
        ChangeState(SpitState.Normal);
    }
}
