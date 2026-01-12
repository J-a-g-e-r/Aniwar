using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GemExplodeVFX : MonoBehaviour
{
    private ParticleSystem ps;
    private void Awake()
    {
        ps = GetComponent<ParticleSystem>();
    }

    public void PlayVFX(GemColor color)
    {
        var main = ps.main;
        switch (color)
        {
            case GemColor.Red:
                main.startColor = Color.red;
                break;
            case GemColor.Blue:
                main.startColor = Color.blue;
                break;
            case GemColor.Yellow:
                main.startColor = Color.yellow;
                break;
            case GemColor.Purple:
                main.startColor = new Color(0.5f, 0f, 0.5f); // Purple color
                break;
            case GemColor.Green:
                main.startColor = Color.green;
                break;
            case GemColor.Orange:
                main.startColor = new Color(1f, 0.5f, 0f); // Orange color
                break;
            default:
                main.startColor = Color.white;
                break;
        }
        ps.Play();
        StartCoroutine(ReturnToPoolAfterDelay());
    }

    private IEnumerator ReturnToPoolAfterDelay()
    {
        yield return new WaitForSeconds(ps.main.duration);
        ObjectPooler.Instance.ReturnObject("GemExplodeVFX", this.gameObject);
    }
}

