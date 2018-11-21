using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapWalker : MonoBehaviour
{
    private void Update()
    {
        GameSystem.MapSystem.SetCurrentAngle(GameSystem.MapSystem.GetAngle(transform.position));
    }
}
