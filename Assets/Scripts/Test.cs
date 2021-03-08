using HUD;
using UnityEngine;

public class Test : MonoBehaviour
{
    private float _timer = 0;

    private void Start()
    {
        HUDMesh.OnGameStart();
    }

    private void OnDestroy()
    {
        HUDMesh.OnGameEnd();
    }

    private void Update()
    {
        _timer += Time.deltaTime;
        if (_timer > 1.0f)
        {
            _timer = 0f;
            HUDRenderManager.Instance.AddHUDNumber(new Vector3(0, 0, 0), 0, Random.Range(0, 10000000), false, false,
                false);
        }
    }
}
