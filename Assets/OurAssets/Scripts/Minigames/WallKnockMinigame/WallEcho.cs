using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class WallEcho : MonoBehaviour
{
	[SerializeField, Min(0.01f)]
	float m_GrowSpeed = 5f;
	[SerializeField, Min(1f)]
	float m_MaxGrowSize = 10f;

	Renderer m_Renderer;
	MaterialPropertyBlock m_MaterialProperyBlock;

	bool m_bDoUpdate;

	float m_CurrentSize;

	void Awake()
	{
		m_Renderer = GetComponent<Renderer>();
		m_MaterialProperyBlock = new MaterialPropertyBlock();
		m_bDoUpdate = false;
	}

	// Start is called once before the first execution of Update after the MonoBehaviour is created
	void Start()
    {
		//StartEcho(10);
    }

    // Update is called once per frame
    void Update()
    {
		if (!m_bDoUpdate) return;
		m_CurrentSize += m_GrowSpeed * Time.deltaTime;
		m_MaterialProperyBlock.SetFloat("_SquareSize", m_CurrentSize);
		m_Renderer.SetPropertyBlock(m_MaterialProperyBlock);
		if (m_CurrentSize >= m_MaxGrowSize) Destroy(gameObject);
    }

	public void StartEcho(int numCircles)
	{
		m_bDoUpdate = true;
		m_CurrentSize = m_Renderer.sharedMaterial.GetFloat("_SquareSize");
		m_MaterialProperyBlock.SetFloat("_SquareSize", m_CurrentSize);
		m_MaterialProperyBlock.SetInteger("_NumCircles", numCircles);
		m_Renderer.SetPropertyBlock(m_MaterialProperyBlock);
	}
}
