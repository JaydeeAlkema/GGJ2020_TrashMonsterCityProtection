using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;


public class PlayerControllerBehaviour : MonoBehaviour
{
	[SerializeField] private Animator animLegs;
	//Player statistics
	[SerializeField]
	[Range(0, 100)]
	private float health = 100f;

	[SerializeField]
	[Range(0, 100)]
	private float stamina = 100;

	[SerializeField]
	private float scrapCollected = 0;

	[SerializeField]
	private TextMeshProUGUI amountScrapText = default;

	[SerializeField]
	private float staminaDrain = 1f;
	[SerializeField]
	private float staminaRegeneration = 0.2f;
	[SerializeField]
	private float staminaCooldown = 0;


	//Handling
	private float rotationSpeed = 450;
	[SerializeField]
	private float walkSpeed = 5;
	[SerializeField]
	private float runSpeed = 8;


	//System
	private Quaternion targetRotation;

	//Components
	private CharacterController controller;

	[SerializeField]
	public GunBehaviour gun;

	[SerializeField]
	private Camera cam;

	public float Health { get => health; set => health = value; }
	public float Stamina { get => stamina; set => stamina = value; }
	public float ScrapCollected { get => scrapCollected; set => scrapCollected = value; }

	void Start()
	{
		controller = GetComponent<CharacterController>();
	}

	void Update()
	{
		ControlMouse();
		if(Input.GetButtonDown("Shoot"))
		{
			gun.Shoot();
		}
		if(health <= 0)
		{
			Die();
		}
		amountScrapText.text = scrapCollected.ToString();
	}

	private void OnTriggerEnter(Collider other)
	{
		if(other.CompareTag("Collectable"))
		{
			scrapCollected++;
		}
	}

	void ControlMouse()
	{
		Vector3 mousePos = Input.mousePosition;
		mousePos = cam.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, cam.transform.position.y - transform.position.y));
		targetRotation = Quaternion.LookRotation(mousePos - new Vector3(transform.position.x, 0, transform.position.z));
		transform.eulerAngles = Vector3.up * Mathf.MoveTowardsAngle(transform.eulerAngles.y, targetRotation.eulerAngles.y, rotationSpeed * Time.deltaTime);



		Vector3 input = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
		Vector3 motion = input;
		motion *= (Mathf.Abs(input.x) == 1 && Mathf.Abs(input.z) == 1) ? .7f : 1f;

		if(Input.GetButton("Run") && stamina > 0 && staminaCooldown < 1)
		{
			animLegs.SetBool("Walking", false);
			animLegs.SetBool("Running", true);
			motion *= runSpeed;
			stamina -= staminaDrain;
		}
		else
		{
			animLegs.SetBool("Walking", true);
			animLegs.SetBool("Running", false);
			staminaCooldown--;

			motion *= walkSpeed;

			if(stamina <= 0)
			{
				staminaCooldown = 60f;
			}

			if(stamina < 100)
			{
				stamina += staminaRegeneration;
			}
		}


		motion += Vector3.up * -8;

		controller.Move(motion * Time.deltaTime);
	}

	void Die()
	{
		SceneManager.LoadScene("mainMenu");
	}
}
