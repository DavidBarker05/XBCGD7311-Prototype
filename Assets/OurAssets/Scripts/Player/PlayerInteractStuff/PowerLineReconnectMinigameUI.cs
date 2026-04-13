using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PowerLineReconnectMinigameUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject reconnectionPanelRoot;
    [SerializeField] private RectTransform rotatingContactArmRect;
    [SerializeField] private RectTransform liveWireSafeArcRect;
    [SerializeField] private Image liveWireSafeArcImage;
    [SerializeField] private Image rotatingContactArmImage;
    [SerializeField] private Image junctionRingImage;

    [Header("Input + Timing")]
    [SerializeField] private KeyCode commitStripAlignmentKey = KeyCode.E;
    [SerializeField] private float arcFlashDurationSeconds = 0.35f;
    [SerializeField] private float safeArcVisualAngleOffset = 0f;

    [Header("State Dependencies")]
    [SerializeField] private PlayerControlGate playerControlGate;
    [SerializeField] private PlayerInteractor playerInteractor;

    [Header("Feedback Colors")]
    [SerializeField] private Color linesRestoredColor = Color.green;
    [SerializeField] private Color reconnectionFailedColor = Color.red;
    [SerializeField] private Color idleJunctionColor = Color.white;

    private PowerLineReconnectInteractable activePowerLineTerminal;
    private float currentContactArmAngle;
    private float safeArcCenterAngle;
    private bool awaitingStripCommitInput;

    public bool IsReconnectionPanelOpen => awaitingStripCommitInput || activePowerLineTerminal != null;

    private void Awake()
    {
        if (reconnectionPanelRoot != null)
        {
            reconnectionPanelRoot.SetActive(false);
        }
    }

    private void Update()
    {
        if (!awaitingStripCommitInput || activePowerLineTerminal == null)
        {
            return;
        }

        AdvanceContactArmAngle(activePowerLineTerminal.RotationSpeed);

        if (Input.GetKeyDown(commitStripAlignmentKey))
        {
            EvaluateStripAlignment();
        }
    }

    public void StartReconnectionMinigame(PowerLineReconnectInteractable powerLineTerminal)
    {
        if (powerLineTerminal == null)
        {
            return;
        }

        activePowerLineTerminal = powerLineTerminal;
        awaitingStripCommitInput = true;
        currentContactArmAngle = 0f;
        SetContactArmRotation(currentContactArmAngle);

        RandomizeLiveWireSafeArc(powerLineTerminal.SuccessZoneSize);
        SetJunctionFeedbackColor(idleJunctionColor);

        if (reconnectionPanelRoot != null)
        {
            reconnectionPanelRoot.SetActive(true);
        }

        playerControlGate?.SetGameplayEnabled(false);
    }

    private void AdvanceContactArmAngle(float degreesPerSecond)
    {
        currentContactArmAngle = Mathf.Repeat(currentContactArmAngle + degreesPerSecond * Time.deltaTime, 360f);
        SetContactArmRotation(currentContactArmAngle);
    }

    private void SetContactArmRotation(float angleDegrees)
    {
        if (rotatingContactArmRect != null)
        {
            rotatingContactArmRect.localRotation = Quaternion.Euler(0f, 0f, angleDegrees);
        }
    }

    private void RandomizeLiveWireSafeArc(float arcSpanDegrees)
    {
        safeArcCenterAngle = Random.Range(0f, 360f);

        if (liveWireSafeArcImage != null)
        {
            liveWireSafeArcImage.fillAmount = Mathf.Clamp01(arcSpanDegrees / 360f);
        }

        if (liveWireSafeArcRect != null)
        {
            float arcStartAngle = safeArcCenterAngle - (arcSpanDegrees * 0.5f) + safeArcVisualAngleOffset;
            liveWireSafeArcRect.localRotation = Quaternion.Euler(0f, 0f, arcStartAngle);
        }
    }

    private void EvaluateStripAlignment()
    {
        awaitingStripCommitInput = false;

        float angularSeparation = Mathf.Abs(Mathf.DeltaAngle(currentContactArmAngle, safeArcCenterAngle));
        bool linesRestored = angularSeparation <= activePowerLineTerminal.SuccessZoneSize * 0.5f;

        StartCoroutine(FlashArcAndClosePanel(linesRestored));
    }

    private IEnumerator FlashArcAndClosePanel(bool linesRestored)
    {
        SetJunctionFeedbackColor(linesRestored ? linesRestoredColor : reconnectionFailedColor);
        yield return new WaitForSeconds(arcFlashDurationSeconds);

        PowerLineReconnectInteractable resolvedTerminal = activePowerLineTerminal;
        CloseReconnectionPanel();
        resolvedTerminal?.ResolveReconnectionAttempt(linesRestored);
    }

    private void SetJunctionFeedbackColor(Color color)
    {
        if (rotatingContactArmImage != null)
        {
            rotatingContactArmImage.color = color;
        }

        if (junctionRingImage != null)
        {
            junctionRingImage.color = color;
        }
    }

    private void CloseReconnectionPanel()
    {
        activePowerLineTerminal = null;
        awaitingStripCommitInput = false;

        if (reconnectionPanelRoot != null)
        {
            reconnectionPanelRoot.SetActive(false);
        }

        SetJunctionFeedbackColor(idleJunctionColor);
        playerControlGate?.SetGameplayEnabled(true);
        playerInteractor?.NotifyReconnectionPanelClosed();
    }
}