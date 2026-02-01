using UnityEngine;
using DG.Tweening;

public class Door : MonoBehaviour
{
    [SerializeField] private DoorLinker doorLinker;
    [SerializeField] private SpriteRenderer[] keyLights;
    [SerializeField] private SpriteRenderer _doorSprite;
    [SerializeField] private Collider2D _doorCollider; // Referencia al collider

    private void OnEnable()
    {
        if (doorLinker != null)
        {
            doorLinker.OnDoorOpened += OpenDoor;
            doorLinker.OnKeyCollected += OnKeyCollected;
        }
    }

    private void OnDisable()
    {
        if (doorLinker != null)
        {
            doorLinker.OnDoorOpened -= OpenDoor;
            doorLinker.OnKeyCollected -= OnKeyCollected;
        }
    }

    private void OnKeyCollected(int currentKeys, int requiredKeys)
    {
        int lightIndex = currentKeys - 1;

        if (lightIndex >= 0 && lightIndex < keyLights.Length)
        {
            SpriteRenderer light = keyLights[lightIndex];
            if (light != null)
            {
                // Using DOFloat on the material
                light.material.DOFloat(1f, "_Alpha", 0.2f)
                    .OnComplete(() => light.material.DOFloat(0.5f, "_Alpha", 0.5f));
            }
        }
    }

    private void OpenDoor()
    {
        Debug.Log("Door Opened!");

        _doorSprite.DOFade(0f, 0.5f)
            .OnComplete(() =>
            {
                // Cuando la puerta se vuelve completamente transparente, apagamos el collider
                if (_doorSprite.color.a <= 0.01f && _doorCollider != null)
                {
                    _doorCollider.enabled = false;
                }
            });
    }

    // Opcional: También puedes verificar el estado del collider en cada frame si es necesario
    private void Update()
    {
        // Si quieres asegurarte de que el collider esté desactivado cuando la puerta es transparente
        // y activado cuando no lo es, puedes usar este método:
        /*
        if (_doorCollider != null)
        {
            _doorCollider.enabled = _doorSprite.color.a > 0.1f;
        }
        */
    }
}