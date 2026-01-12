using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Unity.Mathematics.math;

public class GameManager : MonoBehaviour
{
    public bool IsPaused { get; private set; }
    public void PauseGame() => IsPaused = true;
    public void ResumeGame() => IsPaused = false;

}
