using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public void PressUp(InputAction.CallbackContext context)
    {
        if (!context.started) return;
        if (GameManager.Instance.gameState != GameState.Playing) return;
        
        NoteManager.Instance.CheckTiming(NoteType.Up);
        NoteManager.Instance.TriggerPlayerAnimation(NoteType.Up);
    }
    public void PressDown(InputAction.CallbackContext context)
    {
        if (!context.started) return;
        if (GameManager.Instance.gameState != GameState.Playing) return;
        
        NoteManager.Instance.CheckTiming(NoteType.Down);
        NoteManager.Instance.TriggerPlayerAnimation(NoteType.Down);
    }
    public void PressLeft(InputAction.CallbackContext context)
    {
        if (!context.started) return;
        if (GameManager.Instance.gameState != GameState.Playing) return;
        
        NoteManager.Instance.CheckTiming(NoteType.Left);
        NoteManager.Instance.TriggerPlayerAnimation(NoteType.Left);
    }
    public void PressRight(InputAction.CallbackContext context)
    {
        if (!context.started) return;
        if (GameManager.Instance.gameState != GameState.Playing) return;
        
        NoteManager.Instance.CheckTiming(NoteType.Right);
        NoteManager.Instance.TriggerPlayerAnimation(NoteType.Right);
    }
    public void TutorialProceed(InputAction.CallbackContext context)
    {
        if (!context.started) return;
        
        NoteManager.Instance.ProceedTutorial();
    }
}
