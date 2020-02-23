using System;
using UnityEngine;

[Serializable]
public class UpdateMode
{
    #region Events
    /**********************************
     *********     EVENTS     *********
     *********************************/

    /// <summary>
    /// Event called when this update mode gets updated, that is when timer reach <see cref="updateInterval"/>.
    /// </summary>
    public event Action             OnUpdate =              null;
    #endregion

    #region Fields / Properties
    /**********************************
     *********     FIELDS     *********
     *********************************/

    /// <summary>Backing field for <see cref="IsFrameInterval"/>.</summary>
    [SerializeField, Tooltip("Is this update mode interval in frames or seconds")]
    private bool                    isFrameInterval =       true;

    /// <summary>
    /// Time between each update in frames or seconds.
    /// Refers to <see cref="IsFrameInterval"/> to determine the unit type.
    /// </summary>
    [SerializeField, Min(0), Tooltip("Time between each update of this update mode, in frames or seconds")]
    private float                   updateInterval =        0;

    /// <summary>Backing field for <see cref="Timeline"/>.</summary>
    [SerializeField]
    private UpdateModeTimeline      timeline =              (UpdateModeTimeline)(-1);


    /**********************************
     *******     PROPERTIES     *******
     *********************************/

    /// <summary>
    /// Indicates if this update interval is in frames or seconds.
    /// </summary>
    public bool                     IsFrameInterval         { get { return isFrameInterval; } }

    /// <summary>
    /// Timeline associated with this update mode.
    /// </summary>
    public UpdateModeTimeline       Timeline                { get { return timeline; } }
    #endregion

    #region Memory
    /**********************************
     *********     MEMORY     *********
     *********************************/

    /// <summary>
    /// Timer used to do the update when reaching
    /// <see cref="updateInterval"/> value.
    /// </summary>
    private float   timer = 0;
    #endregion

    #region Constructors
    /**********************************
     ******     CONSTRUCTORS     ******
     *********************************/

    /// <summary>
    /// Create a new update mode, with an associated timeline.
    /// </summary>
    /// <param name="_timeline">Timeline of this update mode.</param>
    public UpdateMode(UpdateModeTimeline _timeline)
    {
        timeline = _timeline;
    }
    #endregion

    #region Methods
    /***********************************
     *********     METHODS     *********
     **********************************/

    /// <summary>
    /// Increase update mode timer.
    /// </summary>
    public void IncreaseTimer() => IncreaseTimer(1);

    /// <summary>
    /// Increase update mode timer.
    /// </summary>
    /// <param name="_deltaTime">Time to increase timer by.</param>
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
