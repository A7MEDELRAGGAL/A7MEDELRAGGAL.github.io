using UnityEngine;

namespace Clone.Fps
{
    public class FpsWeapon : MonoBehaviour
    {
        [Header("Damage")]
        public float Damage = 25f;
        public float HeadshotMultiplier = 2f;
        public float Range = 200f;

        [Header("Fire")]
        public float FireRate = 9f;
        public float MagazineSize = 30f;
        public float ReloadSeconds = 2f;

        [Header("Recoil")]
        public float RecoilPitch = 1.2f;
        public float RecoilYaw = 0.4f;

        [Header("Aiming")]
        public float AdsTime = 0.15f;
        public float HipFireSpreadDegrees = 2.5f;
        public float AdsSpreadDegrees = 0.4f;

        [Header("References")]
        public Transform AimPoint;
        public LayerMask HitMask = ~0;

        public float CurrentAmmo { get; private set; }
        public float MaxAmmo => MagazineSize;
        public bool IsReloading => _reloadTimer > 0f;
        public bool IsAds { get; private set; }

        FpsCameraRig _cameraRig;
        FpsWeaponView _weaponView;
        Camera _aimCamera;
        float _fireCooldown;
        float _reloadTimer;
        float _adsBlend;

        void Awake()
        {
            _cameraRig = GetComponentInParent<FpsCameraRig>();
            _weaponView = GetComponentInChildren<FpsWeaponView>();
            _aimCamera = Camera.main;
            CurrentAmmo = MagazineSize;
        }

        void Update()
        {
            HandleAiming();
            HandleFire();
            HandleReload();
        }

        void HandleAiming()
        {
            bool aimHeld = Input.GetMouseButton(1);
            IsAds = aimHeld && !IsReloading;
            _adsBlend = Mathf.MoveTowards(_adsBlend, IsAds ? 1f : 0f, Time.deltaTime / Mathf.Max(AdsTime, 0.01f));

            if (_weaponView != null)
            {
                _weaponView.IsAds = IsAds;
            }

            if (_cameraRig != null)
            {
                _cameraRig.IsAds = IsAds;
            }
        }

        void HandleFire()
        {
            _fireCooldown -= Time.deltaTime;

            bool fireHeld = Input.GetMouseButton(0);
            if (!fireHeld || IsReloading || _fireCooldown > 0f)
            {
                return;
            }

            if (CurrentAmmo <= 0f)
            {
                StartReload();
                return;
            }

            _fireCooldown = 1f / Mathf.Max(FireRate, 0.01f);
            CurrentAmmo = Mathf.Max(CurrentAmmo - 1f, 0f);

            FireShot();

            if (CurrentAmmo <= 0f)
            {
                StartReload();
            }
        }

        void FireShot()
        {
            if (_aimCamera == null)
            {
                return;
            }

            Vector3 origin = AimPoint != null ? AimPoint.position : _aimCamera.transform.position;
            Vector3 direction = _aimCamera.transform.forward;

            float spread = Mathf.Lerp(HipFireSpreadDegrees, AdsSpreadDegrees, _adsBlend);
            if (spread > 0f)
            {
                direction = ApplySpread(direction, spread);
            }

            if (Physics.Raycast(origin, direction, out RaycastHit hit, Range, HitMask, QueryTriggerInteraction.Ignore))
            {
                FpsTargetDummy dummy = hit.collider.GetComponentInParent<FpsTargetDummy>();
                if (dummy != null)
                {
                    bool headshot = hit.collider.CompareTag("Head");
                    float damage = headshot ? Damage * HeadshotMultiplier : Damage;
                    dummy.TakeDamage(damage, hit.point, headshot);
                }
            }

            ApplyRecoil();
        }

        Vector3 ApplySpread(Vector3 direction, float spreadDegrees)
        {
            float half = spreadDegrees * 0.5f;
            float yaw = Random.Range(-half, half);
            float pitch = Random.Range(-half, half);
            Quaternion offset = Quaternion.Euler(pitch, yaw, 0f);
            return offset * direction;
        }

        void ApplyRecoil()
        {
            if (_cameraRig == null)
            {
                return;
            }

            float yawKick = Random.Range(-RecoilYaw, RecoilYaw);
            _cameraRig.AddRecoil(RecoilPitch);
            _cameraRig.AddYawRecoil(yawKick);
        }

        void HandleReload()
        {
            if (Input.GetKeyDown(KeyCode.R) && !IsReloading && CurrentAmmo < MagazineSize)
            {
                StartReload();
            }

            if (_reloadTimer > 0f)
            {
                _reloadTimer -= Time.deltaTime;
                if (_reloadTimer <= 0f)
                {
                    CurrentAmmo = MagazineSize;
                }
            }
        }

        void StartReload()
        {
            if (IsReloading || CurrentAmmo >= MagazineSize)
            {
                return;
            }

            _reloadTimer = Mathf.Max(ReloadSeconds, 0.01f);
        }
    }
}
