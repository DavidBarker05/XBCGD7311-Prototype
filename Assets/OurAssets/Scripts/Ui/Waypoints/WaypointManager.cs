using System.Collections.Generic;
using UnityEngine;

public class WaypointManager : MonoBehaviour
{
    public static WaypointManager Instance { get; private set; }

    [SerializeField]
    RectTransform m_WaypointParent; // Any RectTransform under a Canvas - a HUD panel works fine, and lets that panel's active state toggle all waypoints at once
    [SerializeField]
    WaypointIndicator m_WaypointIndicatorPrefab;
    [SerializeField]
    Sprite m_DefaultIcon;
    [SerializeField]
    Camera m_Camera; // Falls back to Camera.main if left unset
    [SerializeField]
    Vector3 m_DefaultWorldOffset = new Vector3(0f, 2f, 0f);

    readonly Dictionary<Transform, WaypointIndicator> m_ActiveWaypoints = new Dictionary<Transform, WaypointIndicator>();

    void Awake()
    {
        if (Instance && Instance != this) Destroy(gameObject);
        else Instance = this;
    }

    public WaypointIndicator AddWaypoint(Transform target, Sprite icon = null, Vector3? worldOffset = null)
    {
        if (!target) return null;
        if (m_ActiveWaypoints.TryGetValue(target, out WaypointIndicator existing) && existing) return existing;

        WaypointIndicator indicator = Instantiate(m_WaypointIndicatorPrefab, m_WaypointParent);
        indicator.Init(target, m_Camera ? m_Camera : Camera.main, icon ? icon : m_DefaultIcon, worldOffset ?? m_DefaultWorldOffset);
        m_ActiveWaypoints[target] = indicator;
        return indicator;
    }

    public void RemoveWaypoint(Transform target)
    {
        if (!target || !m_ActiveWaypoints.TryGetValue(target, out WaypointIndicator indicator)) return;
        if (indicator) Destroy(indicator.gameObject);
        m_ActiveWaypoints.Remove(target);
    }

    public void ClearAllWaypoints()
    {
        foreach (WaypointIndicator indicator in m_ActiveWaypoints.Values) if (indicator) Destroy(indicator.gameObject);
        m_ActiveWaypoints.Clear();
    }
}
