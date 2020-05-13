using System.Collections;
using System.Collections.Generic;
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

    private SpitState _spitState;
    // Start is called before the first frame update
    void Start()
    {
        _spitState = SpitState.Normal;
        _timer = 0;
        _canShoot = true;
    }

    // Update is called once per frame
    void Update()
    {
        _timer -= Time.deltaTime;
        if (_timer <= 0)
        {
            _canShoot = true;
            switch (_spitState)
            {
                case SpitState.Normal:
                    _timer = NormalTime;
                    break;
                case SpitState.Auto:
                    _timer = AutoTime;
                    break;
                case SpitState.Spread:
                    _timer = SpreadTime;
                    break;
                case SpitState.Sniper:
                    _timer = SniperTime;
                    break;
                default:
                    break;
            }
        }
    }

    public void Spit()
    {
        if (_canShoot)
        {
            _canShoot = false;
            _ammo--;
            switch (_spitState)
            {
                case SpitState.Normal:
                    Instantiate(NormalSpit, transform.position, transform.rotation);
                    _timer = NormalTime;
                    break;
                case SpitState.Auto:
                    Instantiate(AutoSpit, transform.position, transform.rotation);
                    _timer = AutoTime;
                    if (_ammo <= 0)
                    {
                        ChangeState(SpitState.Normal);
                    }
                    break;
                case SpitState.Spread:
                    Instantiate(SpreadSpit, transform.position, transform.rotation);
                    _timer = SpreadTime;
                    if (_ammo <= 0)
                    {
                        ChangeState(SpitState.Normal);
                    }
                    break;
                case SpitState.Sniper:
                    Instantiate(SniperSpit, transform.position, transform.rotation);
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
