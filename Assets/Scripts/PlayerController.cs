using System;
using System.Collections;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.Rendering.DebugUI.Table;

public class PlayerController : MonoBehaviour
{
    //Game Objects and Components
    private Camera fpCam;
    private Animator animator;
    private Rigidbody rb;
    private Boolean airborne = false;
    private Transform defaultPos;
    private GameObject winText;
    private GameObject loseText;

    public TextMeshProUGUI debugText;
    [SerializeField] private float walkSpeed;
    [SerializeField] private float runSpeed;
    [SerializeField] private float cameraSpeedHorizontal;
    [SerializeField] private float cameraSpeedVertical;
    [SerializeField] private float jumpForce;
    [SerializeField] private bool lookInversion;
    private float speedMod;
    float currentVertRotation;
    float currentHorizRotation;

    //Input System Properties
    InputAction lookAction;
    InputAction movementAction;
    InputAction sprintAction;
    InputAction jumpAction;


    bool jumpEnable = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        fpCam = this.GetComponentInChildren<Camera>();
        animator = this.GetComponent<Animator>();
        rb = this.GetComponent <Rigidbody>();
        defaultPos = this.transform;
        winText = GameObject.Find("Win Text");
        loseText = GameObject.Find("Lose Text");

        winText.SetActive(false);
        loseText.SetActive(false);

        //Input system instatiation
        lookAction = InputSystem.actions.FindAction("Look");
        movementAction = InputSystem.actions.FindAction("Move");
        sprintAction = InputSystem.actions.FindAction("Sprint");
        jumpAction = InputSystem.actions.FindAction("Jump");


        //Hide Mouse
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        

        speedMod = walkSpeed;
    }

    // Update is called once per frame
    void Update()
    {
        ///CAMERA AND FIRST PERSON CONTROLS///
        //Vertical values are lookVals.y
        //Horizontal vals are lookVals.x
        Vector2 lookVals = lookAction.ReadValue<Vector2>();
        float horizontalLook = lookVals.x * Time.deltaTime * cameraSpeedHorizontal;
        this.transform.Rotate(0, horizontalLook, 0); //* new Vector3(0f, 1f, 0f));
        horizontalLook += currentHorizRotation;
        currentHorizRotation = horizontalLook;
        int lookInvCoef = (lookInversion) ? 1 : -1;
        float verticalLook = currentVertRotation + lookVals.y * Time.deltaTime * cameraSpeedVertical * lookInvCoef;
        currentVertRotation = Mathf.Clamp(verticalLook, -90, 90);
        fpCam.transform.rotation = Quaternion.Euler(verticalLook, this.transform.rotation.eulerAngles.y, 0f);

        //debug text
        debugText.text = "lookVals = " + lookVals +
            "\nHorizontal : " + horizontalLook +
            "\nVertical : " + verticalLook +
            "\nLinear Velocity: " + rb.linearVelocity;

        ///MOVEMENT CONTROLS///
        //w -> y=+1, s-> y=-1
        //a -> x=-1, d-> x=+1
        Vector2 moveVals = movementAction.ReadValue<Vector2>();
        if (moveVals.y > 0)
        {
            animator.SetBool("Forward", true);
        }
        else if (moveVals.y < 0)
        {
            animator.SetBool("Back", true);
        }
        else
        {
            animator.SetBool("Forward", false);
            animator.SetBool("Back", false);
        }

        if (moveVals.x > 0)
        {
            animator.SetBool("Right", true);
        }
        else if (moveVals.x < 0)
        {
            animator.SetBool("Left", true);
        }
        else
        {
            animator.SetBool("Right", false);
            animator.SetBool("Left", false);
        }
        if (moveVals.magnitude > 1f)
        {
            moveVals.Normalize();
            
        }
        moveVals *= speedMod * Time.deltaTime;
        transform.Translate(moveVals.x, 0, moveVals.y);


        ///ACTION CONTROLS///
        if (sprintAction.IsPressed())
        {
            animator.SetBool("Run", true);
            speedMod = runSpeed;
        }
        else
        {
            animator.SetBool("Run", false);
            speedMod = walkSpeed;
        }
        if (jumpAction.WasPressedThisFrame() && !airborne)
        {
            animator.SetTrigger("Jump");
            jumpEnable = true;

        }

        //if ( rb.linearVelocity.y > 0.1)
        if (airborne)
        {
            animator.SetBool("Airborne", true);
        }
        else
        {
            animator.SetBool("Airborne", false);
        }



        //Reset if falling
        if (this.transform.position.y < -25) {
            resetPos();
        }
    }

    private void resetPos()
    {
        this.transform.rotation = Quaternion.Euler(0, 90, 0);
        this.transform.position = new Vector3(25, 0, 6);
    }

    IEnumerator endGame(GameObject text)
    {

        text.SetActive(true);
        Time.timeScale = 0;
        yield return new WaitForSecondsRealtime(3);
        EditorApplication.ExitPlaymode();
    }

    //Physics update loop
    void FixedUpdate()
    {
        if (jumpEnable)
        {
            rb.AddForce(new Vector3(0, jumpForce, 0), ForceMode.Impulse);
            jumpEnable = false;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            airborne = false;
        }
        if (collision.gameObject.CompareTag("Enemy"))
        {
            StartCoroutine(endGame(loseText));
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            airborne = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Goal"))
        {
            StartCoroutine(endGame(winText));
        }
    }
}
