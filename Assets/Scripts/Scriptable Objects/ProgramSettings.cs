using UnityEngine;

[CreateAssetMenu(fileName = "ProgramSettings", menuName = "Datas/Program Settings", order = 50)]
public class ProgramSettings : ScriptableObject
{
    #region Physics
    /**********************************
     *********     FIELDS     *********
     *********************************/

    /// <summary>Backing field for <see cref="GroundDecelerationForce"/>.</summary>
    [SerializeField]
    private float       groundDecelerationForce =     .2f;

    /// <summary>Backing field for <see cref="GravityMinYForce"/>.</summary>
    [SerializeField]
    private float       gravityMinYForce =              -25;


    /**********************************
     *******     PROPERTIES     *******
     *********************************/

    /// <summary>
    /// Object deceleration coefficient applied to velocity force
    /// when grounded.
    /// </summary>
    public float        GroundDecelerationForce { get { return groundDecelerationForce; } }

    /// <summary>
    /// Minimal Y value of the force applied on objects
    /// for physics gravity simulation.
    /// </summary>
    public float        GravityMinYForce        { get { return gravityMinYForce; } }
    #endregion
}
