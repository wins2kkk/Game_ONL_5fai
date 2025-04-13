using UnityEngine;
using Fusion;
using Fusion.Addons.SimpleKCC;
using UnityEngine.Rendering;

namespace Starter.Shooter
{
    /// <summary>
    /// Script chính điều khiển nhân vật người chơi - bao gồm di chuyển và hoạt ảnh.
    /// </summary>
    public sealed class Player : NetworkBehaviour // Kế thừa từ Fusion.NetworkBehaviour để hỗ trợ multiplayer
    {
        // Các tham chiếu đến các thành phần liên quan
        [Header("References")]
        public Health Health; // Xử lý máu của nhân vật
        public SimpleKCC KCC; // Điều khiển di chuyển nhân vật (Kinematic Character Controller của Fusion)
        public PlayerInput PlayerInput; // Dữ liệu input của người chơi
        public Animator Animator; // Bộ điều khiển hoạt ảnh
        public Transform CameraPivot; // Gốc xoay camera (ảnh hưởng tới ngực nhân vật)
        public Transform CameraHandle; // Điểm đặt camera (phục vụ raycast bắn)
        public Transform ScalingRoot; // Gốc để scale nhân vật (khi nhảy)
        public UINameplate Nameplate; // Tên người chơi hiển thị trên đầu
        public Collider Hitbox; // Va chạm khi bị bắn
        public Renderer[] HeadRenderers; // Dùng để ẩn đầu khi nhìn góc nhìn thứ nhất
        public GameObject[] FirstPersonOverlayObjects; // Các object hiển thị riêng trong overlay camera (ví dụ: vũ khí)

        // Cấu hình di chuyển
        [Header("Movement Setup")]
        public float WalkSpeed = 2f; // Tốc độ đi bộ
        public float JumpImpulse = 10f; // Lực nhảy
        public float UpGravity = 25f; // Trọng lực khi đi lên
        public float DownGravity = 40f; // Trọng lực khi rơi

        [Header("Movement Accelerations")]
        public float GroundAcceleration = 55f; // Tăng tốc khi đang đứng trên mặt đất
        public float GroundDeceleration = 25f; // Giảm tốc khi đứng trên đất
        public float AirAcceleration = 25f; // Tăng tốc khi đang bay
        public float AirDeceleration = 1.3f; // Giảm tốc khi đang bay

        // Cấu hình bắn
        [Header("Fire Setup")]
        public LayerMask HitMask; // Layer cho raycast
        public GameObject ImpactPrefab; // Hiệu ứng va chạm
        public ParticleSystem MuzzleParticle; // Hiệu ứng súng bắn

        // Cấu hình hoạt ảnh
        [Header("Animation Setup")]
        public Transform ChestTargetPosition; // Vị trí ngực mong muốn (phục vụ IK)
        public Transform ChestBone; // Bone ngực trong Rig

        // Âm thanh
        [Header("Sounds")]
        public AudioSource FireSound;
        public AudioSource FootstepSound;
        public AudioClip JumpAudioClip;

        [Header("VFX")]
        public ParticleSystem DustParticles; // Hạt bụi khi đi bộ

        // Biến mạng (Networked Variables)
        [Networked, HideInInspector, Capacity(24), OnChangedRender(nameof(OnNicknameChanged))]
        public string Nickname { get; set; } // Tên người chơi

        [Networked, HideInInspector]
        public int ChickenKills { get; set; } // Số gà tiêu diệt

        [Networked, OnChangedRender(nameof(OnJumpingChanged))]
        private NetworkBool _isJumping { get; set; } // Trạng thái đang nhảy

        [Networked] private Vector3 _hitPosition { get; set; } // Vị trí bắn trúng
        [Networked] private Vector3 _hitNormal { get; set; } // Hướng pháp tuyến tại điểm trúng
        [Networked] private int _fireCount { get; set; } // Số lần bắn

        // ID animation (được hash từ tên animation)
        private int _animIDSpeedX;
        private int _animIDSpeedZ;
        private int _animIDMoveSpeedZ;
        private int _animIDGrounded;
        private int _animIDPitch;
        private int _animIDShoot;

        private Vector3 _moveVelocity; // Vận tốc di chuyển thực tế
        private int _visibleFireCount; // Biến đếm để xử lý hiệu ứng bắn

        private GameManager _gameManager;

        public override void Spawned()
        {
            // Nếu là chủ sở hữu trạng thái (StateAuthority)
            if (HasStateAuthority)
            {
                _gameManager = FindObjectOfType<GameManager>();

                // Lấy nickname từ PlayerPrefs
                Nickname = PlayerPrefs.GetString("PlayerName");
            }

            OnNicknameChanged(); // Cập nhật hiển thị tên
            _visibleFireCount = _fireCount; // Reset bộ đếm bắn

            if (HasStateAuthority)
            {
                // Ẩn đầu để không cản tầm nhìn
                foreach (var head in HeadRenderers)
                    head.shadowCastingMode = ShadowCastingMode.ShadowsOnly;

                // Đặt layer cho vũ khí về "FirstPersonOverlay"
                int overlayLayer = LayerMask.NameToLayer("FirstPersonOverlay");
                foreach (var obj in FirstPersonOverlayObjects)
                    obj.layer = overlayLayer;

                // Tắt nội suy xoay cho người chơi local
                KCC.Settings.ForcePredictedLookRotation = true;
            }
        }

        public override void FixedUpdateNetwork()
        {
            // Rơi khỏi bản đồ
            if (KCC.Position.y < -15f)
            {
                Health.TakeHit(1000);
            }

            // Nếu chết và hết thời gian chết => hồi sinh
            if (Health.IsFinished)
            {
                Respawn(_gameManager.GetSpawnPosition());
            }

            var input = Health.IsAlive ? PlayerInput.CurrentInput : default;
            ProcessInput(input); // Xử lý input

            // Nếu chạm đất thì ngừng nhảy
            if (KCC.IsGrounded)
                _isJumping = false;

            // Kích hoạt KCC nếu còn sống
            KCC.SetActive(Health.IsAlive);
            PlayerInput.ResetInput(); // Reset input sau mỗi frame
        }

        public override void Render()
        {
            if (HasStateAuthority)
            {
                // Cập nhật hướng nhìn
                KCC.SetLookRotation(PlayerInput.CurrentInput.LookRotation, -90f, 90f);
            }

            // Cập nhật hoạt ảnh di chuyển
            var moveSpeed = transform.InverseTransformVector(KCC.RealVelocity);
            Animator.SetFloat(_animIDSpeedX, moveSpeed.x, 0.1f, Time.deltaTime);
            Animator.SetFloat(_animIDSpeedZ, moveSpeed.z, 0.1f, Time.deltaTime);
            Animator.SetBool(_animIDGrounded, KCC.IsGrounded);
            Animator.SetFloat(_animIDPitch, KCC.GetLookRotation(true, false).x, 0.02f, Time.deltaTime);

            // Bật tiếng bước chân nếu đang chạy dưới đất
            FootstepSound.enabled = KCC.IsGrounded && KCC.RealSpeed > 1f;

            // Hiệu ứng co giãn khi nhảy hoặc chạy
            ScalingRoot.localScale = Vector3.Lerp(ScalingRoot.localScale, Vector3.one, Time.deltaTime * 8f);

            // Bật bụi nếu đang đi bộ trên đất
            var emission = DustParticles.emission;
            emission.enabled = KCC.IsGrounded && KCC.RealSpeed > 1f;

            // Hiệu ứng khi bắn
            ShowFireEffects();

            // Tắt hitbox nếu đã chết
            Hitbox.enabled = Health.IsAlive;
        }

        private void Awake()
        {
            AssignAnimationIDs(); // Lấy ID animation từ tên
        }

        private void LateUpdate()
        {
            if (!Health.IsAlive)
                return;

            // Cập nhật xoay camera (ảnh hưởng tới chest IK)
            var pitchRotation = KCC.GetLookRotation(true, false);
            CameraPivot.localRotation = Quaternion.Euler(pitchRotation);

            // Blend vị trí chest IK
            float blendAmount = HasStateAuthority ? 0.05f : 0.2f;
            ChestBone.position = Vector3.Lerp(ChestTargetPosition.position, ChestBone.position, blendAmount);
            ChestBone.rotation = Quaternion.Lerp(ChestTargetPosition.rotation, ChestBone.rotation, blendAmount);

            // Nếu là người chơi local thì cập nhật camera
            if (HasStateAuthority)
            {
                Camera.main.transform.SetPositionAndRotation(CameraHandle.position, CameraHandle.rotation);
            }
        }

        private void ProcessInput(GameplayInput input)
        {
            KCC.SetLookRotation(input.LookRotation, -90f, 90f);
            KCC.SetGravity(KCC.RealVelocity.y >= 0f ? UpGravity : DownGravity);

            var moveDirection = KCC.TransformRotation * new Vector3(input.MoveDirection.x, 0f, input.MoveDirection.y);
            var desiredMoveVelocity = moveDirection * WalkSpeed;

            float acceleration = desiredMoveVelocity == Vector3.zero
                ? (KCC.IsGrounded ? GroundDeceleration : AirDeceleration)
                : (KCC.IsGrounded ? GroundAcceleration : AirAcceleration);

            _moveVelocity = Vector3.Lerp(_moveVelocity, desiredMoveVelocity, acceleration * Runner.DeltaTime);
            float jumpImpulse = 0f;

            if (KCC.IsGrounded && input.Jump)
            {
                jumpImpulse = JumpImpulse;
                _isJumping = true;
            }

            KCC.Move(_moveVelocity, jumpImpulse);

            // Cập nhật Camera Pivot
            var pitchRotation = KCC.GetLookRotation(true, false);
            CameraPivot.localRotation = Quaternion.Euler(pitchRotation);

            // Xử lý bắn
            if (input.Fire)
            {
                Fire();
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("HealthPack"))
            {
                Health.Heal(1); // Hồi máu
                Destroy(other.gameObject); // Xóa pack
            }
        }

        private void Fire()
        {
            _hitPosition = Vector3.zero;

            if (Physics.Raycast(CameraHandle.position, CameraHandle.forward, out var hitInfo, 200f, HitMask))
            {
                var health = hitInfo.collider != null ? hitInfo.collider.GetComponentInParent<Health>() : null;

                if (health != null)
                {
                    health.Killed = OnEnemyKilled;
                    health.TakeHit(1, true);
                }

                _hitPosition = hitInfo.point;
                _hitNormal = hitInfo.normal;
            }

            _fireCount++;
        }

        private void Respawn(Vector3 position)
        {
            ChickenKills = 0;
            Health.Revive();
            KCC.SetPosition(position);
            KCC.SetLookRotation(0f, 0f);
            _moveVelocity = Vector3.zero;
        }

        private void OnEnemyKilled(Health enemyHealth)
        {
            ChickenKills += enemyHealth.GetComponent<Chicken>() != null ? 1 : -10;
        }

        private void ShowFireEffects()
        {
            if (_visibleFireCount < _fireCount)
            {
                FireSound.PlayOneShot(FireSound.clip);
                MuzzleParticle.Play();
                Animator.SetTrigger(_animIDShoot);

                if (_hitPosition != Vector3.zero)
                {
                    Instantiate(ImpactPrefab, _hitPosition, Quaternion.LookRotation(_hitNormal));
                }
            }

            _visibleFireCount = _fireCount;
        }

        private void AssignAnimationIDs()
        {
            _animIDSpeedX = Animator.StringToHash("SpeedX");
            _animIDSpeedZ = Animator.StringToHash("SpeedZ");
            _animIDGrounded = Animator.StringToHash("Grounded");
            _animIDPitch = Animator.StringToHash("Pitch");
            _animIDShoot = Animator.StringToHash("Shoot");
        }

        private void OnJumpingChanged()
        {
            if (HasStateAuthority == false)
            {
                ScalingRoot.localScale = _isJumping ? new Vector3(0.5f, 1.5f, 0.5f) : new Vector3(1.25f, 0.75f, 1.25f);
            }
        }

        private void OnNicknameChanged()
        {
            if (HasStateAuthority)
                return;

            Nameplate.SetNickname(Nickname);
        }
    }
}
