using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Platforming : MonoBehaviour
{
    // our guy's hitbox
    private Bounds hitbox;
    
    // array (or list?) to hold all platforms
    private GameObject[] platforms;
    private Bounds[] boundaries;

    // we need to keep track of the current platform
    // in order for the player to fall off of it
    // this makes sense because the player can only
    // be standing on EXACTLY ONE platform
    private Bounds currPlatform;

    private Movement movement;

    // 8/27/17
    // in order for side collisions to work properly
    // we may need to keep track of the position on
    // the previous frame
    private Vector3 prevPos;

    //private int landTimer;
    //public int landDuration;

    // 9/2/17
    // we need a constant value for sqrt(2)
    private float halfroot2;

	// Use this for initialization
	void Start ()
    {
        // 9/2/17 setting val for root2
        halfroot2 = Mathf.Sqrt(2f) / 2;

        // 9/2/17
        // debug for dot product
        Vector3 myUpVector = new Vector3(0f, 1f);
        myUpVector.Normalize();

        Vector3 myHalfAngleVector = new Vector3(1f, 1f);
        myHalfAngleVector.Normalize();

        float myDotProduct = Vector3.Dot(myHalfAngleVector, myUpVector);
        Debug.Log
        (
            "(" + myUpVector.x + ", " + myUpVector.y + ") dotted with (" +
            myHalfAngleVector.x + ", " + myHalfAngleVector.y + ") = " +
            myDotProduct
        );

        Debug.Log(halfroot2);

        string message = "NOOOOOOOO wtf";
        if (halfroot2 == myDotProduct)
            message = "awwww sick";
        Debug.Log(message);

        // back to old stuff
        prevPos = transform.position;
        
        // assume midair, so not landed yet
        //landTimer = 0;

        // get movement script and hitbox
        movement = gameObject.GetComponent<Movement>();
        hitbox = gameObject.GetComponent<BoxCollider2D>().bounds;

		// populate platforms array
        platforms = GameObject.FindGameObjectsWithTag("Platform");
        boundaries = new Bounds[platforms.Length];

        for (int i = 0; i < platforms.Length; ++i)
        {
            // get all the bounds structs from each platform's AABB collider
            boundaries[i] = platforms[i].GetComponent<Collider2D>().bounds;

            // DEBUG
            Debug.Log("Index " + i + " is " + platforms[i].name +
                "\r\nMin X = " + boundaries[i].min.x +
                "\r\nMax X = " + boundaries[i].max.x +
                "\r\nMin Y = " + boundaries[i].min.y +
                "\r\nMax Y = " + boundaries[i].max.y);
        }
        // DEBUG
        Debug.Log("Player" +
            "\r\nMin X = " + hitbox.min.x +
            "\r\nMax X = " + hitbox.max.x +
            "\r\nMin Y = " + hitbox.min.y +
            "\r\nMax Y = " + hitbox.max.y);

        Debug.Log("Player pos " +
            "\r\nX = " + transform.position.x +
            "\r\nY = " + transform.position.y);

        // now that platforms array has been populated, just
        // initialize the current platform as null

        // now that currPlatform is a Bounds, this line 
        // generates compiler errors, so I just commented
        // it out
        //currPlatform = null;
        currPlatform = new Bounds(new Vector3(float.MinValue, float.MinValue), new Vector3(float.MinValue, float.MinValue));
        //Debug.Log("currPlatform = " + currPlatform);
	}
	
	// Update is called once per frame
	void Update ()
    {
        // test time simulation
        Debug.Log("time => " + Time.timeScale);

        if (movement.up)
            CheckAllPlatforms();
        else
            CheckCurrPlatform();

        // get hitbox on every frame
        hitbox = gameObject.GetComponent<BoxCollider2D>().bounds;

        // DEBUG
        //Debug.Log("Player" +
        //    "\r\nMin X = " + hitbox.min.x +
        //    "\r\nMax X = " + hitbox.max.x +
        //    "\r\nMin Y = " + hitbox.min.y +
        //    "\r\nMax Y = " + hitbox.max.y);
	}

    void CheckAllPlatforms()
    {
        foreach (Bounds b in boundaries)
        {
        //    // 8/5/17
        //    // new way, hopefully the good way
        //    // check sides, and only do other 2
        //    // checks if not hitting sides
        //    if (HittingSides(b))
        //    {
        //        Debug.Log("hitting sides");
        //    }
        //    else
        //    {
        //        CheckTop(b);
        //        CheckBottom(b);
        //    }

            // old way - do all 3 checks every time
            CheckTop(b);
            CheckSides(b);
            CheckBottom(b);
        }
    }

    void CheckTop(Bounds b)
    {
        if
            (
                // old version with position
                transform.position.x >= b.min.x &&
                transform.position.x <= b.max.x &&
                transform.position.y <= b.max.y &&
                transform.position.y >= b.min.y //&&
                //movement.vel.y < 0

                // new version with hitbox
                //hitbox.min.x <= b.max.x &&
                //hitbox.max.x >= b.min.x &&
                //transform.position.y /*hitbox.min.y*/ <= b.max.y
                //&& transform.position.y >= b.min.y
                //&&hitbox.max.y >= b.min.y
            )
        {
            // top edge - landing!
            if (movement.vel.y <= 0)
            {
                movement.Land(b.max.y);
                // DEBUG
                //Debug.Log("Hey, land! and current platform = " + b.ToString());
                // 7/11/17 - NOW, SET CURRENT PLATFORM HERE
                currPlatform = b;
            }
            /*
                // bottom edge
            else if (movement.vel.y >= 0)
            {
                Debug.Log("bottom edge");
            }
            //*/
        }
        /*
            // let's try writing the logic for having the guy fall off
        // the conditions for it to fall off are that
        // A. it's already grounded
        // B. it walks off the left or right edge
        // 7/11/17 - MOVE THIS OUT OF THE FOREACH LOOP
        // BUT STILL CHECK EVERY FRAME
        else if (!movement.up &&
            (transform.position.x < b.min.x ||
                transform.position.x > b.max.x))
        {
            //Debug.Log("You should fall off now");
            //break;
            // now that the conditions are correct for falling off,
            // set up = false!
            //movement.up = false;
        }
        //*/
    }

    void CheckSides(Bounds b)
    {
        //// left and right sides
        //if 
        //(
        //    (
        //        hitbox.max.x >= b.min.x ||
        //        hitbox.min.x <= b.max.x
        //    ) &&
        //    hitbox.min.y <= b.max.y &&
        //    hitbox.max.y >= b.min.y &&
        //    movement.vel.x != 0 &&
        //    !movement.up
        //)
        //{
        //    movement.vel.x = 0;
        //    Debug.Log("You're hitting the sides");
        //}


        // 8/27/17
        // if, on previous frame, player was not inside,
        // and it IS inside on current frame,...
        // then, pop player out from side
        if
        (
                (
                    prevPos.x <= b.min.x ||
                    prevPos.x >= b.max.x
                )
        )
        {
        }
    }

    bool HittingSides(Bounds b)
    {
        bool hitting = false;
        // left and right sides
        if 
        (
            (
                hitbox.max.x >= b.min.x ||
                hitbox.min.x <= b.max.x
            ) &&
            hitbox.min.y <= b.max.y &&
            hitbox.max.y >= b.min.y &&
            movement.vel.x != 0
            &&movement.up
        )
        {
            movement.vel.x = 0;
            hitting = true;
            Debug.Log("You're hitting the sides");
        }
        return hitting;
    }

    void CheckBottom(Bounds b)
    {
        if
        (
            hitbox.max.x >= b.min.x &&
            hitbox.min.x <= b.max.x &&
            hitbox.max.y <= b.max.y &&
            hitbox.max.y >= b.min.y
            &&movement.vel.y >= 0
        )
        {
            movement.vel.y = 0;//-movement.vel.y;
            //Debug.Log("you're hitting the bottom");
        }
    }

    void CheckCurrPlatform()
    {
        if
            (
                currPlatform.center.x != float.MinValue &&
                !movement.up &&
                (
                    transform.position.x < currPlatform.min.x ||
                    transform.position.x > currPlatform.max.x
                )
            )
        {
            //Debug.Log("You should fall off now");
            //break;
            // now that the conditions are correct for falling off,
            // set up = false!
            movement.up = true;
        }
    }
}
