using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleMovement : MonoBehaviour {

	public Transform target;
	public float force;

	ParticleSystem ps;

	// Use this for initialization
	void Start () {
		ps = GetComponent<ParticleSystem> ();

		Debug.Log (ps.name);
	}
	
	// Update is called once per frame
	void LateUpdate () {
		ParticleSystem.Particle[] particles = new ParticleSystem.Particle[ps.particleCount];
		ps.GetParticles (particles);
		Vector3 particleWorldPosition;

		for(int i = 0; i < particles.Length; i++){
			ParticleSystem.Particle p = particles [i];

			if (ps.main.simulationSpace == ParticleSystemSimulationSpace.Local) {
				particleWorldPosition = transform.TransformPoint (p.position);
			} else if (ps.main.simulationSpace == ParticleSystemSimulationSpace.Custom) {
				particleWorldPosition = ps.main.customSimulationSpace.TransformPoint (p.position);
			} else {
				particleWorldPosition = p.position;
			}

			Vector3 directionToTarget = (target.position - particleWorldPosition).normalized;

			Vector3 seekForce = directionToTarget * force * Time.deltaTime;

			p.velocity += seekForce;
			particles [i] = p;
		}

		ps.SetParticles (particles, particles.Length);
	}
}
