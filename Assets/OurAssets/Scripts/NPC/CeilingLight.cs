using System.Collections.Generic;
using UnityEngine;

public class CeilingLight : MonoBehaviour
{
    [SerializeField]
    Light m_Light;
    [SerializeField]
    MeshRenderer m_LightMesh;
    [SerializeField]
    Material m_OnMaterial;
    [SerializeField]
    Material m_OffMaterial;

    readonly List<Material> m_OnMaterialList = new List<Material>();
    readonly List<Material> m_OffMaterialList = new List<Material>();

    void Awake()
    {
        m_OnMaterialList.Add(m_OnMaterial);
        m_OffMaterialList.Add(m_OffMaterial);
    }

    public void TurnOn()
    {
        m_Light.enabled = true;
        m_LightMesh.SetMaterials(m_OnMaterialList);
    }

    public void TurnOff()
    {
        m_Light.enabled = false;
        m_LightMesh.SetMaterials(m_OffMaterialList);
    }
}
