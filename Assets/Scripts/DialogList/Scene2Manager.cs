using UnityEngine;
using System.Collections;

// SCENE 2 MANAGER - Orchestrator
// Flow:
//   1. Nara dialog:  "Alip! Bareng aku dong pulangnya."
//   2. Alip dialog:  "Eh sorry Nara, aku dijemput nyokap nih. Duluan ya!"
//   3. Alip jalan pergi ke kanan
//   4. Nara monolog: "Oh… oke deh." → "Ya udah. Sendirian lagi."

public class Scene2Manager : MonoBehaviour
{
    [Header("Characters")]
    public WorldSpaceDialog naraDialog;
    public WorldSpaceDialog alipDialog;
    public Rigidbody2D alipRigidbody;
    public Animator alipAnimator;

    [Header("Dialog Lines")]
    [TextArea(2, 4)]
    public string naraLine = "Alip! Bareng aku dong pulangnya.";
    [TextArea(2, 4)]
    public string alipLine = "Eh sorry Nara, aku dijemput nyokap nih. Duluan ya!";

    [Header("Monolog Lines")]
    [TextArea(2, 4)]
    public string monolog1 = "Oh… oke deh.";
    [TextArea(2, 4)]
    public string monolog2 = "Ya udah. Sendirian lagi.";

    [Header("Alip Exit Settings")]
    public float alipWalkSpeed = 3f;
    public float alipExitX = 20f;
    public string alipWalkAnim = "Walk";

    [Header("Facing Alip")]
    public FacingMode alipFacingMode = FacingMode.FlipXScale;
    public enum FacingMode { FlipSprite, FlipXScale, RotateY }
    public SpriteRenderer alipSpriteRenderer;
    public float alipRightRotationY = -270f;

    [Header("Timing")]
    public float delayBetweenDialogs = 0.5f;
    public float delayBeforeAlipExit = 0.3f;
    public float delayBeforeMonolog = 0.8f;

    [Header("Auto Trigger (Optional)")]
    public bool useColliderTrigger = false;

    private bool isRunning = false;
    private bool alipHasLeft = false;

    public void TriggerScene()
    {
        if (isRunning) return;
        StartCoroutine(RunScene());
    }

    IEnumerator RunScene()
    {
        isRunning = true;
        alipHasLeft = false;

        // 1. Nara dialog
        yield return StartCoroutine(PlayWorldDialog(naraDialog, naraLine));
        yield return new WaitForSeconds(delayBetweenDialogs);

        // 2. Alip dialog
        yield return StartCoroutine(PlayWorldDialog(alipDialog, alipLine));
        yield return new WaitForSeconds(delayBeforeAlipExit);

        // 3. Alip pergi
        SetAlipFacingRight(true);
        StartCoroutine(MoveAlipToExit());
        yield return new WaitUntil(() => alipHasLeft);
        yield return new WaitForSeconds(delayBeforeMonolog);

        // 4. Monolog Nara
        if (MonologSubtitle.Instance != null)
        {
            bool mono1Done = false;
            MonologSubtitle.Instance.Show(monolog1, () => mono1Done = true);
            yield return new WaitUntil(() => mono1Done);

            bool mono2Done = false;
            MonologSubtitle.Instance.Show(monolog2, () => mono2Done = true);
            yield return new WaitUntil(() => mono2Done);
        }
        else
        {
            Debug.LogWarning("[Scene2] MonologSubtitle tidak ditemukan di scene!");
        }

        isRunning = false;
        OnScene2Complete();
    }

    IEnumerator PlayWorldDialog(WorldSpaceDialog dialogComp, string line)
    {
        if (dialogComp == null) yield break;

        dialogComp.dialogLines = new string[] { line };
        dialogComp.ShowDialog();

        float duration = (line.Length * dialogComp.typingSpeed) + dialogComp.autoAdvanceDelay;
        yield return new WaitForSeconds(duration);

        dialogComp.HideDialog();
    }

    IEnumerator MoveAlipToExit()
    {
        if (alipRigidbody == null)
        {
            alipHasLeft = true;
            yield break;
        }
        if (alipAnimator != null)
            alipAnimator.Play(alipWalkAnim);

        while (alipRigidbody.transform.position.x < alipExitX)
        {
            alipRigidbody.linearVelocity = new Vector2(alipWalkSpeed, alipRigidbody.linearVelocity.y);
            yield return null;
        }

        alipRigidbody.linearVelocity = Vector2.zero;
        alipRigidbody.gameObject.SetActive(false);
        alipHasLeft = true;
    }

    void SetAlipFacingRight(bool facingRight)
    {
        switch (alipFacingMode)
        {
            case FacingMode.FlipSprite:
                if (alipSpriteRenderer != null)
                    alipSpriteRenderer.flipX = !facingRight;
                break;

            case FacingMode.FlipXScale:
                if (alipRigidbody != null)
                {
                    Vector3 scale = alipRigidbody.transform.localScale;
                    scale.x = facingRight ? Mathf.Abs(scale.x) : -Mathf.Abs(scale.x);
                    alipRigidbody.transform.localScale = scale;
                }
                break;

            case FacingMode.RotateY:
                if (alipRigidbody != null)
                {
                    Vector3 rot = alipRigidbody.transform.eulerAngles;
                    rot.y = facingRight ? alipRightRotationY : (alipRightRotationY + 180f);
                    alipRigidbody.transform.eulerAngles = rot;
                }
                break;
        }
    }

    void OnScene2Complete()
    {
        Debug.Log("[Scene2] Selesai. Tambah logic lo di sini.");
        // Contoh: trigger mental bar, enable player movement, dll
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!useColliderTrigger) return;
        if (other.CompareTag("Player"))
            TriggerScene();
    }
}