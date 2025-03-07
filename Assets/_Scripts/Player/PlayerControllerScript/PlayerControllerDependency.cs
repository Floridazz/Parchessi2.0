﻿using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(PlayerController))]
public class PlayerControllerDependency : NetworkBehaviour
{
    protected PlayerController PlayerController;

    public virtual void Awake()
    {
        PlayerController = GetComponent<PlayerController>();
    }
}
