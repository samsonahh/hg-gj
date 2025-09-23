using UnityEngine;

public abstract class WeaponState : MonoBehaviour
{
    public abstract void Enter();
    public abstract void Exit();
    public abstract void OnShoot();
    public abstract void OnReload();
    public abstract void OnParry();
    public abstract void OnJump();
    public abstract void OnWalk();
}