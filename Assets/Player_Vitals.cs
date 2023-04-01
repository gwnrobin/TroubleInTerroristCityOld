using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_Vitals : EntityComponent
{
    void Start()
    {
        Entity.Health.AddChangeListener(Heal);
    }

    private void Heal(float heal)
    {
        print("sike");
    }
}
