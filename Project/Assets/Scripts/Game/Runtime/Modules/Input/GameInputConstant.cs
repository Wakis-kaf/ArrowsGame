using Framework.Runtime.MLanAndTheme;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Modules.GModuleInput
{
    public static class GameInputConstant
    {
        public static string Play_Input_Layer = "PlayLayer";
        public static string Play_Input_Horzontal_Name = "horizontalInput";
        public static string Play_Input_Vertical_Name = "verticalInput";
        public static string Ui_Input_Layer = "UILayer";
        public static string Ui_Input_Left_Rot_Name = "leftRot";
        public static string Ui_Input_Right_Rot_Name = "rightRot";
        public static string Ui_Input_Flip_Name = "flip";
        public static string Ui_Input_MouseLeft_Name = "mouseLeft";
        public static string Ui_Input_MouseRight_Name = "mouseRight";
        public static string Ui_Input_MousePos_Name = "mousePos";
        public static string Ui_Input_KeyAlt_Name = "keyAlt";
        public readonly static InputLayerOption[] GameInputLayer = new InputLayerOption[]
        {

            new InputLayerOption(Play_Input_Layer)
            {
                Enable = true,
                Inputs = new InputModule.InputData[]
                {
                    new InputModule.InputData()
                    {
                        inputName = Play_Input_Horzontal_Name,
                        inputType = InputModule.InputType.AxisValue,
                        axisName = "Horizontal",
                    },
                    new InputModule.InputData()
                    {
                        inputName = Play_Input_Vertical_Name,
                        inputType = InputModule.InputType.AxisValue,
                        axisName = "Vertical",
                    },
                }
            },
            new InputLayerOption(Ui_Input_Layer)
            {
                Enable = true,
                Inputs = new InputModule.InputData[]
                {
                    new InputModule.InputData()
                    {
                        inputName = Ui_Input_Left_Rot_Name,
                        inputType = InputModule.InputType.Keyboard,
                        keyCode = KeyCode.Q,

                    },
                    new InputModule.InputData()       {
                        inputName = Ui_Input_Right_Rot_Name,
                        inputType = InputModule.InputType.Keyboard,
                        keyCode = KeyCode.E,
                    },
                    new InputModule.InputData()
                    {
                        inputName = Ui_Input_Flip_Name,
                        inputType = InputModule.InputType.Keyboard,
                        keyCode = KeyCode.F,

                    },
                    new InputModule.InputData()
                    {
                        inputName = Ui_Input_KeyAlt_Name,
                        inputType = InputModule.InputType.Keyboard,
                        keyCode = KeyCode.LeftAlt,

                    },
                     new InputModule.InputData()
                    {
                        inputName = Ui_Input_MouseLeft_Name,
                        inputType = InputModule.InputType.MouseButton,
                        mouseKey = 0
                    },
                        new InputModule.InputData()
                    {
                        inputName = Ui_Input_MouseRight_Name,
                        inputType = InputModule.InputType.MouseButton,
                        mouseKey = 1


                    },
                    new InputModule.InputData()
                    {
                        inputName = Ui_Input_MousePos_Name,
                        inputType = InputModule.InputType.MousePosition
                    }

            }
             }
        };
    }

}
