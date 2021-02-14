
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.IO;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace NeptuneCamera
{
    public class ModuleNeptuneCamera : PartModule
    {
        [KSPField]
        public string cameraTransformName = "cameraTransform";

        [KSPField(isPersistant = false)]
        public string cameraType = CAMERA_TYPE_FULL_COLOUR;

        [KSPField]
        public bool cameraHasCustomFieldOfView = false;

        [KSPField(isPersistant = true, guiActive = true, guiName = "Field of View", groupName = GROUP_CODE, groupDisplayName = GROUP_NAME, groupStartCollapsed = true), UI_FloatRange(minValue = 0f, maxValue = 100f, stepIncrement = 1f)]
        public float cameraFieldOfView = 70f;

        [KSPField]
        public float cameraFieldOfViewMax = 0f;

        [KSPField]
        public float cameraFieldOfViewMin = 0f;

        [KSPField(isPersistant = false)]
        public int cameraHorizontalResolution = 256;

        [KSPField(isPersistant = false)]
        public int cameraVerticalResolution = 256;

        [KSPField]
        public bool cameraHasErrors = false;

        [KSPField]
        public int cameraErrorRate = 0;

        [KSPField]
        public bool cameraHasNoise = false;

        [KSPField]
        public int cameraNoiseMaxStrength = 0;

        [KSPField]
        public bool cameraHasCustomNearClipPlane = false;

        [KSPField]
        public float cameraCustomNearClipPlane = 0.01f;

        [KSPField]
        public bool cameraHasCustomTitle = false;

        [KSPField]
        public string cameraCustomTitle = "Camera";

        [KSPField]
        public bool cameraHasDisplayWindow = true;

        private GameObject _cameraGameObject = null;
        private GameObject _nearGameObject = null;
        private GameObject _farGameObject = null;
        private GameObject _scaledGameObject = null;
        private GameObject _galaxyGameObject = null;

        private Camera _nearCamera = null;
        private Camera _farCamera = null;
        private Camera _scaledCamera = null;
        private Camera _galaxyCamera = null;

        const string CAMERA_PREFIX = "Tantares_";
        const string GALAXY_CAMERA_NAME = "GalaxyCamera";
        const string SCALED_CAMERA_NAME = "Camera ScaledSpace";
        const string FAR_CAMERA_NAME = "Camera 01";//"UIMainCamera";
        const string NEAR_CAMERA_NAME = "Camera 00";

        const string DEBUG_LOG_PREFIX = "ModuleTantaresCamera";

        const string GROUP_CODE = "NEPTUNECAMERA";
        const string GROUP_NAME = "Neptune Camera";

        const string CAMERA_TYPE_FULL_COLOUR = "FULL_COLOUR";
        const string CAMERA_TYPE_RED_COLOUR = "RED_COLOUR";
        const string CAMERA_TYPE_GREEN_COLOUR = "GREEN_COLOUR";
        const string CAMERA_TYPE_BLUE_COLOUR = "BLUE_COLOUR";
        const string CAMERA_TYPE_GREYSCALE = "GREYSCALE_COLOUR";
        const string CAMERA_TYPE_ULTRAVIOLET = "ULTRAVIOLET_COLOUR";
        const string CAMERA_TYPE_INFRARED = "INFRARED_COLOUR";

        RenderTexture _renderTextureColor;
        RenderTexture _renderTextureDepth;

        const string PART_INFO_TEMPLATE
            = @"Camera Type: {0}\nHorizontal Resolution: {1}\nVertical Resolution: {2}\nField of View: {3}";

        // GUI

        private bool _isDisplayWindowVisible = false;

        const int DISPLAY_TEXTURE_WIDTH = 128;
        const int DISPLAY_TEXTURE_HEIGHT = 128;

        RenderTexture _displayTextureColor;
        RenderTexture _displayTextureDepth;

        Rect _displayWindowRect = new Rect(Screen.width / 2, Screen.height / 2, 148, 178);
        Rect _displayWindowLabelRect = new Rect(10, 20, 128, 20);
        Rect _displayWindowTextureRect = new Rect(10, 40, 128, 128);

        public void Start()
        {
            _cameraGameObject = base.gameObject.GetChild(cameraTransformName);

            if (_cameraGameObject == null)
            {
                Debug.LogFormat("[{0}] Camera game object is missing.", DEBUG_LOG_PREFIX);
                return;
            }

            // Create the camera render texture.

            _renderTextureColor = new RenderTexture(cameraHorizontalResolution, cameraVerticalResolution, 0);
            _renderTextureDepth = new RenderTexture(cameraHorizontalResolution, cameraVerticalResolution, 24);
            _renderTextureColor.Create();
            _renderTextureDepth.Create();

            // Create the GUI render texture.

            _displayTextureColor = new RenderTexture(DISPLAY_TEXTURE_WIDTH, DISPLAY_TEXTURE_HEIGHT, 0);
            _displayTextureDepth = new RenderTexture(DISPLAY_TEXTURE_WIDTH, DISPLAY_TEXTURE_HEIGHT, 24);
            _displayTextureColor.Create();
            _displayTextureDepth.Create();

            // Setup all the cameras.

            _nearGameObject = new GameObject();
            _farGameObject = new GameObject();
            _scaledGameObject = new GameObject();
            _galaxyGameObject = new GameObject();

            // Add the near camera.

            _nearCamera = _nearGameObject.AddComponent<Camera>();
            var nearCameraReference = UnityEngine.Camera.allCameras.FirstOrDefault(cam => cam.name == NEAR_CAMERA_NAME);
            if (nearCameraReference != null)
            {
                _nearCamera.CopyFrom(nearCameraReference);
                _nearCamera.name = CAMERA_PREFIX + NEAR_CAMERA_NAME;
                _nearCamera.enabled = false;

                // The camera is attached to our object transform and does not move from there.

                _nearCamera.transform.parent = _cameraGameObject.transform;
                _nearCamera.transform.localPosition = Vector3.zero;
                _nearCamera.transform.localRotation = Quaternion.identity;

                if (cameraHasCustomNearClipPlane)
                    _nearCamera.nearClipPlane = cameraCustomNearClipPlane;

                if (cameraHasCustomFieldOfView)
                    _nearCamera.fieldOfView = cameraFieldOfView;
            }

            // Add the far camera.

            _farCamera = _farGameObject.AddComponent<Camera>();
            var farCameraReference = UnityEngine.Camera.allCameras.FirstOrDefault(cam => cam.name == FAR_CAMERA_NAME);
            if (farCameraReference != null)
            {
                _farCamera.CopyFrom(farCameraReference);
                _farCamera.name = CAMERA_PREFIX + FAR_CAMERA_NAME;
                _farCamera.enabled = false;

                // The camera is attached to our object transform and does not move from there.

                _farCamera.transform.parent = _cameraGameObject.transform;
                _farCamera.transform.localPosition = Vector3.zero;
                _farCamera.transform.localRotation = Quaternion.identity;

                if (cameraHasCustomFieldOfView)
                    _farCamera.fieldOfView = cameraFieldOfView;
            }

            // Add the scaled camera.

            _scaledCamera = _scaledGameObject.AddComponent<Camera>();
            var scaledCameraReference = UnityEngine.Camera.allCameras.FirstOrDefault(cam => cam.name == SCALED_CAMERA_NAME);
            if (scaledCameraReference != null)
            {
                _scaledCamera.CopyFrom(scaledCameraReference);
                _scaledCamera.name = CAMERA_PREFIX + SCALED_CAMERA_NAME;
                _scaledCamera.enabled = false;

                // Scaled cam has no parent.

                if (cameraHasCustomFieldOfView)
                    _scaledCamera.fieldOfView = cameraFieldOfView;
            }

            // Add the galaxy camera.

            _galaxyCamera = _galaxyGameObject.AddComponent<Camera>();
            var galaxyCameraReference = UnityEngine.Camera.allCameras.FirstOrDefault(cam => cam.name == GALAXY_CAMERA_NAME);
            if (galaxyCameraReference != null)
            {
                _galaxyCamera.CopyFrom(galaxyCameraReference);
                _galaxyCamera.name = CAMERA_PREFIX + GALAXY_CAMERA_NAME;
                _galaxyCamera.enabled = false;

                // Galaxy camera renders the galaxy skybox and is not 
                // actually moving, but only rotating to look at the galaxy cube.

                Transform galaxyRoot = GalaxyCubeControl.Instance.transform.parent;
                _galaxyCamera.transform.parent = galaxyRoot;
                _galaxyCamera.transform.localPosition = Vector3.zero;
                _galaxyCamera.transform.localRotation = Quaternion.identity;
            }

            // Configure which events are available.

            Actions["ActionCaptureFullColourImage"].active = (cameraType == CAMERA_TYPE_FULL_COLOUR);
            Events["EventCaptureFullColourImage"].active = (cameraType == CAMERA_TYPE_FULL_COLOUR);

            Actions["ActionCaptureRedImage"].active = (cameraType == CAMERA_TYPE_FULL_COLOUR || cameraType == CAMERA_TYPE_RED_COLOUR);
            Events["EventCaptureRedImage"].active = (cameraType == CAMERA_TYPE_FULL_COLOUR || cameraType == CAMERA_TYPE_RED_COLOUR);

            Actions["ActionCaptureGreenImage"].active = (cameraType == CAMERA_TYPE_FULL_COLOUR || cameraType == CAMERA_TYPE_GREEN_COLOUR);
            Events["EventCaptureGreenImage"].active = (cameraType == CAMERA_TYPE_FULL_COLOUR || cameraType == CAMERA_TYPE_GREEN_COLOUR);

            Actions["ActionCaptureBlueImage"].active = (cameraType == CAMERA_TYPE_FULL_COLOUR || cameraType == CAMERA_TYPE_BLUE_COLOUR);
            Events["EventCaptureBlueImage"].active = (cameraType == CAMERA_TYPE_FULL_COLOUR || cameraType == CAMERA_TYPE_BLUE_COLOUR);

            Actions["ActionCaptureGreyscaleImage"].active = (cameraType == CAMERA_TYPE_FULL_COLOUR || cameraType == CAMERA_TYPE_GREYSCALE);
            Events["EventCaptureGreyscaleImage"].active = (cameraType == CAMERA_TYPE_FULL_COLOUR || cameraType == CAMERA_TYPE_GREYSCALE);

            Actions["ActionCaptureUltravioletImage"].active = (cameraType == CAMERA_TYPE_ULTRAVIOLET);
            Events["EventCaptureUltravioletImage"].active = (cameraType == CAMERA_TYPE_ULTRAVIOLET);

            Actions["ActionCaptureInfraredImage"].active = (cameraType == CAMERA_TYPE_INFRARED);
            Events["EventCaptureInfraredImage"].active = (cameraType == CAMERA_TYPE_INFRARED);

            Actions["ActionToggleDisplayWindow"].active = cameraHasDisplayWindow;
            Events["EventToggleDisplayWindow"].active = cameraHasDisplayWindow;


            // Rename the events if using a custom title.

            if (cameraHasCustomTitle)
            {
                string descFullColour = $"({cameraCustomTitle}) Full Colour Image";
                string descRed = $"({cameraCustomTitle}) Red Image";
                string descGreen = $"({cameraCustomTitle}) Green Image";
                string descBlue = $"({cameraCustomTitle}) Blue Image";
                string descGreyscale = $"({cameraCustomTitle}) Greyscale Image";
                string descUltraviolet = $"({cameraCustomTitle}) Ultraviolet Image";
                string descInfrared = $"({cameraCustomTitle}) Infrared Image";
                string descDisplayWindow = $"({cameraCustomTitle}) Toggle Display Widnow";

                Actions["ActionCaptureFullColourImage"].guiName = descFullColour;
                Events["EventCaptureFullColourImage"].guiName = descFullColour;

                Actions["ActionCaptureRedImage"].guiName = descRed;
                Events["EventCaptureRedImage"].guiName = descRed;

                Actions["ActionCaptureGreenImage"].guiName = descGreen;
                Events["EventCaptureGreenImage"].guiName = descGreen;

                Actions["ActionCaptureBlueImage"].guiName = descBlue;
                Events["EventCaptureBlueImage"].guiName = descBlue;

                Actions["ActionCaptureGreyscaleImage"].guiName = descGreyscale;
                Events["EventCaptureGreyscaleImage"].guiName = descGreyscale;

                Actions["ActionCaptureUltravioletImage"].guiName = descUltraviolet;
                Events["EventCaptureUltravioletImage"].guiName = descUltraviolet;

                Actions["ActionCaptureInfraredImage"].guiName = descInfrared;
                Events["EventCaptureInfraredImage"].guiName = descInfrared;

                Actions["ActionToggleDisplayWindow"].guiName = descDisplayWindow;
                Events["EventToggleDisplayWindow"].guiName = descDisplayWindow;
            }

            // Setup the slider.

            if (cameraFieldOfViewMin == 0f)
                cameraFieldOfViewMin = cameraFieldOfView;

            if (cameraFieldOfViewMax == 0f)
                cameraFieldOfViewMax = cameraFieldOfView;

            UI_FloatRange cameraFieldOfViewEditorSlider = (UI_FloatRange)Fields["cameraFieldOfView"].uiControlEditor;
            cameraFieldOfViewEditorSlider.minValue = cameraFieldOfViewMin;
            cameraFieldOfViewEditorSlider.maxValue = cameraFieldOfViewMax;

            UI_FloatRange cameraFieldOfViewFlightSlider = (UI_FloatRange)Fields["cameraFieldOfView"].uiControlFlight;
            cameraFieldOfViewFlightSlider.minValue = cameraFieldOfViewMin;
            cameraFieldOfViewFlightSlider.maxValue = cameraFieldOfViewMax;
        }

        public string GetModuleTitle()
        {
            return "Neptune Camera";
        }

        public override string GetInfo()
        {
            return string.Format(PART_INFO_TEMPLATE, cameraType, cameraHorizontalResolution, cameraVerticalResolution, cameraFieldOfView);
        }

        public void Update()
        {
            _scaledCamera.transform.position = ScaledSpace.LocalToScaledSpace(_cameraGameObject.transform.position);
            _scaledCamera.transform.rotation = _cameraGameObject.transform.rotation;

            _galaxyGameObject.transform.rotation = _cameraGameObject.transform.rotation;

            if (_isDisplayWindowVisible)
                CaptureDisplayWindowImage();
        }

        public void LateUpdate()
        {
            _nearCamera.enabled = false;
            _farCamera.enabled = false;
            _scaledCamera.enabled = false;
            _galaxyCamera.enabled = false;
        }

        [KSPAction(guiName = "Capture Full Colour Image", activeEditor = true)]
        public void ActionCaptureFullColourImage(KSPActionParam param)
        {
            CaptureImage(CAMERA_TYPE_FULL_COLOUR);
        }

        [KSPEvent(guiActive = true, guiActiveEditor = true, guiName = "Capture Full Colour Image", active = true, groupName = GROUP_CODE, groupDisplayName = GROUP_NAME, groupStartCollapsed = true)]
        public void EventCaptureFullColourImage()
        {
            CaptureImage(CAMERA_TYPE_FULL_COLOUR);
        }

        [KSPAction(guiName = "Capture Red Image", activeEditor = true)]
        public void ActionCaptureRedImage(KSPActionParam param)
        {
            CaptureImage(CAMERA_TYPE_RED_COLOUR);
        }

        [KSPEvent(guiActive = true, guiActiveEditor = true, guiName = "Capture Red Image", active = true, groupName = GROUP_CODE, groupDisplayName = GROUP_NAME, groupStartCollapsed = true)]
        public void EventCaptureRedImage()
        {
            CaptureImage(CAMERA_TYPE_RED_COLOUR);
        }

        [KSPAction(guiName = "Capture Green Image", activeEditor = true)]
        public void ActionCaptureGreenImage(KSPActionParam param)
        {
            CaptureImage(CAMERA_TYPE_GREEN_COLOUR);
        }

        [KSPEvent(guiActive = true, guiActiveEditor = true, guiName = "Capture Green Image", active = true, groupName = GROUP_CODE, groupDisplayName = GROUP_NAME, groupStartCollapsed = true)]
        public void EventCaptureGreenImage()
        {
            CaptureImage(CAMERA_TYPE_GREEN_COLOUR);
        }

        [KSPAction(guiName = "Capture Blue Image", activeEditor = true)]
        public void ActionCaptureBlueImage(KSPActionParam param)
        {
            CaptureImage(CAMERA_TYPE_BLUE_COLOUR);
        }

        [KSPEvent(guiActive = true, guiActiveEditor = true, guiName = "Capture Blue Image", active = true, groupName = GROUP_CODE, groupDisplayName = GROUP_NAME, groupStartCollapsed = true)]
        public void EventCaptureBlueImage()
        {
            CaptureImage(CAMERA_TYPE_BLUE_COLOUR);
        }

        [KSPAction(guiName = "Capture Greyscale Image", activeEditor = true)]
        public void ActionCaptureGreyscaleImage(KSPActionParam param)
        {
            CaptureImage(CAMERA_TYPE_GREYSCALE);
        }

        [KSPEvent(guiActive = true, guiActiveEditor = true, guiName = "Capture Greyscale Image", active = true, groupName = GROUP_CODE, groupDisplayName = GROUP_NAME, groupStartCollapsed = true)]
        public void EventCaptureGreyscaleImage()
        {
            CaptureImage(CAMERA_TYPE_GREYSCALE);
        }

        [KSPAction(guiName = "Capture Ultraviolet Image", activeEditor = true)]
        public void ActionCaptureUltravioletImage(KSPActionParam param)
        {
            CaptureImage(CAMERA_TYPE_ULTRAVIOLET);
        }

        [KSPEvent(guiActive = true, guiActiveEditor = true, guiName = "Capture Ultraviolet Image", active = true, groupName = GROUP_CODE, groupDisplayName = GROUP_NAME, groupStartCollapsed = true)]
        public void EventCaptureUltravioletImage()
        {
            CaptureImage(CAMERA_TYPE_ULTRAVIOLET);
        }

        [KSPAction(guiName = "Capture Infrared Image", activeEditor = true)]
        public void ActionCaptureInfraredImage(KSPActionParam param)
        {
            CaptureImage(CAMERA_TYPE_INFRARED);
        }

        [KSPEvent(guiActive = true, guiActiveEditor = true, guiName = "Capture Infrared Image", active = true, groupName = GROUP_CODE, groupDisplayName = GROUP_NAME, groupStartCollapsed = true)]
        public void EventCaptureInfraredImage()
        {
            CaptureImage(CAMERA_TYPE_INFRARED);
        }

        [KSPAction(guiName = "Toggle Display Window", activeEditor = false)]
        public void ActionToggleDisplayWindow(KSPActionParam param)
        {
            _isDisplayWindowVisible = !_isDisplayWindowVisible;
        }

        [KSPEvent(guiActive = true, guiActiveEditor = false, guiName = "Toggle Display Window", active = true, groupName = GROUP_CODE, groupDisplayName = GROUP_NAME, groupStartCollapsed = true)]
        public void EventToggleDisplayWindow()
        {
            _isDisplayWindowVisible = !_isDisplayWindowVisible;
        }


        public void CaptureImage(string captureType)
        {

            try
            {
                // Switch the camera on.

                Debug.LogFormat("[{0}] Switching camera on.", DEBUG_LOG_PREFIX);

                _nearCamera.enabled = true;
                if (GameSettings.GraphicsVersion != GameSettings.GraphicsType.D3D11)
                    _farCamera.enabled = true;
                _scaledCamera.enabled = true;
                _galaxyCamera.enabled = true;

                // Switch the camera FOV.

                if (cameraHasCustomFieldOfView)
                {
                    _nearCamera.fieldOfView = cameraFieldOfView;
                    if (_farCamera != null)
                        _farCamera.fieldOfView = cameraFieldOfView;
                    _scaledCamera.fieldOfView = cameraFieldOfView;
                }

                // Render camera to texture.

                Debug.LogFormat("[{0}] Rendering cameras to texture.", DEBUG_LOG_PREFIX);

                var imageTexture = new Texture2D(cameraHorizontalResolution, cameraVerticalResolution, TextureFormat.RGB24, false);

                RenderTexture.active = _renderTextureColor;

                _nearCamera.SetTargetBuffers(_renderTextureColor.colorBuffer, _renderTextureDepth.depthBuffer);
                _farCamera.SetTargetBuffers(_renderTextureColor.colorBuffer, _renderTextureDepth.depthBuffer);
                _scaledCamera.SetTargetBuffers(_renderTextureColor.colorBuffer, _renderTextureDepth.depthBuffer);
                _galaxyCamera.SetTargetBuffers(_renderTextureColor.colorBuffer, _renderTextureDepth.depthBuffer);

                _galaxyCamera.Render();
                _scaledCamera.Render();
                if (GameSettings.GraphicsVersion != GameSettings.GraphicsType.D3D11)
                    _farCamera.Render();
                _nearCamera.Render();

                imageTexture.ReadPixels(new Rect(0, 0, cameraHorizontalResolution, cameraVerticalResolution), 0, 0);
                imageTexture.Apply();

                Debug.LogFormat("[{0}] Applying effects to the image texture.", DEBUG_LOG_PREFIX);

                // Apply error scrambling if enabled.

                if (cameraHasErrors)
                    imageTexture = ModuleNeptuneCameraEffects.GetErrorDamagedTexture(imageTexture, cameraErrorRate);

                // Apply filtering based on capture type.

                if (captureType == CAMERA_TYPE_RED_COLOUR)
                    imageTexture = ModuleNeptuneCameraEffects.GetRedTexture(imageTexture);

                if (captureType == CAMERA_TYPE_GREEN_COLOUR)
                    imageTexture = ModuleNeptuneCameraEffects.GetGreenTexture(imageTexture);

                if (captureType == CAMERA_TYPE_BLUE_COLOUR)
                    imageTexture = ModuleNeptuneCameraEffects.GetBlueTexture(imageTexture);

                if (captureType == CAMERA_TYPE_GREYSCALE)
                    imageTexture = ModuleNeptuneCameraEffects.GetGreyscaleTexture(imageTexture);

                if (captureType == CAMERA_TYPE_ULTRAVIOLET)
                    imageTexture = ModuleNeptuneCameraEffects.GetProtanopiaTexture(imageTexture);

                if (captureType == CAMERA_TYPE_INFRARED)
                    imageTexture = ModuleNeptuneCameraEffects.GetTritanopiaTexture(imageTexture);

                // Apply noise if enabled.

                if (cameraHasNoise)
                    imageTexture = ModuleNeptuneCameraEffects.GetNoisyTexture(imageTexture, cameraNoiseMaxStrength);


                Debug.LogFormat("[{0}] Encoding image texture to bytes.", DEBUG_LOG_PREFIX);

                byte[] bytes = imageTexture.EncodeToPNG();
                Destroy(imageTexture);

                // Disable the render texture.

                Debug.LogFormat("[{0}] Cleaning up render textures.", DEBUG_LOG_PREFIX);

                RenderTexture.active = null;
                _nearCamera.targetTexture = null;
                _farCamera.targetTexture = null;
                _scaledCamera.targetTexture = null;
                _galaxyCamera.targetTexture = null;

                // Switch the camera off.

                Debug.LogFormat("[{0}] Switching camera off.", DEBUG_LOG_PREFIX);

                _nearCamera.enabled = false;
                _farCamera.enabled = false;
                _scaledCamera.enabled = false;
                _galaxyCamera.enabled = false;

                // Write the file.
                // neptune_0000_00_00_00_00_00_000_ABCDEFGH

                string path = KSPUtil.ApplicationRootPath + "Screenshots/neptune_{0}_{1}_{2}_{3}_{4}_{5}_{6}_{7}.png";

                string fileName = string.Format
                (
                    path,
                    DateTime.Now.Year,
                    DateTime.Now.Month,
                    DateTime.Now.Day,
                    DateTime.Now.Hour,
                    DateTime.Now.Minute,
                    DateTime.Now.Second,
                    DateTime.Now.Millisecond,
                    Guid.NewGuid().ToString().Substring(0, 8)
                );

                File.WriteAllBytes(fileName, bytes);
            }
            catch (Exception ex)
            {
                Debug.LogFormat("[{0}] Error capturing image.", DEBUG_LOG_PREFIX);
                Debug.LogFormat("[{0}] {1}.", DEBUG_LOG_PREFIX, ex.Message);
            }
        }

        public void CaptureDisplayWindowImage()
        {
            // Switch the camera on.

            _nearCamera.enabled = true;
            if (GameSettings.GraphicsVersion != GameSettings.GraphicsType.D3D11)
                _farCamera.enabled = true;
            _scaledCamera.enabled = true;
            _galaxyCamera.enabled = true;

            // Switch the camera FOV.

            if (cameraHasCustomFieldOfView)
            {
                _nearCamera.fieldOfView = cameraFieldOfView;
                if (_farCamera != null)
                    _farCamera.fieldOfView = cameraFieldOfView;
                _scaledCamera.fieldOfView = cameraFieldOfView;
            }

            // Render camera to texture.

            RenderTexture.active = _displayTextureColor;

            _nearCamera.SetTargetBuffers(_displayTextureColor.colorBuffer, _displayTextureDepth.depthBuffer);
            _farCamera.SetTargetBuffers(_displayTextureColor.colorBuffer, _displayTextureDepth.depthBuffer);
            _scaledCamera.SetTargetBuffers(_displayTextureColor.colorBuffer, _displayTextureDepth.depthBuffer);
            _galaxyCamera.SetTargetBuffers(_displayTextureColor.colorBuffer, _displayTextureDepth.depthBuffer);

            _galaxyCamera.Render();
            _scaledCamera.Render();
            if (GameSettings.GraphicsVersion != GameSettings.GraphicsType.D3D11)
                _farCamera.Render();
            _nearCamera.Render();

            // Switch the camera off.

            _nearCamera.enabled = false;
            if (GameSettings.GraphicsVersion != GameSettings.GraphicsType.D3D11)
                _farCamera.enabled = false;
            _scaledCamera.enabled = false;
            _galaxyCamera.enabled = false;
        }


        public void OnGUI()
        {
            if (!_isDisplayWindowVisible)
                return;

            _displayWindowRect = GUI.Window(1, _displayWindowRect, DisplayWindow, "Neptune Camera", HighLogic.Skin.window);
        }

        public void DisplayWindow(int windowID)
        {
            GUI.DrawTexture(_displayWindowTextureRect, _displayTextureColor, ScaleMode.ScaleToFit);
            GUI.Label(_displayWindowLabelRect, cameraCustomTitle);
            GUI.DragWindow();
        }
    }
}