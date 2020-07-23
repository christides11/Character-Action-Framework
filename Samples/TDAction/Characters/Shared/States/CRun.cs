﻿using System.Collections;
using System.Collections.Generic;
using TDAction.Inputs;
using UnityEngine;

namespace TDAction.Entities.Characters
{
    public class CRun : CharacterState
    {
        public override string GetName()
        {
            return "Run";
        }

        public override void OnUpdate()
        {
            CharacterManager c = GetCharacterController();
            CharacterStats stats = (CharacterStats)c.entityDefinition.GetEntityStats();
            Vector2 movementAxis = c.InputManager.GetAxis2D((int)EntityInputs.MOVEMENT);
            float maxSpeed = (stats.runMaxSpeed * movementAxis.x);

            Vector3 currentSpeed = c.PhysicsManager.forceMovement;

            if (Mathf.Abs(c.PhysicsManager.forceMovement.x) > Mathf.Abs(maxSpeed))
            {
                c.PhysicsManager.ApplyMovementFriction(stats.groundFriction);
            }
            else
            {
                float tempAcc = ((stats.runAcceleration * Mathf.Abs(movementAxis.x)) + stats.runBaseAcceleration)
                    * c.FaceDirection;

                currentSpeed.x += tempAcc;
                if (currentSpeed.x * c.FaceDirection > maxSpeed * c.FaceDirection)
                {
                    currentSpeed.x = maxSpeed;
                }
            }

            c.PhysicsManager.forceMovement = currentSpeed;

            c.SetFaceDirection((int)Mathf.Sign(movementAxis.x));

            CheckInterrupt();
        }

        public override bool CheckInterrupt()
        {
            CharacterManager c = GetCharacterController();
            if (c.InputManager.GetButton((int)EntityInputs.JUMP).firstPress)
            {
                c.StateManager.ChangeState((int)CharacterStates.JUMP_SQUAT);
                return true;
            }
            if (Mathf.Abs(c.InputManager.GetAxis2D((int)EntityInputs.MOVEMENT).x) <= InputConstants.moveDeadzone)
            {
                c.StateManager.ChangeState((int)CharacterStates.IDLE);
                return true;
            }
            return false;
        }
    }
}