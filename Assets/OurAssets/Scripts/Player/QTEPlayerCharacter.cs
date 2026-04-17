using UnityEngine;
using UnityEngine.Events;
using Util.SystemUtils;

public class QTEPlayerCharacterInitData : IPlayerCharacterInitData { }

public class QTEPlayerCharacterUpdateData : IPlayerCharacterUpdateData
{
	public float DeltaTime { get; set; }
	public Quaternion CameraRotation { get; set; }
	public MouseInfo MouseInfo { get; set; }

	public bool DidQTEInput { get; set; }
}

public class QTEPlayerCharacter : PlayerCharacter
{
	public override bool HasBeenInitialised { get; protected set; }

	public override bool MouseVisible => true;
	public override bool DoCameraRotation => false;
	public override bool UseMouseScreenPosition => false;

	public UnityEvent OnQTEInput { get; private set; } = new UnityEvent();

	public override void Init(IPlayerCharacterInitData playerCharacterInitData)
	{
		QTEPlayerCharacterInitData initData = Sys.AssertType<QTEPlayerCharacterInitData>(playerCharacterInitData, nameof(playerCharacterInitData));
		HasBeenInitialised = true;
	}

	public override void UpdateCharacter(ref IPlayerCharacterUpdateData playerCharacterUpdateData)
	{
		QTEPlayerCharacterUpdateData input = Sys.AssertType<QTEPlayerCharacterUpdateData>(playerCharacterUpdateData, nameof(playerCharacterUpdateData));
		if (input.DidQTEInput) OnQTEInput.Invoke();
		input.DidQTEInput = false;
	}

	void OnDestroy() => OnQTEInput.RemoveAllListeners();
}
