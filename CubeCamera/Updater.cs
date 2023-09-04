using System.Diagnostics.CodeAnalysis;
using CubeCamera.Textures;
using UnityEngine;

namespace CubeCamera;

/// <summary>
/// Cycle through the faces of each cube to update.
/// </summary>
public class Updater : MonoBehaviour
{
    private readonly CameraController _cameraController;
    private readonly Camera _mainCamera;
    private readonly OverlayEffect _overlayEffect;
    private readonly Transform _transform;

    private bool _update;
    private bool _draw;
    private bool _keepContinue;
    
    private TextureBase _texture;
    private TextureBase.FaceTextures? _bufferedTexture;
    private ConvertibleTexture? _convertableTexture;
    
    private Action? _onPhaseContinue;
    private Action? _onPhaseEnd;
    
    /// <summary>
    /// Sets a texture to render.
    /// </summary>
    public TextureBase Texture
    {
        [MemberNotNull(nameof(_texture))]
        set
        {
            _texture = value;
            _bufferedTexture = value is ConvertibleTexture ? null : new TextureBase.FaceTextures(value.Faces.Size);
            _convertableTexture = value as ConvertibleTexture;
        }
    }
    
    /// <summary>
    /// Set up free camera immediately.
    /// </summary>
    public bool FreeCamera
    {
        set
        {
            _cameraController.m_freeCamera = value;
            _overlayEffect.enabled = !value; // disappear immediately (no time to wait for it to fade)
        }
    }
    
    /// <summary>
    /// Sets a target pose.
    /// </summary>
    public Pose TargetPose
    {
        set => _targetPose = value;
    }
    private Pose? _targetPose;

    /// <summary>
    /// Phase cycle to update.
    /// </summary>
    private enum Phase
    {
        Idle, Start, Continue, Front, Left, Right, Back, Top, Bottom, End
    }
    private Phase _phase;
    
    /// <summary>
    /// Get the next phase based on current phase.
    /// </summary>
    private Phase NextPhase => _phase switch
    {
        Phase.Start => Phase.Front,
        Phase.End => Phase.Idle,
        Phase.Bottom when _keepContinue  => Phase.Continue,
        Phase.Bottom when !_keepContinue => Phase.End,
        _ => _phase + 1,
    };
    
    /// <summary>
    /// Backup of values to restore in exit
    /// </summary>
    private class BackupValues
    {
        public required float FieldOfView;
        public required Vector2 Angle;
        public required Vector3 Position;
        public required RenderTexture ActiveRenderTexture;
        public required Quaternion TransformRotation;
        public required Vector3 TransformPosition;
    }
    private BackupValues? _backupValues;
    
    public Updater()
    {
        _cameraController = gameObject.GetComponentInParent<CameraController>();
        _mainCamera = gameObject.GetComponent<Camera>();
        _overlayEffect = _mainCamera.GetComponent<OverlayEffect>();
        _transform = this.transform;
        Texture = CubeCamera.Texture;
    }
    
    private void Update()
    {
        if (!_update) return;

        _phase = NextPhase;
        
        switch (_phase)
        {
            case Phase.Idle:
                throw new InvalidOperationException();
            
            case Phase.Start:
            {
                _backupValues ??= new BackupValues
                {
                    FieldOfView = _mainCamera.fieldOfView,
                    Angle = _cameraController.m_targetAngle,
                    Position = _cameraController.m_targetPosition,
                    ActiveRenderTexture = RenderTexture.active,
                    TransformPosition = _transform.position,
                    TransformRotation = _transform.rotation
                };

                _mainCamera.fieldOfView = 89.9999f;
            }
                return;

            case Phase.Continue:
            {
                UpdateBufferedTexture();
                _convertableTexture?.Convert();
                _onPhaseContinue?.Invoke();
            }
                return;
            
            case Phase.Front:
            case Phase.Left:
            case Phase.Right:
            case Phase.Back:
            case Phase.Top:
            case Phase.Bottom:
                return;

            case Phase.End:    
            {
                _convertableTexture?.Convert();
                _onPhaseEnd?.Invoke();
                
                if (_backupValues is null) throw new NullReferenceException();
                _mainCamera.fieldOfView = _backupValues.FieldOfView;
                _cameraController.m_targetAngle.x = _backupValues.Angle.x;
                _cameraController.m_targetAngle.y = _backupValues.Angle.y;
                _cameraController.m_targetPosition = _backupValues.Position;
                RenderTexture.active = _backupValues.ActiveRenderTexture;

                _mainCamera.targetTexture = null;
                _backupValues = null;
                _bufferedTexture = null;
                _targetPose = null;
                _onPhaseContinue = null;
                _onPhaseEnd = null;
            }
                return;
        }
    }

    private void LateUpdate()
    {
        if (!_update) return;
        
        switch (_phase)
        {
            case >= Phase.Continue and <= Phase.Bottom when _backupValues is null: throw new NullReferenceException();

            case Phase.Continue:
            {
                // Update the fixed camera transform.
                _backupValues.TransformRotation = _transform.rotation;
                _backupValues.TransformPosition = _transform.position;
                
                // No needed to wait frame; go to next phase.
                _phase = NextPhase;
            }
                goto NEXT;

            case >= Phase.Continue and <= Phase.Bottom: NEXT:
            {
                // Prevent that camera transform being different for each face.
                _transform.position = _backupValues.TransformPosition;
                _transform.rotation = _backupValues.TransformRotation;
                
                _mainCamera.fieldOfView = 89.9999f;
                
                // If target transform exists, set it to that.
                if (_targetPose is Pose target)
                {
                    _transform.position = target.Position;
                    _transform.rotation = target.Rotation;
                }
                
                // Rotate the transform to fit each face.
                transform.rotation *= _phase switch
                {
                    Phase.Front => Quaternion.identity,
                    Phase.Left => Quaternion.Euler(0f, -90f, 0f),
                    Phase.Right => Quaternion.Euler(0f, 90f, 0f),
                    Phase.Back => Quaternion.Euler(0f, 180f, 0f),
                    Phase.Top => Quaternion.Euler(-90f, 0f, 0f),
                    Phase.Bottom => Quaternion.Euler(90f, 0f, 0f),
                    _ => throw new ArgumentOutOfRangeException()
                };
            }
                return;
            
            case Phase.End:
            {
                // End the render.
                _update = false;
                _phase = NextPhase;
            }
                return;
        }
    }

    private void OnPostRender()
    {
        if (!_update) return;

        switch (_phase)
        {
            // Not sure why, but without it, first front texture is distorted.
            case Phase.Start: Render(_texture.Faces.Front);
                return;
            
            case Phase.Front:  Render(_texture.Faces.Front);
                return;
            case Phase.Left:   Render(_texture.Faces.Left);
                return;
            case Phase.Right:  Render(_texture.Faces.Right);
                return;
            case Phase.Back:   Render(_texture.Faces.Back);
                return;
            case Phase.Top:    Render(_texture.Faces.Top);
                return;
            case Phase.Bottom: Render(_texture.Faces.Bottom);
                return;

            case Phase.Idle:
            case Phase.Continue:
            case Phase.End:
                throw new InvalidOperationException();
                
            default:
                return;
        }
        
        void Render(RenderTexture renderTexture)
        {
            _mainCamera.targetTexture = renderTexture;
        }
    }

    public void OnGUI()
    {
        // Draw shortcut: Alt+Shift+P
        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.P) && !_update ) CubeCamera.Draw();
        
        if (!_draw) return;

        GL.sRGBWrite = true; // for correct gamma

        if (_convertableTexture is not null)
        {
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), _convertableTexture.Converted, CubeCamera.ScreenScaleMode, false);
        }
        else
        {
            float size = Screen.height * 0.5f;

            _bufferedTexture ??= new TextureBase.FaceTextures(_texture.Faces.Size);
            GUI.DrawTexture(new Rect(0, 0, size, size), _bufferedTexture.Front, ScaleMode.ScaleToFit, false);
            GUI.DrawTexture(new Rect(size, 0, size, size), _bufferedTexture.Right, ScaleMode.ScaleToFit, false);
            GUI.DrawTexture(new Rect(size * 2, 0, size, size), _bufferedTexture.Back, ScaleMode.ScaleToFit, false);
            GUI.DrawTexture(new Rect(0, size, size, size), _bufferedTexture.Left, ScaleMode.ScaleToFit, false);
            GUI.DrawTexture(new Rect(size, size, size, size), _bufferedTexture.Top, ScaleMode.ScaleToFit, false);
            GUI.DrawTexture(new Rect(size * 2, size, size, size), _bufferedTexture.Bottom, ScaleMode.ScaleToFit, false);
        }
        
        //GL.sRGBWrite = false; // Strangely, this causes a flicker on any key input. It doesn't seem to matter even if don't set it back to false, so I'm commenting.
        
        if (Input.GetKey(KeyCode.Escape))
        {
            FreeCamera = false;
            Stop();
        };
    }

    public void StartCycle(Action? onPhaseContinue = null, Action? onPhaseEnd = null)
    {
        _update = true;
        _keepContinue = true;
        _onPhaseContinue = onPhaseContinue;
        _onPhaseEnd = onPhaseEnd;
    }
    
    public void StartDraw()
    {
        _draw = true;
        _keepContinue = true;
    }

    public void Stop()
    {
        _keepContinue = false;
        _draw = false;
    }
    
    private void UpdateBufferedTexture()
    {
        if (_bufferedTexture is null) return;
        
        Graphics.Blit(_texture.Faces.Front, _bufferedTexture.Front);
        Graphics.Blit(_texture.Faces.Left, _bufferedTexture.Left);
        Graphics.Blit(_texture.Faces.Right, _bufferedTexture.Right);
        Graphics.Blit(_texture.Faces.Back, _bufferedTexture.Back);
        Graphics.Blit(_texture.Faces.Top, _bufferedTexture.Top);
        Graphics.Blit(_texture.Faces.Bottom, _bufferedTexture.Bottom);
    }
}