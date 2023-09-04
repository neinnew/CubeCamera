using System.Globalization;
using ICities;
using ColossalFramework.UI;
using CubeCamera.Textures;
using UnityEngine;
using Cubemap = CubeCamera.Textures.Cubemap;

namespace CubeCamera;

public static class SettingsUI
{
    private static UITextField? _startPointTextField;
    private static UITextField? _endPointTextField;
    private static UILabel? _continuousLabel;
    private static UILabel? _continuousProgressLabel;

    private static float _duration = 1.0f;
    private static int _frameRate = 30;
    
    public static void OnSettingsUI(UIHelperBase helper)
    {
        helper.AddButton("Take", CubeCamera.Take);
        
        helper.AddButton("Draw", () =>
        {
            UIView.GetAView().FindUIComponent<UIPanel>("OptionsPanel(Clone)").GetComponent<OptionsMainPanel>().OnClosed();
            
            CubeCamera.Draw();
        });
        
        helper.AddButton("Draw(Free Camera)", () =>
        {
            UIView.GetAView().FindUIComponent<UIPanel>("OptionsPanel(Clone)").GetComponent<OptionsMainPanel>().OnClosed();
            
            CubeCamera.Draw(freeCamera: true);
        });

        helper.AddTextfield(
            text: "Face Size",
            defaultContent: CubeCamera.FaceSize.ToString(),
            eventChangedCallback: _ => {},
            eventSubmittedCallback: value =>
            {
                bool success = int.TryParse(value, out int i);
                if (!success) return;
                CubeCamera.FaceSize = Mathf.Clamp(i, 32, 4096);
                ModConfig.Save();
            });
        
        helper.AddTextfield(
            text: "Equirectangular Width",
            defaultContent: CubeCamera.EquirectangularWidth.ToString(),
            eventChangedCallback: _ => {},
            eventSubmittedCallback: value =>
            {
                bool success = int.TryParse(value, out int i);
                if (!success) return;
                CubeCamera.EquirectangularWidth = Mathf.Clamp(i, 32, 16384);
                ModConfig.Save();
            });
        
        helper.AddTextfield(
            text: "Equirectangular Height",
            defaultContent: CubeCamera.EquirectangularHeight.ToString(),
            eventChangedCallback: _ => {},
            eventSubmittedCallback: value =>
            {
                bool success = int.TryParse(value, out int i);
                if (!success) return;
                CubeCamera.EquirectangularHeight = Mathf.Clamp(i, 32, 16384);
                ModConfig.Save();
            });

        {
            string[] options =
            {
                nameof(Pieces),
                nameof(Cubemap), 
                nameof(Equirectangular),
            };
            helper.AddDropdown(
                text: "Mapping Format",
                options: options,
                defaultSelection: Array.IndexOf(options, CubeCamera.MappingFormat),
                eventCallback: value =>
                {
                    CubeCamera.MappingFormat = options[value];
                    ModConfig.Save();
                });
        }

        {
            string[] options =
            {
                nameof(Cubemap.GridPreset.Cross4x3), 
                nameof(Cubemap.GridPreset.Cross3x4),
                nameof(Cubemap.GridPreset.Pano2VR3x2),
                nameof(Cubemap.GridPreset.Facebook3x2),
                nameof(Cubemap.GridPreset.Row6x1),
                nameof(Cubemap.GridPreset.Column1x6),
            };
            helper.AddDropdown(
                text: "Cubemap Grid",
                options: options,
                defaultSelection: Array.IndexOf(options, CubeCamera.CubemapGrid),
                eventCallback: value =>
                {
                    CubeCamera.CubemapGrid = options[value];
                    ModConfig.Save();
                });
        }
        
        helper.AddDropdown(
            text: "Save File Format", 
            options: Enum.GetNames(typeof(FileFormat)), 
            defaultSelection: (int)CubeCamera.SaveFileFormat, 
            eventCallback: value =>
            {
                CubeCamera.SaveFileFormat = (FileFormat)value; 
                ModConfig.Save();
            });
        
        helper.AddDropdown(
            text: "Screen Scale Mode", 
            options: Enum.GetNames(typeof(ScaleMode)), 
            defaultSelection: (int)CubeCamera.ScreenScaleMode, 
            eventCallback: value =>
            {
                CubeCamera.ScreenScaleMode = (ScaleMode)value; 
                ModConfig.Save();
            });

        helper.AddButton("Reset Configs", () =>
        {
            CubeCamera.FaceSize = ModConfig.Defaults.FaceSize;
            CubeCamera.EquirectangularWidth = ModConfig.Defaults.EquirectangularWidth;
            CubeCamera.EquirectangularHeight = ModConfig.Defaults.EquirectangularHeight;
            CubeCamera.MappingFormat = ModConfig.Defaults.MappingFormat;
            CubeCamera.SaveFileFormat = ModConfig.Defaults.SaveFileFormat;
            CubeCamera.CubemapGrid = ModConfig.Defaults.CubemapGrid;
            CubeCamera.ScreenScaleMode = ModConfig.Defaults.ScreenScaleMode;
            ModConfig.Save();
        });

        
        // It's pretty crazy below here. But I don't have time to clean up the UI...
        
        var continuousModeGroup = helper.AddGroup("Continuous Mode") as UIHelper ?? throw new NullReferenceException();
        
        _startPointTextField = continuousModeGroup.AddTextfield(
            text: "Start Point", 
            defaultContent: "x, y, z, x, y, z",
            eventChangedCallback: _ => {},
            eventSubmittedCallback: text =>
            {
                var args = text.Split(',');
                if (args.Length != 6) return;

                bool success = true;

                success &= float.TryParse(args[0], out float posx);
                success &= float.TryParse(args[1], out float posy);
                success &= float.TryParse(args[2], out float posz);
                success &= float.TryParse(args[3], out float rotx);
                success &= float.TryParse(args[4], out float roty);
                success &= float.TryParse(args[5], out float rotz);

                if (!success) return;
                
                CubeCamera.ContinuousMode = new ContinuousMode(
                    startPoint: new Pose(new Vector3(posx, posy, posz), Quaternion.Euler(rotx, roty, rotz)),
                    endPoint: CubeCamera.ContinuousMode?.EndPoint ?? new Pose(),
                    division: CubeCamera.ContinuousMode?.Division ?? (uint)(_duration * _frameRate)
                );
            }) as UITextField ?? throw new NullReferenceException();
        
        _endPointTextField = continuousModeGroup.AddTextfield(
            text: "End Point", 
            defaultContent: "x, y, z, x, y, z",
            eventChangedCallback: _ => {},
            eventSubmittedCallback: text =>
            {
                var args = text.Split(',');
                if (args.Length != 6) return;

                bool success = true;

                success &= float.TryParse(args[0], out float posx);
                success &= float.TryParse(args[1], out float posy);
                success &= float.TryParse(args[2], out float posz);
                success &= float.TryParse(args[3], out float rotx);
                success &= float.TryParse(args[4], out float roty);
                success &= float.TryParse(args[5], out float rotz);

                if (!success) return;
                
                CubeCamera.ContinuousMode = new ContinuousMode(
                    startPoint: CubeCamera.ContinuousMode?.StartPoint ?? new Pose(),
                    endPoint: new Pose(new Vector3(posx, posy, posz), Quaternion.Euler(rotx, roty, rotz)),
                    division: CubeCamera.ContinuousMode?.Division ?? (uint)(_duration * _frameRate)
                );
            }) as UITextField ?? throw new NullReferenceException();

        continuousModeGroup.AddButton("Set here as start point", () =>
        {
            Transform transform = GameObject.FindGameObjectWithTag("MainCamera").transform;
            var position = transform.position;
            var rotation = transform.rotation;
            CubeCamera.ContinuousMode = new ContinuousMode(
                startPoint: new Pose(position, rotation),
                endPoint: CubeCamera.ContinuousMode?.EndPoint ?? new Pose(),
                division: CubeCamera.ContinuousMode?.Division ?? (uint)(_duration * _frameRate)
            );

            _startPointTextField.text =
                $"{position.x}, {position.y}, {position.z}, {rotation.x}, {rotation.y}, {rotation.z}";
        });
        
        continuousModeGroup.AddButton("Set here as end point", () =>
        {
            Transform transform = GameObject.FindGameObjectWithTag("MainCamera").transform;
            var position = transform.position;
            var rotation = transform.rotation;
            CubeCamera.ContinuousMode = new ContinuousMode(
                startPoint: CubeCamera.ContinuousMode?.StartPoint ?? new Pose(),
                endPoint: new Pose(position, rotation),
                division: CubeCamera.ContinuousMode?.Division ?? (uint)(_duration * _frameRate)
            );

            _endPointTextField.text =
                $"{position.x}, {position.y}, {position.z}, {rotation.x}, {rotation.y}, {rotation.z}";
        });
        
        continuousModeGroup.AddTextfield("Duration", _duration.ToString(CultureInfo.InvariantCulture), text =>
        {
            float.TryParse(text, out float f);
            _duration = f;

            CubeCamera.ContinuousMode = new ContinuousMode(
                startPoint: CubeCamera.ContinuousMode?.StartPoint ?? new Pose(),
                endPoint: CubeCamera.ContinuousMode?.EndPoint ?? new Pose(),
                division: (uint)(_duration * _frameRate));
            if (_continuousLabel is not null)
            {
                _continuousLabel.text = $"{(uint)(_duration * _frameRate)} images will be taken.";
            }
        }, _ => { });
        
        continuousModeGroup.AddTextfield("Frame Rate", _frameRate.ToString(), text =>
        {
            int.TryParse(text, out int i);
            _frameRate = i;
            
            CubeCamera.ContinuousMode = new ContinuousMode(
                startPoint: CubeCamera.ContinuousMode?.StartPoint ?? new Pose(),
                endPoint: CubeCamera.ContinuousMode?.EndPoint ?? new Pose(),
                division: (uint)(_duration * _frameRate));
            if (_continuousLabel is not null)
            {
                _continuousLabel.text = $"{(uint)(_duration * _frameRate)} images will be taken.";
            }
        }, _ => { });

        _continuousLabel = (continuousModeGroup.self as UIPanel)?.AddUIComponent<UILabel>();
        if (_continuousLabel is not null) _continuousLabel.text = $"{(uint)(_duration * _frameRate)} images will be taken.";
        _continuousProgressLabel = (continuousModeGroup.self as UIPanel)?.AddUIComponent<UILabel>();
        
        continuousModeGroup.AddButton("Take Continuous (play)", () =>
        {
            CubeCamera.TakeContinuous(pause: false, saveCallback: i =>
            {
                if (_continuousProgressLabel is null) return;
                _continuousProgressLabel.text = $"progress {i}/{(uint)(_duration * _frameRate)}";
            });
        });
        
        continuousModeGroup.AddButton("Take Continuous (pause)", () =>
        {
            CubeCamera.TakeContinuous(pause: true, saveCallback: i =>
            {
                if (_continuousProgressLabel is null) return;
                _continuousProgressLabel.text = $"progress {i}/{(uint)(_duration * _frameRate)}";
            });
        });
        
        continuousModeGroup.AddButton("Force stop", CubeCamera.ForceStop);
    }
}