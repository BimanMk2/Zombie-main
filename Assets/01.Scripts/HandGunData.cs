using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable/GunDate", fileName ="Gun Date")]

public class HandGunData : ScriptableObject
{
    public AudioClip shotClip; // 발사 소리
    public AudioClip reloadClip; // 재장전 소리

    public float damage = 100; // 공격력

    public int startAmmoRemain = 100; // 처음에 주어질 전체 탄약
    public int magCapacity = 5; // 탄창 용량

    public float timeBetFire = 0.5f; // 총알 발사 간격
    public float reloadTime = 1.8f; // 재장전 소요 시간
}