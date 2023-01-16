using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using UnityEngine.UI;
using TMPro;
using System.ComponentModel;

public class ClientPuppetcontroller : MonoBehaviour
{
    [SerializeField]
    private string playerName;

    public string PlayerName
    {
        get { return playerName; }
    }

    [SerializeField]
    [Range(0.1f, 10f)]
    private float playerSpeed = 1f;

    [SerializeField]
    [Range(0f, 300f)]
    private float playerRotationSpeed = 50f;

    private Camera mainCamera;

    private Quaternion cameraRot;

    private TMP_Text playerText;

    private Transform playerTransform;

    private float playerRotation;

    private Vector3 playerVelocity;


    private void Awake()
    {
        playerText = GetComponentInChildren<TMP_Text>();
        if (playerText == null )
        {
            playerText = gameObject.AddComponent<TMP_Text>();
        }
        mainCamera = Camera.main;
        if (mainCamera == null )
        {
            mainCamera = gameObject.AddComponent<Camera>();
        }
        playerTransform = gameObject.transform;
    }

    // Start is called before the first frame update
    void Start()
    {
        if (playerName == "")
        {
            playerName = gameObject.name;
        }
    }

    // Update is called once per frame
    void Update()
    {
        CameraUpdateRotation();
        GetPlayerInput();
        MovePlayer();
    }

    void CameraUpdateRotation()
    {
        cameraRot = mainCamera.transform.rotation;
        playerText.transform.rotation = new Quaternion
        (
            playerText.transform.rotation.x,
            cameraRot.y,
            playerText.transform.rotation.z,
            playerText.transform.rotation.w
        );
    }

    void GetPlayerInput()
    {
        playerVelocity.x = Input.GetAxis("Horizontal");
        playerVelocity.z = Input.GetAxis("Vertical");
        playerRotation = Input.GetAxis("Rotation");
    }

    void MovePlayer()
    {
        playerTransform.position += playerVelocity.normalized * playerSpeed * Time.deltaTime;
        playerTransform.eulerAngles = new Vector3(
            playerTransform.eulerAngles.x,
            playerTransform.eulerAngles.y + playerRotation * playerRotationSpeed * Time.deltaTime,
            playerTransform.eulerAngles.z
            );
    }
}
