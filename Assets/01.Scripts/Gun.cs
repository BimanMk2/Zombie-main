using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

// 총을 구현
public class Gun : MonoBehaviour {
    // 총의 상태를 표현하는 데 사용할 타입을 선언
    public enum State {
        Ready, // 발사 준비됨
        Empty, // 탄알집이 빔
        Reloading // 재장전 중
    }

    public State state { get; private set; } // 현재 총의 상태

    public Transform fireTransform; // 탄알이 발사될 위치

    public ParticleSystem muzzleFlashEffect; // 총구 화염 효과
    public ParticleSystem shellEjectEffect; // 탄피 배출 효과

    private LineRenderer bulletLineRenderer; // 탄알 궤적을 그리기 위한 렌더러

    private AudioSource gunAudioPlayer; // 총 소리 재생기

    public GunData gunData; // 총의 현재 데이터

    private float fireDistance = 50f; // 사정거리

    public int ammoRemain = 100; // 남은 전체 탄알
    public int magAmmo; // 현재 탄알집에 남아 있는 탄알

    private float lastFireTime; // 총을 마지막으로 발사한 시점

    private void Awake() {
        gunAudioPlayer = GetComponent<AudioSource>();
        bulletLineRenderer = GetComponent<LineRenderer>();
        // 사용할 컴포넌트의 참조 가져오기
        bulletLineRenderer.positionCount = 2;
        //사용할 점을 두 개로 변경
        bulletLineRenderer.enabled = false;
        //라인랜더러를 비활성화
    }

    private void OnEnable() {
        ammoRemain = gunData.startAmmoRemain;
        //전체 예비탄환 초기화
        magAmmo = gunData.magCapacity;
        // 현 상태 탄환 초기화
        state = State.Ready;
        //총의 현재 상태를 발사 준비로 초기화
        lastFireTime = 0;
        //마지막으로 총을 쏜 시점을 초기화
        // 총 상태 초기화


    }

    // 발사 시도
    public void Fire() {
        //현재 상태가 발사 가능한 상태
        //&& 마지막 총 발사 시점에서 gunDate.timeBetFire 이상의 시간이 지남
        if (state == State.Ready && Time.time >= lastFireTime + gunData.timeBetFire)
        {

            //마지막 총 발사 시점 갱신
            lastFireTime = Time.time;
            Shot();
        }


    }

    // 실제 발사 처리
    private void Shot() 
    {

        //레이캐스에 의한 충돌 정보를 저장하는 컨테이너
        RaycastHit hit;
        //탄알이 맞은 곳을 저장할 변수
        Vector3 hitPositon = Vector3.zero;

        //레이캐스트 시작지점, 방향, 충돌 정보 컨테이너 ,사정거리

        if (Physics. Raycast(fireTransform.position, fireTransform.forward, out hit,
            fireDistance))
            {
            //레이가 어떤 물체와 충돌한 경우

            //충돌한 상대방으로부터 IDamgeable 오브젝트 가져오기 시도

            IDamageable target = hit.collider.GetComponent<IDamageable>();

            // 상대방으로부터 IDamageable 오브젝트를 가져오는데 성공한다면

            if (target != null)
            {// 상대방의 OnDamgeable을 실행 데미지 추가 발동
                target.OnDamage(gunData.damage, hit.point, hit.normal);

                hitPositon = hit.point;
            }

            else
            {
                //레이가 다른 물체와 충돌 안했다면
                //탄알이 최대 거리까지 날아갔을때의 위치를 충돌 위치로 사용

                hitPositon = fireTransform.position + fireTransform.forward * fireDistance;
            }
            //발사 이펙트 재생시작
            StartCoroutine(ShotEffect(hitPositon));

            //남은 탄알수 -1
            magAmmo--;
            if (magAmmo <= 0)
            {
                //탄창에 남은 탄알이 없다면 총의 현재 상태를 Empty로 갱신

                state = State.Empty;
            } 
        } 
    }

        

    // 발사 이펙트와 소리를 재생하고 탄알 궤적을 그림
    private IEnumerator ShotEffect(Vector3 hitPosition) {
        //총구 화염 효과 재상
        muzzleFlashEffect.Play();
        //탄피 배출 효과 재생
        shellEjectEffect.Play();

        //총격 소리 재생
        gunAudioPlayer.PlayOneShot(gunData.shotClip);

        //선 시작점 총구 위치
        bulletLineRenderer.SetPosition(0, fireTransform.position);

        //선의 끝점 입력으로 들어오는 충돌 위치
        bulletLineRenderer.SetPosition(1, hitPosition);

        // 라인 렌더러를 활성화하여 탄알 궤적을 그림
        bulletLineRenderer.enabled = true;

        // 0.03초 동안 잠시 처리를 대기
        yield return new WaitForSeconds(0.03f);

        // 라인 렌더러를 비활성화하여 탄알 궤적을 지움
        bulletLineRenderer.enabled = false;
    }

    // 재장전 시도
    public bool Reload() {
        if (state == State.Reloading || ammoRemain <= 0 || magAmmo >= gunData.magCapacity)

        {//이미 재장전중이나 남은 탄알이 없거나
            //탄창에 탄알이 이미 가득찬 경우 재장전 불가
            return false;
        }
        //재장전 처리
        StartCoroutine(ReloadRoutine());
        return true;
    }

    // 실제 재장전 처리를 진행
    private IEnumerator ReloadRoutine() {
        // 현재 상태를 재장전 중 상태로 전환
        state = State.Reloading;
        // 재장전 소리 재생
        gunAudioPlayer.PlayOneShot(gunData.reloadClip);
      
       
        yield return new WaitForSeconds(gunData.reloadTime);

        //탄창에 채울 탄알을 계산
        int ammoTofill = gunData.magCapacity - magAmmo;

        //탄창에 채워야 할 탄알이 남은 탄알보다 많다면
        //채워야 할 탄알 수를 남은 탄알 수에 맞춰 줄임
        if (ammoRemain < ammoTofill)
        { 
        ammoTofill = ammoRemain;
        }


        //탄창을 채움

        magAmmo += ammoTofill;

        //남은 탄알에서 탄창에 채운만큼 탄알을 뺌
        ammoRemain -= ammoTofill;
        // 재장전 소요 시간 만큼 처리 쉬기

        // 총의 현재 상태를 발사 준비된 상태로 변경
        state = State.Ready;
    }
}