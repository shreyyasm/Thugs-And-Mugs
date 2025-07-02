using UnityEngine;

public class SuperStiffRagdoll : MonoBehaviour
{
    [Header("Joint Settings")]
    public float stiffness = 10000f;
    public float damping = 1000f;
    public float twistLimit = 2f;
    public float swingLimit = 2f;

    void Start()
    {
        CharacterJoint[] joints = GetComponentsInChildren<CharacterJoint>();

        foreach (CharacterJoint joint in joints)
        {
            // Apply twist spring
            SoftJointLimitSpring twistSpring = joint.twistLimitSpring;
            twistSpring.spring = stiffness;
            twistSpring.damper = damping;
            joint.twistLimitSpring = twistSpring;

            // Apply swing spring
            SoftJointLimitSpring swingSpring = joint.swingLimitSpring;
            swingSpring.spring = stiffness;
            swingSpring.damper = damping;
            joint.swingLimitSpring = swingSpring;

            // Lock twist limits
            SoftJointLimit lowTwist = joint.lowTwistLimit;
            lowTwist.limit = -twistLimit;
            joint.lowTwistLimit = lowTwist;

            SoftJointLimit highTwist = joint.highTwistLimit;
            highTwist.limit = twistLimit;
            joint.highTwistLimit = highTwist;

            // Lock swing limits
            SoftJointLimit swing1 = joint.swing1Limit;
            swing1.limit = swingLimit;
            joint.swing1Limit = swing1;

            SoftJointLimit swing2 = joint.swing2Limit;
            swing2.limit = swingLimit;
            joint.swing2Limit = swing2;
        }
    }
}
