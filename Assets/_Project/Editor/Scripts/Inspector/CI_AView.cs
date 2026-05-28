using System;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Metroma.Editor
{
    [CustomEditor(typeof(FixedView))]
    public class CI_AView : UnityEditor.Editor
    {
        
        private Camera _previewCamera;
        private RenderTexture _renderTexture;
        private Image _previewImage;
        
        private void OnEnable()
        {
            GameObject go = new GameObject("Temp_PreviewCamera");
            
            go.hideFlags = HideFlags.HideAndDontSave; 
            _previewCamera = go.AddComponent<Camera>();
            
            FixedView view = (FixedView)target;
            CameraHelpers.ApplyConfiguration(_previewCamera, view.GetConfiguration());

            _renderTexture = Resources.Load<RenderTexture>("RT_PreviewCamera");
            _previewCamera.targetTexture = _renderTexture;
            
            EditorApplication.update += UpdatePreview;
            
        }
        
        private void OnDisable()
        {
            EditorApplication.update -= UpdatePreview;

            if (_previewCamera != null)
            {
                DestroyImmediate(_previewCamera.gameObject);
            }

            if (_renderTexture != null)
            {
                _renderTexture.Release();
            }
        }

        private void UpdatePreview()
        {
            if (_previewCamera != null && _renderTexture != null)
            {
                CameraHelpers.ApplyConfiguration(_previewCamera, ((FixedView)target).GetConfiguration());
                _previewCamera.Render();
                if (_previewImage != null)
                {
                    _previewImage.MarkDirtyRepaint();
                }
            }
        }
        
        public override VisualElement CreateInspectorGUI()
        {
            VisualElement root = new VisualElement();

            _previewImage = new Image
            {
                image = _renderTexture,
                style =
                {
                    height = 200,
                    marginBottom = 15,
                    backgroundColor = Color.black,
                    unityBackgroundScaleMode = ScaleMode.ScaleToFit
                }
            };
            root.Add(_previewImage);
            
            InspectorElement.FillDefaultInspector(root, this.serializedObject, this); //Merci UI toolkit de m'avoir préevenu que pour render le defaultInspector il ne faut pas faire base.CreateInspectorGUI()


            return root;
        }
    }
}
