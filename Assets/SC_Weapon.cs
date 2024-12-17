using System.Collections;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(LineRenderer))]  // Добавляем LineRenderer для визуализации луча

public class SC_Weapon : MonoBehaviour
{
    public bool singleFire = false;
    public float fireRate = 0.1f;
    public GameObject bulletPrefab;
    public Transform firePoint;
    public int bulletsPerMagazine = 30;
    public float timeToReload = 1.5f;
    public float weaponDamage = 15; //How much damage should this weapon deal
    public AudioClip fireAudio;
    public AudioClip reloadAudio;

    [HideInInspector]
    public SC_WeaponManager manager;

    // Добавляем переменную для LineRenderer
    private LineRenderer lineRenderer;

    float nextFireTime = 0;
    bool canFire = true;
    int bulletsPerMagazineDefault = 0;
    AudioSource audioSource;

    // Start is called before the first frame update
    void Start()
    {
        bulletsPerMagazineDefault = bulletsPerMagazine;
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
        //Make sound 3D
        audioSource.spatialBlend = 1f;

        // Инициализируем LineRenderer
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = 2;  // Два конца линии (точка старта и точка попадания)
        lineRenderer.enabled = false;  // Сначала отключаем визуализацию луча
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0) && singleFire)
        {
            Fire();
        }
        if (Input.GetMouseButton(0) && !singleFire)
        {
            Fire();
        }
        if (Input.GetKeyDown(KeyCode.R) && canFire)
        {
            StartCoroutine(Reload());
        }
    }

    void Fire()
    {
        if (canFire)
        {
            if (Time.time > nextFireTime)
            {
                nextFireTime = Time.time + fireRate;

                if (bulletsPerMagazine > 0)
                {
                    //Point fire point at the current center of Camera
                    Vector3 firePointPointerPosition = manager.playerCamera.transform.position + manager.playerCamera.transform.forward * 100;
                    RaycastHit hit;
                    
                    // Запускаем Raycast
                    if (Physics.Raycast(manager.playerCamera.transform.position, manager.playerCamera.transform.forward, out hit, 100))
                    {
                        firePointPointerPosition = hit.point;

                        // Визуализируем луч через LineRenderer
                        DrawRay(manager.playerCamera.transform.position, hit.point);
                    }
                    else
                    {
                        // Если ничего не попало, рисуем луч на максимальную дистанцию
                        DrawRay(manager.playerCamera.transform.position, firePointPointerPosition);
                    }

                    // Создаем пулю
                    GameObject bulletObject = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
                    SC_Bullet bullet = bulletObject.GetComponent<SC_Bullet>();
                    // Устанавливаем урон пули
                    bullet.SetDamage(weaponDamage);

                    bulletsPerMagazine--;
                    audioSource.clip = fireAudio;
                    audioSource.Play();
                }
                else
                {
                    StartCoroutine(Reload());
                }
            }
        }
    }

    // Метод для рисования луча с использованием LineRenderer
    private void DrawRay(Vector3 startPosition, Vector3 endPosition)
    {
        lineRenderer.enabled = true; // Включаем LineRenderer для рисования луча
        lineRenderer.SetPosition(0, startPosition);  // Начало луча - позиция игрока
        lineRenderer.SetPosition(1, endPosition);    // Конец луча - точка попадания

        // Изменяем цвет луча (можно сделать его цветным, например, красным)
        lineRenderer.startColor = Color.red;  // Начальный цвет
        lineRenderer.endColor = Color.red;    // Конечный цвет

        // Можно добавить плавное исчезновение луча
        StartCoroutine(HideRayAfterTime(0.1f)); // Скрываем луч через 0.1 секунды
    }

    // Корутина для скрытия луча через время
    IEnumerator HideRayAfterTime(float time)
    {
        yield return new WaitForSeconds(time);
        lineRenderer.enabled = false; // Отключаем LineRenderer после таймаута
    }

    IEnumerator Reload()
    {
        canFire = false;

        audioSource.clip = reloadAudio;
        audioSource.Play();

        yield return new WaitForSeconds(timeToReload);

        bulletsPerMagazine = bulletsPerMagazineDefault;

        canFire = true;
    }

    // Called from SC_WeaponManager
    public void ActivateWeapon(bool activate)
    {
        StopAllCoroutines();
        canFire = true;
        gameObject.SetActive(activate);
    }
}
