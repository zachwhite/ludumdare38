﻿namespace PrimordialOoze
{
	using System.Collections;
	using UnityEngine;


	public class DashAttack : PrimaryAttack
	{
		[SerializeField]
		private ContactDamage attackDamageField;

		[SerializeField]
		private GameObject dashEffect;

		public override void Attack(float x, float y)
		{
			if (this.Microbe.IsAttacking
				|| this.Microbe.IsCoolingDown
				|| this.attackDamageField == null)
			{
				return;
			}

			if (this.Microbe.OrientMovement)
			{
				if (this.Microbe.AttacksBackward)
					this.Microbe.RotateAwayFrom(x, y);
				else
					this.Microbe.RotateToward(x, y);
			}

			this.attackDamageField.Damage = this.Microbe.Strength;
			this.Microbe.IsAttacking = true;
			this.Microbe.InvulnerabilityTimeLeft = 0.5f;
			this.Microbe.Animator.Play(Microbe.AttackAnimation);
			this.Microbe.GamePhysics.SetMovement(
				new Vector2(x, y).normalized
				* (this.Microbe.AttackSpeed + this.Microbe.MaxSpeed));
			this.attackDamageField.gameObject.SetActive(true);
			if (this.dashEffect != null)
			{
				Instantiate(
					this.dashEffect,
					this.transform.position,
					this.transform.rotation);
			}

			StartCoroutine(WaitToFinishAttack(x, y));
		}


		public override void Awake()
		{
			base.Awake();

			if (this.attackDamageField != null)
			{
				this.attackDamageField.gameObject.SetActive(false);
				var contactDamage = this.attackDamageField.GetComponent<ContactDamage>();
				if (contactDamage != null)
					contactDamage.DidDamage += Recoil;
			}
		}


		private void Recoil(IDamageable damageable)
		{
			Microbe microbe = damageable as Microbe;
			if (microbe != null)
			{
				Vector2 direction = this.Microbe.GamePhysics.Velocity * -1;
				this.Microbe.GamePhysics.SetMovement(
					direction.normalized
					* (this.Microbe.AttackSpeed + this.Microbe.MaxSpeed));
			}
			else
			{
				this.Microbe.GamePhysics.SetMovement(Vector2.zero);
			}
		}


		#region Helper Methods
		private IEnumerator WaitToFinishAttack(float x, float y)
		{
			this.Microbe.IsMoving = true;
			yield return new WaitForSeconds(this.Microbe.AttackDuration);

			this.Microbe.IsAttacking = false;
			this.Microbe.IsCoolingDown = true;
			this.Microbe.GamePhysics.SetMovement(new Vector2(x, y) * this.Microbe.MaxSpeed);
			this.attackDamageField.gameObject.SetActive(false);
			this.Microbe.Animator.Play(Microbe.IdleAnimation);
			StartCoroutine(this.Microbe.WaitForCooldownEnd());
		}
		#endregion
	}
}