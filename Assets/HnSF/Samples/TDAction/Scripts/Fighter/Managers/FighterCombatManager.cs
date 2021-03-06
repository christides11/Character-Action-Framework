﻿using HnSF.Combat;
using System.Collections;
using System.Collections.Generic;
using TDAction.Combat;
using TDAction.Inputs;
using UnityEngine;
using HitInfo = TDAction.Combat.HitInfo;

namespace TDAction.Fighter
{
    public class FighterCombatManager : HnSF.Fighters.FighterCombatManager
    {
        public override HnSF.Combat.MovesetDefinition CurrentMoveset { get { return (manager as FighterManager).entityDefinition.movesets[currentMoveset]; } }

        public TeamTypes team = TeamTypes.FFA;

        public override int GetTeam()
        {
            return (int)team; 
        }

        public override int GetMovesetCount()
        {
            return (manager as FighterManager).entityDefinition.movesets.Count;
        }

        public override HnSF.Combat.MovesetDefinition GetMoveset(int index)
        {
            return (manager as FighterManager).entityDefinition.movesets[index];
        }

        public float pForcePercentage = 1.0f;
        public override HitReactionBase Hurt(HurtInfoBase hurtInfoBase)
        {
            FighterPhysicsManager physicsManager = (FighterPhysicsManager)manager.PhysicsManager;

            HurtInfo2D hurtInfo2D = (HurtInfo2D)hurtInfoBase;
            HitInfo hInfo = hurtInfo2D.hitInfo as HitInfo;

            // Check if the box can hit this entity.
            if(hInfo.groundOnly && !physicsManager.IsGrounded
                || hInfo.airOnly && !physicsManager.IsGrounded)
            {
                return new HitReactionBase();
            }
            // Got hit, apply stun, damage, and forces.
            SetHitStop(hInfo.hitstop);
            SetHitStun(hInfo.hitstun);

            // Convert forces the attacker-based forward direction.
            switch (hInfo.forceType)
            {
                case HitboxForceType.SET:
                    Vector2 baseForce = hInfo.opponentForce;
                    Vector3 forces = new Vector3(baseForce.x * hurtInfo2D.faceDirection, 0, 0);
                    physicsManager.forceGravity.y = baseForce.y;
                    physicsManager.forceMovement = forces;
                    break;
                case HitboxForceType.PULL:
                    Vector2 dir = (Vector2)transform.position - hurtInfo2D.center;
                    if (!hInfo.forceIncludeYForce)
                    {
                        dir.y = 0;
                    }
                    Vector2 forceDir = Vector2.ClampMagnitude((dir) * hInfo.opponentForceMultiplier, hInfo.opponentMaxMagnitude);
                    float yForce = forceDir.y;
                    forceDir.y = 0;
                    if (hInfo.forceIncludeYForce)
                    {
                        physicsManager.forceGravity.y = yForce;
                    }
                    physicsManager.forceMovement = forceDir;
                    break;
            }

            if (hInfo.autoLink)
            {
                physicsManager.forceGravity.y += hurtInfo2D.attackerVelocity.y * pForcePercentage;
                physicsManager.forceMovement.x += hurtInfo2D.attackerVelocity.x * pForcePercentage;
            }

            if (physicsManager.forceGravity.y > 0)
            {
                physicsManager.SetGrounded(false);
            }

            ((FighterManager)manager).healthManager.Hurt(hInfo.damageOnHit);

            // Change into the correct state.
            if (hInfo.groundBounces && physicsManager.IsGrounded)
            {
                //manager.StateManager.ChangeState((int)EntityStates);
            }else if (hInfo.causesTumble)
            {
                manager.StateManager.ChangeState((int)FighterStates.TUMBLE);
            }
            else
            {
                manager.StateManager.ChangeState((ushort)(physicsManager.IsGrounded ? FighterStates.FLINCH_GROUND : FighterStates.FLINCH_AIR));
            }
            return new HitReactionBase();
        }

        protected override bool CheckStickDirection(HnSF.Input.InputDefinition sequenceInput, uint framesBack)
        {
            Vector2 stickDir = manager.InputManager.GetAxis2D((int)EntityInputs.MOVEMENT, framesBack);
            if (stickDir.magnitude < 0.2f)
            {
                return false;
            }

            if (Vector2.Dot(stickDir, sequenceInput.stickDirection) >= sequenceInput.directionDeviation)
            {
                return true;
            }
            return false;
        }
    }
}