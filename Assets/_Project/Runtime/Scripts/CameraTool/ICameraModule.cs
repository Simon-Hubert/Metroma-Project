using UnityEngine;

namespace Metroma.CameraTool
{
    /// <summary>
    /// Base interface for all camera logic modules.
    /// Handled by the CameraRig to allow modular extension of camera behaviors.
    /// </summary>
    public interface ICameraModule
    {
        /// <summary>
        /// Called when the module is initialized by the Rig.
        /// </summary>
        void Initialize(CameraRig rig);

        /// <summary>
        /// Called every frame (during LateUpdate) to calculate or apply camera state.
        /// </summary>
        void OnUpdate(float deltaTime);

        /// <summary>
        /// Priority of the module. Higher priority modules can override lower ones.
        /// </summary>
        int Priority { get; }
        
        /// <summary>
        /// Whether this module is currently actively controlling the camera.
        /// </summary>
        bool IsActive { get; }
    }
}
