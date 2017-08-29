using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
{
    // kinematic attributes
    public Vector3 vel;
    public Vector3 acc;
    public float walkSpeed;

    // dynamic attributes
    public float mass;
    public float maxVel;
    public float minVel;
    public float maxAcc;

    // fun jumping attributes
    public float gravity;
    public float height;
    private float initJumpV; // this = sqrt (2 * gravity * height)

    // flag attributes
    // jumping
    public bool up;

    // fast-falling
    private bool fastfall;
    public bool doFastfall; // press Alpha1 to toggle whether fastfall acts
    
    // Simon Belmont style jump
    private bool belmont;
    public bool doBelmont; // press Alpha2 to toggle whether belmont acts
    private KeyCode dir; // track current key for direction of belmont jump

    // variable height jumping
    public bool doVar; // press Alpha3 to toggle whether VHJ applies
    private int counter = 0; // keeps track of # of frames button held
    public int maxCounts; // maximum number of count increments before peak

    // double jumping
    public bool doDoubleJump; // press Alpha4 to toggle double jumping
    private bool doubleJump; // if true, press K if up==true to jump again
    private int jumpCounter = 0; // current number of times player has jumped
    public int maxJumps; // maximum number of jumps allowed

    // walking with forces attributes
    public float friction;
    public bool doFriction;

    // floor object attribute
    public GameObject floorObj;
    private float floor;

	// Use this for initialization
	void Start ()
    {
        // assume actor spawns in the air
        // so, it's up and fastfalling if doFastfall
        up = true;
        if (doFastfall)
            fastfall = true;

        // also, assume belmont and fall straight if doBelmont
        dir = KeyCode.None;
        if (doBelmont)
            belmont = true;

        // also, assume able to double jump
        if (doDoubleJump)
            doubleJump = true;

        // calculate initial jump delta v
        initJumpV = Mathf.Sqrt(2 * gravity * height);
        Debug.Log("initial jumping force magnitude = " + initJumpV);

        // set floor value for jumping
        floor = floorObj.transform.position.y + 1.7f;
        Debug.Log("floor height = " + floor);

        // calculate minimum velocity based on friction
        minVel = 0.01f;//friction * 0.1f;
	}
	
	// Update is called once per frame
	void Update ()
    {
        //up = true;
        //if (transform.position.y <= floor && up)
        //    Land(floor);
        ProcessInput(); // process input for left/right and jumping

        if (up)
        {
            if (fastfall)
            {
                ApplyForce(new Vector3(0f, -4.5f * gravity)); // apply increased gravity after the peak of a jump
            }
            else
            {
                ApplyForce(new Vector3(0f, -gravity)); // apply force due to gravity
            }
        }
        UpdatePos(); // update current position based on previous state
        //ProcessInput(); // process input for left/right and jumping
    }

    void UpdatePos()
    {
        /*
        // cap acceleration if needed
        if (acc.magnitude > maxAcc)
        {
            acc.Normalize();
            acc *= maxAcc;
        }
        */

        // EULER
        // add velocity to acceleration * delta(t)
        vel += acc * Time.deltaTime;


        // cap velocity if needed
        if (Mathf.Abs(vel.x) > maxVel)
        {
            if (vel.x < 0.0f)
                vel.x = -maxVel;
            else
                vel.x = maxVel;
        }

        // make sure our guy actually stops!
        if (Mathf.Abs(vel.x) < Mathf.Abs(minVel))
            vel.x = 0.0f;

        // add position to velocity * delta(t)
        transform.position += vel * Time.deltaTime;

        //if (transform.position.y <= floor && up)
        //    Land(floor);
        //else
        //{
            // white if airborn
            //up = true;
            //doubleJump = true;
            //GetComponentInChildren<SpriteRenderer>().color = Color.white;
        //}
        /*
        // fix position to the floor
        if (transform.position.y <= floor && up)
        {
            transform.position = new Vector3(transform.position.x, floor);
            acc.y = 0.0f; // not moving, so acceleration is zero
            vel.y = 0.0f; // and velocity to zero
            up = false; // grounded...
            belmont = false; // ...so don't work belmont magic
            doubleJump = false; // disable double jumping too
            // this is a problem line
            // because this if statement is checked every frame
            // even when already grounded
            // but can't we just add an extra conditional??????

            // reset counter
            counter = 0;
            jumpCounter = 0;
            
            // new debugging shenanigans, black if grounded
            //GetComponentInChildren<SpriteRenderer>().color = Color.black;
        }
        else
        {
            // white if airborn
            //up = true;
            //doubleJump = true;
            //GetComponentInChildren<SpriteRenderer>().color = Color.white;
        }
        */

        // test for fastfalling
        if (up && vel.y <= 0f && doFastfall)
        {
            fastfall = true;
            //GetComponentInChildren<SpriteRenderer>().color = Color.red;
        }
        else
        {
            fastfall = false;
            //GetComponentInChildren<SpriteRenderer>().color = Color.blue;
        }

        // debug for DOUBLE JUMP flag
        // green if
        //if (doubleJump)
        //    GetComponentInChildren<SpriteRenderer>().color = Color.green;
        // red if not
        //else
        //    GetComponentInChildren<SpriteRenderer>().color = Color.red;
    }

    public void Land(float h)
    {
        // fix position to the floor
        transform.position = new Vector3(transform.position.x, h);

        // 7/21/17
        // set ALL velocities to zero
        //acc = Vector3.zero;
        //vel = Vector3.zero;

        acc.y = 0.0f; // not moving, so acceleration is zero
        vel.y = 0.0f; // and velocity to zero
        up = false; // grounded...
        belmont = false; // ...so don't work belmont magic
        doubleJump = false; // disable double jumping too
        // this is a problem line
        // because this if statement is checked every frame
        // even when already grounded
        // but can't we just add an extra conditional??????

        // reset counter
        counter = 0;
        jumpCounter = 0;

        // new debugging shenanigans, black if grounded
        //GetComponentInChildren<SpriteRenderer>().color = Color.black;
    }

    void ProcessInput()
    {
        // toggle fastfall, belmont, and var
        if (Input.GetKeyDown(KeyCode.Alpha1))
            doFastfall = !doFastfall;
        if (Input.GetKeyDown(KeyCode.Alpha2))
            doBelmont = !doBelmont;
        if (Input.GetKeyDown(KeyCode.Alpha3))
            doVar = !doVar;
        if (Input.GetKeyDown(KeyCode.Alpha4))
            doDoubleJump = !doDoubleJump;

        if (!belmont)
            Walk(); // walk by applying a delta(v) in x, independent of y
        if (doFriction)
            ApplyFriction();
        Jump(); // jumping (this is the fun part)
    }

    void Toggle(KeyCode key, bool flag)
    {
        if (Input.GetKeyDown(key))
            flag = !flag;
    }

    void ApplyForce(Vector3 force)
    {
        // F = MA
        // A = M/F
        acc += force / mass;
    }

    void Walk()
    {
        dir = KeyCode.None;
        vel.x = 0;

        // walk left
        if (Input.GetKey(KeyCode.A))
        {
            // regular walking
            //transform.position = new Vector3(transform.position.x - walkSpeed, transform.position.y);
            vel.x = -walkSpeed;
            //vel.x = -walkSpeed;
            dir = KeyCode.A;
        }
        // walk right
        if (Input.GetKey(KeyCode.D))
        {
            //transform.position = new Vector3(transform.position.x + walkSpeed, transform.position.y);
            vel.x = walkSpeed;
            //vel.x = walkSpeed;
            dir = KeyCode.D;
        }
    }

    void Jump()
    {
        // start single jump
        if (Input.GetKeyDown(KeyCode.K) && !up)
        {
            // OLD
            // apply jumping force
            //ApplyForce(new Vector3(0f, initJumpV));

            // NEW
            // just change velocity directly and discontinuously ONCE
            //vel = new Vector3(vel.x, vel.y + initJumpV);
            vel.y += initJumpV;
            up = true;

            // get counter started
            ++counter;

            // start belmont
            if (doBelmont)
                belmont = true;

            // BAD, MOVE TO KEYUP
            /*// also start double jumping
            if (doDoubleJump)
            {
                doubleJump = true;
                
                // DEBUG
                Debug.Log("Now you can double jump!");
            }*/

            // increment jump counter once - it's the first jump!
            jumpCounter++;
        }

        // variable height jumping (only if flag = true)
        if (doVar)
        {
            // press and hold for longer jump for maxCounts number of frames
            if (Input.GetKey(KeyCode.K) && counter > 0 && counter < maxCounts)
            {
                ++counter;
                vel.y += initJumpV;// = new Vector3(vel.x, vel.y + initJumpV);
            }
            else
            {
                counter = 0;
                //belmont = false;
            }
        }
        
        // belmont magic - fix x velocity
        if (belmont)
        {
            if (dir == KeyCode.A)
            {
                //transform.position = new Vector3(transform.position.x - walkSpeed, transform.position.y);
                vel.x = -walkSpeed;
            }
            else if (dir == KeyCode.D)
            {
                //transform.position = new Vector3(transform.position.x + walkSpeed, transform.position.y);
                vel.x = walkSpeed;
            }
        }

        // NEW KEYUP IF STATEMENT
        if (up && Input.GetKeyUp(KeyCode.K) && doDoubleJump && !doubleJump)
        {
            doubleJump = true;

            // DEBUG
            //Debug.Log("Now you can double jump!");
        }
        /* HERE'S WHERE PROBLEMS ARE/WERE
         * this if statement is true on the first frame, we don't want that!
         * -
         * DESIRED BEHAVIOR
         * user presses K once, only one jump
         * regardless of whether held down or let go
         * after letting up, enable double jumping
         * NOW, if user pressees jump again, do the second jump
         * -
         * IMPLEMENTATION
         * keydown k => jump (first if statement)
         * don't change doubleJump here, even if canDoubleJump!!!!!
         * new if statemnt
         * keyup k and canDoubleJump => doubleJump = true
         * else the following if statement
        */
        // double jump
        else if (Input.GetKeyDown(KeyCode.K) && doubleJump && jumpCounter < maxJumps)
        {
            // set vel and acc to zero, it's as if jumping from ground
            vel.y = 0f;
            acc.y = 0f;

            // now get double jump going
            vel.y += initJumpV;

            // set double jump flag to false
            doubleJump = false;

            // reset counter for new jump
            counter = 1;
            jumpCounter++;
        }

        //Debug.Log("counter = " + jumpCounter);
    }

    void ApplyFriction()
    {
        // make sure all of the following are true
        // no directional buttons pressed
        // velocity is nonzero
        // AND grounded
        if(dir == KeyCode.None && vel.x != 0.0f && !up)
        {
            // then set temp equal to friction constant
            // and if velocity is positive, make temp = -temp
            // then apply force (directly or via apply force? => try both!)
            float fricTemp = friction;
            if (vel.x > 0.0f)
                fricTemp = -fricTemp;

            vel.x += fricTemp;
            
            // DEBUG
            //Debug.Log("fricTemp = " + fricTemp);
        }
    }
}
