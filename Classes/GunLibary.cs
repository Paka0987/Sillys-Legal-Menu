using BepInEx;
using GorillaLocomotion;
using Oculus.Platform;
using System;
using System.Collections;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR;
using Random = UnityEngine.Random;

namespace Juul
{
    public class GunLib : MonoBehaviour
    {
        #region Networking Properties
        public static bool IsGunNetworkingEnabled = true;
        private static float gunSyncTimer = 0f;
        private static Dictionary<Player, NetworkedGunData> networkGuns = new Dictionary<Player, NetworkedGunData>();

        public class NetworkedGunData
        {
            public GameObject gunLineObject;
            public GameObject spherePointer;
            public VRRig lockedPlayer;
            public Vector3 lr;
            public bool isLocked;
            public float lastSyncTime;
            public GunLineStyle lineStyle;
            public float lineWidth;
            public float sphereSize;
        }
        #endregion

        private void FixedUpdate()
        {
            LineColor = Core.BaseColor;
            PointerColor = Core.BaseColor;
        }

        public static void ChangeGunStyle(bool forward = true)
        {
            if (forward)
                currentLineStyle++;
            else
                currentLineStyle--;

            if (currentLineStyle > GunLineStyle.Tentacle)
                currentLineStyle = GunLineStyle.Normal;
            if (currentLineStyle < GunLineStyle.Normal)
                currentLineStyle = GunLineStyle.Tentacle;

            if (ExtraButtons.GunStyleButton != null)
                ExtraButtons.GunStyleButton.Name = "Gun Style: " + currentLineStyle.ToString();

            if (IsGunNetworkingEnabled && PhotonNetwork.IsConnected)
            {
                SyncGunProperties();
            }
        }

        public static void ChangeGunLineSize(bool forward = true)
        {
            if (forward)
                currentLineSize++;
            else
                currentLineSize--;

            if (currentLineSize >= LineScales.Length)
                currentLineSize = 0;
            if (currentLineSize < 0)
                currentLineSize = LineScales.Length - 1;

            GunLineWidth = LineScales[currentLineSize];

            if (ExtraButtons.GunLineSizeButton != null)
                ExtraButtons.GunLineSizeButton.Name = string.Format("Gun Line Size: {0}", GunLineWidth);

            if (IsGunNetworkingEnabled && PhotonNetwork.IsConnected)
            {
                SyncGunProperties();
            }
        }

        public static void ChangeGunSphereScale(bool forward = true)
        {
            if (forward)
                currentSphereSize++;
            else
                currentSphereSize--;

            if (currentSphereSize >= SphereScales.Length)
                currentSphereSize = 0;
            if (currentSphereSize < 0)
                currentSphereSize = SphereScales.Length - 1;

            SphereSize = SphereScales[currentSphereSize];

            if (spherepointer != null)
                spherepointer.transform.localScale = Vector3.one * SphereSize;

            if (ExtraButtons.GunSphereSizeButton != null)
                ExtraButtons.GunSphereSizeButton.Name = string.Format("Gun Sphere Size: {0}", SphereSize);

            if (IsGunNetworkingEnabled && PhotonNetwork.IsConnected)
            {
                SyncGunProperties();
            }
        }

        #region Networking M
        public static void SyncGunProperties()
        {
            if (!IsGunNetworkingEnabled || !PhotonNetwork.IsConnected || PhotonNetwork.LocalPlayer == null) return;

            ExitGames.Client.Photon.Hashtable props = new ExitGames.Client.Photon.Hashtable();
            props["Gun_Style"] = (int)currentLineStyle;
            props["Gun_LineWidth"] = GunLineWidth;
            props["Gun_SphereSize"] = SphereSize;
            props["Gun_CurrentLineSize"] = currentLineSize;
            props["Gun_CurrentSphereSize"] = currentSphereSize;

            PhotonNetwork.LocalPlayer.SetCustomProperties(props);
        }

        public static bool HasGunProperty(Player player)
        {
            return player != null && player.CustomProperties.ContainsKey("Gun_Style");
        }

        public static void UpdateNetworkGuns()
        {
            if (!IsGunNetworkingEnabled) return;

            if (VRRigCache.ActiveRigs == null) return;

            List<Player> currentPlayers = new List<Player>();

            foreach (VRRig rig in VRRigCache.ActiveRigs)
            {
                if (rig == null || rig.isOfflineVRRig || rig.isMyPlayer) continue;
                if (rig.netView == null) continue;

                Player player = null;
                try
                {
                    PhotonView photonView = Rigs.GetPhotonViewFromVRRig(rig);
                    if (photonView != null && photonView.Owner != null)
                        player = photonView.Owner;
                }
                catch { }

                if (player == null) continue;
                currentPlayers.Add(player);

                if (HasGunProperty(player))
                {
                    UpdateOrCreateNetworkGun(player, rig);
                }
                else
                {
                    RemoveNetworkGun(player);
                }
            }

            List<Player> toRemove = new List<Player>();
            foreach (var kvp in networkGuns)
            {
                if (!currentPlayers.Contains(kvp.Key))
                    toRemove.Add(kvp.Key);
            }
            foreach (Player p in toRemove)
            {
                RemoveNetworkGun(p);
            }
        }

        private static void UpdateOrCreateNetworkGun(Player player, VRRig rig)
        {
            if (networkGuns.ContainsKey(player) && networkGuns[player] != null)
            {
                UpdateExistingNetworkGun(player, rig);
            }
            else
            {
                CreateNetworkGun(player, rig);
            }
        }

        private static void CreateNetworkGun(Player player, VRRig rig)
        {
            NetworkedGunData gunData = new NetworkedGunData();

            if (player.CustomProperties.ContainsKey("Gun_Style"))
                gunData.lineStyle = (GunLineStyle)(int)player.CustomProperties["Gun_Style"];
            if (player.CustomProperties.ContainsKey("Gun_LineWidth"))
                gunData.lineWidth = (float)player.CustomProperties["Gun_LineWidth"];
            if (player.CustomProperties.ContainsKey("Gun_SphereSize"))
                gunData.sphereSize = (float)player.CustomProperties["Gun_SphereSize"];

            gunData.gunLineObject = new GameObject($"NetworkGun_{player.NickName}");
            gunData.gunLineObject.transform.SetParent(rig.transform);

            LineRenderer lineRenderer = gunData.gunLineObject.AddComponent<LineRenderer>();
            lineRenderer.useWorldSpace = true;
            lineRenderer.startWidth = gunData.lineWidth;
            lineRenderer.endWidth = gunData.lineWidth;

            Material lineMaterial = new Material(Core.UberShader);
            lineRenderer.material = lineMaterial;

            gunData.spherePointer = LineLib.CreateSphere(Vector3.zero, gunData.sphereSize, Core.BaseColor, true);
            gunData.spherePointer.transform.SetParent(rig.transform);

            networkGuns[player] = gunData;
        }

        private static void UpdateExistingNetworkGun(Player player, VRRig rig)
        {
            NetworkedGunData gunData = networkGuns[player];

            if (player.CustomProperties.ContainsKey("Gun_Style"))
            {
                GunLineStyle newStyle = (GunLineStyle)(int)player.CustomProperties["Gun_Style"];
                if (gunData.lineStyle != newStyle)
                {
                    gunData.lineStyle = newStyle;
                }
            }

            if (player.CustomProperties.ContainsKey("Gun_LineWidth"))
            {
                float newWidth = (float)player.CustomProperties["Gun_LineWidth"];
                if (Math.Abs(gunData.lineWidth - newWidth) > 0.001f)
                {
                    gunData.lineWidth = newWidth;
                    LineRenderer lr = gunData.gunLineObject.GetComponent<LineRenderer>();
                    if (lr != null)
                    {
                        lr.startWidth = newWidth;
                        lr.endWidth = newWidth;
                    }
                }
            }

            if (player.CustomProperties.ContainsKey("Gun_SphereSize"))
            {
                float newSize = (float)player.CustomProperties["Gun_SphereSize"];
                if (Math.Abs(gunData.sphereSize - newSize) > 0.001f)
                {
                    gunData.sphereSize = newSize;
                    if (gunData.spherePointer != null)
                        gunData.spherePointer.transform.localScale = Vector3.one * newSize;
                }
            }

            Transform handTransform = GetHandTransform(rig);
            if (handTransform != null && gunData.spherePointer != null)
            {
                Vector3 handPos = handTransform.position;
                Vector3 forward = handTransform.forward;

                RaycastHit hit;
                if (Physics.Raycast(handPos, forward, out hit, 512f, NoInvisLayerMask()))
                {
                    gunData.spherePointer.transform.position = hit.point;
                    gunData.spherePointer.SetActive(true);
                    UpdateNetworkGunLine(gunData, handPos, hit.point);
                }
                else
                {
                    gunData.spherePointer.SetActive(false);
                    if (gunData.gunLineObject != null)
                        gunData.gunLineObject.SetActive(false);
                }
            }
        }

        private static void UpdateNetworkGunLine(NetworkedGunData gunData, Vector3 handPos, Vector3 targetPos)
        {
            if (gunData.gunLineObject == null) return;

            LineRenderer lr = gunData.gunLineObject.GetComponent<LineRenderer>();
            if (lr == null) return;

            gunData.gunLineObject.SetActive(true);

            Vector3 midPoint = (handPos + targetPos) / 2f;
            gunData.lr = Vector3.Lerp(gunData.lr, midPoint, Time.deltaTime * 6f);

            SetNetworkLineStyle(lr, handPos, targetPos, gunData.lr, gunData.lineStyle);

            lr.startColor = LineColor;
            lr.endColor = LineColor;

            Material material = lr.material;
            if (material != null)
                material.color = LineColor;
        }

        private static void SetNetworkLineStyle(LineRenderer lr, Vector3 start, Vector3 end, Vector3 smoothMid, GunLineStyle style)
        {
            switch (style)
            {
                case GunLineStyle.Normal:
                    CurveLineRendererNetwork(lr, start, smoothMid, end);
                    break;
                case GunLineStyle.Straight:
                    lr.positionCount = 2;
                    lr.SetPosition(0, start);
                    lr.SetPosition(1, end);
                    break;
                case GunLineStyle.Morphine:
                    DrawMorphineLineNetwork(lr, start, end, 150);
                    break;
                case GunLineStyle.Flare:
                    DrawFlareLineNetwork(lr, start, end, 150);
                    break;
                case GunLineStyle.Tentacle:
                    DrawTentacleLineNetwork(lr, start, end, 150);
                    break;
            }
        }

        private static void RemoveNetworkGun(Player player)
        {
            if (networkGuns.ContainsKey(player))
            {
                NetworkedGunData gunData = networkGuns[player];
                if (gunData.gunLineObject != null)
                    GameObject.Destroy(gunData.gunLineObject);
                if (gunData.spherePointer != null)
                    LineLib.DestroySphere(gunData.spherePointer);
                networkGuns.Remove(player);
            }
        }

        private static Transform GetHandTransform(VRRig rig)
        {
            bool isRightHanded = true;
            if (PhotonNetwork.LocalPlayer != null && PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey("Juul_H"))
                isRightHanded = (bool)PhotonNetwork.LocalPlayer.CustomProperties["Juul_H"];

            return isRightHanded ? rig.rightHandTransform : rig.leftHandTransform;
        }

        private static void CurveLineRendererNetwork(LineRenderer lr, Vector3 start, Vector3 mid, Vector3 end)
        {
            lr.positionCount = LineCurve;
            for (int i = 0; i < LineCurve; i++)
            {
                float t = (float)i / (LineCurve - 1);
                lr.SetPosition(i, CalculateBezierPoint(start, mid, end, t));
            }
        }

        private static void DrawMorphineLineNetwork(LineRenderer lr, Vector3 start, Vector3 end, int segments)
        {
            lr.positionCount = segments;
            for (int i = 0; i < segments; i++)
            {
                float t = (float)i / (segments - 1);
                Vector3 pos = Vector3.Lerp(start, end, t);
                float waveX = Mathf.Sin(Time.time * 3f + i * 0.2f) * 0.02f;
                float waveY = Mathf.Cos(Time.time * 2.5f + i * 0.15f) * 0.02f;
                float waveZ = Mathf.Sin(Time.time * 3.5f + i * 0.25f) * 0.02f;
                pos += new Vector3(waveX, waveY, waveZ);
                lr.SetPosition(i, pos);
            }
        }

        private static void DrawFlareLineNetwork(LineRenderer lr, Vector3 start, Vector3 end, int points)
        {
            lr.positionCount = points;
            for (int i = 0; i < points; i++)
            {
                float t = (float)i / (points - 1);
                Vector3 pos = Vector3.Lerp(start, end, t);
                lr.SetPosition(i, pos);
            }
        }

        private static void DrawTentacleLineNetwork(LineRenderer lr, Vector3 start, Vector3 end, int points)
        {
            lr.positionCount = points;
            Vector3 direction = (end - start).normalized;
            Vector3 up = Vector3.up;
            Vector3 right = Vector3.Cross(direction, up).normalized;
            Vector3 forward = Vector3.Cross(right, up).normalized;

            float tentacleLength = Vector3.Distance(start, end);

            for (int i = 0; i < points; i++)
            {
                float t = (float)i / (points - 1);
                Vector3 basePos = Vector3.Lerp(start, end, t);

                float sinWave = Mathf.Sin(t * Mathf.PI * 3f + Time.time * 3f);
                float cosWave = Mathf.Cos(t * Mathf.PI * 2.5f + Time.time * 2.5f);
                float secondaryWave = Mathf.Sin(t * Mathf.PI * 5f + Time.time * 4f) * 0.5f;

                Vector3 tentacleOffset = up * (sinWave * 0.08f * (1 - t)) +
                                        right * (cosWave * 0.06f * (1 - t)) +
                                        forward * (secondaryWave * 0.04f * t);

                float thickness = Mathf.Lerp(1f, 0.2f, Mathf.Pow(t, 1.5f));
                lr.startWidth = GunLineWidth;
                lr.endWidth = GunLineWidth * thickness;

                float curveIntensity = Mathf.Sin(t * Mathf.PI) * 0.08f * tentacleLength;
                tentacleOffset += right * curveIntensity;

                float depthOffset = Mathf.Sin(t * Mathf.PI * 4f + Time.time * 5f) * 0.03f;
                tentacleOffset += forward * depthOffset;

                lr.SetPosition(i, basePos + tentacleOffset);
            }
        }
        #endregion

        public enum GunLineStyle
        {
            Normal,
            Straight,
            Morphine,
            Flare,
            Tentacle,
        }

        public static int LineCurve = 150;
        private const float LineSmoothFactor = 6f;
        private const float DestroyDelay = 0.02f;
        public static float PulseSpeed = 2f;
        public static float PulseAmplitude = 0.03f;
        public static GameObject spherepointer;
        public static VRRig LockedPlayer;
        public static bool isLocked;
        public static Vector3 lr;
        public static Color32 PointerColor = Core.BaseColor;
        public static Color32 LineColor = Core.BaseColor;
        public static GunLineStyle currentLineStyle = GunLineStyle.Normal;
        public static RaycastHit raycastHit;
        public static float SphereSize = 0.04f;
        public static float GunLineWidth = 0.01f;
        private static int currentLineSize = 0;
        private static readonly float[] LineScales = { 0.001f, 0.002f, 0.003f, 0.004f, 0.005f, 0.006f, 0.007f, 0.008f, 0.009f, 0.01f, 0.011f, 0.012f, 0.013f, 0.014f, 0.015f, 0.016f, 0.017f, 0.018f, 0.019f, 0.02f };
        private static int currentSphereSize = 0;
        private static readonly float[] SphereScales = { 0.01f, 0.02f, 0.03f, 0.04f, 0.05f, 0.06f, 0.07f, 0.08f, 0.09f, 0.1f, 0.11f, 0.12f, 0.13f, 0.14f, 0.15f, 0.16f, 0.17f, 0.18f, 0.19f, 0.2f };

        private static GameObject _gunLineObject;
        private static LineRenderer _gunLineRenderer;
        private static Material _gunLineMaterial;
        private static Coroutine _gunLineCoroutine;
        private static MonoBehaviour _gunLineHost;

        private static Vector3[] _morphinePositions = new Vector3[150];
        private static float[] _morphineTimers = new float[150];
        private static Vector3[] _morphineVelocities = new Vector3[150];

        private static LineRenderer GetOrCreateGunLine()
        {
            if (_gunLineRenderer == null || _gunLineObject == null)
            {
                if (_gunLineObject != null) GameObject.Destroy(_gunLineObject);
                _gunLineObject = new GameObject("GunLine_Persistent");
                _gunLineRenderer = _gunLineObject.AddComponent<LineRenderer>();
                _gunLineMaterial = new Material(Core.UberShader);
                _gunLineRenderer.material = _gunLineMaterial;
                _gunLineRenderer.useWorldSpace = true;
                _gunLineHost = _gunLineObject.AddComponent<GunLib>();
                _gunLineCoroutine = null;
            }
            return _gunLineRenderer;
        }

        private static void UpdateGunLine(Vector3 handPos, Vector3 spherePos, Vector3 smoothMid)
        {
            var lineRenderer = GetOrCreateGunLine();
            lineRenderer.startWidth = GunLineWidth;
            lineRenderer.endWidth = GunLineWidth;
            lineRenderer.startColor = LineColor;
            lineRenderer.endColor = LineColor;
            _gunLineMaterial.color = LineColor;
            _gunLineObject.SetActive(true);

            SetLineStyle(lineRenderer, handPos, spherePos, smoothMid);
            lineRenderer.startColor = Color.Lerp(Core.BaseColor, Core.BaseColor, Mathf.PingPong(Time.time * 1, 0.5f));
            lineRenderer.endColor = lineRenderer.startColor;
        }

        private static void HideGunLine()
        {
            if (_gunLineObject != null)
                _gunLineObject.SetActive(false);
        }

        private static int? noInvisLayerMask;
        public static int NoInvisLayerMask()
        {
            noInvisLayerMask ??= ~(
                (1 << LayerMask.NameToLayer("TransparentFX")) |
                (1 << LayerMask.NameToLayer("Ignore Raycast")) |
                (1 << LayerMask.NameToLayer("Zone")) |
                (1 << LayerMask.NameToLayer("Gorilla Trigger")) |
                (1 << LayerMask.NameToLayer("Gorilla Boundary")) |
                (1 << LayerMask.NameToLayer("GorillaCosmetics")) |
                (1 << LayerMask.NameToLayer("GorillaParticle"))
            );

            return noInvisLayerMask ?? GTPlayer.Instance.locomotionEnabledLayers;
        }

        public static void GunPreview()
        {
            StartPointerSystem(() => { }, false);
        }

        public static void GunPreviewLock()
        {
            StartPointerSystem(() => { }, true);
        }

        public static void SetSphereSize(float newSize)
        {
            SphereSize = Mathf.Clamp(newSize, 0.01f, 0.3f);
            if (spherepointer != null)
                spherepointer.transform.localScale = Vector3.one * SphereSize;
        }

        public static void SetGunLineWidth(float newWidth)
        {
            GunLineWidth = Mathf.Clamp(newWidth, 0.001f, 0.1f);
        }

        private static void SetLineStyle(LineRenderer lineRenderer, Vector3 start, Vector3 end, Vector3 smoothMid)
        {
            switch (currentLineStyle)
            {
                case GunLineStyle.Normal: CurveLineRenderer(lineRenderer, start, smoothMid, end); break;
                case GunLineStyle.Straight: lineRenderer.positionCount = 2; lineRenderer.SetPosition(0, start); lineRenderer.SetPosition(1, end); break;
                case GunLineStyle.Morphine: DrawMorphineLine(lineRenderer, start, end, 150); break;
                case GunLineStyle.Flare: DrawFlareLine(lineRenderer, start, end, 150); break;
                case GunLineStyle.Tentacle: DrawTentacleLine(lineRenderer, start, end, 150); break;
            }
        }

        public static void CycleLineStyle()
        {
            currentLineStyle = (GunLineStyle)(((int)currentLineStyle + 1) % Enum.GetNames(typeof(GunLineStyle)).Length);
        }

        private static Vector3 CalculateBezierPoint(Vector3 start, Vector3 mid, Vector3 end, float t)
        {
            return Mathf.Pow(1 - t, 2) * start + 2 * (1 - t) * t * mid + Mathf.Pow(t, 2) * end;
        }

        private static void CurveLineRenderer(LineRenderer lineRenderer, Vector3 start, Vector3 mid, Vector3 end)
        {
            lineRenderer.positionCount = LineCurve;
            for (int i = 0; i < LineCurve; i++)
            {
                float t = (float)i / (LineCurve - 1);
                lineRenderer.SetPosition(i, CalculateBezierPoint(start, mid, end, t));
            }
            lineRenderer.SetPosition(0, start);
            lineRenderer.SetPosition(LineCurve - 1, end);
        }

        private static void DrawMorphineLine(LineRenderer lr, Vector3 start, Vector3 end, int segments)
        {
            lr.positionCount = segments;

            if (_morphinePositions[0] == Vector3.zero)
            {
                for (int i = 0; i < segments; i++)
                {
                    float t = (float)i / (segments - 1);
                    _morphinePositions[i] = Vector3.Lerp(start, end, t);
                    _morphineVelocities[i] = Vector3.zero;
                }
            }

            float segmentLength = Vector3.Distance(start, end) / segments;
            float stiffness = 0.95f;
            float damping = 0.98f;
            float zeroGravityStrength = 0.2f;

            Vector3 zeroGravityNoise = new Vector3(
                Mathf.Sin(Time.time * 1.7f) * 0.03f,
                Mathf.Cos(Time.time * 1.3f) * 0.03f,
                Mathf.Sin(Time.time * 2.1f) * 0.03f
            );

            _morphinePositions[0] = start;
            _morphinePositions[segments - 1] = end;

            for (int iteration = 0; iteration < 5; iteration++)
            {
                for (int i = 1; i < segments - 1; i++)
                {
                    Vector3 targetPos = _morphinePositions[i];

                    targetPos += zeroGravityNoise * zeroGravityStrength * (1 - Mathf.Abs((float)i / segments - 0.5f) * 2);

                    Vector3 toPrev = _morphinePositions[i - 1] - targetPos;
                    Vector3 toNext = _morphinePositions[i + 1] - targetPos;

                    Vector3 springForce = (toPrev + toNext) * stiffness;
                    _morphineVelocities[i] += springForce * Time.deltaTime * 20f;
                    _morphineVelocities[i] *= damping;

                    targetPos += _morphineVelocities[i] * Time.deltaTime;

                    if (toPrev.magnitude > segmentLength * 1.2f)
                        targetPos = _morphinePositions[i - 1] - toPrev.normalized * segmentLength;
                    if (toNext.magnitude > segmentLength * 1.2f)
                        targetPos = _morphinePositions[i + 1] - toNext.normalized * segmentLength;

                    float waveX = Mathf.Sin(Time.time * 3f + i * 0.2f) * 0.02f;
                    float waveY = Mathf.Cos(Time.time * 2.5f + i * 0.15f) * 0.02f;
                    float waveZ = Mathf.Sin(Time.time * 3.5f + i * 0.25f) * 0.02f;

                    targetPos += new Vector3(waveX, waveY, waveZ);

                    _morphinePositions[i] = Vector3.Lerp(_morphinePositions[i], targetPos, Time.deltaTime * 15f);
                    lr.SetPosition(i, _morphinePositions[i]);
                }
            }

            lr.SetPosition(0, start);
            lr.SetPosition(segments - 1, end);
        }

        private static void DrawFlareLine(LineRenderer lr, Vector3 start, Vector3 end, int points)
        {
            lr.positionCount = points;
            for (int i = 0; i < points; i++)
            {
                float t = (float)i / (points - 1);
                Vector3 pos = Vector3.Lerp(start, end, t);

                float widthMultiplier = Mathf.Lerp(0.5f, 3f, t);
                lr.startWidth = GunLineWidth;
                lr.endWidth = GunLineWidth * widthMultiplier;
                lr.SetPosition(i, pos);
            }
            lr.SetPosition(0, start);
            lr.SetPosition(points - 1, end);
        }

        private static void DrawTentacleLine(LineRenderer lr, Vector3 start, Vector3 end, int points)
        {
            lr.positionCount = points;
            Vector3 direction = (end - start).normalized;
            Vector3 up = Vector3.up;
            Vector3 right = Vector3.Cross(direction, up).normalized;
            Vector3 forward = Vector3.Cross(right, up).normalized;

            float tentacleLength = Vector3.Distance(start, end);

            for (int i = 0; i < points; i++)
            {
                float t = (float)i / (points - 1);
                Vector3 basePos = Vector3.Lerp(start, end, t);

                float mainSine = Mathf.Sin(t * Mathf.PI * 3f + Time.time * 3f);
                float mainCosine = Mathf.Cos(t * Mathf.PI * 2.5f + Time.time * 2.5f);
                float secondaryWave = Mathf.Sin(t * Mathf.PI * 5f + Time.time * 4f) * 0.5f;
                float tertiaryWave = Mathf.Cos(t * Mathf.PI * 6f + Time.time * 3.5f) * 0.3f;

                Vector3 tentacleOffset = up * (mainSine * 0.1f * (1 - t)) +
                                        right * (mainCosine * 0.08f * (1 - t)) +
                                        forward * (secondaryWave * 0.06f * t);

                tentacleOffset += up * (tertiaryWave * 0.04f * Mathf.Sin(t * Mathf.PI));

                float thickness = Mathf.Lerp(1f, 0.15f, Mathf.Pow(t, 1.5f));
                lr.startWidth = GunLineWidth;
                lr.endWidth = GunLineWidth * thickness;

                float curveIntensity = Mathf.Sin(t * Mathf.PI) * 0.1f * tentacleLength;
                tentacleOffset += right * curveIntensity;

                float depthOffset = Mathf.Sin(t * Mathf.PI * 4f + Time.time * 5f) * 0.04f;
                float depthOffset2 = Mathf.Cos(t * Mathf.PI * 3f + Time.time * 4f) * 0.02f;
                tentacleOffset += forward * (depthOffset + depthOffset2);

                float twistOffset = Mathf.Sin(t * Mathf.PI * 8f + Time.time * 6f) * 0.02f;
                tentacleOffset += right * twistOffset;

                lr.SetPosition(i, basePos + tentacleOffset);
            }

            lr.SetPosition(0, start);
            lr.SetPosition(points - 1, end);
        }

        private static IEnumerator StartCurvyLineRenderer(LineRenderer lineRenderer, Vector3 start, Vector3 end, Vector3 smoothMid)
        {
            while (lineRenderer != null)
            {
                if (lineRenderer == null) yield break;
                SetLineStyle(lineRenderer, start, end, smoothMid);
                lineRenderer.startColor = Color.Lerp(Core.BaseColor, Core.BaseColor, Mathf.PingPong(Time.time * 1, 0.5f));
                lineRenderer.endColor = lineRenderer.startColor;
                yield return null;
            }
        }

        public static void StartVrGunR(Action action, bool LockOn)
        {
            if (ControllerInputPoller.instance.rightGrab)
            {
                isLocked = false;
                Physics.Raycast(GorillaTagger.Instance.rightHandTransform.position, -GorillaTagger.Instance.rightHandTransform.up, out raycastHit, 512f, NoInvisLayerMask());
                if (spherepointer == null)
                {
                    spherepointer = LineLib.CreateSphere(raycastHit.point, SphereSize, LineColor, true);
                    lr = GorillaTagger.Instance.offlineVRRig.rightHandTransform.position;
                }
                if (LockedPlayer == null)
                {
                    spherepointer.transform.position = raycastHit.point;
                    spherepointer.GetComponent<Renderer>().material.color = LineColor;
                    isLocked = false;
                }
                else
                {
                    spherepointer.transform.position = LockedPlayer.transform.position;
                }

                Vector3 handPos = GorillaTagger.Instance.rightHandTransform.position;
                Vector3 spherePos = spherepointer.transform.position;

                lr = Vector3.Lerp(lr, (handPos + spherePos) / 2f, Time.deltaTime * 6f);

                UpdateGunLine(handPos, spherePos, lr);

                if (ControllerInputPoller.instance.rightControllerIndexFloat > 0.5f)
                {
                    trigger = true;
                    if (LockOn)
                    {
                        if (LockedPlayer == null)
                        {
                            LockedPlayer = raycastHit.collider.GetComponentInParent<VRRig>();
                        }
                        if (LockedPlayer != null)
                        {
                            spherepointer.transform.position = LockedPlayer.transform.position;
                            action();
                            isLocked = true;
                        }
                        return;
                    }
                    action();
                    return;
                }
                else
                {
                    trigger = false;
                    if (LockedPlayer != null)
                    {
                        LockedPlayer = null;
                        return;
                    }
                }
            }
            else
            {
                HideGunLine();
                if (spherepointer != null)
                {
                    LineLib.DestroySphere(spherepointer);
                    spherepointer = null;
                    LockedPlayer = null;
                }
            }
        }

        public static void StartVrGunL(Action action, bool LockOn)
        {
            if (ControllerInputPoller.instance.leftGrab)
            {
                Physics.Raycast(GorillaTagger.Instance.leftHandTransform.position, -GorillaTagger.Instance.leftHandTransform.up, out raycastHit, 512f, NoInvisLayerMask());
                if (spherepointer == null)
                {
                    spherepointer = LineLib.CreateSphere(raycastHit.point, SphereSize, LineColor, true);
                    lr = GorillaTagger.Instance.offlineVRRig.leftHandTransform.position;
                }
                if (LockedPlayer == null)
                {
                    spherepointer.transform.position = raycastHit.point;
                    spherepointer.GetComponent<Renderer>().material.color = LineColor;
                }
                else
                {
                    spherepointer.transform.position = LockedPlayer.transform.position;
                }

                Vector3 handPos = GorillaTagger.Instance.leftHandTransform.position;
                Vector3 spherePos = spherepointer.transform.position;

                lr = Vector3.Lerp(lr, (handPos + spherePos) / 2f, Time.deltaTime * 6f);

                UpdateGunLine(handPos, spherePos, lr);

                if (ControllerInputPoller.instance.leftControllerIndexFloat > 0.5f)
                {
                    trigger = true;
                    if (LockOn)
                    {
                        if (LockedPlayer == null)
                        {
                            LockedPlayer = raycastHit.collider.GetComponentInParent<VRRig>();
                        }
                        if (LockedPlayer != null)
                        {
                            spherepointer.transform.position = LockedPlayer.transform.position;
                            action();
                            isLocked = true;
                        }
                        return;
                    }
                    action();
                    return;
                }
                else if (LockedPlayer != null)
                {
                    LockedPlayer = null;
                    return;
                }
            }
            else
            {
                HideGunLine();
                if (spherepointer != null)
                {
                    LineLib.DestroySphere(spherepointer);
                    spherepointer = null;
                    LockedPlayer = null;
                }
            }
        }

        public static void StartPcGun(Action action, bool LockOn)
        {
            Ray ray = GameObject.Find("Shoulder Camera").activeSelf ? GameObject.Find("Shoulder Camera").GetComponent<Camera>().ScreenPointToRay(UnityInput.Current.mousePosition) : GorillaTagger.Instance.mainCamera.GetComponent<Camera>().ScreenPointToRay(UnityInput.Current.mousePosition);
            if (Mouse.current.rightButton.isPressed)
            {
                RaycastHit raycastHit;
                if (Physics.Raycast(ray.origin, ray.direction, out raycastHit, 512f, NoInvisLayerMask()) && spherepointer == null)
                {
                    if (spherepointer == null)
                    {
                        spherepointer = LineLib.CreateSphere(raycastHit.point, SphereSize, LineColor, true);
                        lr = GorillaTagger.Instance.offlineVRRig.rightHandTransform.position;
                    }
                }
                if (LockedPlayer == null)
                {
                    spherepointer.transform.position = raycastHit.point;
                    spherepointer.GetComponent<Renderer>().material.color = LineColor;
                }
                else
                {
                    spherepointer.transform.position = LockedPlayer.transform.position;
                }

                Vector3 handPos = GorillaTagger.Instance.rightHandTransform.position;
                Vector3 spherePos = spherepointer.transform.position;

                lr = Vector3.Lerp(lr, (handPos + spherePos) / 2f, Time.deltaTime * 6f);

                UpdateGunLine(handPos, spherePos, lr);

                if (Mouse.current.leftButton.isPressed)
                {
                    trigger = true;
                    if (LockOn)
                    {
                        if (LockedPlayer == null)
                        {
                            LockedPlayer = raycastHit.collider.GetComponentInParent<VRRig>();
                        }
                        if (LockedPlayer != null)
                        {
                            spherepointer.transform.position = LockedPlayer.transform.position;
                            action();
                            isLocked = true;
                        }
                        return;
                    }
                    action();
                    return;
                }
                else
                {
                    trigger = false;
                    if (LockedPlayer != null)
                    {
                        LockedPlayer = null;
                        return;
                    }
                }
            }
            else
            {
                HideGunLine();
                if (spherepointer != null)
                {
                    LineLib.DestroySphere(spherepointer);
                    spherepointer = null;
                    LockedPlayer = null;
                }
            }
        }

        public static void StartPointerSystem(Action action, bool locko)
        {
            if (XRSettings.isDeviceActive)
            {
                if (gunLeft == true)
                {
                    StartVrGunL(action, locko);
                }
                else
                {
                    StartVrGunR(action, locko);
                }
            }
            if (!XRSettings.isDeviceActive)
            {
                StartPcGun(action, locko);
            }
        }

        public static bool trigger = false;
        public static bool gunLeft = false;
    }
}



