﻿using System;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;
using LiteNetworking;
using UnityStandardAssets.Characters.ThirdPerson;

public class ThirdPersonPeriodicUpdate : LitePacket
{
    public NetworkedEntity thisEntity;
    public Vector3 m_Move;
    [LevelStreaming.Position]
    public Vector3 position;
    public bool crouch;
    public bool m_Jump;

    public override void Execute()
    {
        (thisEntity as LitePlayer).GetComponent<ThirdPersonUserControl>().ExternalUpdate(m_Move, crouch, m_Jump, position);
    }
}


[RequireComponent(typeof(ThirdPersonCharacter))]
public class ThirdPersonUserControl : MonoBehaviour
{
    private ThirdPersonCharacter m_Character; // A reference to the ThirdPersonCharacter on the object
    private Transform m_Cam;                  // A reference to the main camera in the scenes transform
    private Vector3 m_CamForward;             // The current forward direction of the camera
    private Vector3 m_Move;
    private bool m_Jump;                      // the world-relative desired move direction, calculated from the camForward and user input.


    private void Start()
    {
        // get the transform of the main camera
        if (Camera.main != null)
        {
            m_Cam = Camera.main.transform;
        }
        else
        {
            Debug.LogWarning(
                "Warning: no main camera found. Third person character needs a Camera tagged \"MainCamera\", for camera-relative controls.", gameObject);
            // we use self-relative controls in this case, which probably isn't what the user wants, but hey, we warned them!
        }

        // get the third person character ( this should never be null due to require component )
        m_Character = GetComponent<ThirdPersonCharacter>();
    }


    private void Update()
    {
        if (!m_Jump && Networking.HasLocalAuthority(gameObject))
        {
            m_Jump = CrossPlatformInputManager.GetButtonDown("Jump");
        }
    }


    // Fixed update is called in sync with physics
    private void FixedUpdate()
    {
        if(Networking.HasLocalAuthority(gameObject))
        {
            // read inputs
            float h = CrossPlatformInputManager.GetAxis("Horizontal");
            float v = CrossPlatformInputManager.GetAxis("Vertical");
            bool crouch = Input.GetKey(KeyCode.C);

            // calculate move direction to pass to character
            if (m_Cam != null)
            {
                // calculate camera relative direction to move:
                m_CamForward = Vector3.Scale(m_Cam.forward, new Vector3(1, 0, 1)).normalized;
                m_Move = v * m_CamForward + h * m_Cam.right;
            }
            else
            {
                // we use world-relative directions in the case of no main camera
                m_Move = v * Vector3.forward + h * Vector3.right;
            }
#if !MOBILE_INPUT
            // walk speed multiplier
            if (Input.GetKey(KeyCode.LeftShift)) m_Move *= 0.5f;
#endif

            // pass all parameters to the character control script
            m_Character.Move(m_Move, crouch, m_Jump);

            ThirdPersonPeriodicUpdate update = new ThirdPersonPeriodicUpdate();
            update.m_Jump = m_Jump;
            update.m_Move = m_Move;
            update.thisEntity = GetComponent<NetworkedEntity>();
            update.position = transform.position;
            update.crouch = crouch;
            LiteNetworkingGenerated.PacketSender.SendThirdPersonPeriodicUpdate(update);
            
            m_Jump = false;
        }
    }

    public void ExternalUpdate(Vector3 m_Move, bool crouch, bool m_Jump, Vector3 position)
    {
        transform.position = position;
        m_Character.Move(m_Move, crouch, m_Jump);
    }
}