using UnityEngine;

public static class AnimatorStates
{
    public static int Idle = Animator.StringToHash("Base Layer.Idle");
    public static int Confirming = Animator.StringToHash("Base Layer.Confirming");
    public static int Clicked = Animator.StringToHash("Base Layer.Clicked");
    public static int Opened = Animator.StringToHash("Base Layer.Opened");
    public static int Opening = Animator.StringToHash("Base Layer.Opening");
    public static int Closed = Animator.StringToHash("Base Layer.Closed");
    public static int Closing = Animator.StringToHash("Base Layer.Closing");
    public static int FadedIn = Animator.StringToHash("Base Layer.FadedIn");
    public static int FadingOut = Animator.StringToHash("Base Layer.FadingOut");
    public static int FadedOut = Animator.StringToHash("Base Layer.FadedOut");
    public static int FadingIn = Animator.StringToHash("Base Layer.FadingIn");
}
