using System.Collections;
using System.Collections.Generic;
using Gameplay;
using UnityEngine;
using UnityEngine.UI;

public class UIHider : MonoBehaviour
{
    [SerializeField] private Transform targetUI;

    private float shiftingHeight;
    private bool isHidden;
    
    // Start is called before the first frame update
    void Start()
    {
        shiftingHeight = GetComponent<RectTransform>().rect.height*.85f;
        HideHide();
    }

    public void ShowHide()
    {
        if (!isHidden) return;
        isHidden = false;
        targetUI.transform.Translate(new Vector2(0, -shiftingHeight));
        StopCoroutine(CheckArchiveHide());
    }

    public void HideHide()
    {
        if (isHidden) return;
        isHidden = true;
        targetUI.transform.Translate(new Vector2(0, shiftingHeight));
        StartCoroutine(CheckArchiveHide());
    }

    IEnumerator CheckArchiveHide()
    {
        yield return new WaitForSeconds(.1f);
        if (UIManager.Instance.archiveOpen && isHidden)
        {
            UIManager.Instance.ToggleArchive();
        }
    }
}
