using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyColumnVFX : MonoBehaviour
{
    private void Start()
    {
        StartCoroutine(ReturnToPoolAfterDelay());
    }

    private IEnumerator ReturnToPoolAfterDelay()
    {
        yield return new WaitForSeconds(1f);
        ObjectPooler.Instance.ReturnObject("DestroyColumnVFX", this.gameObject);
    }
}
