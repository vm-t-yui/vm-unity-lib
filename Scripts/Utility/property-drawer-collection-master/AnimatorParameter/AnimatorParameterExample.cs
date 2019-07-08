using UnityEngine;

[RequireComponent(typeof(Animator))]
public class AnimatorParameterExample : MonoBehaviour
{
    [AnimatorParameter]
    public string param;
    [AnimatorParameter(AnimatorParameterAttribute.ParameterType.Float)]
    public string floatParam;
    [AnimatorParameter(AnimatorParameterAttribute.ParameterType.Int)]
    public string intParam;
    [AnimatorParameter(AnimatorParameterAttribute.ParameterType.Bool)]
    public string boolParam;
    [AnimatorParameter(AnimatorParameterAttribute.ParameterType.Trigger)]
    public string triggerParam;


    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        animator.GetFloat(floatParam);

        animator.GetInteger(intParam);

        animator.GetBool(boolParam);

        animator.SetTrigger(triggerParam);
    }
}
