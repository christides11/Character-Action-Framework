﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TDAction.Entities
{
    public class EntityState : CAF.Entities.EntityState
    {
        public virtual EntityController GetEntityController()
        {
            return (EntityController)Controller;
        }
    }
}