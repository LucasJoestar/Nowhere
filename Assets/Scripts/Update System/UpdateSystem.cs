using UnityEngine;

[CreateAssetMenu(fileName = "UpdateSystem", menuName = "Datas/Update System", order = 50)]
public class UpdateSystem : ScriptableObject
{
    #region Fields / Properties
    /**********************************
     *********     FIELDS     *********
     *********************************/

    /// <summary>Backing field for <see cref="UpdateModes"/>.</summary>
    [SerializeField]
    private UpdateMode[] updateModes = new UpdateMode[] { };


    /**********************************
     *******     PROPERTIES     *******
     *********************************/

    /// <summary>
    /// All update modes used within this system.
    /// </summary>
    public UpdateMode[] UpdateModes { get { return updateModes; } }
    #endregion
}
