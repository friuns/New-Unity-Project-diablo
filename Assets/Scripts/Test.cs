using System;
using UnityEngine;
using System.Collections;

public class Test : Bs
{
 public void Update()
 {
     GetComponent<NavMeshAgent>().SetDestination(_Player.pos);
 }
}
