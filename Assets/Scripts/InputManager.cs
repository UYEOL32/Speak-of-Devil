using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public void PressUp(InputAction.CallbackContext context)
    {
        if (!context.started) return;
        
        NoteManager.Instance.CheckTiming(NoteType.Up);
    }
    public void PressDown(InputAction.CallbackContext context)
    {
        if (!context.started) return;
        
        NoteManager.Instance.CheckTiming(NoteType.Down);
    }
    public void PressLeft(InputAction.CallbackContext context)
    {
        if (!context.started) return;
        
        NoteManager.Instance.CheckTiming(NoteType.Left);
    }
    public void PressRight(InputAction.CallbackContext context)
    {
        if (!context.started) return;
        
        NoteManager.Instance.CheckTiming(NoteType.Right);
    }
    public void TutorialProceed(InputAction.CallbackContext context)
    {
        if (!context.started) return;
        
        NoteManager.Instance.ProceedTutorial();
    }
}
