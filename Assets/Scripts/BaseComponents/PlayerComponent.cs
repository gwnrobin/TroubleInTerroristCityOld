﻿using Unity.Netcode;
using UnityEngine;

public abstract class PlayerComponent : NetworkBehaviour
{
    /// <summary></summary>
    public Player Player
    {
        get
        {
            if (!m_Player)
                m_Player = GetComponent<Player>();
            if (!m_Player)
                m_Player = GetComponentInParent<Player>();

            return m_Player;
        }
    }

    private Player m_Player;
}