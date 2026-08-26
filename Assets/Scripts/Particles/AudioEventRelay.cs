using UnityEngine;

/// <summary>
/// Puente para poder invocar AudioManager.Instance desde un UnityEvent (ej. ParticleEntry.onPlayed)
/// sin necesitar arrastrar una referencia de escena en el Inspector — algo que un prefab no puede
/// hacer si AudioManager vive como singleton en la escena.
///
/// Colgalo del mismo GameObject/prefab que tiene el ParticleSequenceController, y en cada
/// entrada apuntá el UnityEvent "On Played" a este componente ? PlaySfx(string) ? el id del sonido.
/// </summary>
public class AudioEventRelay : MonoBehaviour
{
    public void PlaySfx(string sfxId)
    {
        AudioManager.Instance?.Play(sfxId);
    }
}