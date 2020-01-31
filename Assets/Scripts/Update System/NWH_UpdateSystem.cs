using System;
using System.Collections;
using UnityEngine;

public class NWH_UpdateSystem : MonoBehaviour
{
    #region Events
    /**********************************
     *********     EVENTS     *********
     *********************************/

    /// <summary>
    /// Called every frame for frames update modes.
    /// </summary>
    private event Action OnFrameUpdate;

    /// <summary>
    /// Called every frame with <see cref="Time.deltaTime"/> as parameter
    /// for seconds update modes.
    /// </summary>
    private event Action<float> OnTimeUpdate;
    #endregion

    #region Singleton
    /*********************************
     *******     SINGLETON     *******
     ********************************/

    /// <summary>
    /// Singleton instance of this class.
    /// </summary>
    public static NWH_UpdateSystem I = null;
    #endregion

    #region Methods

    #region Original Methods
    /*********************************************
     *****     RUNTIME INSTANCE CREATION     *****
     ********************************************/

    /// <summary>
    /// Create an instance of this class and set it as singleton.
    /// Should be called on game load.
    /// </summary>
    public static void CreateInstance(NWH_UpdateMode[] _updateModes)
    {
        // Create a new GameObject and add an UpdateSystem component to it,
        // which is set as singleton and not destroyed on load
        if (I) return;

        I = (NWH_UpdateSystem)new GameObject("[GAME LOGIC] Update System").AddComponent(typeof(NWH_UpdateSystem));

        DontDestroyOnLoad(I);

        // In the original array order,
        // subscribe each update mode IncreaseTimer method to the correct event
        for (int _i = 0; _i < _updateModes.Length; _i++)
        {
            if (_updateModes[_i].IsFrameInterval) I.OnFrameUpdate += _updateModes[_i].IncreaseTimer;
            else I.OnTimeUpdate += _updateModes[_i].IncreaseTimer;
        }
    }


    /***********************************
     ******     UPDATE SYSTEM     ******
     **********************************/

    /// <summary>
    /// Coroutine managing all the game update system.
    /// </summary>
    /// <returns>IEnumerator, baby.</returns>
    private IEnumerator DoTheUpdate()
    {
        // Until application exit, do the update every frame
        while (true)
        {
            yield return null;

            // Update all update modes by calling events
            OnFrameUpdate?.Invoke();
            OnTimeUpdate?.Invoke(Time.deltaTime);
        }
    }
    #endregion

    #region Unity Methods
    /*********************************
     *****     MONOBEHAVIOUR     *****
     ********************************/

    // Start is called before the first frame update
    private void Start()
    {
        // Destroy object if not singleton
        if (I != this)
        {
            Destroy(this);
            return;
        }

        // Start the update system
        StartCoroutine(DoTheUpdate());
    }
    #endregion

    #endregion
}

[Serializable]
public class NWH_UpdateOrder
{
    /**********************************
     *********     FIELDS     *********
     *********************************/

    /// <summary>Backing field for <see cref="UpdateModes"/>.</summary>
    [SerializeField]
    private NWH_UpdateMode[]    updateModes =   new NWH_UpdateMode[] { };


    /**********************************
     *******     PROPERTIES     *******
     *********************************/

    /// <summary>
    /// All update modes used within this order.
    /// </summary>
    public NWH_UpdateMode[]     UpdateModes { get { return updateModes; } }
}

[Serializable]
public class NWH_UpdateMode
{
    #region Events
    /**********************************
     *********     EVENTS     *********
     *********************************/

    /// <summary>
    /// Event called on this class update.
    /// </summary>
    public event Action OnUpdate = null;
    #endregion

    #region Fields / Properties
    /**********************************
     *********     FIELDS     *********
     *********************************/

    /// <summary>Backing field for <see cref="IsFrameInterval"/>.</summary>
    [SerializeField]
    private bool        isFrameInterval =   true;

    /// <summary>
    /// Time between each update.
    /// </summary>
    [SerializeField, Min(0)]
    private float       updateInterval =    0;

    /// <summary>Backing field for <see cref="Name"/>.</summary>
    [SerializeField]
    private string      name =              "New Update Mode";


    /**********************************
     *******     PROPERTIES     *******
     *********************************/

    /// <summary>
    /// Indicates if this update interval is in frames or seconds.
    /// </summary>
    public bool     IsFrameInterval     { get { return isFrameInterval; } }

    /// <summary>
    /// Name of this update mode,
    /// associated with a <see cref="UpdateTimeline"/> value.
    /// </summary>
    public string   Name                { get { return name; } }
    #endregion

    #region Memory
    /**********************************
     *********     MEMORY     *********
     *********************************/

    /// <summary>
    /// Timer used to do the update when reaching
    /// <see cref="updateInterval"/> value.
    /// </summary>
    private float timer = 0;
    #endregion

    #region Methods
    /***********************************
     *********     METHODS     *********
     **********************************/

    /// <summary>
    /// Increase timer for frames interval.
    /// </summary>
    public void IncreaseTimer()
    {
        timer++;
        Update();
    }

    /// <summary>
    /// Increase timer for seconds interval.
    /// </summary>
    /// <param name="_deltaTime">The completion time in seconds since the last frame.</param>
    public void IncreaseTimer(float _deltaTime)
    {
        timer += _deltaTime;
        Update();
    }


    /// <summary>
    /// Do the update when reaching this mode time interval.
    /// </summary>
    private void Update()
    {
        // Return if not enough time spend since last update
        if (timer < updateInterval) return;

        // Reset timer and call event
        timer = 0;
        OnUpdate?.Invoke();
    }
    #endregion
}
