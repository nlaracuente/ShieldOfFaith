using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles playing/stopping the different types of particles effects the shield has
/// </summary>
public class ShieldParticleSystem : MonoBehaviour
{
    [System.Serializable]
    struct ShieldParticles
    {
        public ShieldParticle name;
        public ParticleSystem particles;
        public SoundEffect sfx;
    }

    [SerializeField, Tooltip("List all the shield particles available for us to play")]
    List<ShieldParticles> shieldParticles;
    Dictionary<ShieldParticle, ShieldParticles> particleMapping;

    private void Start()
    {
        particleMapping = new Dictionary<ShieldParticle, ShieldParticles>();
        foreach (var shieldParticle in shieldParticles)
            particleMapping[shieldParticle.name] = shieldParticle;
    }

    ShieldParticles GetParticleSystem(ShieldParticle name)
    {
        return particleMapping.ContainsKey(name) ? particleMapping[name] : default(ShieldParticles);
    }

    public void Play(ShieldParticle name)
    {
        var system = GetParticleSystem(name);
        var particles = system.particles;
        if (particles != null && !particles.isPlaying)
        {
            Enable(name);
            particles.Play();
            AudioManager.instance.Play(system.sfx);
        }
    }

    public void Stop(ShieldParticle name)
    {
        var particles = GetParticleSystem(name).particles;
        if (particles != null)
            particles.Stop();
    }

    public void Pause(ShieldParticle name)
    {
        var particles = GetParticleSystem(name).particles;
        if (particles != null)
            particles.Pause();
    }

    public void Enable(ShieldParticle name)
    {
        var particles = GetParticleSystem(name).particles;
        if (particles != null)
            particles.gameObject.SetActive(true);
    }

    public void Disable(ShieldParticle name)
    {
        var particles = GetParticleSystem(name).particles;
        if (particles != null)
        {
            particles.Stop();
            particles.gameObject.SetActive(false);
        }
            
    }
}
