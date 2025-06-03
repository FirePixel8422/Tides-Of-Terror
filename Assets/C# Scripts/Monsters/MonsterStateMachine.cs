using System.Collections;
using UnityEngine;


[RequireComponent(typeof(Animator))]
public class MonsterStateMachine : MonoBehaviour
{
    private Animator anim;


    #region animationStrings

    [Header("Start Animation")]
    [SerializeField] private string currentAnimation = "Idle";


    [Header("Animation Names")]

    [SerializeField] private string idleAnimation = "Idle";
    [SerializeField] private string moveAnimation = "Move";

    [SerializeField] private string[] attackAnimations = new string[] { "Attack1", "Attack2", "Attack3" };

    [SerializeField] private string hurtAnimation = "Hurt";
    [SerializeField] private string deathAnimation = "Death";

    #endregion


    [SerializeField] private bool dead;

    


    private void Start()
    {
        anim = GetComponent<Animator>();
    }


    /// <returns>true if the animation has changed, false otherwise</returns>
    private bool TryTransitionAnimation(string animationString, float transitionDuration = 0.25f, float speed = 1, int layer = 0)
    {
        //if the new animation is the same as current, return false
        if (currentAnimation == animationString) return false;

        currentAnimation = animationString;

        anim.speed = speed;
        anim.CrossFade(animationString, transitionDuration, layer);

        return true;
    }



    public void Idle()
    {
        if (dead) return;

        TryTransitionAnimation(idleAnimation);
    }

    public void Run()
    {
        if (dead) return;

        TryTransitionAnimation(moveAnimation);
    }

    public void Attack(int attackId)
    {
        if (dead) return;

        TryTransitionAnimation(attackAnimations[attackId]);
    }

    public void GetHurt()
    {
        if (dead) return;

        string cAnim = currentAnimation;

        TryTransitionAnimation(hurtAnimation);

        StopAllCoroutines();
        StartCoroutine(AutoTransition(cAnim));
    }

    public void Die()
    {
        dead = true;

        TryTransitionAnimation(deathAnimation);
    }



    private IEnumerator AutoTransition(string animationString)
    {
        float clipTime = anim.GetCurrentAnimatorStateInfo(0).length;

        yield return new WaitForSeconds(clipTime);

        TryTransitionAnimation(animationString);
    }
}