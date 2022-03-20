using UnityEngine;

public class SkillFxItem : MonoBehaviour
{
    [SerializeField] protected ParticleSystem _particleSystem;
    [SerializeField] protected Renderer _renderer;

    public ParticleSystem particleSystem { get { return _particleSystem; } }
    public Renderer renderer { get { return _renderer; } }
}
